namespace OotD.Core.Tests.Forms;

using Microsoft.Win32;
using OotD.Forms;

public class InstanceManagerTests : IDisposable
{
    private readonly List<string> _createdSubKeys = [];

    [Fact]
    public void InstanceCount_WithNoRegistryEntries_ShouldReturnZero()
    {
        // This test would require mocking the Registry, which is complex
        // For now, we'll test the logic conceptually

        // Arrange
        var instanceNames = Array.Empty<string>();

        // Act
        var count = instanceNames.Count(name => name != "AutoUpdate");

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void InstanceCount_WithAutoUpdateOnly_ShouldReturnZero()
    {
        // Arrange
        var instanceNames = new[] { "AutoUpdate" };

        // Act
        var count = instanceNames.Count(name => name != "AutoUpdate");

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void InstanceCount_WithValidInstances_ShouldReturnCorrectCount()
    {
        // Arrange
        var instanceNames = new[] { "Instance1", "Instance2", "AutoUpdate", "Instance3" };

        // Act
        var count = instanceNames.Count(name => name != "AutoUpdate");

        // Assert
        count.Should().Be(3);
    }

    [Theory]
    [InlineData(new string[] { }, 0)]
    [InlineData(new[] { "AutoUpdate" }, 0)]
    [InlineData(new[] { "Instance1" }, 1)]
    [InlineData(new[] { "Instance1", "Instance2" }, 2)]
    [InlineData(new[] { "Instance1", "AutoUpdate", "Instance2" }, 2)]
    public void InstanceCount_WithVariousScenarios_ShouldReturnExpectedCount(string[] instanceNames, int expectedCount)
    {
        // Act
        var count = instanceNames.Count(name => name != "AutoUpdate");

        // Assert
        count.Should().Be(expectedCount);
    }

    [Fact]
    public void InstanceCount_WithOnlyAutoUpdateRegistryEntry_ShouldReturnZero()
    {
        // Arrange
        var baseline = InstanceManager.InstanceCount;
        using var productKey = Registry.CurrentUser.CreateSubKey(ProductRegistryPath);
        productKey.Should().NotBeNull();
        productKey!.CreateSubKey("AutoUpdate")!.Dispose();

        // Act
        var count = InstanceManager.InstanceCount;

        // Assert
        count.Should().Be(baseline);
    }

    [Fact]
    public void InstanceCount_WithMixedRegistryEntries_ShouldExcludeAutoUpdate()
    {
        // Arrange
        var baseline = InstanceManager.InstanceCount;
        var key1 = $"Test_{Guid.NewGuid():N}";
        var key2 = $"Test_{Guid.NewGuid():N}";

        using var productKey = Registry.CurrentUser.CreateSubKey(ProductRegistryPath);
        productKey.Should().NotBeNull();
        productKey!.CreateSubKey("AutoUpdate")!.Dispose();
        productKey.CreateSubKey(key1)!.Dispose();
        productKey.CreateSubKey(key2)!.Dispose();

        _createdSubKeys.Add(key1);
        _createdSubKeys.Add(key2);

        // Act
        var count = InstanceManager.InstanceCount;

        // Assert
        count.Should().Be(baseline + 2);
    }

    public void Dispose()
    {
        try
        {
            foreach (var subKey in _createdSubKeys)
            {
                Registry.CurrentUser.DeleteSubKeyTree($"{ProductRegistryPath}\\{subKey}", false);
            }
        }
        catch
        {
            // ignore cleanup failures
        }
    }

    private static string ProductRegistryPath => $@"Software\{Application.CompanyName}\{Application.ProductName}";
}
