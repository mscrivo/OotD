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

    [SkippableFact]
    public void IsVirtualDesktopSupported_ShouldReturnBoolean()
    {
        Skip.If(IsRunningInCi, "Virtual Desktop COM APIs not available in CI environment");

        // Act
        var action = () => VirtualDesktopManager.IsVirtualDesktopSupported;

        // Assert - should not throw
        action.Should().NotThrow();
    }

    [SkippableFact]
    public void GetWindowDesktopId_WithInvalidHandle_ShouldNotThrow()
    {
        Skip.If(IsRunningInCi, "Virtual Desktop COM APIs not available in CI environment");

        // Arrange
        var invalidHandle = IntPtr.Zero;

        // Act
        var action = () => VirtualDesktopManager.GetWindowDesktopId(invalidHandle);

        // Assert
        action.Should().NotThrow();
    }

    [SkippableFact]
    public void GetVirtualDesktops_ShouldReturnList()
    {
        Skip.If(IsRunningInCi, "Virtual Desktop COM APIs not available in CI environment");

        // Act
        var desktops = VirtualDesktopManager.GetVirtualDesktops();

        // Assert
        desktops.Should().NotBeNull();
        desktops.Should().BeOfType<List<VirtualDesktopInfo>>();
    }

    [SkippableFact]
    public void GetCurrentDesktopId_ShouldNotThrow()
    {
        Skip.If(IsRunningInCi, "Virtual Desktop COM APIs not available in CI environment");

        // Act
        var action = () => VirtualDesktopManager.GetCurrentDesktopId();

        // Assert
        action.Should().NotThrow();
    }

    [SkippableFact]
    public void MoveWindowToDesktop_WithInvalidHandle_ShouldReturnFalse()
    {
        Skip.If(IsRunningInCi, "Virtual Desktop COM APIs not available in CI environment");

        // Arrange
        var invalidHandle = IntPtr.Zero;
        var randomDesktopId = Guid.NewGuid();

        // Act
        var result = VirtualDesktopManager.MoveWindowToDesktop(invalidHandle, randomDesktopId);

        // Assert
        result.Should().BeFalse();
    }

    [SkippableFact]
    public void IsWindowOnCurrentDesktop_WithInvalidHandle_ShouldNotThrow()
    {
        Skip.If(IsRunningInCi, "Virtual Desktop COM APIs not available in CI environment");

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
