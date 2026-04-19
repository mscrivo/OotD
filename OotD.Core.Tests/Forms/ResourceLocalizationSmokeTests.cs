namespace OotD.Core.Tests.Forms;

using System.Drawing;
using System.Globalization;
using OotD.Properties;

public class ResourceLocalizationSmokeTests
{
    private static readonly string[] _supportedCultures = ["en-US", "es-ES", "de-DE", "fr-FR", "it-IT"];
    private static readonly string[] _requiredStringKeys =
    [
        "RestoreDefaults",
        "RestoreDefaultsConfirmation",
        "Required",
        "RenameInstance",
        "RenameThisInstance",
        "RemoveThisInstance",
        "MoveToVirtualDesktop",
        "OutlookViews",
        "DefaultView",
        "TodoList"
    ];

    [Theory]
    [MemberData(nameof(GetSupportedCultures))]
    public void ResourceManager_GetString_ForRequiredLocalizedKeys_ReturnsValues(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);

        foreach (var key in _requiredStringKeys)
        {
            var value = Resources.ResourceManager.GetString(key, culture);
            value.Should().NotBeNullOrWhiteSpace($"resource '{key}' should exist for culture {cultureName}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonStringResourceCultures))]
    public void ResourceManager_GetObject_ForBitmapAndIconResources_ReturnsExpectedTypes(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);

        Resources.ResourceManager.GetObject("Today", culture).Should().BeOfType<Bitmap>();
        Resources.ResourceManager.GetObject("Month", culture).Should().BeOfType<Bitmap>();
        Resources.ResourceManager.GetObject("_1", culture).Should().BeOfType<Icon>();
    }

    public static TheoryData<string> GetSupportedCultures()
    {
        var data = new TheoryData<string>();
        foreach (var culture in _supportedCultures)
        {
            data.Add(culture);
        }

        return data;
    }

    public static TheoryData<string> GetNonStringResourceCultures()
    {
        var data = new TheoryData<string>();
        foreach (var culture in _supportedCultures.Where(c => c != "en-US"))
        {
            data.Add(culture);
        }

        return data;
    }
}
