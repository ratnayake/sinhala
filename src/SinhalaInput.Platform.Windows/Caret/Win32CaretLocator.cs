using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Automation.Text;

namespace SinhalaInput.Platform.Windows.Caret;

/// <summary>
/// <see cref="ICaretLocator"/> implemented with <c>GetGUIThreadInfo</c>, falling back to
/// <c>GetCaretPos</c>/<c>ClientToScreen</c>, and then to UI Automation's
/// <c>TextPattern.GetBoundingRectangles</c> (design doc §8.3).
/// </summary>
/// <remarks>
/// The first two strategies are classic Win32 APIs that only see a caret exposed by native
/// Win32 text-rendering (e.g. an <c>Edit</c> control). Chromium-based applications (Edge,
/// Chrome, CEF hosts) and some UWP surfaces render text themselves and never populate either
/// API, so both strategies silently fail there. <see cref="TryGetFromUIAutomation"/> is the
/// fallback for exactly that case: UI Automation walks <see cref="AutomationElement.FocusedElement"/>
/// (which works across processes without the <c>AttachThreadInput</c> dance the two Win32
/// strategies need) and, when the focused element supports <see cref="TextPattern"/>, reads the
/// bounding rectangle of its current selection/caret range. If the element exposes no text
/// pattern at all, or the pattern reports no rectangles for a collapsed caret (some UIA
/// providers do this), the element's own bounding rectangle is used instead - anchoring near the
/// input field as a whole is still far better than not anchoring at all.
///
/// As a last resort, if none of the three strategies can locate anything, the current mouse
/// cursor position (<c>GetCursorPos</c>, which always succeeds regardless of which application
/// or control has focus) is used, so <see cref="TryGetCaretScreenPosition"/> essentially never
/// returns <see langword="false"/> in practice and the candidate popup is never left anchored to
/// a stale, off-screen, or default position.
/// </remarks>
public sealed class Win32CaretLocator : ICaretLocator
{
    public bool TryGetCaretScreenPosition(out ScreenPoint position) =>
        TryGetFromGuiThreadInfo(out position)
        || TryGetFromCaretPos(out position)
        || TryGetFromUIAutomation(out position)
        || TryGetFromCursorPos(out position);

    private static bool TryGetFromGuiThreadInfo(out ScreenPoint position)
    {
        position = default;

        nint foregroundWindow = NativeMethods.GetForegroundWindow();
        if (foregroundWindow == nint.Zero)
        {
            return false;
        }

        uint threadId = NativeMethods.GetWindowThreadProcessId(foregroundWindow, out _);

        var info = default(NativeMethods.GUITHREADINFO);
        info.cbSize = (uint)Marshal.SizeOf<NativeMethods.GUITHREADINFO>();

        if (!NativeMethods.GetGUIThreadInfo(threadId, ref info) || info.hwndCaret == nint.Zero)
        {
            return false;
        }

        // rcCaret is expressed in the caret window's client coordinates.
        var topLeft = new NativeMethods.POINT { X = info.rcCaret.Left, Y = info.rcCaret.Top };
        if (!NativeMethods.ClientToScreen(info.hwndCaret, ref topLeft))
        {
            return false;
        }

        position = new ScreenPoint(topLeft.X, topLeft.Y);
        return true;
    }

