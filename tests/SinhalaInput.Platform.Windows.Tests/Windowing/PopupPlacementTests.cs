using SinhalaInput.Platform.Windows.Caret;
using SinhalaInput.Platform.Windows.Windowing;

namespace SinhalaInput.Platform.Windows.Tests.Windowing;

public class PopupPlacementTests
{
    // A secondary monitor above the primary, like the negative-Y layouts that exposed the bug.
    private static readonly NativeMethods.RECT WorkArea = new() { Left = 0, Top = -1160, Right = 1920, Bottom = -80 };

    [Fact]
    public void PlacesPopupBelowCaret_WhenItFits()
    {
        (int x, int y) = PopupPlacement.ComputeTopLeft(new ScreenPoint(600, -600), 150, 100, WorkArea, lineOffset: 20);

        Assert.Equal((600, -580), (x, y));
    }

    [Fact]
    public void FlipsAboveCaret_WhenNoRoomBelow()
    {
        (int x, int y) = PopupPlacement.ComputeTopLeft(new ScreenPoint(600, -150), 150, 100, WorkArea, lineOffset: 20);

        Assert.Equal((600, -250), (x, y));
    }

    [Fact]
    public void ClampsToRightEdgeOfWorkArea()
    {
        (int x, _) = PopupPlacement.ComputeTopLeft(new ScreenPoint(1900, -600), 150, 100, WorkArea, lineOffset: 20);

        Assert.Equal(1920 - 150, x);
    }

    [Fact]
    public void ClampsAnchorOutsideWorkArea_BackOntoIt()
    {
        (int x, int y) = PopupPlacement.ComputeTopLeft(new ScreenPoint(-500, -5000), 150, 100, WorkArea, lineOffset: 20);

        Assert.Equal((0, -1160), (x, y));
    }
}
