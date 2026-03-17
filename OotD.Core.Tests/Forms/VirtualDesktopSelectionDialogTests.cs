using System.Reflection;
using OotD.Forms;

namespace OotD.Core.Tests.Forms;

public class VirtualDesktopSelectionDialogTests
{
    [Fact]
    public void Constructor_ShouldInitializeSelectionState()
    {
        using var dialog = new VirtualDesktopSelectionDialog();

        dialog.SelectedDesktopId.Should().BeNull();
        dialog.ClearAssignment.Should().BeFalse();
    }

    [Fact]
    public void ClearButton_Click_ShouldSetClearAssignmentAndResetSelection()
    {
        using var dialog = new VirtualDesktopSelectionDialog();

        InvokePrivateHandler(dialog, "ClearButton_Click");

        dialog.ClearAssignment.Should().BeTrue();
        dialog.SelectedDesktopId.Should().BeNull();
        dialog.DialogResult.Should().Be(DialogResult.OK);
    }

    [Fact]
    public void OkButton_Click_WithSelectedDesktop_ShouldSetSelectedDesktopId()
    {
        using var dialog = new VirtualDesktopSelectionDialog();
        var expectedDesktopId = Guid.NewGuid();

        var desktopListBox = GetDesktopListBox(dialog);
        desktopListBox.Items.Clear();
        desktopListBox.Items.Add(CreateDesktopListItem(expectedDesktopId, "Desktop Test"));
        desktopListBox.SelectedIndex = 0;

        InvokePrivateHandler(dialog, "OkButton_Click");

        dialog.ClearAssignment.Should().BeFalse();
        dialog.SelectedDesktopId.Should().Be(expectedDesktopId);
    }

    [Fact]
    public void DesktopListBox_DoubleClick_WithSelectedDesktop_ShouldSetDialogResultOk()
    {
        using var dialog = new VirtualDesktopSelectionDialog();
        var expectedDesktopId = Guid.NewGuid();

        var desktopListBox = GetDesktopListBox(dialog);
        desktopListBox.Items.Clear();
        desktopListBox.Items.Add(CreateDesktopListItem(expectedDesktopId, "Desktop Test"));
        desktopListBox.SelectedIndex = 0;

        InvokePrivateHandler(dialog, "DesktopListBox_DoubleClick");

        dialog.DialogResult.Should().Be(DialogResult.OK);
        dialog.SelectedDesktopId.Should().Be(expectedDesktopId);
        dialog.ClearAssignment.Should().BeFalse();
    }

    private static ListBox GetDesktopListBox(VirtualDesktopSelectionDialog dialog)
    {
        var field = typeof(VirtualDesktopSelectionDialog).GetField("_desktopListBox",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();
        return (ListBox)field!.GetValue(dialog)!;
    }

    private static object CreateDesktopListItem(Guid id, string name)
    {
        var nestedType = typeof(VirtualDesktopSelectionDialog).GetNestedType("DesktopListItem",
            BindingFlags.NonPublic);
        nestedType.Should().NotBeNull();

        var instance = Activator.CreateInstance(
            nestedType!,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [id, name],
            culture: null);

        instance.Should().NotBeNull();
        return instance!;
    }

    private static void InvokePrivateHandler(VirtualDesktopSelectionDialog dialog, string methodName)
    {
        var method = typeof(VirtualDesktopSelectionDialog).GetMethod(methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.Should().NotBeNull();
        method!.Invoke(dialog, [null, EventArgs.Empty]);
    }
}
