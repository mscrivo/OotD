using System.Drawing;
using OotD.Forms;

namespace OotD.Core.Tests.Forms;

public class MainFormScreenBoundsTests
{
    [Fact]
    public void IsWindowOnAnyScreen_WhenFullyOnScreen_ReturnsTrue()
    {
        var window = new Rectangle(100, 100, 300, 200);
        var screens = new[] { new Rectangle(0, 0, 1920, 1080) };

        MainForm.IsWindowOnAnyScreen(window, screens).Should().BeTrue();
    }

    [Fact]
    public void IsWindowOnAnyScreen_WhenPartiallyOnScreen_ReturnsTrue()
    {
        var window = new Rectangle(-100, -50, 300, 200);
        var screens = new[] { new Rectangle(0, 0, 1920, 1080) };

        MainForm.IsWindowOnAnyScreen(window, screens).Should().BeTrue();
    }

    [Fact]
    public void IsWindowOnAnyScreen_WhenCompletelyOffScreen_ReturnsFalse()
    {
        var window = new Rectangle(3000, 2000, 300, 200);
        var screens = new[] { new Rectangle(0, 0, 1920, 1080) };

        MainForm.IsWindowOnAnyScreen(window, screens).Should().BeFalse();
    }

    [Fact]
    public void IsWindowOnAnyScreen_WhenOnSecondMonitor_ReturnsTrue()
    {
        var window = new Rectangle(2000, 100, 300, 200);
        var screens = new[]
        {
            new Rectangle(0, 0, 1920, 1080),
            new Rectangle(1920, 0, 1920, 1080)
        };

        MainForm.IsWindowOnAnyScreen(window, screens).Should().BeTrue();
    }

    [Fact]
    public void IsWindowOnAnyScreen_WhenSecondMonitorDisconnected_ReturnsFalse()
    {
        var window = new Rectangle(2000, 100, 300, 200);
        var screens = new[] { new Rectangle(0, 0, 1920, 1080) };

        MainForm.IsWindowOnAnyScreen(window, screens).Should().BeFalse();
    }

    [Fact]
    public void IsWindowOnAnyScreen_WithNoScreens_ReturnsFalse()
    {
        var window = new Rectangle(100, 100, 300, 200);

        MainForm.IsWindowOnAnyScreen(window, []).Should().BeFalse();
    }

    [Theory]
    [InlineData(5000, 500, 300, 200)]
    [InlineData(-500, -500, 300, 200)]
    [InlineData(1920, 0, 300, 200)]
    public void IsWindowOnAnyScreen_WhenSavedPositionBeyondSingleScreen_ReturnsFalse(
        int left, int top, int width, int height)
    {
        var window = new Rectangle(left, top, width, height);
        var screens = new[] { new Rectangle(0, 0, 1920, 1080) };

        MainForm.IsWindowOnAnyScreen(window, screens).Should().BeFalse();
    }
}
