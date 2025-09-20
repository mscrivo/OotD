using OotD.Events;
using OotD.Forms;

namespace OotD.Core.Tests.Forms;

public class InputBoxTests : IDisposable
{
    private readonly Form? _ownerForm;

    public InputBoxTests()
    {
        _ownerForm = new Form();
    }

    [Fact]
    public void InputBox_ShouldBeDerivedFromForm()
    {
        // Assert
        typeof(InputBox).Should().BeDerivedFrom<Form>();
    }

    [Fact]
    public void InputBoxValidatingEventArgs_ShouldHaveCorrectInitialValues()
    {
        // Act
        var eventArgs = new InputBoxValidatingEventArgs();

        // Assert
        eventArgs.Text.Should().BeNull();
        eventArgs.Message.Should().BeNull();
        eventArgs.Cancel.Should().BeFalse();
    }

    [Fact]
    public void InputBoxValidatingEventArgs_ShouldAllowPropertyModification()
    {
        // Arrange
        var eventArgs = new InputBoxValidatingEventArgs
        {
            Text = "Test Text",
            Message = "Error Message",
            Cancel = true
        };

        // Assert
        eventArgs.Text.Should().Be("Test Text");
        eventArgs.Message.Should().Be("Error Message");
        eventArgs.Cancel.Should().BeTrue();
    }

    [Fact]
    public void InputBoxValidatingEventHandler_ShouldHaveCorrectSignature()
    {
        // Arrange
        InputBoxValidatingEventHandler? handler = null;

        // Act & Assert
        var action = () =>
        {
            handler = (sender, e) =>
            {
                e.Should().NotBeNull();
                e.Cancel.Should().BeFalse();
            };
        };

        action.Should().NotThrow();
        handler.Should().NotBeNull();
    }

    [Fact]
    public void InputBoxResult_ShouldHaveDefaultValues()
    {
        // Act
        var result = new InputBoxResult();

        // Assert
        result.Ok.Should().BeFalse();
        result.Text.Should().BeNullOrEmpty();
    }

    [Fact]
    public void InputBoxResult_ShouldAllowPropertySetting()
    {
        // Act
        var result = new InputBoxResult
        {
            Ok = true,
            Text = "User Input"
        };

        // Assert
        result.Ok.Should().BeTrue();
        result.Text.Should().Be("User Input");
    }

    [Theory]
    [InlineData(true, "User clicked OK")]
    [InlineData(false, "User cancelled")]
    [InlineData(true, "")]
    [InlineData(false, null)]
    public void InputBoxResult_WithVariousValues_ShouldRetainCorrectValues(bool ok, string text)
    {
        // Act
        var result = new InputBoxResult
        {
            Ok = ok,
            Text = text
        };

        // Assert
        result.Ok.Should().Be(ok);
        result.Text.Should().Be(text);
    }

    [Fact]
    public void InputBoxValidatingEventArgs_WithNullText_ShouldWork()
    {
        // Act
        var eventArgs = new InputBoxValidatingEventArgs { Text = null };

        // Assert
        eventArgs.Text.Should().BeNull();
    }

    [Fact]
    public void InputBoxValidatingEventArgs_WithEmptyText_ShouldWork()
    {
        // Act
        var eventArgs = new InputBoxValidatingEventArgs { Text = "" };

        // Assert
        eventArgs.Text.Should().Be("");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Test text")]
    [InlineData("Multi\nLine\nText")]
    [InlineData("Special chars: åäöÅÄÖ")]
    public void InputBoxValidatingEventArgs_WithVariousText_ShouldWork(string testText)
    {
        // Act
        var eventArgs = new InputBoxValidatingEventArgs { Text = testText };

        // Assert
        eventArgs.Text.Should().Be(testText);
    }

    [Fact]
    public void InputBoxResult_MultipleAssignments_ShouldOverwritePreviousValues()
    {
        // Arrange
        var result = new InputBoxResult
        {
            // Act
            Ok = true,
            Text = "First value"
        };

        result.Ok = false;
        result.Text = "Second value";

        // Assert
        result.Ok.Should().BeFalse();
        result.Text.Should().Be("Second value");
    }

    [Fact]
    public void InputBoxResult_TextProperty_ShouldAcceptLongStrings()
    {
        // Arrange
        var longText = new string('A', 10000);
        var result = new InputBoxResult
        {
            // Act
            Text = longText
        };

        // Assert
        result.Text.Should().Be(longText);
        result.Text.Length.Should().Be(10000);
    }

    [Fact]
    public void InputBoxResult_ShouldBeClassType()
    {
        // Assert
        typeof(InputBoxResult).IsClass.Should().BeTrue();
        typeof(InputBoxResult).IsValueType.Should().BeFalse();
    }

    [Fact]
    public void InputBoxResult_Properties_ShouldHaveCorrectTypes()
    {
        // Arrange
        var type = typeof(InputBoxResult);

        // Act
        var okProperty = type.GetProperty("Ok");
        var textProperty = type.GetProperty("Text");

        // Assert
        okProperty.Should().NotBeNull();
        okProperty!.PropertyType.Should().Be<bool>();

        textProperty.Should().NotBeNull();
        textProperty!.PropertyType.Should().Be<string>();
    }

    public void Dispose()
    {
        _ownerForm?.Dispose();
        GC.SuppressFinalize(this);
    }
}
