using CommandLine;

namespace OotD.Core.Tests.Core;

public class OptionsTests
{
    [Theory]
    [InlineData("-d", true, false, false)]
    [InlineData("--debug", true, false, false)]
    [InlineData("-s", false, true, false)]
    [InlineData("--createStartupEntry", false, true, false)]
    [InlineData("-r", false, false, true)]
    [InlineData("--removeStartupEntry", false, false, true)]
    public void ParseArguments_WithKnownSwitches_ShouldSetExpectedOption(
        string argument,
        bool expectedStartDebugger,
        bool expectedCreateStartupEntry,
        bool expectedRemoveStartupEntry)
    {
        // Act
        var options = ParseOptions(argument);

        // Assert
        options.StartDebugger.Should().Be(expectedStartDebugger);
        options.CreateStartupEntry.Should().Be(expectedCreateStartupEntry);
        options.RemoveStartupEntry.Should().Be(expectedRemoveStartupEntry);
    }

    [Fact]
    public void ParseArguments_WithNoArguments_ShouldUseDefaultValues()
    {
        // Act
        var options = ParseOptions();

        // Assert
        options.StartDebugger.Should().BeFalse();
        options.CreateStartupEntry.Should().BeFalse();
        options.RemoveStartupEntry.Should().BeFalse();
    }

    [Fact]
    public void ParseArguments_WithMultipleSwitches_ShouldSetAllProvidedOptions()
    {
        // Act
        var options = ParseOptions("--debug", "--createStartupEntry", "--removeStartupEntry");

        // Assert
        options.StartDebugger.Should().BeTrue();
        options.CreateStartupEntry.Should().BeTrue();
        options.RemoveStartupEntry.Should().BeTrue();
    }

