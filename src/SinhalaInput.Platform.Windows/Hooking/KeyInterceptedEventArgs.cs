namespace SinhalaInput.Platform.Windows.Hooking;

/// <summary>Raised for every key event observed by <see cref="IKeyboardHook"/>.</summary>
public sealed class KeyInterceptedEventArgs(int virtualKeyCode, bool isKeyDown) : EventArgs
{
    /// <summary>The Win32 virtual-key code (<c>VK_*</c>) for the event.</summary>
    public int VirtualKeyCode { get; } = virtualKeyCode;

    /// <summary><see langword="true"/> for a key-down event, <see langword="false"/> for key-up.</summary>
    public bool IsKeyDown { get; } = isKeyDown;

    /// <summary>
    /// Set to <see langword="true"/> to swallow the key so it never reaches the focused
    /// application (used when the tool itself has already acted on it, e.g. a candidate
    /// digit or the commit trigger it is about to replay via <c>ITextInjector</c>).
    /// </summary>
    public bool Handled { get; set; }
}
