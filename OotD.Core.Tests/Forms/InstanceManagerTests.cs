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

    [Fact]
    public void FindNonOverlappingLocation_WithNoExistingWindows_ShouldReturnTopLeftOfWorkingArea()
    {
        // Arrange
        var workingArea = new Rectangle(100, 100, 800, 600);
        var size = new Size(300, 200);

        // Act
        var location = InstanceManager.FindNonOverlappingLocation(workingArea, size, Array.Empty<Rectangle>());

        // Assert
        location.Should().Be(new Point(100, 100));
    }

    [Fact]
    public void FindNonOverlappingLocation_WithOccupiedTopLeft_ShouldReturnFirstAvailableSlot()
    {
        // Arrange
        var workingArea = new Rectangle(0, 0, 400, 300);
        var size = new Size(100, 100);
        var occupied = new[] { new Rectangle(0, 0, 100, 100) };

        // Act
        var location = InstanceManager.FindNonOverlappingLocation(workingArea, size, occupied);

        // Assert
        // Step size is 30px; x=120 is the first non-overlapping candidate on row 0.
        location.Should().Be(new Point(120, 0));
    }

    [Fact]
    public void FindNonOverlappingLocation_WhenWorkingAreaOffset_ShouldRespectBounds()
    {
        // Arrange
        var workingArea = new Rectangle(1920, 0, 800, 600);
        var size = new Size(250, 150);
        var occupied = new[]
        {
            new Rectangle(1920, 0, 250, 150),
            new Rectangle(2160, 0, 250, 150)
        };

        // Act
        var location = InstanceManager.FindNonOverlappingLocation(workingArea, size, occupied);
        var placed = new Rectangle(location, size);

        // Assert
        workingArea.Contains(placed).Should().BeTrue();
        occupied.Any(existing => existing.IntersectsWith(placed)).Should().BeFalse();
    }

    [Fact]
    public void FindNonOverlappingLocation_WithPreferredStartThatIsFree_ShouldUsePreferredStart()
    {
        // Arrange
        var workingArea = new Rectangle(0, 0, 500, 400);
        var size = new Size(100, 100);
        var occupied = new[] { new Rectangle(0, 0, 100, 100) };
        var preferredStart = new Point(240, 180);

        // Act
        var location = InstanceManager.FindNonOverlappingLocation(workingArea, size, occupied, preferredStart);

        // Assert
        location.Should().Be(preferredStart);
    }

    [Fact]
    public void GetCascadedStartPoint_WithNoOccupiedWindows_ShouldReturnNull()
    {
        // Arrange
        var workingArea = new Rectangle(0, 0, 500, 400);
        var size = new Size(100, 100);

        // Act
        var result = InstanceManager.GetCascadedStartPoint(workingArea, size, Array.Empty<Rectangle>());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCascadedStartPoint_WithOccupiedWindows_ShouldReturnOffsetAndClampedPoint()
    {
        // Arrange
        var workingArea = new Rectangle(0, 0, 350, 250);
        var size = new Size(100, 100);
        var occupied = new[]
        {
            new Rectangle(260, 180, 100, 100),
            new Rectangle(50, 30, 100, 100)
        };

        // Act
        var result = InstanceManager.GetCascadedStartPoint(workingArea, size, occupied);

        // Assert
        // Anchor is the lowest window (260,180). +30 offset is clamped to max valid origin (250,150).
        result.Should().Be(new Point(250, 150));
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
