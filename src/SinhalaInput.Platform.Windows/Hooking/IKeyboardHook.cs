namespace SinhalaInput.Platform.Windows.Hooking;

/// <summary>
/// Observes keystrokes system-wide via a Win32 low-level keyboard hook
/// (design doc §2 Option B, §8.1).
/// </summary>
public interface IKeyboardHook : IDisposable
{
    /// <summary>Raised for every key event; set <see cref="KeyInterceptedEventArgs.Handled"/> to suppress it.</summary>
    event EventHandler<KeyInterceptedEventArgs>? KeyIntercepted;

    /// <summary>Installs the hook. Safe to call once; throws if already started.</summary>
    void Start();

    /// <summary>Removes the hook if installed. Safe to call even if not started.</summary>
    void Stop();
}
