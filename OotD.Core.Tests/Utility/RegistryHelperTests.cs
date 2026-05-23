using Microsoft.Win32;
using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class RegistryHelperTests : IDisposable
{
    private readonly string _testRootPath = $@"Software\OotDTests\RegistryHelper\{Guid.NewGuid():N}";

    [Fact]
    public void RenameSubKey_ShouldMoveValuesWithOriginalRegistryKinds()
    {
        // Arrange
        using var parentKey = Registry.CurrentUser.CreateSubKey(_testRootPath);
        using var sourceKey = parentKey.CreateSubKey("OldName");
        sourceKey.SetValue("StringValue", "hello", RegistryValueKind.String);
        sourceKey.SetValue("DwordValue", 42, RegistryValueKind.DWord);
        sourceKey.SetValue("BinaryValue", new byte[] { 1, 2, 3 }, RegistryValueKind.Binary);
        sourceKey.SetValue("MultiStringValue", new[] { "one", "two" }, RegistryValueKind.MultiString);

        // Act
        RegistryHelper.RenameSubKey(parentKey, "OldName", "NewName");

        // Assert
        using var renamedKey = parentKey.OpenSubKey("NewName");
        renamedKey.Should().NotBeNull();
        renamedKey!.GetValue("StringValue").Should().Be("hello");
        renamedKey.GetValueKind("StringValue").Should().Be(RegistryValueKind.String);
        renamedKey.GetValue("DwordValue").Should().Be(42);
        renamedKey.GetValueKind("DwordValue").Should().Be(RegistryValueKind.DWord);
        renamedKey.GetValue("BinaryValue").Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
        renamedKey.GetValueKind("BinaryValue").Should().Be(RegistryValueKind.Binary);
        renamedKey.GetValue("MultiStringValue").Should().BeEquivalentTo(new[] { "one", "two" });
        renamedKey.GetValueKind("MultiStringValue").Should().Be(RegistryValueKind.MultiString);
    }

    [Fact]
    public void RenameSubKey_ShouldCopyNestedSubKeysAndDeleteOriginalTree()
    {
        // Arrange
        using var parentKey = Registry.CurrentUser.CreateSubKey(_testRootPath);
        using var sourceKey = parentKey.CreateSubKey("OldName");
        using var nestedKey = sourceKey.CreateSubKey(@"Nested\Leaf");
        nestedKey.SetValue("LeafValue", "copied");

        // Act
        RegistryHelper.RenameSubKey(parentKey, "OldName", "NewName");

        // Assert
        using var oldKey = parentKey.OpenSubKey("OldName");
        oldKey.Should().BeNull();

        using var renamedLeaf = parentKey.OpenSubKey(@"NewName\Nested\Leaf");
        renamedLeaf.Should().NotBeNull();
        renamedLeaf!.GetValue("LeafValue").Should().Be("copied");
    }

    [Fact]
    public void RenameSubKey_WithEmptySourceKey_ShouldCreateDestinationAndDeleteSource()
    {
        // Arrange
        using var parentKey = Registry.CurrentUser.CreateSubKey(_testRootPath);
        parentKey.CreateSubKey("OldName")!.Dispose();

        // Act
        RegistryHelper.RenameSubKey(parentKey, "OldName", "NewName");

        // Assert
        parentKey.OpenSubKey("OldName").Should().BeNull();
        using var renamedKey = parentKey.OpenSubKey("NewName");
        renamedKey.Should().NotBeNull();
        renamedKey!.GetValueNames().Should().BeEmpty();
        renamedKey.GetSubKeyNames().Should().BeEmpty();
    }

    [Fact]
    public void RenameSubKey_WhenDestinationExists_ShouldOverwriteMatchingValuesAndKeepExistingValues()
    {
        // Arrange
        using var parentKey = Registry.CurrentUser.CreateSubKey(_testRootPath);
        using var sourceKey = parentKey.CreateSubKey("OldName");
        sourceKey.SetValue("Shared", "from source");
        sourceKey.SetValue("SourceOnly", "copied");

        using var destinationKey = parentKey.CreateSubKey("NewName");
        destinationKey.SetValue("Shared", "from destination");
        destinationKey.SetValue("DestinationOnly", "kept");

        // Act
        RegistryHelper.RenameSubKey(parentKey, "OldName", "NewName");

        // Assert
        using var renamedKey = parentKey.OpenSubKey("NewName");
        renamedKey.Should().NotBeNull();
        renamedKey!.GetValue("Shared").Should().Be("from source");
        renamedKey.GetValue("SourceOnly").Should().Be("copied");
        renamedKey.GetValue("DestinationOnly").Should().Be("kept");
    }

    [Fact]
    public void RenameSubKey_WhenSourceKeyDoesNotExist_ShouldThrowArgumentException()
    {
        // Arrange
        using var parentKey = Registry.CurrentUser.CreateSubKey(_testRootPath);

        // Act
        var action = () => RegistryHelper.RenameSubKey(parentKey, "Missing", "NewName");

        // Assert
        action.Should().Throw<ArgumentException>();
    }

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

    public void Dispose()
    {
        Registry.CurrentUser.DeleteSubKeyTree(_testRootPath, false);
    }
}
