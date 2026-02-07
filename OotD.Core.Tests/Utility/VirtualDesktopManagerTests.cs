using OotD.Utility;

namespace OotD.Core.Tests.Utility;

/// <summary>
///     Tests for VirtualDesktopInfo class.
///     Note: VirtualDesktopManager tests that require COM interop are skipped in CI
///     because the virtual desktop APIs require a Windows desktop session.
/// </summary>
public class VirtualDesktopManagerTests
{
    /// <summary>
    ///     Checks if we're running in a CI environment or headless Windows Server.
    /// </summary>
    private static bool IsRunningInCi =>
        Environment.GetEnvironmentVariable("CI") != null ||
        Environment.GetEnvironmentVariable("GITHUB_ACTIONS") != null ||
        Environment.GetEnvironmentVariable("TF_BUILD") != null;

    private static void SkipIfCi()
    {
        if (IsRunningInCi)
        {
            Assert.Skip("Virtual Desktop COM APIs not available in CI environment");
        }
    }

    [Fact]
    public void IsVirtualDesktopSupported_ShouldReturnBoolean()
    {
        SkipIfCi();

        // Act
        var action = () => VirtualDesktopManager.IsVirtualDesktopSupported;

        // Assert - should not throw
        action.Should().NotThrow();
    }

    [Fact]
    public void GetWindowDesktopId_WithInvalidHandle_ShouldNotThrow()
    {
        SkipIfCi();

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
        SkipIfCi();

        // Act
        var desktops = VirtualDesktopManager.GetVirtualDesktops();

        // Assert
        desktops.Should().NotBeNull();
        desktops.Should().BeOfType<List<VirtualDesktopInfo>>();
    }

    [Fact]
    public void GetCurrentDesktopId_ShouldNotThrow()
    {
        SkipIfCi();

        // Act
        var action = () => VirtualDesktopManager.GetCurrentDesktopId();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void MoveWindowToDesktop_WithInvalidHandle_ShouldReturnFalse()
    {
        SkipIfCi();

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
        SkipIfCi();

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
