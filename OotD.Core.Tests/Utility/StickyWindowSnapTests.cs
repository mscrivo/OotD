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

    [Theory]
    [InlineData(UnsafeNativeMethods.HT.HTTOPLEFT, 10)]
    [InlineData(UnsafeNativeMethods.HT.HTTOP, 2)]
    [InlineData(UnsafeNativeMethods.HT.HTTOPRIGHT, 18)]
    [InlineData(UnsafeNativeMethods.HT.HTRIGHT, 16)]
    [InlineData(UnsafeNativeMethods.HT.HTBOTTOMRIGHT, 20)]
    [InlineData(UnsafeNativeMethods.HT.HTBOTTOM, 4)]
    [InlineData(UnsafeNativeMethods.HT.HTBOTTOMLEFT, 12)]
    [InlineData(UnsafeNativeMethods.HT.HTLEFT, 8)]
    [InlineData(UnsafeNativeMethods.HT.HTCAPTION, 0)]
    [InlineData(-1, 0)]
    public void GetResizeDirection_MapsOnlyResizeTargets(int hitTest, int expected)
    {
        StickyWindow.GetResizeDirection(hitTest).Should().Be((StickyWindow.ResizeDir)expected);
    }

    [Theory]
    [InlineData(-2100, -100, -1925, -7)]
    [InlineData(100, 1200, -5, 1073)]
    [InlineData(-1000, 500, -1005, 493)]
    public void ComputeMoveBounds_ClampsCursorBeforeSubtractingDragOffset(int mouseX, int mouseY,
        int expectedX, int expectedY)
    {
        var result = StickyWindow.ComputeMoveBounds(new Rectangle(0, 0, 300, 200), new Point(mouseX, mouseY),
            new Point(5, 7), new Rectangle(-1920, 0, 1920, 1080), [], Gap, false, false);

        result.Should().Be(new Rectangle(expectedX, expectedY, 300, 200));
    }

    [Theory]
    [InlineData(false, false, 7)]
    [InlineData(true, false, 0)]
    [InlineData(false, true, 5)]
    [InlineData(true, true, 5)]
    public void ComputeMoveBounds_HonorsSnapOptionsAndClosestTarget(bool stickToScreen, bool stickToOther,
        int expectedX)
    {
        var otherWindows = new[] { new Rectangle(-295, 450, 300, 400), new Rectangle(-299, 450, 300, 400) };

        var result = StickyWindow.ComputeMoveBounds(new Rectangle(0, 0, 300, 200), new Point(7, 500),
            Point.Empty, Screen, otherWindows, Gap, stickToScreen, stickToOther);

        result.Should().Be(new Rectangle(expectedX, 500, 300, 200));
    }

    [Fact]
    public void ComputeMoveBounds_NoNearbyTargetsPreservesUnsnappedPosition()
    {
        var result = StickyWindow.ComputeMoveBounds(new Rectangle(0, 0, 300, 200), new Point(500, 500),
            Point.Empty, Screen, [], Gap, true, true);

        result.Should().Be(new Rectangle(500, 500, 300, 200));
    }

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

    [Theory]
    [InlineData(false, false, 0, 0)]
    [InlineData(true, false, 5, 6)]
    [InlineData(false, true, 4, 0)]
    [InlineData(true, true, 4, 6)]
    public void ComputeResizeOffsets_PreservesSnapOptionsAndTargetOrder(bool stickToScreen, bool stickToOther,
        int expectedWidthOffset, int expectedHeightOffset)
    {
        var otherWindows = new[] { new Rectangle(1918, 1000, 300, 100), new Rectangle(1919, 1000, 300, 100) };

        var result = StickyWindow.ComputeResizeOffsets(new Rectangle(1000, 800, 915, 274), Screen, otherWindows,
            StickyWindow.ResizeDir.Right | StickyWindow.ResizeDir.Bottom, Gap, stickToScreen, stickToOther);

        result.Should().Be(new Rectangle(11, 11, expectedWidthOffset, expectedHeightOffset));
    }

    [Fact]
    public void ComputeResizeOffsets_NoNearbyTargetsPreservesSeed()
    {
        var result = StickyWindow.ComputeResizeOffsets(new Rectangle(500, 500, 300, 200), Screen, [],
            StickyWindow.ResizeDir.Top | StickyWindow.ResizeDir.Left, Gap, true, true);

        result.Should().Be(ResizeSeed);
    }

    [Theory]
    [InlineData(8, 80, 100, 320, 200)]
    [InlineData(16, 100, 100, -20, 200)]
    [InlineData(2, 100, 70, 300, 230)]
    [InlineData(4, 100, 100, 300, -30)]
    [InlineData(10, 80, 70, 320, 230)]
    [InlineData(18, 100, 70, -20, 230)]
    [InlineData(12, 80, 100, 320, -30)]
    [InlineData(20, 100, 100, -20, -30)]
    [InlineData(0, 100, 100, 300, 200)]
    public void StretchResizeBounds_ChangesOnlyDraggedEdges(int direction, int left, int top, int width, int height)
    {
        var result = StickyWindow.StretchResizeBounds(new Rectangle(100, 100, 300, 200), new Point(80, 70),
            (StickyWindow.ResizeDir)direction);

        result.Should().Be(new Rectangle(left, top, width, height));
    }

    [Theory]
    [InlineData(50, 100, 0, 80, 1000, 100)]
    [InlineData(50, 60, 0, 80, 1000, 80)]
    [InlineData(1200, 100, 0, 80, 1000, 1000)]
    [InlineData(1200, 100, 500, 80, 1000, 500)]
    [InlineData(300, 100, 500, 80, 1000, 300)]
    [InlineData(300, 100, 50, 80, 1000, 100)]
    public void ConstrainResizeDimension_RespectsLimitPrecedence(int value, int minimum, int maximum,
        int minimumTrack, int maximumTrack, int expected)
    {
        StickyWindow.ConstrainResizeDimension(value, minimum, maximum, minimumTrack, maximumTrack)
            .Should().Be(expected);
    }

    [Theory]
    [InlineData(11, 0)]
    [InlineData(10, 10)]
    [InlineData(-10, -10)]
    [InlineData(0, 0)]
    public void ClearSnapSentinel_OnlyClearsUnchangedOffsets(int offset, int expected)
    {
        StickyWindow.ClearSnapSentinel(offset, Gap).Should().Be(expected);
    }

    [Fact]
    public void ApplyResizeOffsets_TopLeftLimitsKeepOppositeCornerFixed()
    {
        var original = new Rectangle(100, 100, 300, 200);
        var stretched = StickyWindow.StretchResizeBounds(original, new Point(390, 290),
            StickyWindow.ResizeDir.Top | StickyWindow.ResizeDir.Left);

        var result = StickyWindow.ApplyResizeOffsets(original, stretched, ResizeSeed,
            StickyWindow.ResizeDir.Top | StickyWindow.ResizeDir.Left, Gap,
            new Size(100, 80), Size.Empty, new Size(40, 30), new Size(1000, 1000));

        result.Should().Be(new Rectangle(300, 220, 100, 80));
    }

    [Theory]
    [InlineData(10, 96, 96, 304, 204)]
    [InlineData(20, 100, 100, 304, 204)]
    public void ApplyResizeOffsets_CombinesOffsetsAndPreservesAnchors(int direction, int left, int top,
        int width, int height)
    {
        var bounds = new Rectangle(100, 100, 300, 200);

        var result = StickyWindow.ApplyResizeOffsets(bounds, bounds, new Rectangle(7, -2, -3, 6),
            (StickyWindow.ResizeDir)direction, Gap, Size.Empty, Size.Empty, Size.Empty, new Size(1000, 1000));

        result.Should().Be(new Rectangle(left, top, width, height));
    }

    [Fact]
    public void ApplyResizeOffsets_ClearsSentinelsInEveryComponent()
    {
        var bounds = new Rectangle(100, 100, 300, 200);

        var result = StickyWindow.ApplyResizeOffsets(bounds, bounds, new Rectangle(11, 11, 11, 11),
            StickyWindow.ResizeDir.Bottom | StickyWindow.ResizeDir.Right, Gap,
            new Size(500, 500), new Size(600, 600), Size.Empty, new Size(1000, 1000));

        result.Should().Be(bounds);
    }

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
