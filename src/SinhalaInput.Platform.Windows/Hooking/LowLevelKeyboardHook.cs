using System.Runtime.InteropServices;

namespace SinhalaInput.Platform.Windows.Hooking;

/// <summary>
/// <see cref="IKeyboardHook"/> implemented with <c>SetWindowsHookEx(WH_KEYBOARD_LL, ...)</c>.
/// </summary>
/// <remarks>
/// <para>
/// The native hook callback (<see cref="HookCallback"/>) runs on the thread that pumps
/// messages for the hook and must return within a few milliseconds, or Windows silently
/// removes the hook (design doc §8.1, §10 "Performance"). This implementation keeps the
/// callback itself minimal: it only marshals the native <c>KBDLLHOOKSTRUCT</c>/<c>wParam</c>
/// into a <see cref="KeyInterceptedEventArgs"/> and raises <see cref="KeyIntercepted"/>
/// synchronously. Transliteration and UI work must never happen inline here — subscribers
/// (composed in the App layer) are responsible for handling the event fast enough that the
/// combined cost stays within the OS's hook timeout budget, e.g. by posting to a dispatcher
/// instead of doing real work before returning.
/// </para>
/// <para>
/// Never log <see cref="KeyInterceptedEventArgs"/> data (or anything derived from it) to disk,
/// console, or any other diagnostic sink — this hook observes every keystroke system-wide,
/// including in password fields and other sensitive contexts (design doc §10 "Privacy").
/// </para>
/// <para>
/// Also hooking <see cref="Microsoft.Win32.SystemEvents.SessionEnding"/> to force an unhook on
/// logoff/shutdown would be a nice-to-have defence in depth, but is not required for v1: a
/// well-behaved <see cref="Dispose"/> call on normal process exit already removes the hook, and
/// Windows also tears down per-process hooks when the process itself terminates.
/// </para>
/// </remarks>
public sealed class LowLevelKeyboardHook : IKeyboardHook
{
    private readonly NativeMethods.LowLevelKeyboardProc _hookProc;
    private nint _hookHandle;
    private bool _started;
    private bool _disposed;

    public LowLevelKeyboardHook()
    {
        // Stored in a field (rather than a local/lambda) so the delegate - and the unmanaged
        // thunk SetWindowsHookEx receives a pointer to - is kept alive by the GC for as long as
        // this instance is, i.e. for as long as the hook can possibly be invoked.
        _hookProc = HookCallback;
    }

    public event EventHandler<KeyInterceptedEventArgs>? KeyIntercepted;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_started)
        {
            throw new InvalidOperationException("The keyboard hook is already started.");
        }

        nint hookProcPtr = Marshal.GetFunctionPointerForDelegate(_hookProc);
        _hookHandle = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, hookProcPtr, nint.Zero, dwThreadId: 0);
        if (_hookHandle == nint.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to install the low-level keyboard hook (Win32 error {error}).");
        }

        _started = true;
    }

    public void Stop()
    {
        if (!_started)
        {
            return;
        }

        if (_hookHandle != nint.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = nint.Zero;
        }

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

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode == NativeMethods.HC_ACTION && KeyIntercepted is { } handler)
        {
            NativeMethods.KBDLLHOOKSTRUCT data = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
            if (LowLevelKeyboardEventMapper.TryCreateEventArgs(wParam, in data, out KeyInterceptedEventArgs? args))
            {
                handler.Invoke(this, args!);
                if (args!.Handled)
                {
                    return 1;
                }
            }
        }

        return NativeMethods.CallNextHookEx(nint.Zero, nCode, wParam, lParam);
    }
}
