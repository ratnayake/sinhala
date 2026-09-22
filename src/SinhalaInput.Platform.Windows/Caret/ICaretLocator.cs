namespace SinhalaInput.Platform.Windows.Caret;

/// <summary>A point in screen pixel coordinates.</summary>
public readonly record struct ScreenPoint(int X, int Y);

/// <summary>Locates the text caret in screen coordinates, to anchor the candidate popup (design doc §8.3).</summary>
public interface ICaretLocator
{
    /// <summary>
    /// Attempts to find the caret's current screen position, trying (in order) the Win32
    /// GUI thread info, <c>GetCaretPos</c>, and UI Automation's text pattern.
    /// </summary>
    bool TryGetCaretScreenPosition(out ScreenPoint position);
}
