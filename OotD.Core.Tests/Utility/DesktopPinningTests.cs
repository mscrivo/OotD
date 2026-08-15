using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class DesktopPinningTests
{
    [Fact]
    public void Initialize_ShouldNotThrow()
    {
        // Act & Assert - creating the hidden helper window must never throw.
        var action = () => DesktopPinning.Initialize();
        action.Should().NotThrow();
    }

    [Fact]
    public void GetPinnedAnchorWindow_ShouldReturnIntPtr()
    {
        // Arrange
        DesktopPinning.Initialize();

        // Act & Assert - returns the helper handle or IntPtr.Zero (fall back to HWND_BOTTOM).
        var action = () => DesktopPinning.GetPinnedAnchorWindow();
        action.Should().NotThrow();
    }
}
