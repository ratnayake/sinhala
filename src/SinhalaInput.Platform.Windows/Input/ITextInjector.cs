namespace SinhalaInput.Platform.Windows.Input;

/// <summary>Replaces already-typed text in the focused control with synthesized input (design doc §8.2).</summary>
public interface ITextInjector
{
    /// <summary>Sends <paramref name="backspaceCount"/> backspaces, then types <paramref name="text"/>.</summary>
    void ReplaceTypedText(int backspaceCount, string text);
}
