using OotD.Forms;
using OotD.Utility;

namespace OotD.Core.Tests.Forms;

public class VirtualDesktopSelectionDialogTests
{
    [Fact]
    public void Constructor_ShouldCreateDialog()
    {
        // Act
        var action = () => new VirtualDesktopSelectionDialog();

        // Assert - should not throw
        action.Should().NotThrow();
    }

    [Fact]
    public void SelectedDesktopId_WhenNotSelected_ShouldBeNull()
    {
        // Arrange
        using var dialog = new VirtualDesktopSelectionDialog();

        // Act
        var selectedId = dialog.SelectedDesktopId;

        // Assert
        selectedId.Should().BeNull();
    }

    [Fact]
    public void Dialog_ShouldHaveCorrectTitle()
    {
        // Arrange
        using var dialog = new VirtualDesktopSelectionDialog();

        // Act & Assert
        dialog.Text.Should().Be("Select Virtual Desktop");
    }
}
