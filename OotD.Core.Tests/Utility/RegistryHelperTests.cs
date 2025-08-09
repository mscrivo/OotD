using Microsoft.Win32;
using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class RegistryHelperTests
{
    [Fact]
    public void RegistryHelper_ShouldBeStaticClass()
    {
        // Assert
        var type = typeof(RegistryHelper);
        type.IsClass.Should().BeTrue();
        type.IsAbstract.Should().BeTrue(); // Static classes are abstract and sealed
        type.IsSealed.Should().BeTrue();
    }

    [Fact]
    public void RegistryHelper_RenameSubKey_ShouldBeStaticMethod()
    {
        // Arrange
        var type = typeof(RegistryHelper);

        // Act
        var method = type.GetMethod("RenameSubKey");

        // Assert
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
        method.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void RegistryHelper_MethodSignature_ShouldBeCorrect()
    {
        // Arrange
        var type = typeof(RegistryHelper);
        var method = type.GetMethod("RenameSubKey");

        // Assert
        method.Should().NotBeNull();
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].ParameterType.Should().Be<RegistryKey>();
        parameters[1].ParameterType.Should().Be<string>();
        parameters[2].ParameterType.Should().Be<string>();
        method.ReturnType.Should().Be(typeof(void));
    }

    [Fact]
    public void RegistryHelper_ShouldHaveExpectedMethods()
    {
        // Arrange
        var type = typeof(RegistryHelper);

        // Act
        var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        // Assert
        methods.Should().Contain(m => m.Name == "RenameSubKey");
    }

    [Fact]
    public void RegistryHelper_ShouldBeInternalClass()
    {
        // Arrange
        var type = typeof(RegistryHelper);

        // Assert
        type.IsNotPublic.Should().BeTrue(); // Internal classes are not public
        type.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void RegistryHelper_ShouldHaveCorrectNamespace()
    {
        // Arrange
        var type = typeof(RegistryHelper);

        // Assert
        type.Namespace.Should().Be("OotD.Utility");
    }

    [Fact]
    public void RenameSubKey_Method_ShouldHaveCorrectAttributes()
    {
        // Arrange
        var type = typeof(RegistryHelper);
        var method = type.GetMethod("RenameSubKey");

        // Assert
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
        method.IsPublic.Should().BeTrue();
        method.DeclaringType.Should().Be(type);
    }

    [Fact]
    public void RegistryHelper_ShouldNotHaveInstanceConstructor()
    {
        // Arrange
        var type = typeof(RegistryHelper);

        // Act
        var constructors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // Assert
        constructors.Should().BeEmpty("Static classes should not have public instance constructors");
    }
}
