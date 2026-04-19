namespace OotD.Core.Tests.Forms;

using OotD.Forms;

public class MainFormFolderPathTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("Calendar", "Calendar")]
    [InlineData("\\Mailbox - User\\Calendar", "Calendar")]
    [InlineData("\\Mailbox - User\\Projects\\Sprint", "Sprint")]
    public void GetFolderNameFromFullPath_WithVariousInputs_ReturnsExpectedFolderName(string? fullPath, string expected)
    {
        // Act
        var result = MainForm.GetFolderNameFromFullPath(fullPath);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\\\\Personal Folders\\Calendar", "Calendar")]
    [InlineData("\\\\Personal Folders\\Inbox", "Inbox")]
    [InlineData("\\\\Archive\\Calendar", "\\\\Archive\\Calendar")]
    [InlineData("Calendar", "Calendar")]
    public void GetFolderPath_WithVariousInputs_ReturnsExpectedPath(string folderPath, string expected)
    {
        // Act
        var result = MainForm.GetFolderPath(folderPath);

        // Assert
        result.Should().Be(expected);
    }
}
