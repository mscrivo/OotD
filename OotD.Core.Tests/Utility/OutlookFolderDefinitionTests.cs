using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class OutlookFolderDefinitionTests
{
    [Fact]
    public void OutlookFolderDefinition_ShouldInitializeWithDefaultValues()
    {
        // Act
        var definition = new OutlookFolderDefinition();

        // Assert
        definition.OutlookFolderStoreId.Should().BeNull();
        definition.OutlookFolderName.Should().BeNull();
        definition.OutlookFolderEntryId.Should().BeNull();
    }

    [Fact]
    public void OutlookFolderDefinition_ShouldAllowSettingAllProperties()
    {
        // Arrange
        const string storeId = "test-store-id";
        const string folderName = "Test Folder";
        const string entryId = "test-entry-id";

        // Act
        var definition = new OutlookFolderDefinition
        {
            OutlookFolderStoreId = storeId,
            OutlookFolderName = folderName,
            OutlookFolderEntryId = entryId
        };

        // Assert
        definition.OutlookFolderStoreId.Should().Be(storeId);
        definition.OutlookFolderName.Should().Be(folderName);
        definition.OutlookFolderEntryId.Should().Be(entryId);
    }

    [Fact]
    public void OutlookFolderDefinition_ShouldAllowNullValues()
    {
        // Act
        var definition = new OutlookFolderDefinition
        {
            OutlookFolderStoreId = null,
            OutlookFolderName = null,
            OutlookFolderEntryId = null
        };

        // Assert
        definition.OutlookFolderStoreId.Should().BeNull();
        definition.OutlookFolderName.Should().BeNull();
        definition.OutlookFolderEntryId.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void OutlookFolderDefinition_ShouldAllowEmptyAndWhitespaceValues(string value)
    {
        // Act
        var definition = new OutlookFolderDefinition
        {
            OutlookFolderStoreId = value,
            OutlookFolderName = value,
            OutlookFolderEntryId = value
        };

        // Assert
        definition.OutlookFolderStoreId.Should().Be(value);
        definition.OutlookFolderName.Should().Be(value);
        definition.OutlookFolderEntryId.Should().Be(value);
    }

    [Fact]
    public void OutlookFolderDefinition_StructsShouldBeValueEqual()
    {
        // Arrange
        var definition1 = new OutlookFolderDefinition
        {
            OutlookFolderStoreId = "same-store",
            OutlookFolderName = "same-folder",
            OutlookFolderEntryId = "same-entry"
        };

        var definition2 = new OutlookFolderDefinition
        {
            OutlookFolderStoreId = "same-store",
            OutlookFolderName = "same-folder",
            OutlookFolderEntryId = "same-entry"
        };

        // Act & Assert
        definition1.Equals(definition2).Should().BeTrue();
        definition1.GetHashCode().Should().Be(definition2.GetHashCode());
    }

    [Fact]
    public void OutlookFolderDefinition_DifferentValuesShouldNotBeEqual()
    {
        // Arrange
        var definition1 = new OutlookFolderDefinition
        {
            OutlookFolderStoreId = "store1",
            OutlookFolderName = "folder1",
            OutlookFolderEntryId = "entry1"
        };

        var definition2 = new OutlookFolderDefinition
        {
            OutlookFolderStoreId = "store2",
            OutlookFolderName = "folder2",
            OutlookFolderEntryId = "entry2"
        };

        // Act & Assert
        definition1.Equals(definition2).Should().BeFalse();
    }

    [Fact]
    public void OutlookFolderDefinition_ShouldBeStructType()
    {
        // Assert
        typeof(OutlookFolderDefinition).IsValueType.Should().BeTrue();
    }

    [Fact]
    public void OutlookFolderDefinition_ShouldHaveExpectedProperties()
    {
        // Arrange
        var type = typeof(OutlookFolderDefinition);

        // Act
        var properties = type.GetProperties();

        // Assert
        properties.Should().Contain(p => p.Name == "OutlookFolderStoreId");
        properties.Should().Contain(p => p.Name == "OutlookFolderName");
        properties.Should().Contain(p => p.Name == "OutlookFolderEntryId");
    }
}
