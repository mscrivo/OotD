using Microsoft.Win32;
using OotD.Preferences;

namespace OotD.Core.Tests.Preferences;

public class PreferencesRegistryTests : IDisposable
{
    private readonly string _originalRootPath = PreferencesRegistry.RootPath;
    private readonly string _testRootPath = $@"Software\OotDTests\{Guid.NewGuid():N}";

    public PreferencesRegistryTests()
    {
        PreferencesRegistry.RootPath = _testRootPath;
    }

    [Fact]
    public void GetSubKeyNames_WhenRootKeyIsMissing_ReturnsEmptyWithoutCreatingIt()
    {
        PreferencesRegistry.GetSubKeyNames().Should().BeEmpty();

        using var rootKey = Registry.CurrentUser.OpenSubKey(_testRootPath);
        rootKey.Should().BeNull();
    }

    [Fact]
    public void GetSubKeyNames_ReturnsEverySubKeyUnderTheRoot()
    {
        using (var rootKey = Registry.CurrentUser.CreateSubKey(_testRootPath))
        {
            rootKey.CreateSubKey("Work")!.Dispose();
            rootKey.CreateSubKey("Home")!.Dispose();
        }

        PreferencesRegistry.GetSubKeyNames().Should().BeEquivalentTo("Work", "Home");
    }

    public void Dispose()
    {
        PreferencesRegistry.RootPath = _originalRootPath;
        Registry.CurrentUser.DeleteSubKeyTree(_testRootPath, false);
        GC.SuppressFinalize(this);
    }
}
