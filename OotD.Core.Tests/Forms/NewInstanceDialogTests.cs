using System.Reflection;
using OotD.Enums;
using OotD.Events;
using OotD.Forms;
using OotD.Properties;

namespace OotD.Core.Tests.Forms;

public class NewInstanceDialogTests
{
    [Fact]
    public void Constructor_DefaultsToCalendarWithAnEmptyName()
    {
        using var dialog = new NewInstanceDialog((_, _) => { });

        dialog.Text.Should().Be(Resources.AddInstance);
        dialog.InstanceName.Should().BeEmpty();
        dialog.SelectedFolder.Should().Be(FolderViewType.Calendar);
    }

    [Fact]
    public void FolderChoices_ListEveryDefaultFolderInTrayMenuOrderThenSelectFolder()
    {
        using var dialog = new NewInstanceDialog((_, _) => { });
        var comboBox = GetPrivateField<ComboBox>(dialog, "_folderComboBox");

        var choices = new List<(FolderViewType?, string)>();
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            comboBox.SelectedIndex = index;
            choices.Add((dialog.SelectedFolder, comboBox.Text));
        }

        choices.Should().Equal(
            (FolderViewType.Calendar, Resources.Calendar),
            (FolderViewType.Contacts, Resources.Contacts),
            (FolderViewType.Inbox, Resources.Inbox),
            (FolderViewType.Notes, Resources.Notes),
            (FolderViewType.Tasks, Resources.Tasks),
            (FolderViewType.Todo, Resources.ResourceManager.GetString("TodoList")!),
            (null, Resources.SelectFolder));
    }

    [Fact]
    public void OkButton_WhenNameIsRejected_KeepsTheDialogOpenAndShowsTheError()
    {
        using var dialog = new NewInstanceDialog((_, e) =>
        {
            e.Cancel = true;
            e.Message = "Required";
        });

        ClickOk(dialog);

        dialog.DialogResult.Should().Be(DialogResult.None);
        GetPrivateField<ErrorProvider>(dialog, "_errorProvider")
            .GetError(GetPrivateField<TextBox>(dialog, "_nameTextBox")).Should().Be("Required");
    }

    [Fact]
    public void OkButton_WhenNameIsAccepted_ClosesWithTheNameAndFolder()
    {
        InputBoxValidatingEventArgs? validated = null;
        using var dialog = new NewInstanceDialog((_, e) => validated = e);
        GetPrivateField<TextBox>(dialog, "_nameTextBox").Text = "Tasks";
        GetPrivateField<ComboBox>(dialog, "_folderComboBox").SelectedIndex = 4;

        ClickOk(dialog);

        validated!.Text.Should().Be("Tasks");
        dialog.DialogResult.Should().Be(DialogResult.OK);
        dialog.InstanceName.Should().Be("Tasks");
        dialog.SelectedFolder.Should().Be(FolderViewType.Tasks);
    }

    [Fact]
    public void EditingTheName_ClearsAPreviousError()
    {
        using var dialog = new NewInstanceDialog((_, e) =>
        {
            e.Cancel = string.IsNullOrWhiteSpace(e.Text);
            e.Message = "Required";
        });
        var nameTextBox = GetPrivateField<TextBox>(dialog, "_nameTextBox");
        var errorProvider = GetPrivateField<ErrorProvider>(dialog, "_errorProvider");

        ClickOk(dialog);
        nameTextBox.Text = "Work";

        errorProvider.GetError(nameTextBox).Should().BeEmpty();
    }

    private static void ClickOk(NewInstanceDialog dialog)
    {
        var method = typeof(NewInstanceDialog).GetMethod("OkButton_Click",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.Should().NotBeNull();
        method!.Invoke(dialog, [dialog, EventArgs.Empty]);
    }

    private static T GetPrivateField<T>(NewInstanceDialog dialog, string fieldName) where T : class
    {
        var field = typeof(NewInstanceDialog).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();
        return (T)field!.GetValue(dialog)!;
    }
}
