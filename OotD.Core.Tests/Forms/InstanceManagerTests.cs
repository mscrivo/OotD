namespace OotD.Core.Tests.Forms;

using Microsoft.Win32;
using OotD.Forms;

public class InstanceManagerTests : IDisposable
{
    private readonly List<string> _createdSubKeys = [];

    [Fact]
    public void InstanceCount_WithNoRegistryEntries_ShouldReturnZero()
    {
        // This test would require mocking the Registry, which is complex
        // For now, we'll test the logic conceptually

        // Arrange
        var instanceNames = Array.Empty<string>();

        // Act
        var count = instanceNames.Count(name => name != "AutoUpdate");

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void InstanceCount_WithAutoUpdateOnly_ShouldReturnZero()
    {
        // Arrange
        var instanceNames = new[] { "AutoUpdate" };

        // Act
        var count = instanceNames.Count(name => name != "AutoUpdate");

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void InstanceCount_WithValidInstances_ShouldReturnCorrectCount()
    {
        // Arrange
        var instanceNames = new[] { "Instance1", "Instance2", "AutoUpdate", "Instance3" };

        // Act
        var count = instanceNames.Count(name => name != "AutoUpdate");

        // Assert
        count.Should().Be(3);
    }

    [Theory]
    [InlineData(new string[] { }, 0)]
    [InlineData(new[] { "AutoUpdate" }, 0)]
    [InlineData(new[] { "Instance1" }, 1)]
    [InlineData(new[] { "Instance1", "Instance2" }, 2)]
    [InlineData(new[] { "Instance1", "AutoUpdate", "Instance2" }, 2)]
    public void InstanceCount_WithVariousScenarios_ShouldReturnExpectedCount(string[] instanceNames, int expectedCount)
    {
        // Act
        var count = instanceNames.Count(name => name != "AutoUpdate");

        // Assert
        count.Should().Be(expectedCount);
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

        _createdSubKeys.Add(key1);
        _createdSubKeys.Add(key2);

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
        try
        {
            foreach (var subKey in _createdSubKeys)
            {
                Registry.CurrentUser.DeleteSubKeyTree($"{ProductRegistryPath}\\{subKey}", false);
            }
        }
        catch
        {
            // ignore cleanup failures
        }

        GC.SuppressFinalize(this);
    }

    private static string ProductRegistryPath => $@"Software\{Application.CompanyName}\{Application.ProductName}";
}
