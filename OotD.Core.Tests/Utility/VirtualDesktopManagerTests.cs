using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class VirtualDesktopManagerTests
{
    [Fact]
    public void IsVirtualDesktopSupported_ShouldReturnBoolean()
    {
        // Act
        var action = () => VirtualDesktopManager.IsVirtualDesktopSupported;

        // Assert - should not throw
        action.Should().NotThrow();
    }

    [Fact]
    public void GetWindowDesktopId_WithInvalidHandle_ShouldNotThrow()
    {
        // Arrange
        var invalidHandle = IntPtr.Zero;

        // Act
        var action = () => VirtualDesktopManager.GetWindowDesktopId(invalidHandle);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void GetVirtualDesktops_ShouldReturnList()
    {
        // Act
        var desktops = VirtualDesktopManager.GetVirtualDesktops();

        // Assert
        desktops.Should().NotBeNull();
        desktops.Should().BeOfType<List<VirtualDesktopInfo>>();
    }

    [Fact]
    public void GetCurrentDesktopId_ShouldNotThrow()
    {
        // Act
        var action = () => VirtualDesktopManager.GetCurrentDesktopId();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void MoveWindowToDesktop_WithInvalidHandle_ShouldReturnFalse()
    {
        // Arrange
        var invalidHandle = IntPtr.Zero;
        var randomDesktopId = Guid.NewGuid();

        // Act
        var result = VirtualDesktopManager.MoveWindowToDesktop(invalidHandle, randomDesktopId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsWindowOnCurrentDesktop_WithInvalidHandle_ShouldNotThrow()
    {
        // Arrange
        var invalidHandle = IntPtr.Zero;

        // Act
        var action = () => VirtualDesktopManager.IsWindowOnCurrentDesktop(invalidHandle);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void VirtualDesktopInfo_ShouldHaveIdAndNameProperties()
    {
        // Arrange & Act
        var info = new VirtualDesktopInfo
        {
            Id = Guid.NewGuid(),
            Name = "Test Desktop"
        };

        // Assert
        info.Id.Should().NotBe(Guid.Empty);
        info.Name.Should().Be("Test Desktop");
    }

    [Fact]
    public void VirtualDesktopInfo_DefaultName_ShouldBeEmptyString()
    {
        // Arrange & Act
        var info = new VirtualDesktopInfo();

        // Assert
        info.Name.Should().Be(string.Empty);
    }
}
