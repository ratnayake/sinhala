namespace SinhalaInput.Platform.Windows.Caret;

/// <summary>
/// <see cref="ICaretLocator"/> implemented with <c>GetGUIThreadInfo</c>, falling back to
/// <c>GetCaretPos</c>/<c>ClientToScreen</c>, and finally UI Automation's
/// <c>TextPattern.GetBoundingRectangles</c> for controls with no Win32 caret (design doc §8.3).
/// </summary>
public sealed class Win32CaretLocator : ICaretLocator
{
    public bool TryGetCaretScreenPosition(out ScreenPoint position)
    {
        position = default;
        throw new NotImplementedException(
            "Try GetGUIThreadInfo on the foreground window's thread, then GetCaretPos+ClientToScreen, then UI Automation as a last resort.");
    }
}
