using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using Accessibility;
using SinhalaInput.Platform.Windows.Dpi;

namespace SinhalaInput.Platform.Windows.Caret;

/// <summary>
/// <see cref="ICaretLocator"/> implemented with <c>GetGUIThreadInfo</c>, then MSAA's
/// <c>OBJID_CARET</c>, then <c>GetCaretPos</c>/<c>ClientToScreen</c>, then UI Automation's
/// <c>TextPattern.GetBoundingRectangles</c>, then the mouse cursor (design doc §8.3).
/// </summary>
/// <remarks>
/// <para>
/// Every strategy runs inside a <see cref="PerMonitorDpiScope"/>, so results are always physical
/// screen pixels regardless of the calling thread's DPI awareness.
/// </para>
/// <para>
/// <c>GetGUIThreadInfo</c> and <c>GetCaretPos</c> only see a caret created by native Win32
/// text rendering (e.g. an <c>Edit</c> control). Chromium-based applications (Chrome, Edge,
/// Electron/CEF hosts) render text themselves: <c>GetGUIThreadInfo</c> reports no caret and
/// <c>GetCaretPos</c> reports a bogus client <c>(0, 0)</c> "success" that would anchor the popup
/// at the browser window's top-left. Chromium does, however, publish its real caret through
/// MSAA's <c>OBJID_CARET</c> once accessibility is active, which is why that strategy runs
/// before <c>GetCaretPos</c>, and why <c>GetCaretPos</c> is skipped entirely for Chromium
/// windows. Degenerate results from any strategy are rejected via <see cref="CaretGeometry"/>
/// so the chain falls through instead of short-circuiting on them.
/// </para>
/// <para>
/// UI Automation walks <see cref="AutomationElement.FocusedElement"/> across processes; when the
/// element exposes no usable <see cref="TextPattern"/> rectangle, the element's own bounds are
/// used - anchoring near the input field is still far better than not anchoring at all. The UIA
/// query also switches Chromium's accessibility tree on, which is what makes
/// <c>OBJID_CARET</c> exact on subsequent calls.
/// </para>
/// <para>
/// UIA/MSAA are cross-process COM calls that can take hundreds of milliseconds (Chromium builds
/// its accessibility tree lazily on first query), so this must never be called from the
/// keyboard-hook callback; callers run it on a background MTA thread.
/// </para>
/// </remarks>
public sealed class Win32CaretLocator : ICaretLocator
{
    private const string ChromiumWindowClass = "Chrome_WidgetWin_1";

    public bool TryGetCaretScreenPosition(out ScreenPoint position)
    {
        using PerMonitorDpiScope dpiScope = PerMonitorDpiScope.Enter();

        nint foregroundWindow = NativeMethods.GetForegroundWindow();
        bool isChromium = foregroundWindow != nint.Zero
            && string.Equals(GetWindowClassName(foregroundWindow), ChromiumWindowClass, StringComparison.Ordinal);

        return TryGetFromGuiThreadInfo(foregroundWindow, out position)
            || TryGetFromMsaaCaret(foregroundWindow, out position)
            || (!isChromium && TryGetFromCaretPos(foregroundWindow, out position))
            || TryGetFromUIAutomation(out position)
            || TryGetFromCursorPos(out position);
    }

    private static unsafe string GetWindowClassName(nint window)
    {
        const int capacity = 256;
        char* buffer = stackalloc char[capacity];
        int length = NativeMethods.GetClassName(window, buffer, capacity);
        return length > 0 ? new string(buffer, 0, length) : string.Empty;
    }

    private static bool TryGetFromGuiThreadInfo(nint foregroundWindow, out ScreenPoint position)
    {
        position = default;

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

        NativeMethods.RECT caret = info.rcCaret;
        if (!CaretGeometry.IsPlausibleCaretRect(caret.Left, caret.Top, caret.Right - caret.Left, caret.Bottom - caret.Top))
        {
            return false;
        }

        // rcCaret is expressed in the caret window's client coordinates.
        var topLeft = new NativeMethods.POINT { X = caret.Left, Y = caret.Top };
        if (!NativeMethods.ClientToScreen(info.hwndCaret, ref topLeft))
        {
            return false;
        }

        position = new ScreenPoint(topLeft.X, topLeft.Y);
        return true;
    }

