using System.Runtime.InteropServices;

namespace SinhalaInput.Platform.Windows.Input;

/// <summary>
/// <see cref="ITextInjector"/> implemented with <c>SendInput</c>: <paramref name="backspaceCount"/>
/// (see <see cref="ReplaceTypedText"/>) <c>VK_BACK</c> key-down/up pairs followed by one
/// <c>KEYEVENTF_UNICODE</c> key-down/up pair per UTF-16 code unit of the replacement text,
/// batched into a single call (design doc §8.2).
/// </summary>
public sealed class SendInputTextInjector : ITextInjector
{
    public void ReplaceTypedText(int backspaceCount, string text)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(backspaceCount);
        ArgumentNullException.ThrowIfNull(text);

        NativeMethods.INPUT[] inputs = SendInputSequenceBuilder.Build(backspaceCount, text);
        if (inputs.Length == 0)
        {
            return;
        }

        uint sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"SendInput only injected {sent} of {inputs.Length} synthetic events (Win32 error {error}).");
        }
    }
}
