namespace OotD.Core.Tests.Forms;

using Microsoft.Win32;
using OotD.Forms;
using OotD.Preferences;
using OotD.Properties;

public class InstanceManagerTests : IDisposable
{
    private readonly string _originalRootPath = PreferencesRegistry.RootPath;
    private readonly string _testRootPath = $@"Software\OotDTests\{Guid.NewGuid():N}";

    public InstanceManagerTests()
    {
        // Redirect InstanceManager's registry reads to a throwaway key so tests are isolated
        // from (and never pollute) the real application's settings.
        PreferencesRegistry.RootPath = _testRootPath;
    }

    [Theory]
    [InlineData(new string[] { }, 0)]
    [InlineData(new[] { "AutoUpdate" }, 0)]
    [InlineData(new[] { "Instance1" }, 1)]
    [InlineData(new[] { "Instance1", "Instance2" }, 2)]
    [InlineData(new[] { "Instance1", "AutoUpdate", "Instance2" }, 2)]
    [InlineData(new[] { "Instance1", "Instance2", "AutoUpdate", "Instance3" }, 3)]
    public void InstanceCount_WithVariousScenarios_ShouldReturnExpectedCount(string[] instanceNames, int expectedCount)
    {
        using var productKey = Registry.CurrentUser.CreateSubKey(ProductRegistryPath);
        foreach (var instanceName in instanceNames)
        {
            productKey.CreateSubKey(instanceName).Dispose();
        }

        InstanceManager.InstanceCount.Should().Be(expectedCount);
    }

    [Fact]
    public void InstanceCount_WithOnlyAutoUpdateRegistryEntry_ShouldReturnZero()
    {
        // Arrange
        var baseline = InstanceManager.InstanceCount;
        using var productKey = Registry.CurrentUser.CreateSubKey(ProductRegistryPath);
        productKey.Should().NotBeNull();
        productKey!.CreateSubKey("AutoUpdate")!.Dispose();

        // Act
        var count = InstanceManager.InstanceCount;

        // Assert
        count.Should().Be(baseline);
    }

    [Fact]
    public void InstanceCount_WithMixedRegistryEntries_ShouldExcludeAutoUpdate()
    {
        // Arrange
        var baseline = InstanceManager.InstanceCount;
        var key1 = $"Test_{Guid.NewGuid():N}";
        var key2 = $"Test_{Guid.NewGuid():N}";

        using var productKey = Registry.CurrentUser.CreateSubKey(ProductRegistryPath);
        productKey.Should().NotBeNull();
        productKey!.CreateSubKey("AutoUpdate")!.Dispose();
        productKey.CreateSubKey(key1)!.Dispose();
        productKey.CreateSubKey(key2)!.Dispose();

        // Act
        var count = InstanceManager.InstanceCount;

        // Assert
        count.Should().Be(baseline + 2);
    }

    [Fact]
    public void FindNonOverlappingLocation_WithNoExistingWindows_ShouldReturnTopLeftOfWorkingArea()
    {
        // Arrange
        var workingArea = new Rectangle(100, 100, 800, 600);
        var size = new Size(300, 200);

        // Act
        var location = InstanceManager.FindNonOverlappingLocation(workingArea, size, []);

        // Assert
        location.Should().Be(new Point(100, 100));
    }

    [Fact]
    public void FindNonOverlappingLocation_WithOccupiedTopLeft_ShouldReturnFirstAvailableSlot()
    {
        // Arrange
        var workingArea = new Rectangle(0, 0, 400, 300);
        var size = new Size(100, 100);
        var occupied = new[] { new Rectangle(0, 0, 100, 100) };

        // Act
        var location = InstanceManager.FindNonOverlappingLocation(workingArea, size, occupied);

        // Assert
        // Step size is 30px; x=120 is the first non-overlapping candidate on row 0.
        location.Should().Be(new Point(120, 0));
    }

    [Fact]
    public void FindNonOverlappingLocation_WhenWorkingAreaOffset_ShouldRespectBounds()
    {
        // Arrange
        var workingArea = new Rectangle(1920, 0, 800, 600);
        var size = new Size(250, 150);
        var occupied = new[]
        {
            new Rectangle(1920, 0, 250, 150),
            new Rectangle(2160, 0, 250, 150)
        };

        // Act
        var location = InstanceManager.FindNonOverlappingLocation(workingArea, size, occupied);
        var placed = new Rectangle(location, size);

        // Assert
        workingArea.Contains(placed).Should().BeTrue();
        occupied.Any(existing => existing.IntersectsWith(placed)).Should().BeFalse();
    }

