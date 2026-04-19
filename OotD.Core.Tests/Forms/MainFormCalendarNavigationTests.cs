namespace OotD.Core.Tests.Forms;

using OotD.Enums;
using OotD.Forms;

public class MainFormCalendarNavigationTests
{
    [Theory]
    [InlineData(CurrentCalendarView.Day, CurrentCalendarView.Day, 1)]
    [InlineData(CurrentCalendarView.Week, CurrentCalendarView.Week, 7)]
    [InlineData(CurrentCalendarView.WorkWeek, CurrentCalendarView.WorkWeek, 7)]
    [InlineData(CurrentCalendarView.Month, CurrentCalendarView.Month, 1)]
    public void GetNextPreviousOffsetBasedOnCalendarViewMode_WithSupportedModes_ReturnsExpectedOffset(
        CurrentCalendarView mode,
        CurrentCalendarView expectedType,
        int expectedOffset)
    {
        // Act
        var (type, offset) = MainForm.GetNextPreviousOffsetBasedOnCalendarViewMode(mode);

        // Assert
        type.Should().Be(expectedType);
        offset.Should().Be(expectedOffset);
    }

    [Fact]
    public void GetNextPreviousOffsetBasedOnCalendarViewMode_WithUnsupportedMode_ThrowsArgumentOutOfRangeException()
    {
        // Act
        Action act = () => MainForm.GetNextPreviousOffsetBasedOnCalendarViewMode((CurrentCalendarView)999);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
