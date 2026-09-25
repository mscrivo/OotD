using System.Reflection;
using System.Runtime.InteropServices;
using OotD.Properties;
using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class OutlookStartupDiagnosticsTests
{
    [Theory]
    [InlineData(0x80080005, OutlookStartupProblem.ServerExecFailure)]
    [InlineData(0x80040154, OutlookStartupProblem.ClassNotRegistered)]
    [InlineData(0x80040155, OutlookStartupProblem.RegistrationBroken)]
    [InlineData(0x8002801D, OutlookStartupProblem.RegistrationBroken)]
    [InlineData(0x8002802B, OutlookStartupProblem.RegistrationBroken)]
    [InlineData(0x80029C4A, OutlookStartupProblem.RegistrationBroken)]
    [InlineData(0x800700C1, OutlookStartupProblem.RegistrationBroken)]
    [InlineData(0x80020009, OutlookStartupProblem.Unknown)] // DISP_E_EXCEPTION
    public void Classify_WithComException_MapsHResult(uint hresult, OutlookStartupProblem expected)
    {
        var exception = new COMException("COM failure", unchecked((int)hresult));

        OutlookStartupDiagnostics.Classify(exception).Should().Be(expected);
    }

    [Fact]
    public void Classify_WithInvalidCastException_ReturnsRegistrationBroken()
    {
        // What a failed QueryInterface for Outlook's Application interface surfaces as.
        var exception = new InvalidCastException("Unable to cast COM object of type 'System.__ComObject'");

        OutlookStartupDiagnostics.Classify(exception).Should().Be(OutlookStartupProblem.RegistrationBroken);
    }

    [Fact]
    public void Classify_WithBadImageFormatFromComActivation_ReturnsRegistrationBroken()
    {
        // Creating Outlook.Application when its registered server isn't a valid executable.
        var exception = new BadImageFormatException("not a valid Win32 application") { HResult = unchecked((int)0x800700C1) };

        OutlookStartupDiagnostics.Classify(exception).Should().Be(OutlookStartupProblem.RegistrationBroken);
    }

    [Fact]
    public void Classify_WithWrappedComException_UsesInnerException()
    {
        var exception = new TargetInvocationException(new COMException("COM failure", unchecked((int)0x80080005)));

        OutlookStartupDiagnostics.Classify(exception).Should().Be(OutlookStartupProblem.ServerExecFailure);
    }

    [Fact]
    public void Classify_WithUnrelatedException_ReturnsUnknown()
    {
        OutlookStartupDiagnostics.Classify(new InvalidOperationException("boom"))
            .Should().Be(OutlookStartupProblem.Unknown);
    }

    [Fact]
    public void GetMessage_ForKnownProblem_ReturnsSpecificGuidance()
    {
        var exception = new COMException("COM failure", unchecked((int)0x80080005));

        OutlookStartupDiagnostics.GetMessage(exception, newOutlookEnabled: false)
            .Should().Be(Resources.OutlookServerExecFailure);
    }

    [Fact]
    public void GetMessage_ForUnknownProblem_IncludesExceptionMessage()
    {
        var message = OutlookStartupDiagnostics.GetMessage(new InvalidOperationException("boom"), false);

        message.Should().StartWith(Resources.ErrorInitializingApp).And.EndWith("boom");
    }

    [Fact]
    public void GetMessage_WhenNewOutlookEnabled_AppendsHint()
    {
        var exception = new COMException("COM failure", unchecked((int)0x80040154));

        var message = OutlookStartupDiagnostics.GetMessage(exception, newOutlookEnabled: true);

        message.Should().StartWith(Resources.OutlookClassNotRegistered)
            .And.EndWith(Resources.NewOutlookEnabledHint);
    }

    [Fact]
    public void WithNewOutlookHint_WhenNotEnabled_ReturnsMessageUnchanged()
    {
        OutlookStartupDiagnostics.WithNewOutlookHint("message", newOutlookEnabled: false).Should().Be("message");
    }
}
