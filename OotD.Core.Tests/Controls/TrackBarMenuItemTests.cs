using System.Reflection;
using OotD.Controls;

namespace OotD.Core.Tests.Controls;

public class TrackBarMenuItemTests : IDisposable
{
    private readonly TrackBarMenuItem _menuItem = new();

    [Fact]
    public void Constructor_ShouldHostMacTrackBar()
    {
        // Assert
        _menuItem.TrackBar.Should().NotBeNull();
        _menuItem.Control.Should().BeSameAs(_menuItem.TrackBar);
    }

    [Fact]
    public void LocationMinimumMaximumAndValue_ShouldForwardToHostedTrackBar()
    {
        // Arrange
        var location = new Point(12, 34);

        // Act
        _menuItem.Location = location;
        _menuItem.Minimum = 10;
        _menuItem.Maximum = 90;
        _menuItem.Value = 40;

        // Assert
        _menuItem.TrackBar.Location.Should().Be(location);
        _menuItem.TrackBar.Minimum.Should().Be(10);
        _menuItem.TrackBar.Maximum.Should().Be(90);
        _menuItem.TrackBar.Value.Should().Be(40);
        _menuItem.Value.Should().Be(40);
    }

    [Fact]
    public void TrackBarScroll_WithSubscriber_ShouldRaiseValueChangedFromMenuItem()
    {
        // Arrange
        object? senderSeen = null;
        var raised = false;
        _menuItem.ValueChanged += (sender, args) =>
        {
            raised = true;
            senderSeen = sender;
            args.Should().BeSameAs(EventArgs.Empty);
        };

        // Act
        InvokeTrackBarScroll(_menuItem);

        // Assert
        raised.Should().BeTrue();
        senderSeen.Should().BeSameAs(_menuItem);
    }

    [Fact]
    public void TrackBarScroll_WithoutSubscriber_ShouldNotThrow()
    {
        // Act
        var action = () => InvokeTrackBarScroll(_menuItem);

        // Assert
        action.Should().NotThrow();
    }

    public void Dispose()
    {
        _menuItem.Dispose();
        GC.SuppressFinalize(this);
    }

    private static void InvokeTrackBarScroll(TrackBarMenuItem menuItem)
    {
        var method = typeof(TrackBarMenuItem).GetMethod("TrackBar_Scroll",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.Should().NotBeNull();
        method!.Invoke(menuItem, [menuItem.TrackBar, EventArgs.Empty]);
    }
}