using OotD.Events;

namespace OotD.Core.Tests.Events;

public class EventArgsTests
{
    [Fact]
    public void InputBoxValidatingEventArgs_ShouldInitializeWithDefaultValues()
    {
        // Act
        var eventArgs = new InputBoxValidatingEventArgs();

        // Assert
        eventArgs.Text.Should().BeNull();
        eventArgs.Message.Should().BeNull();
        eventArgs.Cancel.Should().BeFalse();
    }

    [Fact]
    public void InputBoxValidatingEventArgs_ShouldAllowSettingText()
    {
        // Arrange
        const string testText = "Test Input";

        // Act
        var eventArgs = new InputBoxValidatingEventArgs { Text = testText };

        // Assert
        eventArgs.Text.Should().Be(testText);
    }

    [Fact]
    public void InputBoxValidatingEventArgs_ShouldAllowSettingProperties()
    {
        // Arrange
        var eventArgs = new InputBoxValidatingEventArgs
        {
            // Act
            Message = "Error message",
            Cancel = true
        };

        // Assert
        eventArgs.Message.Should().Be("Error message");
        eventArgs.Cancel.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Valid input")]
    [InlineData("   ")]
    public void InputBoxValidatingEventArgs_ShouldAcceptAnyTextValue(string? text)
    {
        // Act
        var eventArgs = new InputBoxValidatingEventArgs { Text = text };

        // Assert
        eventArgs.Text.Should().Be(text);
    }

    [Fact]
    public void InstanceRemovedEventArgs_ShouldInitializeWithInstanceName()
    {
        // Arrange
        const string instanceName = "TestInstance";

        // Act
        var eventArgs = new InstanceRemovedEventArgs(instanceName);

        // Assert
        eventArgs.InstanceName.Should().Be(instanceName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Valid Instance Name")]
    [InlineData("Instance-With-Dashes")]
    [InlineData("Instance_With_Underscores")]
    public void InstanceRemovedEventArgs_ShouldAcceptValidInstanceNames(string instanceName)
    {
        // Act
        var eventArgs = new InstanceRemovedEventArgs(instanceName);

        // Assert
        eventArgs.InstanceName.Should().Be(instanceName);
    }

    [Fact]
    public void InstanceRenamedEventArgs_ShouldInitializeWithBothNames()
    {
        // Arrange
        const string oldName = "OldInstance";
        const string newName = "NewInstance";

        // Act
        var eventArgs = new InstanceRenamedEventArgs(oldName, newName);

        // Assert
        eventArgs.OldInstanceName.Should().Be(oldName);
        eventArgs.NewInstanceName.Should().Be(newName);
    }

    [Theory]
    [InlineData("Old", "New")]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    [InlineData("Instance1", "Instance2")]
    public void InstanceRenamedEventArgs_ShouldAcceptValidNamePairs(string oldName, string newName)
    {
        // Act
        var eventArgs = new InstanceRenamedEventArgs(oldName, newName);

        // Assert
        eventArgs.OldInstanceName.Should().Be(oldName);
        eventArgs.NewInstanceName.Should().Be(newName);
    }

    [Fact]
    public void InstanceRenamedEventArgs_WithSameNames_ShouldBeValid()
    {
        // Arrange
        const string sameName = "SameName";

        // Act
        var eventArgs = new InstanceRenamedEventArgs(sameName, sameName);

        // Assert
        eventArgs.OldInstanceName.Should().Be(sameName);
        eventArgs.NewInstanceName.Should().Be(sameName);
    }
}
