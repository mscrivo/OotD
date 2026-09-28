using Microsoft.Win32;
using OotD.Utility;
using static OotD.Utility.UnsafeNativeMethods;

namespace OotD.Core.Tests.Utility;

/// <summary>
///     Exercises VirtualDesktopManager against in-process fakes of the virtual desktop COM APIs and a
///     throwaway registry key, so every fallback path runs deterministically without a real desktop session.
/// </summary>
public class VirtualDesktopManagerFakeTests : IDisposable
{
    private readonly IVirtualDesktopManager? _originalManager;
    private readonly IVirtualDesktopManagerInternal? _originalManagerInternal;
    private readonly string _originalRegistryPath;
    private readonly string _testRegistryPath = $@"Software\OotDTests\{Guid.NewGuid():N}\VirtualDesktops";
    private readonly FakeDesktopManager _manager = new();

    public VirtualDesktopManagerFakeTests()
    {
        _originalManager = VirtualDesktopManager.DesktopManager;
        _originalManagerInternal = VirtualDesktopManager.DesktopManagerInternal;
        _originalRegistryPath = VirtualDesktopManager.RegistryPath;

        VirtualDesktopManager.DesktopManager = _manager;
        VirtualDesktopManager.DesktopManagerInternal = null;
        VirtualDesktopManager.RegistryPath = _testRegistryPath;
    }

