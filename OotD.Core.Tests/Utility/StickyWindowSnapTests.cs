using System.Drawing;
using OotD.Utility;

namespace OotD.Core.Tests.Utility;

/// <summary>
///     Behavioral tests for the pure window snapping / magnetism math extracted from
///     <see cref="StickyWindow" />. These exercise the actual edge-distance comparisons that make
///     windows stick to the screen and to each other while moving and resizing.
/// </summary>
public class StickyWindowSnapTests
{
    private const int Gap = 10;

    // Move() seeds the running offset with (StickGap + 1) in each axis so that any candidate within
    // the gap wins; an axis left at the sentinel means "no snap happened on that axis".
    private static readonly Point MoveSeed = new(Gap + 1, Gap + 1);

    // Resize() seeds the offset rect similarly: X/Y start at (StickGap + 1), Width/Height at 0.
    private static readonly Rectangle ResizeSeed = new(Gap + 1, Gap + 1, 0, 0);

    private static readonly Rectangle Screen = new(0, 0, 1920, 1080);

    #region NormalizeInside

    [Theory]
    [InlineData(50, 0, 100, 50)] // inside the range is unchanged
    [InlineData(-5, 0, 100, 0)] // below the minimum clamps to minimum
    [InlineData(150, 0, 100, 100)] // above the maximum clamps to maximum
    [InlineData(0, 0, 100, 0)] // on the minimum boundary
    [InlineData(100, 0, 100, 100)] // on the maximum boundary
    public void NormalizeInside_ClampsValueIntoRange(int value, int min, int max, int expected)
    {
        StickyWindow.NormalizeInside(value, min, max).Should().Be(expected);
    }

    #endregion

    #region ComputeMoveSnap

    [Fact]
    public void ComputeMoveSnap_WhenLeftEdgeWithinGapOfScreenLeft_SnapsLeftToLeft()
    {
        // Form sits 7px inside the left of the screen -> should snap flush to x = 0.
        var form = new Rectangle(7, 500, 300, 200);

        var offset = StickyWindow.ComputeMoveSnap(form, Screen, MoveSeed, Gap, bInsideStick: false);

        offset.X.Should().Be(-7); // form.Left (7) + offset (-7) == 0
        offset.Y.Should().Be(MoveSeed.Y); // no vertical snap
    }

    [Fact]
    public void ComputeMoveSnap_WhenTopEdgeWithinGapOfScreenTop_SnapsTopToTop()
    {
        var form = new Rectangle(500, 6, 300, 200);

        var offset = StickyWindow.ComputeMoveSnap(form, Screen, MoveSeed, Gap, bInsideStick: false);

        offset.Y.Should().Be(-6); // form.Top (6) + offset (-6) == 0
        offset.X.Should().Be(MoveSeed.X); // no horizontal snap
    }

    [Fact]
    public void ComputeMoveSnap_WhenBeyondGap_DoesNotSnap()
    {
        // Far from every edge of the screen -> both axes remain at the sentinel.
        var form = new Rectangle(500, 500, 300, 200);

        var offset = StickyWindow.ComputeMoveSnap(form, Screen, MoveSeed, Gap, bInsideStick: false);

        offset.Should().Be(MoveSeed);
    }

    [Fact]
    public void ComputeMoveSnap_WithInsideStick_SnapsLeftEdgeToOtherWindowsRightEdge()
    {
        // Another window occupies x [0..300]; our window's left edge sits 7px past its right edge.
        var other = new Rectangle(0, 100, 300, 200);
        var form = new Rectangle(307, 100, 300, 200);

        var offset = StickyWindow.ComputeMoveSnap(form, other, MoveSeed, Gap, bInsideStick: true);

        offset.X.Should().Be(-7); // form.Left (307) + offset (-7) == other.Right (300)
    }

    [Fact]
    public void ComputeMoveSnap_WithoutVerticalOverlap_DoesNotSnapHorizontally()
    {
        // Horizontally within the gap of the screen's left edge, but vertically far below the
        // working area -> the horizontal snap block is guarded by the vertical-overlap check.
        var below = new Rectangle(0, -5000, 1920, 1080);
        var form = new Rectangle(7, 500, 300, 200);

        var offset = StickyWindow.ComputeMoveSnap(form, below, MoveSeed, Gap, bInsideStick: false);

        offset.X.Should().Be(MoveSeed.X);
    }

    #endregion

    #region ComputeResizeSnap

    [Fact]
    public void ComputeResizeSnap_WhenRightEdgeWithinGapOfScreenRight_SnapsRightToRight()
    {
        // Right edge (1915) is 5px short of the screen right (1920) -> Width offset should close it.
        var form = new Rectangle(0, 0, 1915, 200);

        var offset = StickyWindow.ComputeResizeSnap(form, Screen, ResizeSeed,
            StickyWindow.ResizeDir.Right, Gap, bInsideStick: false);

        offset.Width.Should().Be(5); // 1915 + 5 == 1920
    }

    [Fact]
    public void ComputeResizeSnap_WhenBottomEdgeWithinGapOfScreenBottom_SnapsBottomToBottom()
    {
        var form = new Rectangle(0, 0, 300, 1074); // bottom = 1074, 6px short of 1080

        var offset = StickyWindow.ComputeResizeSnap(form, Screen, ResizeSeed,
            StickyWindow.ResizeDir.Bottom, Gap, bInsideStick: false);

        offset.Height.Should().Be(6); // 1074 + 6 == 1080
    }

    [Fact]
    public void ComputeResizeSnap_WhenDirectionExcludesTheNearbyEdge_DoesNotSnap()
    {
        // Right edge is within the gap of the screen right, but we are only resizing the Left edge
        // (which sits 500px from the screen left) -> no change.
        var form = new Rectangle(500, 0, 1415, 200);

        var offset = StickyWindow.ComputeResizeSnap(form, Screen, ResizeSeed,
            StickyWindow.ResizeDir.Left, Gap, bInsideStick: false);

        offset.Should().Be(ResizeSeed);
    }

    [Fact]
    public void ComputeResizeSnap_WhenBeyondGap_DoesNotSnap()
    {
        var form = new Rectangle(500, 500, 300, 200);
        var far = new Rectangle(5000, 5000, 100, 100);

        var offset = StickyWindow.ComputeResizeSnap(form, far, ResizeSeed,
            StickyWindow.ResizeDir.Right | StickyWindow.ResizeDir.Bottom, Gap, bInsideStick: false);

        offset.Should().Be(ResizeSeed);
    }

    #endregion
}
