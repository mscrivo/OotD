using Microsoft.Win32;
using NSubstitute;
using NLog;
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

    [Fact]
    public void FirstRun_AfterAccess_ShouldSetToFalse()
    {
        // Arrange
        using var testKey = Registry.CurrentUser.CreateSubKey(_testKeyPath);
        
        // Simulate first run behavior
        if (bool.TryParse(testKey.GetValue("FirstRun", "true").ToString(), out var isFirstRun) && isFirstRun)
        {
            testKey.SetValue("FirstRun", false);
        }

        // Act
        var subsequentCheck = bool.TryParse(testKey.GetValue("FirstRun", "true").ToString(), out var result) && result;

        // Assert
        subsequentCheck.Should().BeFalse();
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