    [Fact]
    public void FindNonOverlappingLocation_WithPreferredStartThatIsFree_ShouldUsePreferredStart()
    {
        // Arrange
        var workingArea = new Rectangle(0, 0, 500, 400);
        var size = new Size(100, 100);
        var occupied = new[] { new Rectangle(0, 0, 100, 100) };
        var preferredStart = new Point(240, 180);

        // Act
        var location = InstanceManager.FindNonOverlappingLocation(workingArea, size, occupied, preferredStart);

        // Assert
        location.Should().Be(preferredStart);
    }

    [Fact]
    public void GetCascadedStartPoint_WithNoOccupiedWindows_ShouldReturnNull()
    {
        // Arrange
        var workingArea = new Rectangle(0, 0, 500, 400);
        var size = new Size(100, 100);

        // Act
        var result = InstanceManager.GetCascadedStartPoint(workingArea, size, []);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCascadedStartPoint_WithOccupiedWindows_ShouldReturnOffsetAndClampedPoint()
    {
        // Arrange
        var workingArea = new Rectangle(0, 0, 350, 250);
        var size = new Size(100, 100);
        var occupied = new[]
        {
            new Rectangle(260, 180, 100, 100),
            new Rectangle(50, 30, 100, 100)
        };

        // Act
        var result = InstanceManager.GetCascadedStartPoint(workingArea, size, occupied);

        // Assert
        // Anchor is the lowest window (260,180). +30 offset is clamped to max valid origin (250,150).
        result.Should().Be(new Point(250, 150));
    }

    [Fact]
    public void FindNonOverlappingLocation_WhenScreenIsSaturated_ShouldReturnInBoundsBestFallback()
    {
        // Arrange
        var workingArea = new Rectangle(0, 0, 300, 300);
        var size = new Size(200, 200);
        var occupied = new[]
        {
            new Rectangle(0, 0, 200, 200),
            new Rectangle(100, 0, 200, 200),
            new Rectangle(0, 100, 200, 200),
            new Rectangle(100, 100, 200, 200)
        };

        // Act
        var location = InstanceManager.FindNonOverlappingLocation(workingArea, size, occupied);
        var placed = new Rectangle(location, size);

        // Assert
        workingArea.Contains(placed).Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 0, 600, 400, 200, 120)]
    [InlineData(100, 50, 800, 600, 250, 200)]
    [InlineData(1920, 0, 700, 500, 300, 180)]
    public void FindNonOverlappingLocation_WithVariousBounds_ShouldRespectPlacementInvariants(
        int left,
        int top,
        int width,
        int height,
        int windowWidth,
        int windowHeight)
    {
        // Arrange
        var workingArea = new Rectangle(left, top, width, height);
        var size = new Size(windowWidth, windowHeight);
        var occupied = new[]
        {
            new Rectangle(left, top, windowWidth, windowHeight),
            new Rectangle(left + Math.Min(120, Math.Max(0, width - windowWidth)), top,
                windowWidth, windowHeight)
        };

        // Act
        var location1 = InstanceManager.FindNonOverlappingLocation(workingArea, size, occupied);
        var location2 = InstanceManager.FindNonOverlappingLocation(workingArea, size, occupied);
        var placed = new Rectangle(location1, size);

        // Assert
        // Deterministic for same inputs.
        location1.Should().Be(location2);

        // Always remain in working area.
        workingArea.Contains(placed).Should().BeTrue();

        // If there exists at least one valid non-overlapping slot, result should not overlap.
        var hasFreeSlot = HasNonOverlappingCandidate(workingArea, size, occupied);
        if (hasFreeSlot)
        {
            occupied.Any(existing => existing.IntersectsWith(placed)).Should().BeFalse();
        }
    }

    [Fact]
    public void OrderWorkingAreas_WhenCurrentAreaPresent_ShouldPlaceCurrentAreaFirst()
    {
        // Arrange
        var current = new Rectangle(1920, 0, 800, 600);
        var areas = new[]
        {
            new Rectangle(0, 0, 1920, 1080),
            current,
            new Rectangle(2720, 0, 800, 600)
        };

        // Act
        var ordered = InstanceManager.OrderWorkingAreas(current, areas);

        // Assert
        ordered[0].Should().Be(current);
    }

    [Fact]
    public void OrderWorkingAreas_WhenCurrentAreaMissing_ShouldPreserveOriginalOrder()
    {
        // Arrange
        var current = new Rectangle(999, 999, 100, 100);
        var area1 = new Rectangle(0, 0, 1000, 800);
        var area2 = new Rectangle(1000, 0, 1000, 800);
        var areas = new[] { area1, area2 };

        // Act
        var ordered = InstanceManager.OrderWorkingAreas(current, areas);

        // Assert
        ordered.Should().Equal(area1, area2);
    }

