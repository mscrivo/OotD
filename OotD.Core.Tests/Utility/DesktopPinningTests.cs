using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class DesktopPinningTests
{
    [Fact]
    public void Initialize_ShouldNotThrow()
    {
        // Act & Assert - creating the hidden helper window must never throw.
        var action = () => DesktopPinning.Initialize();
        action.Should().NotThrow();
    }

    [Fact]
    public void GetPinnedAnchorWindow_ShouldReturnIntPtr()
    {
        // Arrange
        DesktopPinning.Initialize();

        // Act & Assert - returns the helper handle or IntPtr.Zero (fall back to HWND_BOTTOM).
        var action = () => DesktopPinning.GetPinnedAnchorWindow();
        action.Should().NotThrow();
    }

    [Fact]
    public void Initialize_CreatesHelperWindowOnceAndReusesIt()
    {
        DesktopPinning.Initialize();
        var anchor = DesktopPinning.GetPinnedAnchorWindow();

        DesktopPinning.Initialize();

        anchor.Should().NotBe(IntPtr.Zero);
        DesktopPinning.GetPinnedAnchorWindow().Should().Be(anchor);
        var className = new StringBuilder(64);
        UnsafeNativeMethods.GetClassName(anchor, className, className.Capacity);
        className.ToString().Should().Be("OotDDesktopAnchor");
    }

    [Fact]
    public void AnchorWndProc_BlocksZOrderChanges()
    {
        var windowPos = new UnsafeNativeMethods.WINDOWPOS { flags = 0x0010 };
        var lParam = Marshal.AllocHGlobal(Marshal.SizeOf<UnsafeNativeMethods.WINDOWPOS>());
        try
        {
            Marshal.StructureToPtr(windowPos, lParam, false);

            var result = Invoke<nint>("AnchorWndProc", IntPtr.Zero, 0x46u, (nint)0, (nint)lParam);

            result.Should().Be(0);
            Marshal.PtrToStructure<UnsafeNativeMethods.WINDOWPOS>(lParam).flags
                .Should().Be(0x0010 | UnsafeNativeMethods.SWP_NOZORDER);
        }
        finally
        {
            Marshal.FreeHGlobal(lParam);
        }
    }

    [Fact]
    public void AnchorWndProc_PassesOtherMessagesToDefaultProcedure()
    {
        DesktopPinning.Initialize();

        var result = Invoke<nint>("AnchorWndProc", DesktopPinning.GetPinnedAnchorWindow(), 0u /* WM_NULL */,
            (nint)0, (nint)0);

        result.Should().Be(0);
    }

    [Fact]
    public void SendWindowToBack_InsertsWindowBehindTheAnchor()
    {
        using var form = new Form();

        var action = () => UnsafeNativeMethods.SendWindowToBack(form);

        action.Should().NotThrow();
        DesktopPinning.GetPinnedAnchorWindow().Should().NotBe(IntPtr.Zero);
    }

    [Fact]
    public void SendWindowToTop_MakesWindowTopMost()
    {
        using var form = new Form();

        UnsafeNativeMethods.SendWindowToTop(form);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOPMOST = 0x8;
        (GetWindowLong(form.Handle, GWL_EXSTYLE) & WS_EX_TOPMOST).Should().Be(WS_EX_TOPMOST);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    private static T Invoke<T>(string methodName, params object[] args)
    {
        var method = typeof(DesktopPinning).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!;
        return (T)method.Invoke(null, args)!;
    }
}
