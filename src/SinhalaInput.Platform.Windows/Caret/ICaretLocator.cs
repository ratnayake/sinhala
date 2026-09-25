namespace SinhalaInput.Platform.Windows.Caret;

/// <summary>A point in physical screen pixels (per-monitor-v2 DPI coordinate space).</summary>
public readonly record struct ScreenPoint(int X, int Y);

/// <summary>Locates the text caret in screen coordinates, to anchor the candidate popup (design doc §8.3).</summary>
public interface ICaretLocator
{
    /// <summary>
    /// Attempts to find the top-left of the caret in physical screen pixels.
    /// </summary>
    /// <remarks>
    /// Implementations may make slow cross-process accessibility calls, so this must never be
    /// called from the keyboard-hook callback.
    /// </remarks>
    bool TryGetCaretScreenPosition(out ScreenPoint position);
}
