using SinhalaInput.Platform.Windows.Hooking;

namespace SinhalaInput.Platform.Windows.Tests.Hooking;

/// <summary>
/// Covers only the lifecycle guard logic that does not require an installed hook: a real
/// <c>Start()</c> call installs a system-wide <c>WH_KEYBOARD_LL</c> hook via
/// <c>SetWindowsHookEx</c>, which is deliberately left to manual smoke testing (see
/// MANUAL-TESTING.md) rather than exercised here.
/// </summary>
public class LowLevelKeyboardHookLifecycleTests
{
    [Fact]
    public void Dispose_CalledTwice_IsIdempotent()
    {
        using var hook = new LowLevelKeyboardHook();

        hook.Dispose();
        Exception? exception = Record.Exception(hook.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public void Stop_WithoutStart_IsANoOp()
    {
        using var hook = new LowLevelKeyboardHook();

        Exception? exception = Record.Exception(hook.Stop);

        Assert.Null(exception);
    }

    [Fact]
    public void Start_AfterDispose_ThrowsObjectDisposedException()
    {
        var hook = new LowLevelKeyboardHook();
        hook.Dispose();

        Assert.Throws<ObjectDisposedException>(hook.Start);
    }

    [Fact]
    public void Stop_AfterDispose_DoesNotThrow()
    {
        var hook = new LowLevelKeyboardHook();
        hook.Dispose();

        Exception? exception = Record.Exception(hook.Stop);

        Assert.Null(exception);
    }
}
