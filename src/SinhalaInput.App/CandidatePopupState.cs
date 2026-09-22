using SinhalaInput.Platform.Windows.Caret;

namespace SinhalaInput.App;

/// <summary>
/// A snapshot of what the candidate popup should currently display, raised by
/// <see cref="TypingSessionController"/> whenever the buffered word or candidate selection changes.
/// </summary>
/// <param name="IsOpen">Whether the popup should be visible at all.</param>
/// <param name="PrimaryPreview">
/// The deterministic engine transliteration of the buffered word (design doc §5, "Primary candidate").
/// </param>
/// <param name="Candidates">The ranked candidates; index 0 maps to digit key "1", and so on.</param>
/// <param name="SelectedIndex">The candidate currently highlighted by arrow-key navigation.</param>
/// <param name="Anchor">The screen position to anchor the popup near, if the caret could be located.</param>
public sealed record CandidatePopupState(
    bool IsOpen,
    string PrimaryPreview,
    IReadOnlyList<string> Candidates,
    int SelectedIndex,
    ScreenPoint? Anchor)
{
    /// <summary>The closed, empty popup state.</summary>
    public static CandidatePopupState Closed { get; } = new(false, string.Empty, [], 0, null);
}
