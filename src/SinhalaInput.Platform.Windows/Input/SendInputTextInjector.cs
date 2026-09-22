namespace SinhalaInput.Platform.Windows.Input;

/// <summary>
/// <see cref="ITextInjector"/> implemented with <c>SendInput</c>: <paramref name="backspaceCount"/>
/// <c>VK_BACK</c> key-down/up pairs followed by one <c>KEYEVENTF_UNICODE</c> key-down/up pair per
/// UTF-16 code unit of the replacement text, batched into a single call (design doc §8.2).
/// </summary>
/// <remarks>
/// v1 placeholder: build the <c>INPUT</c> array against signatures added to
/// <see cref="NativeMethods"/> and send it in one <c>SendInput</c> call so the replacement
/// lands atomically from the target application's point of view.
/// </remarks>
public sealed class SendInputTextInjector : ITextInjector
{
    public void ReplaceTypedText(int backspaceCount, string text)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(backspaceCount);
        ArgumentNullException.ThrowIfNull(text);

        throw new NotImplementedException(
            "Build an INPUT[] of backspaceCount VK_BACK pairs followed by KEYEVENTF_UNICODE pairs for `text`, then call NativeMethods.SendInput once.");
    }
}
