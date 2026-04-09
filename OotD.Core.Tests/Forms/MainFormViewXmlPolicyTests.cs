namespace OotD.Core.Tests.Forms;

using OotD.Forms;

public class MainFormViewXmlPolicyTests
{
    [Theory]
    [InlineData("Calendar", "Calendar", true)]
    [InlineData("Tasks", "Calendar", false)]
    [InlineData("To-Do List", "Calendar", false)]
    [InlineData("Notes", "Calendar", false)]
    [InlineData("Inbox", "Calendar", false)]
    [InlineData("Custom Folder", "Calendar", false)]
    [InlineData("calendar", "Calendar", false)]
    [InlineData("Calendar", "calendar", false)]
    [InlineData(null, "Calendar", false)]
    [InlineData("Calendar", null, false)]
    [InlineData("", "Calendar", false)]
    [InlineData("Calendar", "", false)]
    public void ShouldPersistViewXmlForFolder_WithVariousFolderNames_ReturnsExpectedResult(
        string? folderName,
        string? calendarFolderName,
        bool expected)
    {
        // Act
        var result = MainForm.ShouldPersistViewXmlForFolder(folderName, calendarFolderName);

        // Assert
        result.Should().Be(expected);
    }
}
