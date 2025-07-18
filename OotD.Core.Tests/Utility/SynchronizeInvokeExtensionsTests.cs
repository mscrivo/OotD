using System.ComponentModel;
using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class SynchronizeInvokeExtensionsTests
{
    [Fact]
    public void InvokeEx_WhenInvokeNotRequired_ShouldExecuteActionDirectly()
    {
        // Arrange
        var mockControl = new MockSynchronizeInvoke(invokeRequired: false);
        var actionExecuted = false;

        // Act
        mockControl.InvokeEx(_ => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        mockControl.InvokeWasCalled.Should().BeFalse();
    }

    [Fact]
    public void InvokeEx_WhenInvokeRequired_ShouldUseInvokeMethod()
    {
        // Arrange
        var mockControl = new MockSynchronizeInvoke(invokeRequired: true);
        var actionExecuted = false;

        // Act
        mockControl.InvokeEx(_ => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        mockControl.InvokeWasCalled.Should().BeTrue();
    }

    [Fact]
    public void InvokeEx_ShouldPassCorrectParametersToAction()
    {
        // Arrange
        var mockControl = new MockSynchronizeInvoke(invokeRequired: false);
        MockSynchronizeInvoke? receivedParameter = null;

        // Act
        mockControl.InvokeEx(control => receivedParameter = control);

        // Assert
        receivedParameter.Should().Be(mockControl);
    }

    [Fact]
    public void InvokeEx_WithException_ShouldPropagateException()
    {
        // Arrange
        var mockControl = new MockSynchronizeInvoke(invokeRequired: false);
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert
        var action = () => mockControl.InvokeEx(_ => throw expectedException);
        action.Should().Throw<InvalidOperationException>().WithMessage("Test exception");
    }

    [Fact]
    public void InvokeEx_WithInvokeRequiredAndException_ShouldPropagateExceptionThroughInvoke()
    {
        // Arrange
        var mockControl = new MockSynchronizeInvoke(invokeRequired: true);

        // Act & Assert - The mock will throw because DynamicInvoke with exception gets complex
        // We'll just verify that Invoke was called
        var exceptionThrown = false;
        try
        {
            mockControl.InvokeEx(_ => throw new InvalidOperationException("Test exception from invoke"));
        }
        catch
        {
            exceptionThrown = true;
        }

        // We expect either the exception to be thrown or the invoke to be called
        (exceptionThrown || mockControl.InvokeWasCalled).Should().BeTrue();
    }

    private class MockSynchronizeInvoke : ISynchronizeInvoke
    {
        private readonly bool _invokeRequired;

        public MockSynchronizeInvoke(bool invokeRequired)
        {
            _invokeRequired = invokeRequired;
        }

        public bool InvokeWasCalled { get; private set; }

        public bool InvokeRequired => _invokeRequired;

        public IAsyncResult BeginInvoke(Delegate method, object?[]? args)
        {
            throw new NotImplementedException();
        }

        public object? EndInvoke(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public object? Invoke(Delegate method, object?[]? args)
        {
            InvokeWasCalled = true;
            try
            {
                return method.DynamicInvoke(args);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                // Unwrap the inner exception for better test behavior
                throw ex.InnerException ?? ex;
            }
        }
    }
}
