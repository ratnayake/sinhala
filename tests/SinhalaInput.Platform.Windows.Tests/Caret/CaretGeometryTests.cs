using SinhalaInput.Platform.Windows.Caret;

namespace SinhalaInput.Platform.Windows.Tests.Caret;

public class CaretGeometryTests
{
    [Fact]
    public void ClientCaretPoint_AtClientOrigin_IsRejected()
    {
        // Chromium/Edge: GetCaretPos "succeeds" with (0, 0) although no Win32 caret exists.
        Assert.False(CaretGeometry.IsPlausibleClientCaretPoint(0, 0));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 3)]
    [InlineData(42, 17)]
    public void ClientCaretPoint_AwayFromOrigin_IsAccepted(int x, int y)
    {
        Assert.True(CaretGeometry.IsPlausibleClientCaretPoint(x, y));
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(100, 200, 1, 0)]
    [InlineData(100, 200, -1, 15)]
    [InlineData(100, 200, 1, -15)]
    [InlineData(double.NaN, 200, 1, 15)]
    [InlineData(100, double.PositiveInfinity, 1, 15)]
    public void CaretRect_Degenerate_IsRejected(double left, double top, double width, double height)
    {
        Assert.False(CaretGeometry.IsPlausibleCaretRect(left, top, width, height));
    }

    [Theory]
    [InlineData(672, -561, 1, 15)]
    [InlineData(694, -561, 0, 15)] // Chromium sometimes reports a zero-width caret
    [InlineData(10, 10, 2, 16)]
    public void CaretRect_WithHeight_IsAccepted(double left, double top, double width, double height)
    {
        Assert.True(CaretGeometry.IsPlausibleCaretRect(left, top, width, height));
    }

    [Fact]
    public void ElementRect_WithoutWidth_IsRejected()
    {
        Assert.False(CaretGeometry.IsPlausibleElementRect(10, 10, 0, 20));
        Assert.True(CaretGeometry.IsPlausibleElementRect(10, 10, 200, 20));
    }
}