    [Fact]
    public void Options_ShouldHaveCorrectDefaultValues()
    {
        // Act
        var options = new Options();

        // Assert
        options.StartDebugger.Should().BeFalse();
        options.CreateStartupEntry.Should().BeFalse();
        options.RemoveStartupEntry.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void StartDebugger_WhenSet_ShouldReturnCorrectValue(bool expectedValue)
    {
        // Arrange
        var options = new Options
        {
            // Act
            StartDebugger = expectedValue
        };

        // Assert
        options.StartDebugger.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateStartupEntry_WhenSet_ShouldReturnCorrectValue(bool expectedValue)
    {
        // Arrange
        var options = new Options
        {
            // Act
            CreateStartupEntry = expectedValue
        };

        // Assert
        options.CreateStartupEntry.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RemoveStartupEntry_WhenSet_ShouldReturnCorrectValue(bool expectedValue)
    {
        // Arrange
        var options = new Options
        {
            // Act
            RemoveStartupEntry = expectedValue
        };

        // Assert
        options.RemoveStartupEntry.Should().Be(expectedValue);
    }

    [Fact]
    public void Options_Properties_ShouldBeIndependent()
    {
        // Arrange
        var options = new Options
        {
            // Act
            StartDebugger = true,
            CreateStartupEntry = true,
            RemoveStartupEntry = false
        };

        // Assert
        options.StartDebugger.Should().BeTrue();
        options.CreateStartupEntry.Should().BeTrue();
        options.RemoveStartupEntry.Should().BeFalse();

        // Act - Change one property
        options.StartDebugger = false;

        // Assert - Other properties should remain unchanged
        options.StartDebugger.Should().BeFalse();
        options.CreateStartupEntry.Should().BeTrue();
        options.RemoveStartupEntry.Should().BeFalse();
    }

    [Fact]
    public void Options_ShouldBeClassType()
    {
        // Assert
        typeof(Options).IsClass.Should().BeTrue();
        typeof(Options).IsValueType.Should().BeFalse();
    }

    [Fact]
    public void Options_Properties_ShouldHaveCorrectTypes()
    {
        // Arrange
        var type = typeof(Options);

        // Act
        var startDebuggerProperty = type.GetProperty("StartDebugger");
        var createStartupEntryProperty = type.GetProperty("CreateStartupEntry");
        var removeStartupEntryProperty = type.GetProperty("RemoveStartupEntry");

        // Assert
        startDebuggerProperty.Should().NotBeNull();
        startDebuggerProperty!.PropertyType.Should().Be<bool>();

        createStartupEntryProperty.Should().NotBeNull();
        createStartupEntryProperty!.PropertyType.Should().Be<bool>();

        removeStartupEntryProperty.Should().NotBeNull();
        removeStartupEntryProperty!.PropertyType.Should().Be<bool>();
    }

    [Fact]
    public void Options_ShouldHavePublicParameterlessConstructor()
    {
        // Act & Assert
        var action = () => new Options();
        action.Should().NotThrow();

        var options = action();
        options.Should().NotBeNull();
    }

    [Fact]
    public void Options_MultipleInstances_ShouldBeIndependent()
    {
        // Act
        var options1 = new Options { StartDebugger = true, CreateStartupEntry = false };
        var options2 = new Options { StartDebugger = false, CreateStartupEntry = true };

        // Assert
        options1.StartDebugger.Should().BeTrue();
        options1.CreateStartupEntry.Should().BeFalse();

        options2.StartDebugger.Should().BeFalse();
        options2.CreateStartupEntry.Should().BeTrue();
    }

    [Fact]
    public void Options_PropertyChanges_ShouldBeReflectedImmediately()
    {
        // Arrange
        var options = new Options();

        // Act & Assert - Test immediate reflection of changes
        options.StartDebugger.Should().BeFalse(); // Default

        options.StartDebugger = true;
        options.StartDebugger.Should().BeTrue(); // Changed

        options.StartDebugger = false;
        options.StartDebugger.Should().BeFalse(); // Changed back
    }

    [Fact]
    public void Options_AllProperties_ShouldSupportBooleanOperations()
    {
        // Arrange
        var options = new Options();

        // Act & Assert - Test boolean operations
        options.StartDebugger = !options.StartDebugger;
        options.StartDebugger.Should().BeTrue();

        options.CreateStartupEntry = !options.CreateStartupEntry;
        options.CreateStartupEntry.Should().BeTrue();

        options.RemoveStartupEntry = !options.RemoveStartupEntry;
        options.RemoveStartupEntry.Should().BeTrue();
    }

    [Fact]
    public void Options_ShouldHaveCommandLineParserAttributes()
    {
        // Arrange
        var type = typeof(Options);

        // Act
        var startDebuggerProperty = type.GetProperty("StartDebugger");
        var createStartupEntryProperty = type.GetProperty("CreateStartupEntry");
        var removeStartupEntryProperty = type.GetProperty("RemoveStartupEntry");

        // Assert - Check that properties have CommandLine attributes
        startDebuggerProperty!.GetCustomAttributes(typeof(CommandLine.OptionAttribute), false).Should().NotBeEmpty();
        createStartupEntryProperty!.GetCustomAttributes(typeof(CommandLine.OptionAttribute), false).Should().NotBeEmpty();
        removeStartupEntryProperty!.GetCustomAttributes(typeof(CommandLine.OptionAttribute), false).Should().NotBeEmpty();
    }

    [Fact]
    public void Options_CommandLineAttributes_ShouldHaveCorrectDefaults()
    {
        // Arrange
        var type = typeof(Options);
        var startDebuggerProperty = type.GetProperty("StartDebugger");

        // Act
        var attributes = startDebuggerProperty!.GetCustomAttributes(typeof(CommandLine.OptionAttribute), false);
        var optionAttribute = attributes[0] as CommandLine.OptionAttribute;

        // Assert
        optionAttribute.Should().NotBeNull();
        optionAttribute!.Default.Should().Be(false);
    }

    private static Options ParseOptions(params string[] args)
    {
        var result = Parser.Default.ParseArguments<Options>(args);

        result.Tag.Should().Be(ParserResultType.Parsed);
        return ((Parsed<Options>)result).Value;
    }
}
