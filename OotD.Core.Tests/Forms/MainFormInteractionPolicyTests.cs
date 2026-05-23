using System.Drawing;
using OotD.Forms;

namespace OotD.Core.Tests.Forms;

public class MainFormInteractionPolicyTests
{
    [Theory]
    [InlineData(0, 0, 2)]
    [InlineData(0, 99, 8)]
    [InlineData(99, 99, 6)]
    [InlineData(99, 0, 4)]
    [InlineData(0, 50, 1)]
    [InlineData(99, 50, 5)]
    [InlineData(50, 0, 3)]
    [InlineData(50, 99, 7)]
    [InlineData(50, 50, 0)]
    public void GetResizeDirection_WithPointerLocation_ReturnsExpectedDirection(
        int x,
        int y,
        int expectedDirection)
    {
        // Act
        var result = MainForm.GetResizeDirection(new Point(x, y), new Size(100, 100), lockPosition: false);

        // Assert
        result.Should().Be((MainForm.ResizeDirection)expectedDirection);
    }

    [Fact]
    public void GetResizeDirection_WhenPositionIsLocked_ReturnsNoneEvenAtResizeBorder()
    {
        // Act
        var result = MainForm.GetResizeDirection(new Point(0, 0), new Size(100, 100), lockPosition: true);

        // Assert
        result.Should().Be(MainForm.ResizeDirection.None);
    }

    [Theory]
    [MemberData(nameof(CursorMappings))]
    public void GetCursorForResizeDirection_WithDirection_ReturnsExpectedCursor(
        int direction,
        Cursor expectedCursor)
    {
        // Act
        var result = MainForm.GetCursorForResizeDirection((MainForm.ResizeDirection)direction);

        // Assert
        result.Should().BeSameAs(expectedCursor);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(25, 0.25)]
    [InlineData(50, 0.5)]
    [InlineData(99, 0.99)]
    [InlineData(100, 0.99)]
    public void NormalizeOpacityPercentage_WithPercentage_ReturnsExpectedOpacity(int percentage, double expectedOpacity)
    {
        // Act
        var result = MainForm.NormalizeOpacityPercentage(percentage);

        // Assert
        result.Should().Be(expectedOpacity);
    }

    [Fact]
    public void GetAssignedVirtualDesktopId_WithValidNonEmptyGuid_ReturnsGuid()
    {
        // Arrange
        var desktopId = Guid.NewGuid();

        // Act
        var result = MainForm.GetAssignedVirtualDesktopId(desktopId.ToString());

        // Assert
        result.Should().Be(desktopId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void GetAssignedVirtualDesktopId_WithMissingInvalidOrEmptyGuid_ReturnsNull(string? virtualDesktopId)
    {
        // Act
        var result = MainForm.GetAssignedVirtualDesktopId(virtualDesktopId);

        // Assert
        result.Should().BeNull();
    }

    public static TheoryData<int, Cursor> CursorMappings => new()
    {
        { 1, Cursors.SizeWE },
        { 5, Cursors.SizeWE },
        { 3, Cursors.SizeNS },
        { 7, Cursors.SizeNS },
        { 8, Cursors.SizeNESW },
        { 4, Cursors.SizeNESW },
        { 6, Cursors.SizeNWSE },
        { 2, Cursors.SizeNWSE },
        { 0, Cursors.Default }
    };
}
