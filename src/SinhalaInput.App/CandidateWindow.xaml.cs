using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using SinhalaInput.Platform.Windows.Caret;
using SinhalaInput.Platform.Windows.Windowing;

namespace SinhalaInput.App;

/// <summary>
/// A small, always-on-top popup anchored near the text caret that shows the primary Sinhala
/// transliteration plus a numbered list (1-9) of alternates from <see cref="CandidatePopupState"/>.
/// </summary>
/// <remarks>
/// This window must never take keyboard focus away from whatever the user is typing into, so it
/// combines WPF's <c>ShowActivated="False"</c> (suppresses activation on <c>Show()</c>) with the
/// Win32 <c>WS_EX_NOACTIVATE</c> extended style (suppresses activation on every subsequent click
/// too). The latter is a narrow, self-contained P/Invoke local to this window's own concern and
/// is not part of the centralized hook/injection/caret P/Invoke surface owned by
/// <c>SinhalaInput.Platform.Windows</c>.
///
/// Positioning is done in physical pixels by <see cref="PopupPlacement"/> (never via
/// <see cref="Window.Left"/>/<see cref="Window.Top"/>, which are DPI-scaled units).
/// </remarks>
public partial class CandidateWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x00000080;

    /// <summary>
    /// How long a newly opened popup waits for its caret anchor before showing at the mouse
    /// cursor instead. Caret lookup is asynchronous and usually lands within tens of
    /// milliseconds, but Chromium's first accessibility query can take several hundred.
    /// </summary>
    private static readonly TimeSpan AnchorWaitTimeout = TimeSpan.FromMilliseconds(750);

    private readonly DispatcherTimer _anchorWaitTimer;

    // The current word's anchor only: cleared on close so a new word never appears at the
    // previous word's position (possibly in another window or on another monitor).
    private ScreenPoint? _wordAnchor;
    private bool _isOpen;

    public CandidateWindow()
    {
        InitializeComponent();
        SourceInitialized += OnSourceInitialized;

        _anchorWaitTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher) { Interval = AnchorWaitTimeout };
        _anchorWaitTimer.Tick += OnAnchorWaitTimeout;
    }

    /// <summary>Updates the popup's content and position, showing or hiding it as needed.</summary>
    public void Render(CandidatePopupState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _isOpen = state.IsOpen;

        if (!state.IsOpen)
        {
            _anchorWaitTimer.Stop();
            _wordAnchor = null;
            Visibility = Visibility.Collapsed;
            return;
        }

        PrimaryText.Text = state.PrimaryPreview;
        AlternatesList.ItemsSource = state.Candidates
            .Select((candidate, index) => $"{index + 1}. {candidate}")
            .ToList();

        if (state.Anchor is { } anchor)
        {
            _anchorWaitTimer.Stop();
            _wordAnchor = anchor;
            ShowAt(anchor);
            return;
        }

        if (IsVisible)
        {
            // Shown via the timeout fallback and still unanchored: keep the position, resize.
            UpdateLayout();
            return;
        }

        if (!_anchorWaitTimer.IsEnabled)
        {
            _anchorWaitTimer.Start();
        }
    }

    private void OnAnchorWaitTimeout(object? sender, EventArgs e)
    {
        _anchorWaitTimer.Stop();

        if (!_isOpen || IsVisible)
        {
            return;
        }

        nint handle = new WindowInteropHelper(this).EnsureHandle();
        PopupPlacement.PlaceNearCursor(handle);
        Show();
        UpdateLayout();
        PopupPlacement.PlaceNearCursor(handle);
    }

    private void ShowAt(ScreenPoint anchor)
    {
        nint handle = new WindowInteropHelper(this).EnsureHandle();

        if (IsVisible)
        {
            // Let SizeToContent apply the new content's size first so clamping uses it.
            UpdateLayout();
            PopupPlacement.PlaceNearCaret(handle, anchor);
            return;
        }

        // Position before showing so the popup never appears at a stale spot, then re-place once
        // shown, because a hidden window's size is still the previous content's.
        PopupPlacement.PlaceNearCaret(handle, anchor);
        Show();
        UpdateLayout();
        PopupPlacement.PlaceNearCaret(handle, anchor);
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        nint handle = new WindowInteropHelper(this).Handle;
        int extendedStyle = NativeMethods.GetWindowLong(handle, GwlExStyle);
        _ = NativeMethods.SetWindowLong(handle, GwlExStyle, extendedStyle | WsExNoActivate | WsExToolWindow);
    }

    // Classic DllImport (rather than LibraryImport) so this narrow, local P/Invoke does not
    // require enabling AllowUnsafeBlocks for the whole App project.
    private static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(nint hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);
    }
}
