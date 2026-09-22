using SinhalaInput.Platform.Windows;
using SinhalaInput.Platform.Windows.Input;

namespace SinhalaInput.Platform.Windows.Tests.Input;

public class SendInputSequenceBuilderTests
{
    [Fact]
    public void Build_WithNoBackspacesAndEmptyText_ReturnsEmptyArray()
    {
        NativeMethods.INPUT[] inputs = SendInputSequenceBuilder.Build(0, string.Empty);

        Assert.Empty(inputs);
    }

    [Fact]
    public void Build_WithNegativeBackspaceCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SendInputSequenceBuilder.Build(-1, "x"));
    }

    [Fact]
    public void Build_WithNullText_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SendInputSequenceBuilder.Build(0, null!));
    }

    [Fact]
    public void Build_WithOnlyBackspaces_ProducesDownUpPairsPerBackspace()
    {
        NativeMethods.INPUT[] inputs = SendInputSequenceBuilder.Build(3, string.Empty);

        Assert.Equal(6, inputs.Length);
        for (int pair = 0; pair < 3; pair++)
        {
            NativeMethods.INPUT down = inputs[pair * 2];
            NativeMethods.INPUT up = inputs[(pair * 2) + 1];

            Assert.Equal(NativeMethods.INPUT_KEYBOARD, down.type);
            Assert.Equal(NativeMethods.VK_BACK, down.u.ki.wVk);
            Assert.Equal(0u, down.u.ki.dwFlags);

            Assert.Equal(NativeMethods.INPUT_KEYBOARD, up.type);
            Assert.Equal(NativeMethods.VK_BACK, up.u.ki.wVk);
            Assert.Equal(NativeMethods.KEYEVENTF_KEYUP, up.u.ki.dwFlags);
        }
    }

    [Fact]
    public void Build_WithOnlyText_ProducesUnicodeDownUpPairPerCodeUnit()
    {
        const string text = "මම"; // two Sinhala BMP characters, one UTF-16 code unit each.

        NativeMethods.INPUT[] inputs = SendInputSequenceBuilder.Build(0, text);

        Assert.Equal(4, inputs.Length);
        for (int i = 0; i < text.Length; i++)
        {
            NativeMethods.INPUT down = inputs[i * 2];
            NativeMethods.INPUT up = inputs[(i * 2) + 1];

            Assert.Equal(0, down.u.ki.wVk);
            Assert.Equal(text[i], down.u.ki.wScan);
            Assert.Equal(NativeMethods.KEYEVENTF_UNICODE, down.u.ki.dwFlags);

            Assert.Equal(0, up.u.ki.wVk);
            Assert.Equal(text[i], up.u.ki.wScan);
            Assert.Equal(NativeMethods.KEYEVENTF_UNICODE | NativeMethods.KEYEVENTF_KEYUP, up.u.ki.dwFlags);
        }
    }

    [Fact]
    public void Build_WithSurrogatePairCharacter_EmitsOnePairPerUtf16CodeUnit()
    {
        // U+1F600 GRINNING FACE is outside the BMP and encodes as a UTF-16 surrogate pair;
        // SendInput's KEYEVENTF_UNICODE contract works per UTF-16 code unit, not per code point.
        const string emoji = "\U0001F600";
        Assert.Equal(2, emoji.Length);

        NativeMethods.INPUT[] inputs = SendInputSequenceBuilder.Build(0, emoji);

        Assert.Equal(4, inputs.Length);
        Assert.Equal(emoji[0], inputs[0].u.ki.wScan);
        Assert.Equal(emoji[0], inputs[1].u.ki.wScan);
        Assert.Equal(emoji[1], inputs[2].u.ki.wScan);
        Assert.Equal(emoji[1], inputs[3].u.ki.wScan);
    }

    [Fact]
    public void Build_WithBackspacesAndText_OrdersBackspacesBeforeText()
    {
        const string text = "අ";

        NativeMethods.INPUT[] inputs = SendInputSequenceBuilder.Build(2, text);

        Assert.Equal(6, inputs.Length);

        // First two pairs (4 entries) are backspaces...
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(NativeMethods.VK_BACK, inputs[i].u.ki.wVk);
        }

        // ...followed by the unicode pair for the replacement text.
        Assert.Equal(0, inputs[4].u.ki.wVk);
        Assert.Equal(text[0], inputs[4].u.ki.wScan);
        Assert.Equal(0, inputs[5].u.ki.wVk);
        Assert.Equal(text[0], inputs[5].u.ki.wScan);
    }
}
