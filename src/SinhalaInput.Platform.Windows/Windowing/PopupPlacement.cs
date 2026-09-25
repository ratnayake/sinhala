using System.Runtime.InteropServices;
using SinhalaInput.Platform.Windows.Caret;
using SinhalaInput.Platform.Windows.Dpi;

namespace SinhalaInput.Platform.Windows.Windowing;

/// <summary>
/// Positions a popup window just below a caret, in physical pixels, clamped to the work area of
/// the monitor containing the caret.
/// </summary>
/// <remarks>
/// Positioning goes through <c>SetWindowPos</c> under a <see cref="PerMonitorDpiScope"/> rather
/// than WPF's <c>Window.Left</c>/<c>Top</c>: those are device-independent units scaled by the
/// app's own DPI, so feeding them the physical-pixel caret position misplaces the popup on any
/// scaled or mixed-DPI desktop - often onto a different monitor, or entirely off-screen.
/// </remarks>
public static class PopupPlacement
{
    /// <summary>Gap between the caret's top edge and the popup, in 96-DPI units (scaled per monitor).</summary>
    private const int CaretLineOffsetDip = 20;

    /// <summary>
    /// Moves <paramref name="windowHandle"/> near <paramref name="caret"/> and re-asserts
    /// <c>HWND_TOPMOST</c> (other topmost windows, e.g. a browser going fullscreen, can
    /// otherwise end up above the popup). Never activates the window.
    /// </summary>
    public static void PlaceNearCaret(nint windowHandle, ScreenPoint caret)
    {
        using PerMonitorDpiScope dpiScope = PerMonitorDpiScope.Enter();

        nint monitor = NativeMethods.MonitorFromPoint(
            new NativeMethods.POINT { X = caret.X, Y = caret.Y }, NativeMethods.MONITOR_DEFAULTTONEAREST);

        var monitorInfo = new NativeMethods.MONITORINFO { cbSize = (uint)Marshal.SizeOf<NativeMethods.MONITORINFO>() };
        if (monitor == nint.Zero || !NativeMethods.GetMonitorInfo(monitor, ref monitorInfo)
            || !NativeMethods.GetWindowRect(windowHandle, out NativeMethods.RECT windowRect))
        {
            NativeMethods.SetWindowPos(
                windowHandle, NativeMethods.HWND_TOPMOST, caret.X, caret.Y, 0, 0,
                NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
            return;
        }

        uint dpi = NativeMethods.GetDpiForMonitor(monitor, NativeMethods.MDT_EFFECTIVE_DPI, out uint dpiX, out _) >= 0
            ? dpiX
            : 96;
        int lineOffset = (int)Math.Round(CaretLineOffsetDip * dpi / 96.0);

        (int x, int y) = ComputeTopLeft(
            caret,
            windowRect.Right - windowRect.Left,
            windowRect.Bottom - windowRect.Top,
            monitorInfo.rcWork,
            lineOffset);

        NativeMethods.SetWindowPos(
            windowHandle, NativeMethods.HWND_TOPMOST, x, y, 0, 0,
            NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
    }

    /// <summary>
    /// Fallback for when the caret could not be located in time: anchors at the mouse cursor,
    /// which is always current (unlike any previously resolved caret position).
    /// </summary>
    public static void PlaceNearCursor(nint windowHandle)
    {
        NativeMethods.POINT cursor;
        using (PerMonitorDpiScope.Enter())
        {
            if (!NativeMethods.GetCursorPos(out cursor))
            {
                return;
            }
        }

        PlaceNearCaret(windowHandle, new ScreenPoint(cursor.X, cursor.Y));
    }

    /// <summary>
    /// Places the popup <paramref name="lineOffset"/> below the caret's top; flips it above the
    /// caret when there is no room below, then clamps it inside <paramref name="workArea"/>.
    /// </summary>
    internal static (int X, int Y) ComputeTopLeft(
        ScreenPoint caret, int width, int height, NativeMethods.RECT workArea, int lineOffset)
    {
        int x = caret.X;
        int y = caret.Y + lineOffset;

        if (y + height > workArea.Bottom)
        {
            y = caret.Y - height;
        }

        x = Math.Max(workArea.Left, Math.Min(x, workArea.Right - width));
        y = Math.Max(workArea.Top, Math.Min(y, workArea.Bottom - height));
        return (x, y);
    }
}
