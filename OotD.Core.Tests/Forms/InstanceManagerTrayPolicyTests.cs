using OotD.Events;
using OotD.Forms;
using OotD.Properties;

namespace OotD.Core.Tests.Forms;

public class InstanceManagerTrayPolicyTests
{
    private static readonly string[] MultipleInstanceHandlerNames =
    [
        "AddInstanceMenu", "StartWithWindows", "HideShowMenu", "LockPositionMenu", "DisableEnableEditingMenu",
        "AboutMenu", "CheckForUpdatesMenu", "ResetConfigMenu", "ExitMenu"
    ];

    [Fact]
    public void CreateMultipleInstanceMenu_BuildsItemsInOrderWithInstanceSlotAfterAdd()
    {
        var clicked = new List<string>();
        var handlers = MultipleInstanceHandlerNames.ToDictionary(name => name,
            name => new EventHandler((_, _) => clicked.Add(name)));

        using var menu = InstanceManagerTrayPolicy.CreateMultipleInstanceMenu(handlers);

        menu.Items.Cast<ToolStripItem>().Select(item => item is ToolStripSeparator ? "-" : item.Name).Should().Equal(
            "AddInstanceMenu", "-", "-", "StartWithWindows", "-", "HideShowMenu", "LockPositionMenu",
            "DisableEnableEditingMenu", "-", "AboutMenu", "CheckForUpdatesMenu", "ResetConfigMenu", "-", "ExitMenu");
        menu.Items["HideShowMenu"]!.Text.Should().Be(Resources.HideAll);
        menu.Items["DisableEnableEditingMenu"]!.Text.Should().Be(Resources.DisableEditing);

        foreach (var name in MultipleInstanceHandlerNames)
        {
            menu.Items[name]!.PerformClick();
        }

        clicked.Should().Equal(MultipleInstanceHandlerNames);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ToggleMenuCheck_AppliesAndChecksTheOppositeState(bool initiallyChecked, bool expected)
    {
        using var item = new ToolStripMenuItem { Checked = initiallyChecked };
        bool? applied = null;

        InstanceManagerTrayPolicy.ToggleMenuCheck(item, check => applied = check);

        applied.Should().Be(expected);
        item.Checked.Should().Be(expected);
    }

    [Fact]
    public void ToggleMenuCheck_WhenApplyFails_LeavesCheckUnchanged()
    {
        using var item = new ToolStripMenuItem { Checked = false };

        var act = () => InstanceManagerTrayPolicy.ToggleMenuCheck(item,
            _ => throw new InvalidOperationException("task scheduler unavailable"));

        act.Should().Throw<InvalidOperationException>();
        item.Checked.Should().BeFalse();
    }

    [Fact]
    public void ToggleMenuCheck_WithMissingItem_StillApplies()
    {
        bool? applied = null;

        InstanceManagerTrayPolicy.ToggleMenuCheck(null, check => applied = check);

        applied.Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToggleEditing_FlipsEveryInstanceAndTheGlobalCheck(bool editingCurrentlyDisabled)
    {
        using var globalItem = new ToolStripMenuItem { Checked = editingCurrentlyDisabled };
        using var firstForm = new Form();
        using var secondForm = new Form();
        using var firstMenu = CreateMenuWithEditingItem(editingCurrentlyDisabled);
        using var secondMenu = new ContextMenuStrip();

        InstanceManagerTrayPolicy.ToggleEditing(globalItem, [(firstForm, firstMenu), (secondForm, secondMenu)]);

        firstForm.Enabled.Should().Be(editingCurrentlyDisabled);
        secondForm.Enabled.Should().Be(editingCurrentlyDisabled);
        ((ToolStripMenuItem)firstMenu.Items["DisableEnableEditingMenu"]!).Checked
            .Should().Be(!editingCurrentlyDisabled);
        globalItem.Checked.Should().Be(!editingCurrentlyDisabled);
    }

    [Fact]
    public void ToggleEditing_WithMissingGlobalItem_DisablesInstances()
    {
        using var form = new Form();
        using var menu = CreateMenuWithEditingItem(false);

        InstanceManagerTrayPolicy.ToggleEditing(null, [(form, menu)]);

        form.Enabled.Should().BeFalse();
    }

    [Fact]
    public void RenameInstanceMenuItem_UpdatesTextAndName()
    {
        using var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Work") { Name = "Work" });

        InstanceManagerTrayPolicy.RenameInstanceMenuItem(menu, "Work", "Office");

        menu.Items.ContainsKey("Work").Should().BeFalse();
        menu.Items["Office"]!.Text.Should().Be("Office");
    }

    [Fact]
    public void RenameInstanceMenuItem_WithUnknownItemOrNoMenu_DoesNothing()
    {
        using var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Work") { Name = "Work" });

        InstanceManagerTrayPolicy.RenameInstanceMenuItem(menu, "Home", "Office");
        InstanceManagerTrayPolicy.RenameInstanceMenuItem(null, "Work", "Office");

        menu.Items["Work"]!.Text.Should().Be("Work");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InstanceNameValidator_RejectsBlankNames(string? name)
    {
        var args = new InputBoxValidatingEventArgs { Text = name! };

        InstanceManagerTrayPolicy.CreateInstanceNameValidator([])(this, args);

        args.Cancel.Should().BeTrue();
        args.Message.Should().Be(Resources.ResourceManager.GetString("Required"));
    }

    [Theory]
    [InlineData("Work")]
    [InlineData("WORK")]
    public void InstanceNameValidator_RejectsTakenNamesIgnoringCase(string name)
    {
        var args = new InputBoxValidatingEventArgs { Text = name };

        InstanceManagerTrayPolicy.CreateInstanceNameValidator(["Home", "Work"])(this, args);

        args.Cancel.Should().BeTrue();
        args.Message.Should().Be(Resources.InstanceNameTaken);
    }

    [Fact]
    public void InstanceNameValidator_AcceptsNamesThatAreFree()
    {
        var args = new InputBoxValidatingEventArgs { Text = "Work" };

        InstanceManagerTrayPolicy.CreateInstanceNameValidator(["Home"])(this, args);

        args.Cancel.Should().BeFalse();
        args.Message.Should().BeNull();
    }

    [Theory]
    [InlineData(1, 72f, 16)]
    [InlineData(15, 95.9f, 16)]
    [InlineData(31, 96f, 32)]
    [InlineData(28, 144f, 32)]
    public void CreateDateIcon_SizesIconForDpi(int day, float dpiX, int expectedSize)
    {
        using var icon = InstanceManagerTrayPolicy.CreateDateIcon(day, dpiX);

        icon.Size.Should().Be(new Size(expectedSize, expectedSize));
    }

    [Fact]
    public void CreateDateIcon_HasAnIconForEveryDayOfTheMonth()
    {
        for (var day = 1; day <= 31; day++)
        {
            using var icon = InstanceManagerTrayPolicy.CreateDateIcon(day, 96f);
            icon.Should().NotBeNull();
        }
    }

    [Fact]
    public void SelectNewInstanceLocation_WithNoOtherWindows_UsesCurrentScreenTopLeft()
    {
        var current = new Rectangle(0, 0, 1000, 800);

        var location = InstanceManagerPlacementPolicy.SelectNewInstanceLocation(current,
            [new Rectangle(1000, 0, 1000, 800), current], new Size(300, 200), []);

        location.Should().Be(new Point(0, 0));
    }

    [Fact]
    public void SelectNewInstanceLocation_SnapsFlushAgainstAnExistingWindow()
    {
        var current = new Rectangle(0, 0, 1000, 800);
        Rectangle[] occupied = [new Rectangle(0, 0, 100, 100)];

        var location = InstanceManagerPlacementPolicy.SelectNewInstanceLocation(current, [current],
            new Size(100, 100), occupied);

        location.Should().Be(new Point(100, 0));
    }

    [Fact]
    public void SelectNewInstanceLocation_PrefersSnappingOnTheCurrentScreen()
    {
        var current = new Rectangle(0, 0, 1000, 800);
        var other = new Rectangle(1000, 0, 1000, 800);
        Rectangle[] occupied = [new Rectangle(1200, 100, 300, 300), new Rectangle(400, 300, 300, 200)];

        var location = InstanceManagerPlacementPolicy.SelectNewInstanceLocation(current, [other, current],
            new Size(200, 200), occupied);

        current.Contains(new Rectangle(location, new Size(200, 200))).Should().BeTrue();
        location.Should().Be(new Point(700, 300));
    }

    [Fact]
    public void SelectNewInstanceLocation_WhenCurrentScreenIsFull_MovesToAnotherScreen()
    {
        var current = new Rectangle(0, 0, 300, 300);
        var other = new Rectangle(300, 0, 600, 600);

        var location = InstanceManagerPlacementPolicy.SelectNewInstanceLocation(current, [other, current],
            new Size(300, 300), [current]);

        location.Should().Be(new Point(300, 0));
    }

    [Fact]
    public void SelectNewInstanceLocation_WhenEveryScreenIsFull_FallsBackToCurrentScreen()
    {
        var current = new Rectangle(0, 0, 300, 300);
        var other = new Rectangle(300, 0, 300, 300);

        var location = InstanceManagerPlacementPolicy.SelectNewInstanceLocation(current, [current, other],
            new Size(300, 300), [current, other]);

        current.Contains(new Rectangle(location, new Size(300, 300))).Should().BeTrue();
    }

    private static ContextMenuStrip CreateMenuWithEditingItem(bool isChecked)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Disable editing")
        {
            Name = "DisableEnableEditingMenu",
            Checked = isChecked
        });
        return menu;
    }
}
