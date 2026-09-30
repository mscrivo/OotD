namespace OotD.Core.Tests.Updater;

using System.IO;
using System.Security.Cryptography.X509Certificates;
using NetSparkle;

public sealed class AuthenticodeVerifierTests : IDisposable
{
    // The runtime's CoreLib carries an embedded Authenticode signature: CN=.NET, O=Microsoft Corporation.
    private static readonly string SignedFile = typeof(object).Assembly.Location;

    private readonly string _tempDirectory =
        Directory.CreateTempSubdirectory("OotD.AuthenticodeVerifierTests").FullName;

    public void Dispose()
    {
        Directory.Delete(_tempDirectory, true);
    }

    [Fact]
    public void HasValidSignature_WithSignedFile_ReturnsTrue()
    {
        AuthenticodeVerifier.HasValidSignature(SignedFile).Should().BeTrue();
    }

    [Fact]
    public void IsTrusted_WithSignedFileAndNoExpectedPublisher_ReturnsTrue()
    {
        AuthenticodeVerifier.IsTrusted(SignedFile, null).Should().BeTrue();
    }

    [Theory]
    [InlineData("SignPath Foundation")]
    [InlineData("Microsoft Corporation")] // matches O but not CN
    [InlineData(".NET")] // matches CN but not O
    public void IsTrusted_WithSignedFileFromAnotherPublisher_ReturnsFalse(string expectedPublisher)
    {
        AuthenticodeVerifier.IsTrusted(SignedFile, expectedPublisher).Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_WithTamperedSignedFile_ReturnsFalse()
    {
        var tampered = Path.Combine(_tempDirectory, "tampered.dll");
        var bytes = File.ReadAllBytes(SignedFile);
        bytes[bytes.Length / 2] ^= 0xFF;
        File.WriteAllBytes(tampered, bytes);

        AuthenticodeVerifier.IsTrusted(tampered, null).Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_WithUnsignedFile_ReturnsFalse()
    {
        var unsigned = Path.Combine(_tempDirectory, "unsigned.exe");
        File.WriteAllText(unsigned, "not a signed installer");

        AuthenticodeVerifier.IsTrusted(unsigned, null).Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_WithMissingFile_ReturnsFalse()
    {
        AuthenticodeVerifier.IsTrusted(Path.Combine(_tempDirectory, "missing.exe"), null).Should().BeFalse();
    }

    [Theory]
    [InlineData("CN=SignPath Foundation, O=SignPath Foundation, L=Lewes, S=Delaware, C=US", true)]
    [InlineData("CN=SignPath Foundation, O=SignPath Foundation", true)]
    [InlineData("CN=SignPath Foundation, O=Someone Else", false)]
    [InlineData("CN=Someone Else, O=SignPath Foundation", false)]
    [InlineData("CN=SignPath Foundation", false)]
    [InlineData("O=SignPath Foundation", false)]
    [InlineData("CN=signpath foundation, O=signpath foundation", false)]
    [InlineData("CN=SignPath Foundation Evil, O=SignPath Foundation", false)]
    [InlineData("CN=SignPath Foundation, CN=Evil, O=SignPath Foundation", false)]
    public void IsIssuedTo_RequiresCommonNameAndOrganizationToMatchExactly(string subject, bool expected)
    {
        AuthenticodeVerifier.IsIssuedTo(new X500DistinguishedName(subject), "SignPath Foundation")
            .Should().Be(expected);
    }
}
