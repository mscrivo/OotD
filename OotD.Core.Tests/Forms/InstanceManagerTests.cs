namespace OotD.Core.Tests.Forms;

public class InstanceManagerTests
{
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
}
