namespace OotD.Core.Tests.Forms;

using OotD.Forms;

public class MainFormToolbarButtonTests
{
    [Theory]
    [InlineData("IPM.Appointment", true, false)]
    [InlineData("IPM.Note", false, true)]
    [InlineData("IPM.Contact", false, false)]
    [InlineData("IPM.Task", false, false)]
    [InlineData("", false, false)]
    [InlineData(null, false, false)]
    public void GetToolbarButtonVisibilityFor_WithMessageClass_ReturnsExpectedVisibility(
        string? messageClass,
        bool expectedCalendarNavigationVisible,
        bool expectedNewEmailButtonVisible)
    {
        // Act
        var result = MainForm.GetToolbarButtonVisibilityFor(messageClass);

        // Assert
        result.CalendarNavigationVisible.Should().Be(expectedCalendarNavigationVisible);
        result.NewEmailButtonVisible.Should().Be(expectedNewEmailButtonVisible);
    }
}