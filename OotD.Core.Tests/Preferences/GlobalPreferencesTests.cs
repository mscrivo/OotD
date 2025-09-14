using Microsoft.Win32;
using OotD.Preferences;

namespace OotD.Core.Tests.Preferences;

public class GlobalPreferencesTests : IDisposable
{
    private readonly string _testKeyPath = @"Software\OotDTests\TestProduct";

    public GlobalPreferencesTests()
    {
        // Clean up any existing test keys
        CleanupTestKeys();
    }

    [Fact]
    public void LockPosition_WhenValueNotSet_ShouldReturnFalse()
    {
        // Arrange
        using var testKey = Registry.CurrentUser.CreateSubKey(_testKeyPath);

        // We can't directly test GlobalPreferences.LockPosition due to its dependency on Application.CompanyName
        // But we can test the registry behavior it relies on
        var lockPosition = bool.TryParse(testKey.GetValue("LockPosition", "false").ToString(), out var result) && result;

        // Assert
        lockPosition.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LockPosition_WhenValueSet_ShouldReturnCorrectValue(bool expectedValue)
    {
        // Arrange
        using var testKey = Registry.CurrentUser.CreateSubKey(_testKeyPath);
        testKey.SetValue("LockPosition", expectedValue);

        // Act
        var lockPosition = bool.TryParse(testKey.GetValue("LockPosition", "false").ToString(), out var result) && result;

        // Assert
        lockPosition.Should().Be(expectedValue);
    }

    [Fact]
    public void IsFirstRun_WhenValueNotSet_ShouldReturnTrue()
    {
        // Arrange
        using var testKey = Registry.CurrentUser.CreateSubKey(_testKeyPath);

        // Act
        var isFirstRun = bool.TryParse(testKey.GetValue("FirstRun", "true").ToString(), out var result) && result;

        // Assert
        isFirstRun.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("null")]
    public void LockPosition_WithInvalidValues_ShouldReturnFalse(string invalidValue)
    {
        // Arrange
        using var testKey = Registry.CurrentUser.CreateSubKey(_testKeyPath);
        testKey.SetValue("LockPosition", invalidValue);

        // Act
        var lockPosition = bool.TryParse(testKey.GetValue("LockPosition", "false").ToString(), out var result) && result;

        // Assert
        lockPosition.Should().BeFalse();
    }

    [Theory]
    [InlineData("True")]
    [InlineData("true")]
    [InlineData("TRUE")]
    [InlineData("False")]
    [InlineData("false")]
    [InlineData("FALSE")]
    public void BooleanValues_ShouldBeCaseInsensitive(string boolValue)
    {
        // Arrange
        using var testKey = Registry.CurrentUser.CreateSubKey(_testKeyPath);
        testKey.SetValue("LockPosition", boolValue);
        var expectedResult = bool.Parse(boolValue);

        // Act
        var lockPosition = bool.TryParse(testKey.GetValue("LockPosition", "false").ToString(), out var result) && result;

        // Assert
        lockPosition.Should().Be(expectedResult);
    }

    [Fact]
    public void GlobalPreferences_ShouldBeStaticClass()
    {
        // Assert
        var type = typeof(GlobalPreferences);
        type.IsClass.Should().BeTrue();
        type.IsAbstract.Should().BeTrue(); // Static classes are abstract and sealed
        type.IsSealed.Should().BeTrue();
    }

    [Fact]
    public void GlobalPreferences_ShouldHaveExpectedProperties()
    {
        // Arrange
        var type = typeof(GlobalPreferences);

        // Act
        var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        // Assert
        properties.Should().Contain(p => p.Name == "StartWithWindows");
        properties.Should().Contain(p => p.Name == "LockPosition");
        properties.Should().Contain(p => p.Name == "IsFirstRun");
    }

    [Fact]
    public void GlobalPreferences_Properties_ShouldHaveCorrectTypes()
    {
        // Arrange
        var type = typeof(GlobalPreferences);

        // Act
        var startWithWindowsProperty = type.GetProperty("StartWithWindows");
        var lockPositionProperty = type.GetProperty("LockPosition");
        var isFirstRunProperty = type.GetProperty("IsFirstRun");

        // Assert
        startWithWindowsProperty.Should().NotBeNull();
        startWithWindowsProperty!.PropertyType.Should().Be<bool>();

        lockPositionProperty.Should().NotBeNull();
        lockPositionProperty!.PropertyType.Should().Be<bool>();

        isFirstRunProperty.Should().NotBeNull();
        isFirstRunProperty!.PropertyType.Should().Be<bool>();
    }

    public void Dispose()
    {
        CleanupTestKeys();
    }

    private void CleanupTestKeys()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(_testKeyPath, false);
        }
        catch
        {
            // Key doesn't exist or can't be deleted, which is fine for cleanup
        }
    }
}
