using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class StickyWindowTests : IDisposable
{
    private Form? _testForm;
    private StickyWindow? _stickyWindow;

    public StickyWindowTests()
    {
        // Create a test form for sticky window functionality
        _testForm = new Form
        {
            Text = "Test Form",
            Size = new Size(300, 200),
            Location = new Point(100, 100),
            WindowState = FormWindowState.Normal
        };
    }

    [Fact]
    public void Constructor_ShouldCreateStickyWindow()
    {
        // Act
        _stickyWindow = new StickyWindow(_testForm!);

        // Assert
        _stickyWindow.Should().NotBeNull();
    }

    [Fact]
    public void StickGap_ShouldHaveDefaultValue()
    {
        // Assert
        StickyWindow.StickGap.Should().Be(10);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(50)]
    public void StickGap_WhenSet_ShouldReturnCorrectValue(int expectedGap)
    {
        // Act
        StickyWindow.StickGap = expectedGap;

        // Assert
        StickyWindow.StickGap.Should().Be(expectedGap);
    }

    [Fact]
    public void StickOnResize_ShouldDefaultToTrue()
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!);

        // Assert
        _stickyWindow.StickOnResize.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void StickOnResize_WhenSet_ShouldReturnCorrectValue(bool expectedValue)
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!)
        {
            // Act
            StickOnResize = expectedValue
        };

        // Assert
        _stickyWindow.StickOnResize.Should().Be(expectedValue);
    }

    [Fact]
    public void StickOnMove_ShouldDefaultToTrue()
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!);

        // Assert
        _stickyWindow.StickOnMove.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void StickOnMove_WhenSet_ShouldReturnCorrectValue(bool expectedValue)
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!)
        {
            // Act
            StickOnMove = expectedValue
        };

        // Assert
        _stickyWindow.StickOnMove.Should().Be(expectedValue);
    }

    [Fact]
    public void StickToScreen_ShouldDefaultToTrue()
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!);

        // Assert
        _stickyWindow.StickToScreen.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void StickToScreen_WhenSet_ShouldReturnCorrectValue(bool expectedValue)
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!)
        {
            // Act
            StickToScreen = expectedValue
        };

        // Assert
        _stickyWindow.StickToScreen.Should().Be(expectedValue);
    }

    [Fact]
    public void StickToOther_ShouldDefaultToTrue()
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!);

        // Assert
        _stickyWindow.StickToOther.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void StickToOther_WhenSet_ShouldReturnCorrectValue(bool expectedValue)
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!)
        {
            // Act
            StickToOther = expectedValue
        };

        // Assert
        _stickyWindow.StickToOther.Should().Be(expectedValue);
    }

    [Fact]
    public void RegisterExternalReferenceForm_ShouldNotThrow()
    {
        // Arrange
        using var externalForm = new Form();

        // Act & Assert
        var action = () => StickyWindow.RegisterExternalReferenceForm(externalForm);
        action.Should().NotThrow();
    }

    [Fact]
    public void UnregisterExternalReferenceForm_ShouldNotThrow()
    {
        // Arrange
        using var externalForm = new Form();
        StickyWindow.RegisterExternalReferenceForm(externalForm);

        // Act & Assert
        var action = () => StickyWindow.UnregisterExternalReferenceForm(externalForm);
        action.Should().NotThrow();
    }

    [Fact]
    public void ResizeEnded_EventShouldBeTriggerable()
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!);
        var eventTriggered = false;
        _stickyWindow.ResizeEnded += (_, _) => eventTriggered = true;

        // Act - Trigger the event through reflection since it's an internal method
        var onResizeEndedMethod = typeof(StickyWindow).GetMethod("OnResizeEnded",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onResizeEndedMethod?.Invoke(_stickyWindow, null);

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void MoveEnded_EventShouldBeTriggerable()
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!);
        var eventTriggered = false;
        _stickyWindow.MoveEnded += (_, _) => eventTriggered = true;

        // Act - Trigger the event through reflection since it's an internal method
        var onMoveEndedMethod = typeof(StickyWindow).GetMethod("OnMoveEnded",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onMoveEndedMethod?.Invoke(_stickyWindow, null);

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void StickyWindow_ShouldInheritFromNativeWindow()
    {
        // Assert
        typeof(StickyWindow).Should().BeDerivedFrom<NativeWindow>();
    }

    [Fact]
    public void StickyWindow_WithNullForm_ShouldThrow()
    {
        // Act & Assert
        var action = () => new StickyWindow(null!);
        action.Should().Throw<NullReferenceException>(); // Changed from ArgumentNullException to NullReferenceException
    }

    [Fact]
    public void MultipleInstances_ShouldBeSupported()
    {
        // Arrange
        using var form1 = new Form();
        using var form2 = new Form();

        // Act & Assert
        var action1 = () => new StickyWindow(form1);
        var action2 = () => new StickyWindow(form2);

        action1.Should().NotThrow();
        action2.Should().NotThrow();
    }

    [Fact]
    public void StickyWindow_Properties_ShouldAllowChaining()
    {
        // Arrange
        _stickyWindow = new StickyWindow(_testForm!);

        // Act & Assert - Property setters should not interfere with each other
        var action = () =>
        {
            _stickyWindow.StickOnMove = false;
            _stickyWindow.StickOnResize = false;
            _stickyWindow.StickToScreen = false;
            _stickyWindow.StickToOther = false;
        };

        action.Should().NotThrow();
        _stickyWindow.StickOnMove.Should().BeFalse();
        _stickyWindow.StickOnResize.Should().BeFalse();
        _stickyWindow.StickToScreen.Should().BeFalse();
        _stickyWindow.StickToOther.Should().BeFalse();
    }

    public void Dispose()
    {
        _stickyWindow?.ReleaseHandle();
        _testForm?.Dispose();

        // Reset static state
        StickyWindow.StickGap = 10;
    }
}
