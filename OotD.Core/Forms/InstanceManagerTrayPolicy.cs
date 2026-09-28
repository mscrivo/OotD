using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using OotD.Events;
using OotD.Properties;

namespace OotD.Forms;

/// <summary>
///     Tray icon and tray context menu logic extracted from <see cref="InstanceManager" /> for testing.
/// </summary>
internal static class InstanceManagerTrayPolicy
{
    internal const string AutoUpdateInstanceName = "AutoUpdate";
    internal const string CalendarMenuName = "CalendarMenu";
    internal const string AboutMenuName = "AboutMenu";
    internal const string CheckForUpdatesMenuName = "CheckForUpdatesMenu";
    internal const string ResetConfigMenuName = "ResetConfigMenu";

    /// <summary>
    ///     Builds the top-level tray menu shown when more than one instance exists. Each instance's own menu is
    ///     inserted as a submenu at index 2, between the two leading separators.
    /// </summary>
    internal static ContextMenuStrip CreateMultipleInstanceMenu(IReadOnlyDictionary<string, EventHandler> handlers)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem(Resources.AddInstance, null, handlers["AddInstanceMenu"], "AddInstanceMenu"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem(Resources.StartWithWindows, null, handlers["StartWithWindows"],
            "StartWithWindows"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem(Resources.HideAll, null, handlers["HideShowMenu"], "HideShowMenu"));
        menu.Items.Add(new ToolStripMenuItem(Resources.LockPosition, null, handlers["LockPositionMenu"],
            "LockPositionMenu"));
        menu.Items.Add(new ToolStripMenuItem(Resources.DisableEditing, null, handlers["DisableEnableEditingMenu"],
            "DisableEnableEditingMenu"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem(Resources.About, null, handlers[AboutMenuName], AboutMenuName));
        menu.Items.Add(new ToolStripMenuItem(Resources.CheckForUpdates, null, handlers[CheckForUpdatesMenuName],
            CheckForUpdatesMenuName));
        menu.Items.Add(new ToolStripMenuItem(Resources.RestoreDefaults, null, handlers[ResetConfigMenuName],
            ResetConfigMenuName));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem(Resources.Exit, null, handlers["ExitMenu"], "ExitMenu"));
        return menu;
    }

    internal static ToolStripMenuItem CreateInstanceSubmenu(ContextMenuStrip menu, string instanceName)
    {
        if (!menu.Items.ContainsKey(instanceName))
        {
            menu.Items.Insert(0, new ToolStripMenuItem(instanceName)
            {
                Name = instanceName,
                BackColor = SystemColors.ControlLight
            });

            if (!menu.Items.ContainsKey("AddInstanceSeparator"))
            {
                menu.Items.Insert(1, new ToolStripSeparator { Name = "AddInstanceSeparator" });
            }
        }

        SetInstanceMenuVisibility(menu, true);
        return new ToolStripMenuItem(instanceName, null, null, instanceName) { DropDown = menu };
    }

    internal static void SetInstanceMenuVisibility(ContextMenuStrip menu, bool multipleInstances)
    {
        menu.Items["RemoveInstanceMenu"]!.Visible = multipleInstances;
        menu.Items["RenameInstanceMenu"]!.Visible = multipleInstances;
        menu.Items["Separator6"]!.Visible = !multipleInstances;
        menu.Items["ExitMenu"]!.Visible = !multipleInstances;

        foreach (var name in new[] { "AddInstanceMenu", AboutMenuName, "StartWithWindows", "LockPositionMenu",
                     CheckForUpdatesMenuName })
        {
            if (menu.Items[name] is { } item)
            {
                item.Visible = !multipleInstances;
            }
        }
    }

    internal static void ConfigureSingleInstanceMenu(ContextMenuStrip menu,
        IReadOnlyDictionary<string, EventHandler> handlers)
    {
        TrimSingleInstanceMenuItems(menu);
        SetInstanceMenuVisibility(menu, false);
        if (EnsureMenuItem(menu, 0, Resources.AddInstance, "AddInstanceMenu", handlers))
        {
            menu.Items.Insert(1, new ToolStripSeparator { Name = "AddInstanceSeparator" });
        }

        EnsureMenuItem(menu, 12, Resources.StartWithWindows, "StartWithWindows", handlers);
        EnsureMenuItem(menu, 15, Resources.LockPosition, "LockPositionMenu", handlers);
        EnsureMenuItem(menu, 20, Resources.CheckForUpdates, CheckForUpdatesMenuName, handlers);
        EnsureMenuItem(menu, 20, Resources.About, AboutMenuName, handlers);
        var exitIndex = menu.Items.IndexOfKey("ExitMenu");
        EnsureMenuItem(menu, exitIndex < 0 ? menu.Items.Count : exitIndex, Resources.RestoreDefaults,
            ResetConfigMenuName, handlers);
    }

    private static bool EnsureMenuItem(ContextMenuStrip menu, int index, string text, string name,
        IReadOnlyDictionary<string, EventHandler> handlers)
    {
        if (menu.Items.ContainsKey(name))
        {
            return false;
        }

        menu.Items.Insert(index, new ToolStripMenuItem(text, null, handlers[name], name));
        return true;
    }

    internal static IEnumerable<string> FilterInstanceNames(IEnumerable<string> subKeyNames)
    {
        return subKeyNames.Where(instanceName => instanceName != AutoUpdateInstanceName);
    }