    /// <summary>
    /// Any COM failure (no caret object, provider gone, RPC error) is "this strategy found
    /// nothing" rather than an exception out of <see cref="TryGetCaretScreenPosition"/>.
    /// </summary>
    private static bool TryGetFromMsaaCaret(nint foregroundWindow, out ScreenPoint position)
    {
        position = default;

        if (foregroundWindow == nint.Zero)
        {
            return false;
        }

        int hr = NativeMethods.AccessibleObjectFromWindow(
            foregroundWindow, NativeMethods.OBJID_CARET, in NativeMethods.IID_IAccessible, out nint unknown);
        if (hr < 0 || unknown == nint.Zero)
        {
            return false;
        }

        try
        {
            if (Marshal.GetObjectForIUnknown(unknown) is not IAccessible caret)
            {
                return false;
            }

            caret.accLocation(out int left, out int top, out int width, out int height, 0);
            if (!CaretGeometry.IsPlausibleCaretRect(left, top, width, height))
            {
                return false;
            }

            position = new ScreenPoint(left, top);
            return true;
        }
        catch (COMException)
        {
            return false;
        }
        catch (InvalidCastException)
        {
            return false;
        }
        finally
        {
            Marshal.Release(unknown);
        }
    }

    /// <summary>
    /// <c>GetCaretPos</c> only reports a caret owned by a window on the *calling* thread's
    /// input queue, so observing another process's foreground window requires temporarily
    /// attaching input queues via <c>AttachThreadInput</c> — otherwise this fallback would
    /// silently never find anything outside this process's own (caret-less) windows.
    /// </summary>
    private static bool TryGetFromCaretPos(nint foregroundWindow, out ScreenPoint position)
    {
        position = default;

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

        if (!NativeMethods.GetCaretPos(out NativeMethods.POINT clientPoint)
            || !CaretGeometry.IsPlausibleClientCaretPoint(clientPoint.X, clientPoint.Y))
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
    /// Any UI Automation failure (no focused element, RPC error talking to another process,
    /// pattern not supported) is treated as "this strategy found nothing" rather than allowed
    /// to throw out of <see cref="TryGetCaretScreenPosition"/>.
    /// </summary>
    private static bool TryGetFromUIAutomation(out ScreenPoint position)
    {
        position = default;

        try
        {
            AutomationElement? focused = AutomationElement.FocusedElement;
            if (focused is null)
            {
                return false;
            }

            if (TryGetFromTextPattern(focused, out position))
            {
                return true;
            }

            System.Windows.Rect bounds = focused.Current.BoundingRectangle;
            if (bounds.IsEmpty || !CaretGeometry.IsPlausibleElementRect(bounds.Left, bounds.Top, bounds.Width, bounds.Height))
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
        catch (COMException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static bool TryGetFromTextPattern(AutomationElement focused, out ScreenPoint position)
    {
        position = default;

        if (!focused.TryGetCurrentPattern(TextPattern.Pattern, out object? patternObj)
            || patternObj is not TextPattern textPattern)
        {
            return false;
        }

        TextPatternRange[] selection = textPattern.GetSelection();
        if (selection.Length == 0)
        {
            return false;
        }

        // A genuinely collapsed caret sometimes reports no rectangles at all in some UIA
        // providers; the caller falls back to the focused element's own bounds in that case.
        System.Windows.Rect[] rectangles = selection[0].GetBoundingRectangles();
        if (rectangles.Length == 0)
        {
            return false;
        }

        System.Windows.Rect first = rectangles[0];
        if (!CaretGeometry.IsPlausibleCaretRect(first.Left, first.Top, first.Width, first.Height))
        {
            return false;
        }

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
