using OotD.Enums;

namespace OotD.Core.Tests.Enums;

public class EnumTests
{
    // These values must match the <mode> element in Outlook's calendar view XML.
    [Theory]
    [InlineData(CurrentCalendarView.Day, 0)]
    [InlineData(CurrentCalendarView.Week, 1)]
    [InlineData(CurrentCalendarView.Month, 2)]
    [InlineData(CurrentCalendarView.WorkWeek, 4)]
    public void CurrentCalendarView_ShouldHaveCorrectIntegerValues(CurrentCalendarView view, int expectedValue)
    {
        ((int)view).Should().Be(expectedValue);
    }
}
