using OotD.Preferences;

namespace OotD.Core.Tests.Integration;

public class PreferencesIntegrationTests : IDisposable
{
    private readonly string _testInstanceName = "IntegrationTestInstance";

    [Fact]
    public void InstancePreferences_ShouldPersistValuesAcrossInstances()
    {
        // Arrange
        const double expectedOpacity = 0.75;
        const int expectedLeft = 200;
        const int expectedTop = 150;
        const string expectedFolderName = "TestFolder";

        // Act - Set values in first instance
        var preferences1 = new InstancePreferences(_testInstanceName)
        {
            Opacity = expectedOpacity,
            Left = expectedLeft,
            Top = expectedTop,
            OutlookFolderName = expectedFolderName
        };
        // Note: InstancePreferences doesn't implement IDisposable, so we can't use using

        // Act - Read values in second instance
        var preferences2 = new InstancePreferences(_testInstanceName);

        // Assert
        preferences2.Opacity.Should().Be(expectedOpacity);
        preferences2.Left.Should().Be(expectedLeft);
        preferences2.Top.Should().Be(expectedTop);
        preferences2.OutlookFolderName.Should().Be(expectedFolderName);
    }

    [Fact]
    public void MultipleInstancePreferences_ShouldBeIndependent()
    {
        // Arrange
        const string instance1Name = "Instance1";
        const string instance2Name = "Instance2";

        // Act
        var prefs1 = new InstancePreferences(instance1Name);
        var prefs2 = new InstancePreferences(instance2Name);

        prefs1.Opacity = 0.3;
        prefs2.Opacity = 0.8;

        prefs1.Left = 100;
        prefs2.Left = 500;

        // Assert
        prefs1.Opacity.Should().Be(0.3);
        prefs2.Opacity.Should().Be(0.8);
        prefs1.Left.Should().Be(100);
        prefs2.Left.Should().Be(500);
    }

    [Fact]
    public void InstancePreferences_WithSpecialCharactersInInstanceName_ShouldWork()
    {
        // Arrange
        const string specialInstanceName = "Test Instance-Name_With.Special@Characters";

        // Act & Assert
        var action = () =>
        {
            var preferences = new InstancePreferences(specialInstanceName)
            {
                Opacity = 0.6
            };
            var retrievedOpacity = preferences.Opacity;
            retrievedOpacity.Should().Be(0.6);
        };

        action.Should().NotThrow();
    }

    [Fact]
    public void InstancePreferences_PropertyRoundTrip_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var preferences = new InstancePreferences(_testInstanceName);

        var testData = new
        {
            Opacity = 0.42,
            Left = 123,
            Top = 456,
            Width = 789,
            Height = 321,
            OutlookFolderName = "Calendar Test",
            OutlookFolderView = "Month View",
            OutlookFolderEntryId = "entry123",
            OutlookFolderStoreId = "store456",
            DisableEditing = true,
            ViewXml = "<xml><test>content</test></xml>"
        };

        // Act
        preferences.Opacity = testData.Opacity;
        preferences.Left = testData.Left;
        preferences.Top = testData.Top;
        preferences.Width = testData.Width;
        preferences.Height = testData.Height;
        preferences.OutlookFolderName = testData.OutlookFolderName;
        preferences.OutlookFolderView = testData.OutlookFolderView;
        preferences.OutlookFolderEntryId = testData.OutlookFolderEntryId;
        preferences.OutlookFolderStoreId = testData.OutlookFolderStoreId;
        preferences.DisableEditing = testData.DisableEditing;
        preferences.ViewXml = testData.ViewXml;

        // Assert - Retrieve all values and verify they match
        preferences.Opacity.Should().Be(testData.Opacity);
        preferences.Left.Should().Be(testData.Left);
        preferences.Top.Should().Be(testData.Top);
        preferences.Width.Should().Be(testData.Width);
        preferences.Height.Should().Be(testData.Height);
        preferences.OutlookFolderName.Should().Be(testData.OutlookFolderName);
        preferences.OutlookFolderView.Should().Be(testData.OutlookFolderView);
        preferences.OutlookFolderEntryId.Should().Be(testData.OutlookFolderEntryId);
        preferences.OutlookFolderStoreId.Should().Be(testData.OutlookFolderStoreId);
        preferences.DisableEditing.Should().Be(testData.DisableEditing);
        preferences.ViewXml.Should().Be(testData.ViewXml);
    }

    [Fact]
    public void InstancePreferences_ConcurrentAccess_ShouldNotCorruptData()
    {
        // Arrange
        const int numberOfThreads = 5;
        const int operationsPerThread = 10;
        var tasks = new List<Task>();
        var exceptions = new List<Exception>();

        // Act
        for (var i = 0; i < numberOfThreads; i++)
        {
            var threadId = i;
            var task = Task.Run(() =>
            {
                try
                {
                    for (var j = 0; j < operationsPerThread; j++)
                    {
                        var preferences = new InstancePreferences($"ConcurrentTest_{threadId}")
                        {
                            Left = threadId * 100 + j
                        };
                        var retrievedLeft = preferences.Left;
                        retrievedLeft.Should().Be(threadId * 100 + j);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        exceptions.Should().BeEmpty("No exceptions should occur during concurrent access");
    }

    [Fact]
    public void InstancePreferences_LargeStringValues_ShouldBeHandled()
    {
        // Arrange
        var preferences = new InstancePreferences(_testInstanceName);
        var largeXml = new string('X', 10000); // 10KB string
        var largeFolderName = new string('F', 1000); // 1KB string

        // Act & Assert
        var action = () =>
        {
            preferences.ViewXml = largeXml;
            preferences.OutlookFolderName = largeFolderName;

            preferences.ViewXml.Should().Be(largeXml);
            preferences.OutlookFolderName.Should().Be(largeFolderName);
        };

        action.Should().NotThrow();
    }

    [Theory]
    [InlineData(double.Epsilon)]
    [InlineData(0.000001)]
    [InlineData(0.999999)]
    [InlineData(1.0)]
    public void InstancePreferences_OpacityEdgeCases_ShouldBeHandled(double opacityValue)
    {
        // Arrange
        var preferences = new InstancePreferences(_testInstanceName)
        {
            // Act
            Opacity = opacityValue
        };
        var retrievedOpacity = preferences.Opacity;

        // Assert
        retrievedOpacity.Should().BeApproximately(opacityValue, 0.000001);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1000)]
    [InlineData(0)]
    [InlineData(10000)]
    [InlineData(int.MaxValue)]
    public void InstancePreferences_IntegerEdgeCases_ShouldBeHandled(int value)
    {
        // Arrange
        var preferences = new InstancePreferences(_testInstanceName);

        // Act & Assert
        var action = () =>
        {
            preferences.Left = value;
            preferences.Top = value;
            preferences.Width = value;
            preferences.Height = value;

            preferences.Left.Should().Be(value);
            preferences.Top.Should().Be(value);
            preferences.Width.Should().Be(value);
            preferences.Height.Should().Be(value);
        };

        action.Should().NotThrow();
    }

    public void Dispose()
    {
        // Clean up test registry keys
        try
        {
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree($@"Software\OotDTests", false);
        }
        catch
        {
            // Cleanup failure is not critical for tests
        }
    }
}
