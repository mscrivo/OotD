namespace OotD.Core.Tests.Updater;

using NetSparkle;

public class InstallerBatchTests
{
    private const string Installer = @"""C:\Users\Jane Doe\AppData\Local\Temp\ootd-5.6.0.exe"" /silent";
    private const string AppDir = @"C:\Program Files\Outlook on the Desktop";
    private const string AppExe = AppDir + @"\OotD.x64.exe";

    private static string[] Lines(string batch) =>
        batch.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

    [Fact]
    public void BuildInstallerBatch_RelaunchesTheExecutableAfterTheInstallerFinishes()
    {
        var batch = SparkleBatch(AppExe, []);

        Lines(batch).Should().Equal(
            Installer,
            @"cd /d ""C:\Program Files\Outlook on the Desktop""",
            @"start """" ""C:\Program Files\Outlook on the Desktop\OotD.x64.exe""");
    }

    [Fact]
    public void BuildInstallerBatch_PassesTheOriginalArgumentsThrough()
    {
        var batch = SparkleBatch(AppExe, ["-d", "two words"]);

        Lines(batch)[^1].Should().Be(
            @"start """" ""C:\Program Files\Outlook on the Desktop\OotD.x64.exe"" -d ""two words""");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void BuildInstallerBatch_WithoutRelaunchPath_OnlyRunsTheInstaller(string? relaunchPath)
    {
        Lines(SparkleBatch(relaunchPath, ["-d"])).Should().Equal(Installer);
    }

    [Fact]
    public void BuildInstallerBatch_NeverRelaunchesTheDll()
    {
        SparkleBatch(AppExe, []).Should().NotContain(".dll");
    }

    private static string SparkleBatch(string? relaunchPath, string[] arguments) =>
        Sparkle.BuildInstallerBatch(Installer, relaunchPath, arguments, AppDir);
}
