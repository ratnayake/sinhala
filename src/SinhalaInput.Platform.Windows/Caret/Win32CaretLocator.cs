using System.Runtime.InteropServices;

namespace SinhalaInput.Platform.Windows.Caret;

/// <summary>
/// <see cref="ICaretLocator"/> implemented with <c>GetGUIThreadInfo</c>, falling back to
/// <c>GetCaretPos</c>/<c>ClientToScreen</c> (design doc §8.3).
/// </summary>
/// <remarks>
/// A third fallback via UI Automation's <c>TextPattern.GetBoundingRectangles</c> (for controls
/// that expose no Win32 caret at all, e.g. some Chromium/UWP surfaces) is called out in the
/// design doc as a stretch goal. It is deliberately not implemented in v1: it pulls in the
/// <c>UIAutomationClient</c>/<c>UIAutomationTypes</c> dependency and a materially more complex,
/// slower lookup (walking the focused automation element and its text pattern) for a class of
/// apps that is a minority of typing surfaces. The two Win32 strategies below are the required
/// v1 minimum; the gap is documented here so it is easy to find when picking up v1.1 work.
/// </remarks>
public sealed class Win32CaretLocator : ICaretLocator
{
    public bool TryGetCaretScreenPosition(out ScreenPoint position) =>
        TryGetFromGuiThreadInfo(out position) || TryGetFromCaretPos(out position);

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
}