    public void Dispose()
    {
        VirtualDesktopManager.DesktopManager = _originalManager;
        VirtualDesktopManager.DesktopManagerInternal = _originalManagerInternal;
        VirtualDesktopManager.RegistryPath = _originalRegistryPath;
        Registry.CurrentUser.DeleteSubKeyTree(_testRegistryPath.Substring(0, _testRegistryPath.LastIndexOf('\\')),
            false);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void WithoutManager_EveryOperationFallsBackSafely()
    {
        VirtualDesktopManager.DesktopManager = null;

        VirtualDesktopManager.IsVirtualDesktopSupported.Should().BeFalse();
        VirtualDesktopManager.GetWindowDesktopId(1).Should().BeNull();
        VirtualDesktopManager.MoveWindowToDesktop(1, Guid.NewGuid()).Should().BeFalse();
        VirtualDesktopManager.GetVirtualDesktops().Should().BeEmpty();
        VirtualDesktopManager.GetCurrentDesktopId().Should().BeNull();
        VirtualDesktopManager.IsWindowOnCurrentDesktop(1).Should().BeTrue();
    }

    [Fact]
    public void GetWindowDesktopId_ReturnsIdOnlyForSuccessfulNonEmptyResult()
    {
        var desktopId = Guid.NewGuid();
        VirtualDesktopManager.IsVirtualDesktopSupported.Should().BeTrue();

        _manager.GetWindowDesktopIdImpl = _ => (0, desktopId);
        VirtualDesktopManager.GetWindowDesktopId(1).Should().Be(desktopId);

        _manager.GetWindowDesktopIdImpl = _ => (1, desktopId);
        VirtualDesktopManager.GetWindowDesktopId(1).Should().BeNull();

        _manager.GetWindowDesktopIdImpl = _ => (0, Guid.Empty);
        VirtualDesktopManager.GetWindowDesktopId(1).Should().BeNull();

        _manager.GetWindowDesktopIdImpl = _ => throw new InvalidOperationException();
        VirtualDesktopManager.GetWindowDesktopId(1).Should().BeNull();
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(unchecked((int)0x80070057), false)]
    public void MoveWindowToDesktop_ReportsWhetherComCallSucceeded(int hresult, bool expected)
    {
        var desktopId = Guid.NewGuid();
        _manager.MoveResult = hresult;

        VirtualDesktopManager.MoveWindowToDesktop(42, desktopId).Should().Be(expected);
        _manager.MovedTo.Should().Be(((IntPtr)42, desktopId));
    }

    [Fact]
    public void MoveWindowToDesktop_WhenComCallThrows_ReturnsFalse()
    {
        _manager.ThrowOnCall = true;

        VirtualDesktopManager.MoveWindowToDesktop(42, Guid.NewGuid()).Should().BeFalse();
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public void IsWindowOnCurrentDesktop_MapsHresult(int hresult, bool expected)
    {
        _manager.IsOnCurrentResult = hresult;

        VirtualDesktopManager.IsWindowOnCurrentDesktop(1).Should().Be(expected);
    }

    [Fact]
    public void IsWindowOnCurrentDesktop_WhenComCallThrows_AssumesCurrentDesktop()
    {
        _manager.ThrowOnCall = true;

        VirtualDesktopManager.IsWindowOnCurrentDesktop(1).Should().BeTrue();
    }

    [Fact]
    public void GetVirtualDesktops_PrefersInternalApiAndUsesDesktopNamesWhenAvailable()
    {
        var named = new FakeNamedDesktop(Guid.NewGuid(), "Work");
        var unnamed = new FakeNamedDesktop(Guid.NewGuid(), "");
        var nameThrows = new FakeNamedDesktop(Guid.NewGuid(), null);
        var plain = new FakeDesktop(Guid.NewGuid());
        VirtualDesktopManager.DesktopManagerInternal =
            new FakeDesktopManagerInternal(named, "not a desktop", unnamed, nameThrows, plain);

        var desktops = VirtualDesktopManager.GetVirtualDesktops();

        desktops.Select(desktop => (desktop.Id, desktop.Name)).Should().Equal(
            (named.Id, "Work"),
            (unnamed.Id, "Desktop 3"),
            (nameThrows.Id, "Desktop 4"),
            (plain.Id, "Desktop 5"));
    }

    [Fact]
    public void GetVirtualDesktops_WhenInternalApiHasNoDesktops_FallsBackToRegistry()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        VirtualDesktopManager.DesktopManagerInternal = new FakeDesktopManagerInternal { ReturnNullDesktops = true };
        using (var key = Registry.CurrentUser.CreateSubKey(_testRegistryPath))
        {
            key.SetValue("VirtualDesktopIDs", first.ToByteArray().Concat(second.ToByteArray()).ToArray(),
                RegistryValueKind.Binary);
            using var named = key.CreateSubKey($@"Desktops\{{{second}}}");
            named.SetValue("Name", "Personal");
            key.CreateSubKey($@"Desktops\{{{Guid.NewGuid()}}}").Dispose();
            key.CreateSubKey(@"Desktops\not-a-guid").Dispose();
        }

        var desktops = VirtualDesktopManager.GetVirtualDesktops();

        desktops.Select(desktop => (desktop.Id, desktop.Name)).Should().Equal(
            (first, "Desktop 1"),
            (second, "Personal"));
    }

    [Fact]
    public void GetVirtualDesktops_WhenInternalApiThrows_FallsBackToRegistry()
    {
        var desktopId = Guid.NewGuid();
        VirtualDesktopManager.DesktopManagerInternal = new FakeDesktopManagerInternal { ThrowOnCall = true };
        using (var key = Registry.CurrentUser.CreateSubKey(_testRegistryPath))
        {
            key.SetValue("VirtualDesktopIDs", desktopId.ToByteArray(), RegistryValueKind.Binary);
        }

        var desktops = VirtualDesktopManager.GetVirtualDesktops();

        desktops.Should().ContainSingle().Which.Id.Should().Be(desktopId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetVirtualDesktops_WithoutRegistryData_FallsBackToVisibleWindows(bool createEmptyKey)
    {
        if (createEmptyKey)
        {
            using var key = Registry.CurrentUser.CreateSubKey(_testRegistryPath);
            key.SetValue("VirtualDesktopIDs", Array.Empty<byte>(), RegistryValueKind.Binary);
        }

        var desktopId = Guid.NewGuid();
        _manager.GetWindowDesktopIdImpl = _ => (0, desktopId);
        using var visibleWindow = CreateVisibleWindow();

        var desktops = VirtualDesktopManager.GetVirtualDesktops();

        desktops.Should().ContainSingle();
        desktops[0].Id.Should().Be(desktopId);
        desktops[0].Name.Should().Be("Desktop 1");
    }

    [Fact]
    public void GetCurrentDesktopId_PrefersRegistryValue()
    {
        var desktopId = Guid.NewGuid();
        VirtualDesktopManager.DesktopManagerInternal = new FakeDesktopManagerInternal { ThrowOnCall = true };
        using (var key = Registry.CurrentUser.CreateSubKey(_testRegistryPath))
        {
            key.SetValue("CurrentVirtualDesktop", desktopId.ToByteArray(), RegistryValueKind.Binary);
        }

        VirtualDesktopManager.GetCurrentDesktopId().Should().Be(desktopId);
    }

    [Fact]
    public void GetCurrentDesktopId_WithMalformedRegistryValue_UsesInternalApi()
    {
        var current = new FakeDesktop(Guid.NewGuid());
        VirtualDesktopManager.DesktopManagerInternal = new FakeDesktopManagerInternal { CurrentDesktop = current };
        using (var key = Registry.CurrentUser.CreateSubKey(_testRegistryPath))
        {
            key.SetValue("CurrentVirtualDesktop", new byte[] { 1, 2, 3 }, RegistryValueKind.Binary);
        }

        VirtualDesktopManager.GetCurrentDesktopId().Should().Be(current.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetCurrentDesktopId_WithoutInternalApiResult_FallsBackToShellOrForegroundWindow(bool internalThrows)
    {
        var desktopId = Guid.NewGuid();
        VirtualDesktopManager.DesktopManagerInternal = new FakeDesktopManagerInternal { ThrowOnCall = internalThrows };
        _manager.GetWindowDesktopIdImpl = _ => (0, desktopId);

        var result = VirtualDesktopManager.GetCurrentDesktopId();

        var hasFallbackWindow = GetShellWindow() != IntPtr.Zero || GetForegroundWindow() != IntPtr.Zero;
        result.Should().Be(hasFallbackWindow ? desktopId : null);
    }

    [Fact]
    public void GetCurrentDesktopId_WhenNoWindowReportsADesktop_ReturnsNull()
    {
        _manager.GetWindowDesktopIdImpl = _ => (0, Guid.Empty);

        VirtualDesktopManager.GetCurrentDesktopId().Should().BeNull();
    }

    private static Form CreateVisibleWindow()
    {
        var form = new Form
        {
            ShowInTaskbar = false,
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            Bounds = new Rectangle(-32000, -32000, 10, 10),
            Opacity = 0
        };
        form.Show();
        return form;
    }

    private sealed class FakeDesktopManager : IVirtualDesktopManager
    {
        public Func<IntPtr, (int Result, Guid Id)> GetWindowDesktopIdImpl { get; set; } = _ => (1, Guid.Empty);
        public int IsOnCurrentResult { get; set; }
        public int MoveResult { get; set; }
        public bool ThrowOnCall { get; set; }
        public (IntPtr Window, Guid DesktopId)? MovedTo { get; private set; }

        public int IsWindowOnCurrentVirtualDesktop(IntPtr topLevelWindow)
        {
            ThrowIfRequested();
            return IsOnCurrentResult;
        }

        public int GetWindowDesktopId(IntPtr topLevelWindow, out Guid desktopId)
        {
            (var result, desktopId) = GetWindowDesktopIdImpl(topLevelWindow);
            return result;
        }

        public int MoveWindowToDesktop(IntPtr topLevelWindow, Guid desktopId)
        {
            ThrowIfRequested();
            MovedTo = (topLevelWindow, desktopId);
            return MoveResult;
        }

        private void ThrowIfRequested()
        {
            if (ThrowOnCall)
            {
                throw new InvalidOperationException("COM call failed");
            }
        }
    }

    private sealed class FakeDesktopManagerInternal(params object[] desktops) : IVirtualDesktopManagerInternal
    {
        public bool ThrowOnCall { get; init; }
        public bool ReturnNullDesktops { get; init; }
        public IVirtualDesktop? CurrentDesktop { get; init; }

        public IObjectArray GetDesktops(IntPtr hWndOrMon)
        {
            ThrowIfRequested();
            return ReturnNullDesktops ? null! : new FakeObjectArray(desktops);
        }

        public IVirtualDesktop GetCurrentDesktop(IntPtr hWndOrMon)
        {
            ThrowIfRequested();
            return CurrentDesktop!;
        }

        private void ThrowIfRequested()
        {
            if (ThrowOnCall)
            {
                throw new InvalidOperationException("COM call failed");
            }
        }

        public int GetCount(IntPtr hWndOrMon) => throw new NotSupportedException();
        public void MoveViewToDesktop(IntPtr pView, IVirtualDesktop pDesktop) => throw new NotSupportedException();
        public bool CanViewMoveDesktops(IntPtr pView) => throw new NotSupportedException();

        public IVirtualDesktop GetAdjacentDesktop(IVirtualDesktop pDesktopReference, int uDirection) =>
            throw new NotSupportedException();

        public void SwitchDesktop(IntPtr hWndOrMon, IVirtualDesktop pDesktop) => throw new NotSupportedException();
        public IVirtualDesktop CreateDesktop(IntPtr hWndOrMon) => throw new NotSupportedException();

        public void MoveDesktop(IVirtualDesktop pDesktop, IntPtr hWndOrMon, int nIndex) =>
            throw new NotSupportedException();

        public void RemoveDesktop(IVirtualDesktop pRemove, IVirtualDesktop pFallbackDesktop) =>
            throw new NotSupportedException();

        public IVirtualDesktop FindDesktop(ref Guid desktopId) => throw new NotSupportedException();

        public void GetDesktopSwitchIncludeExcludeViews(IVirtualDesktop pDesktop, out IObjectArray ppDesktops1,
            out IObjectArray ppDesktops2) => throw new NotSupportedException();
    }

    private sealed class FakeObjectArray(object[] items) : IObjectArray
    {
        public void GetCount(out int pctInfo) => pctInfo = items.Length;

        public void GetAt(int iIndex, ref Guid riid, out object ppvObject) => ppvObject = items[iIndex];
    }

    private class FakeDesktop(Guid id) : IVirtualDesktop
    {
        public Guid Id => id;

        public bool IsViewVisible(IntPtr pView) => throw new NotSupportedException();

        public void GetID(out Guid pGuid) => pGuid = id;
    }

    /// <summary>A Windows 11 desktop; a null name makes <see cref="GetName" /> throw.</summary>
    private sealed class FakeNamedDesktop(Guid id, string? name) : FakeDesktop(id), IVirtualDesktop2
    {
        public string GetName() => name ?? throw new InvalidOperationException("Name unavailable");
    }
}
