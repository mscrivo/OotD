namespace OotD.Core.Tests.Forms;

using OotD.Enums;
using OotD.Forms;

public class MainFormViewXmlPolicyTests
{
    [Theory]
    [InlineData(FolderViewType.Calendar, false)] // the Calendar keeps its custom month/day ViewXML
    [InlineData(FolderViewType.Inbox, true)]
    [InlineData(FolderViewType.Contacts, true)]
    [InlineData(FolderViewType.Notes, true)]
    [InlineData(FolderViewType.Tasks, true)]
    [InlineData(FolderViewType.Todo, true)]
    public void ShouldClearViewXmlForFolderType_ClearsForEveryNonCalendarFolder(
        FolderViewType folderViewType,
        bool expected)
    {
        MainFormViewXmlPolicy.ShouldClearViewXmlForFolderType(folderViewType).Should().Be(expected);
    }


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
        var result = MainFormViewXmlPolicy.ShouldPersistViewXmlForFolder(folderName, calendarFolderName);

        // Assert
        result.Should().Be(expected);
    }
}
