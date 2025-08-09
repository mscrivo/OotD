using Microsoft.Win32;
using OotD.Preferences;

namespace OotD.Core.Tests.Preferences;

public class InstancePreferencesTests : IDisposable
{
    private readonly string _baseTestPath = $@"Software\OotDTests";
    private InstancePreferences? _preferences;

    [Fact]
    public void Constructor_ShouldCreateWithInstanceName()
    {
        // Act
        _preferences = new InstancePreferences("TestInstance");

        // Assert
        _preferences.Should().NotBeNull();
    }

    [Fact]
    public void Opacity_WhenNotSet_ShouldReturnDefaultValue()
    {
        // Arrange
        var uniqueInstanceName = $"OpacityTest_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act
        var opacity = _preferences.Opacity;

        // Assert
        opacity.Should().Be(InstancePreferences.DefaultOpacity);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(0.9)]
    [InlineData(1.0)]
    public void Opacity_WhenSet_ShouldReturnCorrectValue(double expectedOpacity)
    {
        // Arrange
        var uniqueInstanceName = $"OpacitySet_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName)
        {
            // Act
            Opacity = expectedOpacity
        };
        var actualOpacity = _preferences.Opacity;

        // Assert
        actualOpacity.Should().Be(expectedOpacity);
    }

    [Fact]
    public void Left_WhenNotSet_ShouldReturnDefaultValue()
    {
        // Arrange
        var uniqueInstanceName = $"LeftTest_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act
        var left = _preferences.Left;

        // Assert
        left.Should().Be(InstancePreferences.DefaultLeftPosition);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1920)]
    public void Left_WhenSet_ShouldReturnCorrectValue(int expectedLeft)
    {
        // Arrange
        var uniqueInstanceName = $"LeftSet_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName)
        {
            // Act
            Left = expectedLeft
        };
        var actualLeft = _preferences.Left;

        // Assert
        actualLeft.Should().Be(expectedLeft);
    }

    [Fact]
    public void Top_WhenNotSet_ShouldReturnDefaultValue()
    {
        // Arrange
        var uniqueInstanceName = $"TopTest_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act
        var top = _preferences.Top;

        // Assert
        top.Should().Be(InstancePreferences.DefaultTopPosition);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1080)]
    public void Top_WhenSet_ShouldReturnCorrectValue(int expectedTop)
    {
        // Arrange
        var uniqueInstanceName = $"TopSet_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName)
        {
            // Act
            Top = expectedTop
        };
        var actualTop = _preferences.Top;

        // Assert
        actualTop.Should().Be(expectedTop);
    }

    [Fact]
    public void Width_WhenNotSet_ShouldReturnDefaultValue()
    {
        // Arrange
        var uniqueInstanceName = $"WidthTest_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act
        var width = _preferences.Width;

        // Assert
        width.Should().Be(InstancePreferences.DefaultWidth);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(800)]
    [InlineData(1920)]
    public void Width_WhenSet_ShouldReturnCorrectValue(int expectedWidth)
    {
        // Arrange
        var uniqueInstanceName = $"WidthSet_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName)
        {
            // Act
            Width = expectedWidth
        };
        var actualWidth = _preferences.Width;

        // Assert
        actualWidth.Should().Be(expectedWidth);
    }

    [Fact]
    public void Height_WhenNotSet_ShouldReturnDefaultValue()
    {
        // Arrange
        var uniqueInstanceName = $"HeightTest_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act
        var height = _preferences.Height;

        // Assert
        height.Should().Be(InstancePreferences.DefaultHeight);
    }

    [Theory]
    [InlineData(300)]
    [InlineData(600)]
    [InlineData(1080)]
    public void Height_WhenSet_ShouldReturnCorrectValue(int expectedHeight)
    {
        // Arrange
        var uniqueInstanceName = $"HeightSet_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName)
        {
            // Act
            Height = expectedHeight
        };
        var actualHeight = _preferences.Height;

        // Assert
        actualHeight.Should().Be(expectedHeight);
    }

    [Fact]
    public void OutlookFolderName_WhenNotSet_ShouldReturnDefaultValue()
    {
        // Arrange
        var uniqueInstanceName = $"FolderNameTest_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act
        var folderName = _preferences.OutlookFolderName;

        // Assert
        folderName.Should().Be("Calendar");
    }

    [Theory]
    [InlineData("Calendar")]
    [InlineData("Contacts")]
    [InlineData("Tasks")]
    [InlineData("CustomFolder")]
    public void OutlookFolderName_WhenSet_ShouldReturnCorrectValue(string expectedFolderName)
    {
        // Arrange
        var uniqueInstanceName = $"FolderNameSet_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName)
        {
            // Act
            OutlookFolderName = expectedFolderName
        };
        var actualFolderName = _preferences.OutlookFolderName;

        // Assert
        actualFolderName.Should().Be(expectedFolderName);
    }

    [Fact]
    public void OutlookFolderView_WhenNotSet_ShouldReturnDefaultValue()
    {
        // Arrange
        var uniqueInstanceName = $"FolderViewTest_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act
        var folderView = _preferences.OutlookFolderView;

        // Assert
        folderView.Should().Be("Day/Week/Month");
    }

    [Theory]
    [InlineData("Day")]
    [InlineData("Week")]
    [InlineData("Month")]
    [InlineData("WorkWeek")]
    public void OutlookFolderView_WhenSet_ShouldReturnCorrectValue(string expectedView)
    {
        // Arrange
        var uniqueInstanceName = $"FolderViewSet_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName)
        {
            // Act
            OutlookFolderView = expectedView
        };
        var actualView = _preferences.OutlookFolderView;

        // Assert
        actualView.Should().Be(expectedView);
    }

    [Fact]
    public void DisableEditing_WhenNotSet_ShouldReturnFalse()
    {
        // Arrange
        var uniqueInstanceName = $"DisableEditingTest_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act
        var disableEditing = _preferences.DisableEditing;

        // Assert
        disableEditing.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DisableEditing_WhenSet_ShouldReturnCorrectValue(bool expectedValue)
    {
        // Arrange
        var uniqueInstanceName = $"DisableEditingSet_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName)
        {
            // Act
            DisableEditing = expectedValue
        };
        var actualValue = _preferences.DisableEditing;

        // Assert
        actualValue.Should().Be(expectedValue);
    }

    [Fact]
    public void ViewXml_WhenNotSet_ShouldReturnEmptyString()
    {
        // Arrange
        var uniqueInstanceName = $"ViewXmlTest_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act
        var viewXml = _preferences.ViewXml;

        // Assert
        viewXml.Should().Be("");
    }

    [Theory]
    [InlineData("")]
    [InlineData("<xml>test</xml>")]
    [InlineData("<?xml version=\"1.0\"?><view><mode>1</mode></view>")]
    public void ViewXml_WhenSet_ShouldReturnCorrectValue(string expectedXml)
    {
        // Arrange
        var uniqueInstanceName = $"ViewXmlSet_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName)
        {
            // Act
            ViewXml = expectedXml
        };
        var actualXml = _preferences.ViewXml;

        // Assert
        actualXml.Should().Be(expectedXml);
    }

    [Fact]
    public void Constants_ShouldHaveExpectedValues()
    {
        // Assert
        InstancePreferences.DefaultHeight.Should().Be(500);
        InstancePreferences.DefaultLeftPosition.Should().Be(100);
        InstancePreferences.DefaultOpacity.Should().Be(0.5);
        InstancePreferences.DefaultTopPosition.Should().Be(100);
        InstancePreferences.DefaultWidth.Should().Be(700);
    }

    [Fact]
    public void NullValues_ShouldBeHandledGracefully()
    {
        // Arrange
        var uniqueInstanceName = $"NullTest_{Guid.NewGuid():N}";
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act & Assert - These should not throw exceptions
        var action1 = () => _preferences.OutlookFolderName = null;
        var action2 = () => _preferences.OutlookFolderView = null;
        var action3 = () => _preferences.OutlookFolderEntryId = null;
        var action4 = () => _preferences.OutlookFolderStoreId = null;

        action1.Should().NotThrow();
        action2.Should().NotThrow();
        action3.Should().NotThrow();
        action4.Should().NotThrow();
    }

    [Fact]
    public void Opacity_WithInvalidRegistryValue_ShouldReturnDefault()
    {
        // Arrange
        var uniqueInstanceName = $"InvalidOpacityTest_{Guid.NewGuid():N}";
        var testKeyPath = $@"{_baseTestPath}\TestProduct\{uniqueInstanceName}";

        using var testKey = Registry.CurrentUser.CreateSubKey(testKeyPath);
        testKey.SetValue("Opacity", "invalid_value");
        _preferences = new InstancePreferences(uniqueInstanceName);

        // Act
        var opacity = _preferences.Opacity;

        // Assert
        opacity.Should().Be(InstancePreferences.DefaultOpacity);
    }

    public void Dispose()
    {
        _preferences = null;
        CleanupTestKeys();
    }

    private void CleanupTestKeys()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(_baseTestPath, false);
        }
        catch
        {
            // Key doesn't exist or can't be deleted, which is fine for cleanup
        }
    }
}
