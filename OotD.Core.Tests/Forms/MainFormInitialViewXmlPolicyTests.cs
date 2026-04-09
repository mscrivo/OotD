namespace OotD.Core.Tests.Forms;

using OotD.Forms;

public class MainFormInitialViewXmlPolicyTests
{
    [Theory]
    [InlineData("Calendar", "Calendar", "<month/>", "<month/>")]
    [InlineData("Tasks", "Calendar", "<month/>", "")]
    [InlineData("To-Do List", "Calendar", "<month/>", "")]
    [InlineData("Notes", "Calendar", "<month/>", "")]
    [InlineData("Inbox", "Calendar", "<month/>", "")]
    [InlineData("Custom Folder", "Calendar", "<month/>", "")]
    [InlineData("calendar", "Calendar", "<month/>", "")]
    [InlineData(null, "Calendar", "<month/>", "")]
    [InlineData("Calendar", null, "<month/>", "")]
    public void GetDefaultViewXmlForFolder_WithVariousFolderNames_ReturnsExpectedDefault(
        string? folderName,
        string? calendarFolderName,
        string monthXml,
        string expected)
    {
        // Act
        var result = MainForm.GetDefaultViewXmlForFolder(folderName, calendarFolderName, monthXml);

        // Assert
        result.Should().Be(expected);
    }
}
