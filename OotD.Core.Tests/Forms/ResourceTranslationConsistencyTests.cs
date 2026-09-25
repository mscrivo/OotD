namespace OotD.Core.Tests.Forms;

using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using OotD.Properties;

/// <summary>
///     Checks every translated string against its English original for mistakes that break the UI:
///     mismatched format placeholders (which throw in string.Format), escaped "\n" sequences that show
///     up literally instead of as line breaks, and differing numbers of line breaks.
/// </summary>
public partial class ResourceTranslationConsistencyTests
{
    public static TheoryData<string> TranslatedCultures => ["de", "es", "fr", "it", "ja", "pt-BR", "zh-CN"];

    [Theory]
    [MemberData(nameof(TranslatedCultures))]
    public void Translations_KeepPlaceholdersAndLineBreaksOfEnglish(string cultureName)
    {
        var english = GetStrings(CultureInfo.InvariantCulture);
        var translated = GetStrings(CultureInfo.GetCultureInfo(cultureName));

        translated.Should().NotBeEmpty();

        foreach (var (key, value) in translated)
        {
            english.Should().ContainKey(key, $"'{key}' in {cultureName} should also exist in English");
            var original = english[key];

            Placeholders(value).Should().BeEquivalentTo(Placeholders(original),
                $"'{key}' in {cultureName} should use the same format placeholders as English");
            value.Should().NotContain(@"\n",
                $"'{key}' in {cultureName} should use real line breaks, not an escaped \\n");
            value.Count(c => c == '\n').Should().Be(original.Count(c => c == '\n'),
                $"'{key}' in {cultureName} should have the same line breaks as English");
        }
    }

    private static Dictionary<string, string> GetStrings(CultureInfo culture)
    {
        var resourceSet = Resources.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
        resourceSet.Should().NotBeNull();

        return resourceSet!.Cast<DictionaryEntry>()
            .Where(entry => entry.Value is string)
            .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
    }

    private static List<string> Placeholders(string value)
    {
        return PlaceholderRegex().Matches(value).Select(match => match.Value).Order().ToList();
    }

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex PlaceholderRegex();
}
