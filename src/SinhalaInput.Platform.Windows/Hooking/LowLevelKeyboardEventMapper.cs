namespace SinhalaInput.Platform.Windows.Hooking;

/// <summary>
/// Pure decoding of a <c>WH_KEYBOARD_LL</c> hook callback's <c>wParam</c>/<c>KBDLLHOOKSTRUCT</c>
/// into a <see cref="KeyInterceptedEventArgs"/>. Kept separate from
/// <see cref="LowLevelKeyboardHook"/> so this classification logic is unit testable without an
/// installed hook or any P/Invoke call.
/// </summary>
internal static class LowLevelKeyboardEventMapper
{
    /// <summary>
    /// Returns <see langword="false"/> when <paramref name="wParam"/> is not one of the four
    /// key-down/key-up message identifiers the OS is documented to pass to a
    /// <c>WH_KEYBOARD_LL</c> hook procedure, in which case <paramref name="args"/> is
    /// <see langword="null"/> and the event should not be raised.
    /// </summary>
    internal static bool TryCreateEventArgs(
        nint wParam,
        in NativeMethods.KBDLLHOOKSTRUCT data,
        out KeyInterceptedEventArgs? args)
    {
        bool isKeyDown = wParam == NativeMethods.WM_KEYDOWN || wParam == NativeMethods.WM_SYSKEYDOWN;
        bool isKeyUp = wParam == NativeMethods.WM_KEYUP || wParam == NativeMethods.WM_SYSKEYUP;

        if (!isKeyDown && !isKeyUp)
        {
            args = null;
            return false;
        }

        args = new KeyInterceptedEventArgs((int)data.vkCode, isKeyDown);
        return true;
    }
}
