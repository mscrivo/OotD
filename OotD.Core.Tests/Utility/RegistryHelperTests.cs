using Microsoft.Win32;
using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class RegistryHelperTests : IDisposable
{
    private readonly string _testKeyPath = @"Software\OotDTests";
    private RegistryKey? _testKey;

    public RegistryHelperTests()
    {
        // Clean up any existing test keys before starting
        CleanupTestKeys();

        // Create a test registry key
        _testKey = Registry.CurrentUser.CreateSubKey(_testKeyPath);
    }

    [Fact]
    public void RenameSubKey_WithValidKeys_ShouldRenameSuccessfully()
    {
        // Arrange
        const string originalName = "OriginalKey";
        const string newName = "RenamedKey";

        var originalKey = _testKey!.CreateSubKey(originalName);
        originalKey.SetValue("TestValue", "TestData");
        originalKey.Close();

        // Act
        RegistryHelper.RenameSubKey(_testKey, originalName, newName);

        // Assert
        var renamedKey = _testKey.OpenSubKey(newName);
        renamedKey.Should().NotBeNull();
        renamedKey!.GetValue("TestValue").Should().Be("TestData");
        renamedKey.Close();

        var originalKeyAfterRename = _testKey.OpenSubKey(originalName);
        originalKeyAfterRename.Should().BeNull();
    }

    [Fact]
    public void RenameSubKey_WithNestedKeys_ShouldPreserveStructure()
    {
        // Arrange
        const string originalName = "ParentKey";
        const string newName = "NewParentKey";

        var parentKey = _testKey!.CreateSubKey(originalName);
        var childKey = parentKey.CreateSubKey("ChildKey");
        childKey.SetValue("ChildValue", "ChildData");
        childKey.Close();
        parentKey.Close();

        // Act
        RegistryHelper.RenameSubKey(_testKey, originalName, newName);

        // Assert
        var renamedParent = _testKey.OpenSubKey(newName);
        renamedParent.Should().NotBeNull();

        var renamedChild = renamedParent!.OpenSubKey("ChildKey");
        renamedChild.Should().NotBeNull();
        renamedChild!.GetValue("ChildValue").Should().Be("ChildData");

        renamedChild.Close();
        renamedParent.Close();
    }

    [Fact]
    public void RenameSubKey_WithNonExistentKey_ShouldThrowException()
    {
        // Arrange
        const string nonExistentKey = "NonExistentKey";
        const string newName = "NewName";

        // Act & Assert
        var action = () => RegistryHelper.RenameSubKey(_testKey!, nonExistentKey, newName);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RenameSubKey_WithMultipleValueTypes_ShouldPreserveTypes()
    {
        // Arrange
        const string originalName = "TypeTestKey";
        const string newName = "NewTypeTestKey";

        var originalKey = _testKey!.CreateSubKey(originalName);
        originalKey.SetValue("StringValue", "TestString", RegistryValueKind.String);
        originalKey.SetValue("DWordValue", 42, RegistryValueKind.DWord);
        originalKey.SetValue("BinaryValue", new byte[] { 1, 2, 3, 4 }, RegistryValueKind.Binary);
        originalKey.Close();

        // Act
        RegistryHelper.RenameSubKey(_testKey, originalName, newName);

        // Assert
        var renamedKey = _testKey.OpenSubKey(newName);
        renamedKey.Should().NotBeNull();

        renamedKey!.GetValue("StringValue").Should().Be("TestString");
        renamedKey.GetValueKind("StringValue").Should().Be(RegistryValueKind.String);

        renamedKey.GetValue("DWordValue").Should().Be(42);
        renamedKey.GetValueKind("DWordValue").Should().Be(RegistryValueKind.DWord);

        var binaryValue = renamedKey.GetValue("BinaryValue") as byte[];
        binaryValue.Should().NotBeNull();
        binaryValue.Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4 });
        renamedKey.GetValueKind("BinaryValue").Should().Be(RegistryValueKind.Binary);

        renamedKey.Close();
    }

    [Fact]
    public void RenameSubKey_WithWhitespaceNames_ShouldWork()
    {
        // Arrange
        const string originalName = "OriginalKey";
        const string newName = " SpaceName ";

        var originalKey = _testKey!.CreateSubKey(originalName);
        originalKey.SetValue("TestValue", "TestData");
        originalKey.Close();

        // Act - Registry actually allows whitespace names
        RegistryHelper.RenameSubKey(_testKey, originalName, newName);

        // Assert
        var renamedKey = _testKey.OpenSubKey(newName);
        renamedKey.Should().NotBeNull();
        renamedKey!.GetValue("TestValue").Should().Be("TestData");
        renamedKey.Close();
    }

    [Fact]
    public void RenameSubKey_WithEmptyStringName_ShouldWork()
    {
        // Arrange - Registry actually allows empty string as key name
        const string originalName = "OriginalKey";
        const string newName = "";

        var originalKey = _testKey!.CreateSubKey(originalName);
        originalKey.SetValue("TestValue", "TestData");
        originalKey.Close();

        // Act - This should work since Registry allows empty names
        RegistryHelper.RenameSubKey(_testKey, originalName, newName);

        // Assert
        var renamedKey = _testKey.OpenSubKey(newName);
        renamedKey.Should().NotBeNull();
        renamedKey!.GetValue("TestValue").Should().Be("TestData");
        renamedKey.Close();
    }

    public void Dispose()
    {
        CleanupTestKeys();
    }

    private void CleanupTestKeys()
    {
        try
        {
            _testKey?.Close();
            _testKey = null;

            // Delete individual subkeys first
            using var parentKey = Registry.CurrentUser.OpenSubKey(_testKeyPath, true);
            if (parentKey != null)
            {
                var subKeyNames = parentKey.GetSubKeyNames();
                foreach (var subKeyName in subKeyNames)
                {
                    try
                    {
                        parentKey.DeleteSubKeyTree(subKeyName, false);
                    }
                    catch
                    {
                        // Ignore individual key deletion failures
                    }
                }
            }

            // Finally delete the parent key
            Registry.CurrentUser.DeleteSubKeyTree(_testKeyPath, false);
        }
        catch
        {
            // Key doesn't exist or can't be deleted, which is fine for cleanup
        }
    }
}
