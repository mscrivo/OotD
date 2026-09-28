using System.Reflection;
using Microsoft.Win32;
using OotD.Forms;
using OotD.Utility;
using static OotD.Utility.UnsafeNativeMethods;

namespace OotD.Core.Tests.Forms;

/// <summary>
///     Covers how the dialog populates its list from VirtualDesktopManager, using a throwaway registry key as
///     the desktop source so results don't depend on the machine's virtual desktops.
/// </summary>
public class VirtualDesktopSelectionDialogLoadTests : IDisposable
{
    private readonly IVirtualDesktopManager? _originalManager = VirtualDesktopManager.DesktopManager;
    private readonly IVirtualDesktopManagerInternal? _originalManagerInternal =
        VirtualDesktopManager.DesktopManagerInternal;
    private readonly string _originalRegistryPath = VirtualDesktopManager.RegistryPath;
    private readonly string _testRoot = $@"Software\OotDTests\{Guid.NewGuid():N}";

    public VirtualDesktopSelectionDialogLoadTests()
    {
        VirtualDesktopManager.DesktopManager = new NoWindowDesktopManager();
        VirtualDesktopManager.DesktopManagerInternal = null;
        VirtualDesktopManager.RegistryPath = $@"{_testRoot}\VirtualDesktops";
    }

    public void Dispose()
    {
        VirtualDesktopManager.DesktopManager = _originalManager;
        VirtualDesktopManager.DesktopManagerInternal = _originalManagerInternal;
        VirtualDesktopManager.RegistryPath = _originalRegistryPath;
        Registry.CurrentUser.DeleteSubKeyTree(_testRoot, false);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void WithNoDesktops_DisablesSelectionAndExplains()
    {
        using var dialog = new VirtualDesktopSelectionDialog();

        GetField<Label>(dialog, "_instructionLabel").Text.Should().StartWith("No virtual desktops found.");
        GetField<ListBox>(dialog, "_desktopListBox").Enabled.Should().BeFalse();
        GetField<Button>(dialog, "_okButton").Enabled.Should().BeFalse();
    }

    [Fact]
    public void WithCurrentDesktop_MarksAndPreselectsIt()
    {
        var first = Guid.NewGuid();
        var current = Guid.NewGuid();
        WriteDesktops(current, first, current);

        using var dialog = new VirtualDesktopSelectionDialog();

        var listBox = GetField<ListBox>(dialog, "_desktopListBox");
        listBox.Items.Cast<object>().Select(item => item.ToString())
            .Should().Equal("Desktop 1", "Desktop 2 (Current)");
        listBox.SelectedIndex.Should().Be(1);
    }

    [Fact]
    public void WithoutCurrentDesktop_SelectsFirstDesktop()
    {
        WriteDesktops(null, Guid.NewGuid(), Guid.NewGuid());

        using var dialog = new VirtualDesktopSelectionDialog();

        var listBox = GetField<ListBox>(dialog, "_desktopListBox");
        listBox.Items.Count.Should().Be(2);
        listBox.SelectedIndex.Should().Be(0);
        GetField<Button>(dialog, "_okButton").Enabled.Should().BeTrue();
    }

    private void WriteDesktops(Guid? current, params Guid[] desktopIds)
    {
        using var key = Registry.CurrentUser.CreateSubKey(VirtualDesktopManager.RegistryPath);
        key.SetValue("VirtualDesktopIDs", desktopIds.SelectMany(id => id.ToByteArray()).ToArray(),
            RegistryValueKind.Binary);
        if (current.HasValue)
        {
            key.SetValue("CurrentVirtualDesktop", current.Value.ToByteArray(), RegistryValueKind.Binary);
        }
    }

    private static T GetField<T>(VirtualDesktopSelectionDialog dialog, string name)
    {
        return (T)typeof(VirtualDesktopSelectionDialog)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(dialog)!;
    }

    /// <summary>Reports no desktop for any window, so only registry data produces desktops.</summary>
    private sealed class NoWindowDesktopManager : IVirtualDesktopManager
    {
        public int IsWindowOnCurrentVirtualDesktop(IntPtr topLevelWindow) => 0;

        public int GetWindowDesktopId(IntPtr topLevelWindow, out Guid desktopId)
        {
            desktopId = Guid.Empty;
            return 0;
        }

        public int MoveWindowToDesktop(IntPtr topLevelWindow, Guid desktopId) => 0;
    }
}
