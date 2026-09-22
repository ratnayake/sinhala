namespace SinhalaInput.Platform.Windows.Input;

/// <summary>
/// Pure construction of the <c>INPUT[]</c> array <see cref="SendInputTextInjector"/> sends in a
/// single <c>SendInput</c> call (design doc §8.2). Kept separate from
/// <see cref="SendInputTextInjector"/> so this logic — count, ordering, and flags — is unit
/// testable without touching the real Win32 <c>SendInput</c> API.
/// </summary>
internal static class SendInputSequenceBuilder
{
    /// <summary>
    /// Builds <paramref name="backspaceCount"/> <c>VK_BACK</c> key-down/up pairs followed by
    /// one <c>KEYEVENTF_UNICODE</c> key-down/up pair per UTF-16 code unit of
    /// <paramref name="text"/> (so a Sinhala character outside the BMP that needs a surrogate
    /// pair correctly becomes two code-unit pairs, matching how Windows expects
    /// <c>KEYEVENTF_UNICODE</c> events to be sequenced).
    /// </summary>
    internal static NativeMethods.INPUT[] Build(int backspaceCount, string text)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(backspaceCount);
        ArgumentNullException.ThrowIfNull(text);

        var inputs = new NativeMethods.INPUT[(backspaceCount * 2) + (text.Length * 2)];
        int index = 0;

        for (int i = 0; i < backspaceCount; i++)
        {
            inputs[index++] = CreateKeyEvent(NativeMethods.VK_BACK, isKeyUp: false);
            inputs[index++] = CreateKeyEvent(NativeMethods.VK_BACK, isKeyUp: true);
        }

        foreach (char codeUnit in text)
        {
            inputs[index++] = CreateUnicodeEvent(codeUnit, isKeyUp: false);
            inputs[index++] = CreateUnicodeEvent(codeUnit, isKeyUp: true);
        }

        return inputs;
    }

    private static NativeMethods.INPUT CreateKeyEvent(ushort virtualKeyCode, bool isKeyUp) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        u = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wVk = virtualKeyCode,
                wScan = 0,
                dwFlags = isKeyUp ? NativeMethods.KEYEVENTF_KEYUP : 0,
                time = 0,
                dwExtraInfo = 0,
            },
        },
    };

    private static NativeMethods.INPUT CreateUnicodeEvent(char codeUnit, bool isKeyUp) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        u = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wVk = 0,
                wScan = codeUnit,
                dwFlags = NativeMethods.KEYEVENTF_UNICODE | (isKeyUp ? NativeMethods.KEYEVENTF_KEYUP : 0),
                time = 0,
                dwExtraInfo = 0,
            },
        },
    };
}
