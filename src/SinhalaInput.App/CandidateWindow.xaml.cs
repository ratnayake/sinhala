using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using SinhalaInput.Platform.Windows.Caret;

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
/// </remarks>
public partial class CandidateWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x00000080;

    public CandidateWindow()
    {
        InitializeComponent();
        SourceInitialized += OnSourceInitialized;
    }

    /// <summary>Updates the popup's content and position, showing or hiding it as needed.</summary>
    public void Render(CandidatePopupState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!state.IsOpen)
        {
            Visibility = Visibility.Collapsed;
            return;
        }

        PrimaryText.Text = state.PrimaryPreview;
        AlternatesList.ItemsSource = state.Candidates
            .Select((candidate, index) => $"{index + 1}. {candidate}")
            .ToList();

        if (state.Anchor is { } anchor)
        {
            Left = anchor.X;
            Top = anchor.Y + 20; // just below the caret line
        }

        if (Visibility != Visibility.Visible)
        {
            Show();
        }
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
