namespace SinhalaInput.Platform.Windows.Hooking;

/// <summary>
/// <see cref="IKeyboardHook"/> implemented with <c>SetWindowsHookEx(WH_KEYBOARD_LL, ...)</c>.
/// </summary>
/// <remarks>
/// v1 placeholder: the P/Invoke hook procedure, message pump requirements, and safe
/// unhook-on-dispose/session-end behaviour described in design doc §8.1 and §10
/// ("Resource cleanup") still need to be implemented here, backed by signatures added to
/// <see cref="NativeMethods"/>. The hook callback must return in low single-digit
/// milliseconds — never do transliteration or UI work inline in the callback.
/// </remarks>
public sealed class LowLevelKeyboardHook : IKeyboardHook
{
    private bool _started;
    private bool _disposed;

    public event EventHandler<KeyInterceptedEventArgs>? KeyIntercepted;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_started)
        {
            throw new InvalidOperationException("The keyboard hook is already started.");
        }

        throw new NotImplementedException(
            "Install the WH_KEYBOARD_LL hook via NativeMethods.SetWindowsHookEx and wire the callback to raise KeyIntercepted.");
    }

    public void Stop()
    {
        _started = false;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        _disposed = true;
    }
}
