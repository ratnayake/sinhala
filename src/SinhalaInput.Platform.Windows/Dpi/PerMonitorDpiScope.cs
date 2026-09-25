namespace SinhalaInput.Platform.Windows.Dpi;

/// <summary>
/// Temporarily switches the calling thread to per-monitor-v2 DPI awareness so every Win32,
/// MSAA and UI Automation coordinate read or written inside the scope is in physical screen
/// pixels.
/// </summary>
/// <remarks>
/// Without this, what a caret/placement API returns depends on the caller's awareness: for a
/// DPI-unaware or system-aware caller on a mixed-DPI multi-monitor desktop, Win32 APIs return
/// coordinates virtualized by the system DPI while UI Automation does not, so the strategies in
/// <see cref="Caret.Win32CaretLocator"/> would disagree with each other (and with the popup
/// placement) by tens to hundreds of pixels - enough to put the popup on another monitor.
/// </remarks>
internal readonly struct PerMonitorDpiScope : IDisposable
{
    private readonly nint _previousContext;

    private PerMonitorDpiScope(nint previousContext) => _previousContext = previousContext;

    public static PerMonitorDpiScope Enter() =>
        new(NativeMethods.SetThreadDpiAwarenessContext(NativeMethods.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2));

    public void Dispose()
    {
        // A zero previous context means the switch itself failed, so there is nothing to restore.
        if (_previousContext != nint.Zero)
        {
            NativeMethods.SetThreadDpiAwarenessContext(_previousContext);
        }
    }
}
