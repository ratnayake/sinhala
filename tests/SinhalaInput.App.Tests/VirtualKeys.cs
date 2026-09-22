namespace SinhalaInput.App.Tests;

/// <summary>
/// The subset of Win32 virtual-key codes these tests drive <see cref="TypingSessionController"/>
/// with, mirroring exactly what a real <c>IKeyboardHook</c> implementation would report.
/// </summary>
internal static class VirtualKeys
{
    public const int Back = 0x08;
    public const int Tab = 0x09;
    public const int Return = 0x0D;
    public const int Shift = 0x10;
    public const int Control = 0x11;
    public const int Escape = 0x1B;
    public const int Space = 0x20;
    public const int PageUp = 0x21;
    public const int PageDown = 0x22;
    public const int Left = 0x25;
    public const int Up = 0x26;
    public const int Right = 0x27;
    public const int Down = 0x28;
    public const int D0 = 0x30;
    public const int A = 0x41;
    public const int OemPeriod = 0xBE;

    public static int Digit(int value) => D0 + value;

    public static int Letter(char lowerCaseLetter) => A + (lowerCaseLetter - 'a');
}