    internal static string ResolveSingleInstanceName(int instanceCount, IReadOnlyList<string> subKeyNames,
        string defaultInstanceName)
    {
        if (instanceCount != 1 || subKeyNames.Count == 0)
        {
            return defaultInstanceName;
        }

        var instanceName = subKeyNames[0];
        return instanceName == AutoUpdateInstanceName ? defaultInstanceName : instanceName;
    }

    internal static void TrimSingleInstanceMenuItems(ContextMenuStrip menu)
    {
        while (menu.Items.Count > 0 && menu.Items[0].Name != CalendarMenuName)
        {
            menu.Items.RemoveAt(0);
        }
    }

    internal static void ReorderBottomMenuItems(ContextMenuStrip? menu,
        Func<ToolStripMenuItem> createAboutMenu,
        Func<ToolStripMenuItem> createCheckForUpdatesMenu)
    {
        if (menu == null)
        {
            return;
        }

        if (menu.Items[ResetConfigMenuName] is not ToolStripMenuItem resetDefaultsMenu)
        {
            return;
        }

        var aboutMenu = menu.Items[AboutMenuName] as ToolStripMenuItem;
        var checkForUpdatesMenu = menu.Items[CheckForUpdatesMenuName] as ToolStripMenuItem;

        if (aboutMenu != null)
        {
            menu.Items.Remove(aboutMenu);
        }

        if (checkForUpdatesMenu != null)
        {
            menu.Items.Remove(checkForUpdatesMenu);
        }

        aboutMenu ??= createAboutMenu();
        checkForUpdatesMenu ??= createCheckForUpdatesMenu();

        var resetDefaultsIndex = menu.Items.IndexOf(resetDefaultsMenu);
        menu.Items.Insert(resetDefaultsIndex, aboutMenu);
        menu.Items.Insert(resetDefaultsIndex + 1, checkForUpdatesMenu);
    }

    internal static void ShowHideInstances(ContextMenuStrip menu,
        IReadOnlyCollection<(Form Form, ContextMenuStrip Menu)> instances)
    {
        var visible = ResolveVisibility(menu.Items["HideShowMenu"]!.Text);
        if (visible == null)
        {
            return;
        }

        foreach (var (form, instanceMenu) in instances)
        {
            form.Visible = visible.Value;
            instanceMenu.Items["HideShowMenu"]!.Text = GetVisibilityMenuText(visible.Value, 1);
        }

        menu.Items["HideShowMenu"]!.Text = GetVisibilityMenuText(visible.Value, instances.Count);
    }

    private static bool? ResolveVisibility(string? menuText)
    {
        if (menuText == Resources.HideAll || menuText == Resources.Hide)
        {
            return false;
        }

        return menuText == Resources.ShowAll || menuText == Resources.Show ? true : null;
    }

    private static string GetVisibilityMenuText(bool visible, int instanceCount)
    {
        return visible
            ? instanceCount == 1 ? Resources.Hide : Resources.HideAll
            : instanceCount == 1 ? Resources.Show : Resources.ShowAll;
    }

    /// <summary>
    ///     Flips a checkable tray menu item: computes the new state, hands it to <paramref name="apply" />, and
    ///     only then updates the check mark, so a failing apply leaves the menu unchanged.
    /// </summary>
    internal static void ToggleMenuCheck(ToolStripItem? item, Action<bool> apply)
    {
        var menuItem = item as ToolStripMenuItem;
        var check = menuItem is not { Checked: true };
        apply(check);
        menuItem?.Checked = check;
    }

    /// <summary>
    ///     Toggles editing for every instance from the global "Disable Editing" menu item, flipping each
    ///     instance's own menu check to match.
    /// </summary>
    internal static void ToggleEditing(ToolStripItem? disableEditingItem,
        IEnumerable<(Control Form, ContextMenuStrip Menu)> instances)
    {
        var disableEditingMenu = disableEditingItem as ToolStripMenuItem;
        var enable = disableEditingMenu is { Checked: true };

        foreach (var (form, instanceMenu) in instances)
        {
            form.Enabled = enable;

            if (instanceMenu.Items["DisableEnableEditingMenu"] is ToolStripMenuItem instanceDisableEditingMenu)
            {
                instanceDisableEditingMenu.Checked = !instanceDisableEditingMenu.Checked;
            }
        }

        disableEditingMenu?.Checked = !enable;
    }

    internal static void RenameInstanceMenuItem(ContextMenuStrip? menu, string oldInstanceName,
        string newInstanceName)
    {
        var menuItem = menu?.Items[oldInstanceName];
        if (menuItem != null)
        {
            menuItem.Text = newInstanceName;
            menuItem.Name = newInstanceName;
        }
    }

    internal static void ValidateInstanceName(object? sender, InputBoxValidatingEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Text))
        {
            return;
        }

        e.Cancel = true;
        e.Message = Resources.ResourceManager.GetString("Required")!;
    }

    /// <summary>
    ///     Creates the tray icon showing <paramref name="day" /> of the month, sized for the display DPI.
    /// </summary>
    internal static Icon CreateDateIcon(int day, float dpiX)
    {
        // get new instance of the resource manager.  This will allow us to look up a resource by name.
        var resourceManager = new ResourceManager("OotD.Properties.Resources", typeof(Resources).Assembly);

        // find the icon for the today's day of the month, compensating for user's DPI settings.
        using var dateIcon = (Icon)resourceManager.GetObject("_" + day, CultureInfo.CurrentCulture)!;

        return dpiX < 96f
            ? new Icon(dateIcon, new Size(16, 16))
            : new Icon(dateIcon, new Size(32, 32));
    }
}