    /// <summary>
    /// <c>GetCaretPos</c> only reports a caret owned by a window on the *calling* thread's
    /// input queue, so observing another process's foreground window requires temporarily
    /// attaching input queues via <c>AttachThreadInput</c> — otherwise this fallback would
    /// silently never find anything outside this process's own (caret-less) windows.
    /// </summary>
    private static bool TryGetFromCaretPos(out ScreenPoint position)
    {
        position = default;

        nint foregroundWindow = NativeMethods.GetForegroundWindow();
        if (foregroundWindow == nint.Zero)
        {
            return false;
        }

        uint foregroundThreadId = NativeMethods.GetWindowThreadProcessId(foregroundWindow, out _);
        uint currentThreadId = NativeMethods.GetCurrentThreadId();

        if (foregroundThreadId == currentThreadId)
        {
            return TryReadCaret(foregroundWindow, out position);
        }

        if (!NativeMethods.AttachThreadInput(currentThreadId, foregroundThreadId, fAttach: true))
        {
            return false;
        }

        try
        {
            return TryReadCaret(foregroundWindow, out position);
        }
        finally
        {
            NativeMethods.AttachThreadInput(currentThreadId, foregroundThreadId, fAttach: false);
        }
    }

    private static bool TryReadCaret(nint windowForClientToScreen, out ScreenPoint position)
    {
        position = default;

        if (!NativeMethods.GetCaretPos(out NativeMethods.POINT clientPoint))
        {
            return false;
        }

        if (!NativeMethods.ClientToScreen(windowForClientToScreen, ref clientPoint))
        {
            return false;
        }

        position = new ScreenPoint(clientPoint.X, clientPoint.Y);
        return true;
    }

    /// <summary>
    /// Covers Chromium (Edge/Chrome/CEF) and other surfaces that render text themselves and
    /// never populate a Win32 caret. Any UI Automation failure (no focused element, RPC error
    /// talking to another process, pattern not supported) is treated as "this strategy found
    /// nothing" rather than allowed to throw out of <see cref="TryGetCaretScreenPosition"/>.
    /// </summary>
    private static bool TryGetFromUIAutomation(out ScreenPoint position)
    {
        position = default;

        AutomationElement? focused;
        try
        {
            focused = AutomationElement.FocusedElement;
        }
        catch (ElementNotAvailableException)
        {
            return false;
        }

        if (focused is null)
        {
            return false;
        }

        if (TryGetFromTextPattern(focused, out position))
        {
            return true;
        }

        try
        {
            System.Windows.Rect bounds = focused.Current.BoundingRectangle;
            if (bounds.IsEmpty)
            {
                return false;
            }

            position = new ScreenPoint((int)bounds.Left, (int)bounds.Top);
            return true;
        }
        catch (ElementNotAvailableException)
        {
            return false;
        }
    }

    private static bool TryGetFromTextPattern(AutomationElement focused, out ScreenPoint position)
    {
        position = default;

        object? patternObj;
        try
        {
            if (!focused.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
            {
                return false;
            }
        }
        catch (ElementNotAvailableException)
        {
            return false;
        }

        if (patternObj is not TextPattern textPattern)
        {
            return false;
        }

        TextPatternRange[] selection;
        try
        {
            selection = textPattern.GetSelection();
        }
        catch (ElementNotAvailableException)
        {
            return false;
        }

        if (selection.Length == 0)
        {
            return false;
        }

        System.Windows.Rect[] rectangles;
        try
        {
            rectangles = selection[0].GetBoundingRectangles();
        }
        catch (ElementNotAvailableException)
        {
            return false;
        }

        // A genuinely collapsed caret sometimes reports no rectangles at all in some UIA
        // providers; the caller falls back to the focused element's own bounds in that case.
        if (rectangles.Length == 0)
        {
            return false;
        }

        // Top-left of the caret/selection rectangle, matching the top-left convention the other
        // two strategies use (rcCaret.Left/Top and the raw GetCaretPos client point) — the
        // caller (CandidateWindow) is the one that offsets a fixed amount below this point.
        System.Windows.Rect first = rectangles[0];
        position = new ScreenPoint((int)first.Left, (int)first.Top);
        return true;
    }

    private static bool TryGetFromCursorPos(out ScreenPoint position)
    {
        position = default;

        if (!NativeMethods.GetCursorPos(out NativeMethods.POINT cursor))
        {
            return false;
        }

        position = new ScreenPoint(cursor.X, cursor.Y);
        return true;
    }
}
