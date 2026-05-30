namespace OotD.Core.Tests.Forms;

using OotD.Enums;
using OotD.Forms;

public class MainFormCalendarNavigationTests
{
    [Theory]
    [InlineData("<view><mode>0</mode></view>", CurrentCalendarView.Day)]
    [InlineData("<view><mode>1</mode></view>", CurrentCalendarView.Week)]
    [InlineData("<view><mode>2</mode></view>", CurrentCalendarView.Month)]
    [InlineData("<view><mode>4</mode></view>", CurrentCalendarView.WorkWeek)]
    public void GetCalendarViewModeFromViewXml_WithSupportedModes_ReturnsExpectedMode(
        string viewXml,
        CurrentCalendarView expectedMode)
    {
        // Act
        var result = MainForm.GetCalendarViewModeFromViewXml(viewXml);

        // Assert
        result.Should().Be(expectedMode);
    }

    [Fact]
    public void GetCalendarViewModeFromViewXml_WhenModeElementMissing_ReturnsDay()
    {
        // Act
        var result = MainForm.GetCalendarViewModeFromViewXml("<view />");

        // Assert
        result.Should().Be(CurrentCalendarView.Day);
    }

    [Fact]
    public void GetCalendarViewModeFromViewXml_WithUnsupportedNumericMode_ReturnsUnsupportedMode()
    {
        // Act
        var result = MainForm.GetCalendarViewModeFromViewXml("<view><mode>999</mode></view>");

        // Assert
        result.Should().Be((CurrentCalendarView)999);
    }

    [Fact]
    public void GetCalendarViewModeFromViewXml_WithMalformedXml_ThrowsXmlException()
    {
        // Act
        var action = () => MainForm.GetCalendarViewModeFromViewXml("<view><mode>1</view>");

        // Assert
        action.Should().Throw<System.Xml.XmlException>();
    }

    [Fact]
    public void GetCalendarViewModeFromViewXml_WithNonNumericMode_ThrowsFormatException()
    {
        // Act
        var action = () => MainForm.GetCalendarViewModeFromViewXml("<view><mode>week</mode></view>");

        // Assert
        action.Should().Throw<FormatException>();
    }

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

    [Theory]
    [InlineData("2026-05-15", CurrentCalendarView.Day, 1, "2026-05-16")]
    [InlineData("2026-05-15", CurrentCalendarView.Day, -1, "2026-05-14")]
    [InlineData("2026-05-15", CurrentCalendarView.Week, 7, "2026-05-22")]
    [InlineData("2026-05-15", CurrentCalendarView.WorkWeek, -7, "2026-05-08")]
    [InlineData("2026-01-31", CurrentCalendarView.Month, 1, "2026-02-28")]
    [InlineData("2026-03-31", CurrentCalendarView.Month, -1, "2026-02-28")]
    public void GetCalendarNavigationTargetDate_WithModeAndOffset_ReturnsExpectedDate(
        string selectedDate,
        CurrentCalendarView mode,
        int offset,
        string expectedDate)
    {
        // Act
        var result = MainForm.GetCalendarNavigationTargetDate(DateTime.Parse(selectedDate), mode, offset);

        // Assert
        result.Should().Be(DateTime.Parse(expectedDate));
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