    [Fact]
    public void FilterInstanceNames_ShouldExcludeAutoUpdate()
    {
        // Arrange
        var subKeyNames = new[] { "AutoUpdate", "Default Instance", "Work", "Home" };

        // Act
        var result = InstanceManager.FilterInstanceNames(subKeyNames).ToArray();

        // Assert
        result.Should().Equal("Default Instance", "Work", "Home");
    }

    [Theory]
    [InlineData(0, new string[0], "Default Instance", "Default Instance")]
    [InlineData(1, new[] { "Default Instance" }, "Fallback", "Default Instance")]
    [InlineData(1, new[] { "AutoUpdate" }, "Default Instance", "Default Instance")]
    [InlineData(2, new[] { "AutoUpdate", "Instance1" }, "Default Instance", "Default Instance")]
    public void ResolveSingleInstanceName_WithVariousInputs_ReturnsExpectedName(int instanceCount,
        string[] subKeyNames,
        string defaultInstanceName,
        string expected)
    {
        // Act
        var result = InstanceManager.ResolveSingleInstanceName(instanceCount, subKeyNames, defaultInstanceName);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void TrimSingleInstanceMenuItems_ShouldRemoveItemsBeforeCalendarMenu()
    {
        // Arrange
        using var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Add") { Name = "AddInstanceMenu" });
        menu.Items.Add(new ToolStripMenuItem("About") { Name = "AboutMenu" });
        menu.Items.Add(new ToolStripMenuItem("Calendar") { Name = "CalendarMenu" });
        menu.Items.Add(new ToolStripMenuItem("Inbox") { Name = "InboxMenu" });

        // Act
        InstanceManager.TrimSingleInstanceMenuItems(menu);

        // Assert
        menu.Items.Cast<ToolStripItem>().Select(item => item.Name)
            .Should().Equal("CalendarMenu", "InboxMenu");
    }

    [Fact]
    public void ConfigureSingleInstanceMenu_RepeatedTransitionsPreserveItemsAndHandlers()
    {
        using var menu = CreateInstanceMenu();
        var clicked = new List<string>();
        var handlers = new[] { "AddInstanceMenu", "StartWithWindows", "LockPositionMenu", "CheckForUpdatesMenu",
            "AboutMenu", "ResetConfigMenu" }.ToDictionary(name => name,
            name => new EventHandler((_, _) => clicked.Add(name)));

        InstanceManager.ConfigureSingleInstanceMenu(menu, handlers);
        var originalOrder = menu.Items.Cast<ToolStripItem>().Select(item => item.Name).ToArray();

        using var firstSubmenu = InstanceManager.CreateInstanceSubmenu(menu, "Work");
        using var secondSubmenu = InstanceManager.CreateInstanceSubmenu(menu, "Work");
        menu.Items.Cast<ToolStripItem>().Count(item => item.Name == "Work").Should().Be(1);
        menu.Items.Cast<ToolStripItem>().Count(item => item.Name == "AddInstanceSeparator").Should().Be(1);
        menu.Items["RemoveInstanceMenu"]!.Available.Should().BeTrue();
        menu.Items["RenameInstanceMenu"]!.Available.Should().BeTrue();
        menu.Items["ExitMenu"]!.Available.Should().BeFalse();
        menu.Items["Separator6"]!.Available.Should().BeFalse();
        menu.Items["Work"]!.BackColor.Should().Be(Color.Gainsboro);
        secondSubmenu.DropDown.Should().BeSameAs(menu);
        foreach (var name in handlers.Keys.Where(name => name != "ResetConfigMenu"))
        {
            menu.Items[name]!.Available.Should().BeFalse();
        }

        InstanceManager.ConfigureSingleInstanceMenu(menu, handlers);
        InstanceManager.ConfigureSingleInstanceMenu(menu, handlers);

        menu.Items.Cast<ToolStripItem>().Select(item => item.Name).Should().Equal(originalOrder);
        menu.Items["RemoveInstanceMenu"]!.Available.Should().BeFalse();
        menu.Items["RenameInstanceMenu"]!.Available.Should().BeFalse();
        menu.Items["ExitMenu"]!.Available.Should().BeTrue();
        menu.Items["Separator6"]!.Available.Should().BeTrue();
        foreach (var name in handlers.Keys)
        {
            menu.Items[name]!.Available.Should().BeTrue();
            menu.Items[name]!.PerformClick();
        }

        clicked.Should().Equal(handlers.Keys);
    }

    [Fact]
    public void ConfigureSingleInstanceMenu_PreservesExistingAddItemAndSeparator()
    {
        using var menu = CreateInstanceMenu();
        var clicks = 0;
        var addItem = new ToolStripMenuItem("Add", null, (_, _) => clicks++, "AddInstanceMenu");
        var separator = new ToolStripSeparator { Name = "AddInstanceSeparator" };
        menu.Items.Insert(1, addItem);
        menu.Items.Insert(2, separator);
        var handlers = new[] { "StartWithWindows", "LockPositionMenu", "CheckForUpdatesMenu", "AboutMenu",
            "ResetConfigMenu" }.ToDictionary(name => name, _ => new EventHandler((_, _) => { }));

        InstanceManager.ConfigureSingleInstanceMenu(menu, handlers);
        InstanceManager.ConfigureSingleInstanceMenu(menu, handlers);

        menu.Items["AddInstanceMenu"].Should().BeSameAs(addItem);
        menu.Items["AddInstanceSeparator"].Should().BeSameAs(separator);
        menu.Items.Cast<ToolStripItem>().Count(item => item.Name == "AddInstanceSeparator").Should().Be(1);
        addItem.PerformClick();
        clicks.Should().Be(1);
    }

    [Fact]
    public void CreateInstanceSubmenu_WithoutSharedItemsAddsHeaderAndSeparatorOnce()
    {
        using var menu = CreateInstanceMenu();

        using var submenu = InstanceManager.CreateInstanceSubmenu(menu, "Home");
        using var repeatedSubmenu = InstanceManager.CreateInstanceSubmenu(menu, "Home");

        menu.Items[0].Name.Should().Be("Home");
        menu.Items[1].Name.Should().Be("AddInstanceSeparator");
        menu.Items.Cast<ToolStripItem>().Count(item => item.Name == "Home").Should().Be(1);
        menu.Items.Cast<ToolStripItem>().Count(item => item.Name == "AddInstanceSeparator").Should().Be(1);
        repeatedSubmenu.Text.Should().Be("Home");
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    [InlineData(false, 2)]
    public void ShowHideInstances_UpdatesEveryWindowAndMenu(bool initiallyVisible, int instanceCount)
    {
        using var firstForm = new VisibilityTestForm();
        using var secondForm = new VisibilityTestForm();
        using var firstMenu = CreateInstanceMenu();
        using var secondMenu = CreateInstanceMenu();
        using var globalMenu = CreateInstanceMenu();
        var menu = instanceCount == 1 ? firstMenu : globalMenu;
        menu.Items["HideShowMenu"]!.Text = initiallyVisible
            ? instanceCount == 1 ? Resources.Hide : Resources.HideAll
            : instanceCount == 1 ? Resources.Show : Resources.ShowAll;
        var instances = new List<(Form Form, ContextMenuStrip Menu)> { (firstForm, firstMenu) };
        if (instanceCount == 2)
        {
            instances.Add((secondForm, secondMenu));
        }

        InstanceManager.ShowHideInstances(menu, instances);

        firstForm.RequestedVisibility.Should().Be(!initiallyVisible);
        firstMenu.Items["HideShowMenu"]!.Text.Should().Be(initiallyVisible ? Resources.Show : Resources.Hide);
        if (instanceCount == 2)
        {
            secondForm.RequestedVisibility.Should().Be(!initiallyVisible);
            secondMenu.Items["HideShowMenu"]!.Text.Should().Be(initiallyVisible ? Resources.Show : Resources.Hide);
            menu.Items["HideShowMenu"]!.Text.Should().Be(initiallyVisible ? Resources.ShowAll : Resources.HideAll);
        }
    }

    [Fact]
    public void ShowHideInstances_UnknownLabelDoesNothing()
    {
        using var form = new VisibilityTestForm();
        using var menu = CreateInstanceMenu();
        menu.Items["HideShowMenu"]!.Text = "Unknown";

        InstanceManager.ShowHideInstances(menu, new[] { ((Form)form, menu) });

        form.RequestedVisibility.Should().BeNull();
        menu.Items["HideShowMenu"]!.Text.Should().Be("Unknown");
    }

    private sealed class VisibilityTestForm : Form
    {
        public bool? RequestedVisibility { get; private set; }

        protected override void SetVisibleCore(bool value)
        {
            RequestedVisibility = value;
        }
    }

    private static ContextMenuStrip CreateInstanceMenu()
    {
        var menu = new ContextMenuStrip();
        foreach (var name in new[] { "CalendarMenu", "InboxMenu", "ContactsMenu", "TasksMenu", "NotesMenu",
                     "Separator1", "DateMenu", "Separator2", "PreferencesMenu", "Separator3", "HideShowMenu",
                     "DisableEnableEditingMenu", "Separator4", "OpacityMenu", "Separator5", "RemoveInstanceMenu",
                     "RenameInstanceMenu", "Separator6", "ExitMenu" })
        {
            menu.Items.Add(new ToolStripMenuItem(name) { Name = name });
        }

        return menu;
    }

    [Fact]
    public void TrimSingleInstanceMenuItems_WhenCalendarMenuMissing_ShouldClearMenu()
    {
        // Arrange
        using var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Add") { Name = "AddInstanceMenu" });
        menu.Items.Add(new ToolStripMenuItem("About") { Name = "AboutMenu" });

        // Act
        InstanceManager.TrimSingleInstanceMenuItems(menu);

        // Assert
        menu.Items.Count.Should().Be(0);
    }

    [Fact]
    public void ReorderBottomMenuItems_ShouldMoveExistingAboutAndCheckForUpdatesAboveRestoreDefaults()
    {
        // Arrange
        using var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Start") { Name = "StartWithWindows" });
        menu.Items.Add(new ToolStripMenuItem("Restore") { Name = "ResetConfigMenu" });
        menu.Items.Add(new ToolStripMenuItem("Exit") { Name = "ExitMenu" });
        menu.Items.Add(new ToolStripMenuItem("About") { Name = "AboutMenu" });
        menu.Items.Add(new ToolStripMenuItem("Updates") { Name = "CheckForUpdatesMenu" });

        // Act
        InstanceManager.ReorderBottomMenuItems(
            menu,
            () => throw new InvalidOperationException("About should already exist"),
            () => throw new InvalidOperationException("Check for updates should already exist"));

        // Assert
        menu.Items.Cast<ToolStripItem>().Select(item => item.Name)
            .Should().Equal("StartWithWindows", "AboutMenu", "CheckForUpdatesMenu", "ResetConfigMenu",
                "ExitMenu");
    }

    [Fact]
    public void ReorderBottomMenuItems_ShouldCreateMissingSharedItemsBeforeRestoreDefaults()
    {
        // Arrange
        using var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Start") { Name = "StartWithWindows" });
        menu.Items.Add(new ToolStripMenuItem("Restore") { Name = "ResetConfigMenu" });
        menu.Items.Add(new ToolStripMenuItem("Exit") { Name = "ExitMenu" });

        // Act
        InstanceManager.ReorderBottomMenuItems(
            menu,
            () => new ToolStripMenuItem("About") { Name = "AboutMenu" },
            () => new ToolStripMenuItem("Updates") { Name = "CheckForUpdatesMenu" });

        // Assert
        menu.Items.Cast<ToolStripItem>().Select(item => item.Name)
            .Should().Equal("StartWithWindows", "AboutMenu", "CheckForUpdatesMenu", "ResetConfigMenu",
                "ExitMenu");
    }

    [Fact]
    public void ReorderBottomMenuItems_WhenRestoreDefaultsMissing_ShouldLeaveMenuUnchanged()
    {
        // Arrange
        using var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Start") { Name = "StartWithWindows" });
        menu.Items.Add(new ToolStripMenuItem("Exit") { Name = "ExitMenu" });

        // Act
        InstanceManager.ReorderBottomMenuItems(
            menu,
            () => new ToolStripMenuItem("About") { Name = "AboutMenu" },
            () => new ToolStripMenuItem("Updates") { Name = "CheckForUpdatesMenu" });

        // Assert
        menu.Items.Cast<ToolStripItem>().Select(item => item.Name)
            .Should().Equal("StartWithWindows", "ExitMenu");
    }

    private static bool HasNonOverlappingCandidate(Rectangle workingArea, Size windowSize,
        IReadOnlyCollection<Rectangle> occupied)
    {
        var maxX = Math.Max(workingArea.Left, workingArea.Right - windowSize.Width);
        var maxY = Math.Max(workingArea.Top, workingArea.Bottom - windowSize.Height);

        for (var y = workingArea.Top; y <= maxY; y += 30)
        {
            for (var x = workingArea.Left; x <= maxX; x += 30)
            {
                var candidate = new Rectangle(x, y, windowSize.Width, windowSize.Height);
                if (!occupied.Any(existing => existing.IntersectsWith(candidate)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void Dispose()
    {
        PreferencesRegistry.RootPath = _originalRootPath;
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(_testRootPath, false);
        }
        catch
        {
            // ignore cleanup failures
        }

        GC.SuppressFinalize(this);
    }

    private string ProductRegistryPath => _testRootPath;
}
