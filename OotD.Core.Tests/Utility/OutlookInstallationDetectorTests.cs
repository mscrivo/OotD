using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class OutlookInstallationDetectorTests
{
    [Fact]
    public void Detect_WhenNoOfficeVersionsInstalled_ReturnsOfficeNotInstalled()
    {
        var result = OutlookInstallationDetector.Detect(new FakeOutlookEnvironment());

        result.Error.Should().Be(OutlookDetectionError.OfficeNotInstalled);
        result.IsUsable.Should().BeFalse();
    }

    [Fact]
    public void Detect_WhenOnlyNonNumericOfficeKeys_ReturnsOfficeNotInstalled()
    {
        // The environment already filters unparsable keys, so an empty version list is the input.
        var env = new FakeOutlookEnvironment { Versions = [] };

        OutlookInstallationDetector.Detect(env).Error.Should().Be(OutlookDetectionError.OfficeNotInstalled);
    }

    [Theory]
    [InlineData(9)] // Office 2000
    [InlineData(12)] // Office 2007
    [InlineData(13.9)] // just below the supported boundary
    public void Detect_WhenVersionBelowMinimum_ReturnsUnsupportedVersion(double version)
    {
        var env = new FakeOutlookEnvironment { Versions = [version] };

        OutlookInstallationDetector.Detect(env).Error.Should().Be(OutlookDetectionError.UnsupportedVersion);
    }

    [Fact]
    public void Detect_WhenVersionBelowMinimumAndNoInstallPath_PrefersVersionError()
    {
        // Version is checked before the install path, so the version error wins.
        var env = new FakeOutlookEnvironment { Versions = [12], InstallPath = null };

        OutlookInstallationDetector.Detect(env).Error.Should().Be(OutlookDetectionError.UnsupportedVersion);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Detect_WhenSupportedButNoInstallPath_ReturnsLocationNotFound(string? installPath)
    {
        var env = new FakeOutlookEnvironment { Versions = [16], InstallPath = installPath };

        OutlookInstallationDetector.Detect(env).Error.Should().Be(OutlookDetectionError.OutlookLocationNotFound);
    }

    [Fact]
    public void Detect_WhenInstallPathPresentButExeMissing_ReturnsExecutableNotFound()
    {
        var env = new FakeOutlookEnvironment
        {
            Versions = [16],
            InstallPath = @"C:\Program Files\Microsoft Office\root\Office16",
            ExeExists = false
        };

        OutlookInstallationDetector.Detect(env).Error.Should().Be(OutlookDetectionError.OutlookExecutableNotFound);
    }

    [Theory]
    [InlineData("x64")]
    [InlineData("x86")]
    public void Detect_WhenUsableInstallation_ReturnsRecordedBitness(string bitness)
    {
        var env = new FakeOutlookEnvironment
        {
            Versions = [16],
            InstallPath = @"C:\Office",
            ExeExists = true,
            Bitness = _ => bitness
        };

        var result = OutlookInstallationDetector.Detect(env);

        result.IsUsable.Should().BeTrue();
        result.Error.Should().Be(OutlookDetectionError.None);
        result.Bitness.Should().Be(bitness);
    }

    [Fact]
    public void Detect_WhenBitnessNotRecorded_DefaultsToX86()
    {
        var env = new FakeOutlookEnvironment
        {
            Versions = [16],
            InstallPath = @"C:\Office",
            ExeExists = true,
            Bitness = _ => null
        };

        OutlookInstallationDetector.Detect(env).Bitness.Should().Be("x86");
    }

    [Fact]
    public void Detect_WithMultipleVersions_UsesTheNewestForBitness()
    {
        var queried = new List<double>();
        var env = new FakeOutlookEnvironment
        {
            Versions = [14, 15, 16],
            InstallPath = @"C:\Office",
            ExeExists = true,
            Bitness = v =>
            {
                queried.Add(v);
                return v == 16 ? "x64" : null;
            }
        };

        var result = OutlookInstallationDetector.Detect(env);

        result.Bitness.Should().Be("x64");
        queried.Should().StartWith(16); // newest queried first
    }

    [Fact]
    public void Detect_WhenNewestVersionLacksBitness_FallsBackToOlderVersionKey()
    {
        var env = new FakeOutlookEnvironment
        {
            Versions = [16],
            InstallPath = @"C:\Office",
            ExeExists = true,
            Bitness = v => v == 15 ? "x64" : null
        };

        OutlookInstallationDetector.Detect(env).Bitness.Should().Be("x64");
    }

    [Fact]
    public void IsUsable_TracksTheErrorState()
    {
        new OutlookInstallation(OutlookDetectionError.None, "x64").IsUsable.Should().BeTrue();
        new OutlookInstallation(OutlookDetectionError.OfficeNotInstalled, "").IsUsable.Should().BeFalse();
    }

    private sealed class FakeOutlookEnvironment : IOutlookEnvironment
    {
        public List<double> Versions { get; init; } = [];
        public string? InstallPath { get; init; }
        public bool ExeExists { get; init; }
        public Func<double, string?> Bitness { get; init; } = _ => null;

        public IReadOnlyList<double> GetInstalledOfficeVersions() => Versions;
        public string? GetOutlookInstallPath() => InstallPath;
        public bool OutlookExecutableExists(string installPath) => ExeExists;
        public string? GetBitness(double officeVersion) => Bitness(officeVersion);
    }
}
