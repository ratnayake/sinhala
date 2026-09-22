using SinhalaInput.Platform.Windows;
using SinhalaInput.Platform.Windows.Hooking;

namespace SinhalaInput.Platform.Windows.Tests.Hooking;

public class LowLevelKeyboardEventMapperTests
{
    [Theory]
    [InlineData(NativeMethods.WM_KEYDOWN, true)]
    [InlineData(NativeMethods.WM_SYSKEYDOWN, true)]
    [InlineData(NativeMethods.WM_KEYUP, false)]
    [InlineData(NativeMethods.WM_SYSKEYUP, false)]
    public void TryCreateEventArgs_ForKeyMessages_ReturnsArgsWithExpectedDirection(int wParam, bool expectedIsKeyDown)
    {
        var data = new NativeMethods.KBDLLHOOKSTRUCT { vkCode = 0x41 }; // 'A'

        bool created = LowLevelKeyboardEventMapper.TryCreateEventArgs(wParam, in data, out var args);

        Assert.True(created);
        Assert.NotNull(args);
        Assert.Equal(0x41, args!.VirtualKeyCode);
        Assert.Equal(expectedIsKeyDown, args.IsKeyDown);
        Assert.False(args.Handled);
    }

    [Fact]
    public void TryCreateEventArgs_ForUnrecognizedMessage_ReturnsFalseAndNullArgs()
    {
        var data = new NativeMethods.KBDLLHOOKSTRUCT { vkCode = 0x41 };

        bool created = LowLevelKeyboardEventMapper.TryCreateEventArgs(wParam: 0x9999, in data, out var args);

        Assert.False(created);
        Assert.Null(args);
    }

    [Fact]
    public void TryCreateEventArgs_PropagatesVirtualKeyCodeFromNativeStruct()
    {
        var data = new NativeMethods.KBDLLHOOKSTRUCT { vkCode = 0x0D }; // VK_RETURN

        LowLevelKeyboardEventMapper.TryCreateEventArgs(NativeMethods.WM_KEYDOWN, in data, out var args);

        Assert.Equal(0x0D, args!.VirtualKeyCode);
    }
}
