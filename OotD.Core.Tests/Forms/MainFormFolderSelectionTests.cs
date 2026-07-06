namespace OotD.Core.Tests.Forms;

using OotD.Enums;
using OotD.Forms;

public class MainFormFolderSelectionTests
{
    // Typical set of default-folder display names as Outlook would report them.
    private static Dictionary<FolderViewType, string?> DefaultNames() => new()
    {
        [FolderViewType.Calendar] = "Calendar",
        [FolderViewType.Contacts] = "Contacts",
        [FolderViewType.Inbox] = "Inbox",
        [FolderViewType.Notes] = "Notes",
        [FolderViewType.Tasks] = "Tasks",
        [FolderViewType.Todo] = "To-Do List"
    };

    [Theory]
    [InlineData("Calendar", FolderViewType.Calendar)]
    [InlineData("Contacts", FolderViewType.Contacts)]
    [InlineData("Inbox", FolderViewType.Inbox)]
    [InlineData("Notes", FolderViewType.Notes)]
    [InlineData("Tasks", FolderViewType.Tasks)]
    [InlineData("To-Do List", FolderViewType.Todo)]
    public void MatchFolderViewTypeByName_WhenNameMatchesADefaultFolder_ReturnsThatType(
        string folderName, FolderViewType expected)
    {
        MainFormFolderPolicy.MatchFolderViewTypeByName(folderName, DefaultNames()).Should().Be(expected);
    }

    [Fact]
    public void MatchFolderViewTypeByName_WhenNameMatchesNoDefaultFolder_ReturnsNull()
    {
        // A custom folder the user picked.
        MainFormFolderPolicy.MatchFolderViewTypeByName("Project X", DefaultNames()).Should().BeNull();
    }

    [Fact]
    public void MatchFolderViewTypeByName_IsCaseSensitive()
    {
        // Ordinal comparison: casing must match exactly.
        MainFormFolderPolicy.MatchFolderViewTypeByName("inbox", DefaultNames()).Should().BeNull();
    }

    [Fact]
    public void MatchFolderViewTypeByName_WhenFolderNameIsNull_ReturnsNull()
    {
        MainFormFolderPolicy.MatchFolderViewTypeByName(null, DefaultNames()).Should().BeNull();
    }

    [Fact]
    public void MatchFolderViewTypeByName_WhenTwoFoldersShareAName_PrefersTheEarlierType()
    {
        // Calendar precedes Contacts, so it wins the tie.
        var names = new Dictionary<FolderViewType, string?>
        {
            [FolderViewType.Calendar] = "Shared",
            [FolderViewType.Contacts] = "Shared"
        };

        MainFormFolderPolicy.MatchFolderViewTypeByName("Shared", names).Should().Be(FolderViewType.Calendar);
    }

    [Fact]
    public void MatchFolderViewTypeByName_ToleratesAPartialNameMap()
    {
        // Outlook might not resolve every default folder; only the present ones can match.
        var names = new Dictionary<FolderViewType, string?>
        {
            [FolderViewType.Inbox] = "Inbox"
        };

        MainFormFolderPolicy.MatchFolderViewTypeByName("Inbox", names).Should().Be(FolderViewType.Inbox);
        MainFormFolderPolicy.MatchFolderViewTypeByName("Calendar", names).Should().BeNull();
    }
}
