// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using NetSparkle;
using NLog;
using OotD.Events;
using OotD.Preferences;
using OotD.Properties;
using OotD.Utility;

namespace OotD.Forms;

public partial class InstanceManager : Form
{
    private const string AppCastUrl = "https://outlookonthedesktop.com/ootdAppcast.xml";
    private const string AutoUpdateInstanceName = "AutoUpdate";
    private const int CascadeOffset = 30;
    private const string CalendarMenuName = "CalendarMenu";
    private const string AboutMenuName = "AboutMenu";
    private const string CheckForUpdatesMenuName = "CheckForUpdatesMenu";
    private const string ResetConfigMenuName = "ResetConfigMenu";

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly Graphics _graphics;
    private readonly Dictionary<string, MainForm> _mainFormInstances = [];
    private readonly Sparkle _sparkle;
    private int _currentTrayIconDay;

    public InstanceManager()
    {
        InitializeComponent();

        if (GlobalPreferences.IsFirstRun)
        {
            trayIcon.ShowBalloonTip(2000, Resources.OotdRunning, Resources.RightClickToConfigure, ToolTipIcon.Info);

            _logger.Debug("First Run");
        }

        _graphics = CreateGraphics();

        // setup update checker.
        _sparkle = new Sparkle(AppCastUrl, Resources.AppIcon);

        _sparkle.UpdateDetected += OnSparkleOnUpdateDetectedShowWithToast;
        _sparkle.UpdateWindowDismissed += OnSparkleOnUpdateWindowDismissed;
        _sparkle.CustomInstallerArguments = "/silent";

        // check for updates every 20 days, but don't check on first run because we'll have 2 tooltips popup and will likely confuse the user.
        _sparkle.StartLoop(!GlobalPreferences.IsFirstRun, TimeSpan.FromDays(20));
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x80; // Turn on WS_EX_TOOLWINDOW style bit to hide window from alt-tab
            cp.ExStyle |=
                0x02000000; // Turn on WS_EX_COMPOSITED to turn on double-buffering for the entire form and controls.
            return cp;
        }
    }

    public static int InstanceCount
    {
        get
        {
            var instanceCount = 0;

            using var appReg = Registry.CurrentUser.CreateSubKey(PreferencesRegistry.RootPath);

            instanceCount += appReg.GetSubKeyNames().Count(instanceName => instanceName != AutoUpdateInstanceName);

            return instanceCount;
        }
    }

    private static void OnSparkleOnUpdateWindowDismissed(object? sender, EventArgs args)
    {
        Startup.UpdateDetected = false;
    }

    private void OnSparkleOnUpdateDetectedShowWithToast(object sender, UpdateDetectedEventArgs args)
    {
        Startup.UpdateDetected = true;
        _sparkle.ShowUpdateNeededUI(args.LatestVersion, true);
    }

    private void OnSparkleOnUpdateDetectedShowWithoutToast(object sender, UpdateDetectedEventArgs args)
    {
        Startup.UpdateDetected = true;
        _sparkle.ShowUpdateNeededUI(args.LatestVersion, false);
    }

    public void LoadInstances()
    {
        _logger.Debug("Loading app settings from registry");

        using var appReg = Registry.CurrentUser.CreateSubKey(PreferencesRegistry.RootPath);
        var instanceNames = FilterInstanceNames(appReg.GetSubKeyNames()).ToArray();

        if (instanceNames.Length > 1)
        {
            var menu = CreateMultipleInstanceMenu();
            trayIcon.ContextMenuStrip = menu;
            var insertionIndex = 2;
            foreach (var instanceName in instanceNames)
            {
                var instance = GetOrCreateInstance(instanceName, out var newlyAdded);
                instance.InstanceRemoved -= InstanceRemovedEventHandler;
                instance.InstanceRemoved += InstanceRemovedEventHandler;
                instance.InstanceRenamed -= InstanceRenamedEventHandler;
                instance.InstanceRenamed += InstanceRenamedEventHandler;

                var submenu = CreateInstanceSubmenu(instance.TrayMenu, instanceName);
                submenu.DropDownOpened += InstanceContextMenu_DropDownOpened;
                menu.Items.Insert(insertionIndex++, submenu);
                ShowNewInstance(instance, newlyAdded);
            }
        }
        else
        {
            var instanceName = ResolveSingleInstanceName(instanceNames.Length, instanceNames, "Default Instance");
            var instance = GetOrCreateInstance(instanceName, out var newlyAdded);
            trayIcon.ContextMenuStrip = instance.TrayMenu;
            ConfigureSingleInstanceMenu(instance.TrayMenu, new Dictionary<string, EventHandler>
            {
                ["AddInstanceMenu"] = AddInstanceMenu_Click,
                ["StartWithWindows"] = StartWithWindowsMenu_Click,
                ["LockPositionMenu"] = LockPositionMenu_Click,
                [CheckForUpdatesMenuName] = CheckForUpdates_Click,
                [AboutMenuName] = AboutMenu_Click,
                [ResetConfigMenuName] = ResetConfigMenu_Click
            });
            ShowNewInstance(instance, newlyAdded);
        }

        ReorderBottomMenuItems();

        var startWithWindowsMenu = trayIcon.ContextMenuStrip.Items["StartWithWindows"] as ToolStripMenuItem;
        startWithWindowsMenu!.Checked = GlobalPreferences.StartWithWindows;

        var lockPositionMenu = trayIcon.ContextMenuStrip.Items["LockPositionMenu"] as ToolStripMenuItem;
        lockPositionMenu!.Checked = GlobalPreferences.LockPosition;
        LockOrUnlock(GlobalPreferences.LockPosition);
    }

    private MainForm GetOrCreateInstance(string instanceName, out bool newlyAdded)
    {
        newlyAdded = !_mainFormInstances.TryGetValue(instanceName, out var instance);
        if (newlyAdded)
        {
            _logger.Debug($"Instantiating instance {instanceName}");
            instance = new MainForm(instanceName);
            _mainFormInstances.Add(instanceName, instance);
        }

        return instance!;
    }

    private static void ShowNewInstance(MainForm instance, bool newlyAdded)
    {
        if (!newlyAdded)
        {
            return;
        }

        instance.Show();
        UnsafeNativeMethods.SendWindowToBack(instance);
    }

    private ContextMenuStrip CreateMultipleInstanceMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem(Resources.AddInstance, null, AddInstanceMenu_Click, "AddInstanceMenu"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem(Resources.StartWithWindows, null, StartWithWindowsMenu_Click,
            "StartWithWindows"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem(Resources.HideAll, null, HideShowAllMenu_Click, "HideShowMenu"));
        menu.Items.Add(new ToolStripMenuItem(Resources.LockPosition, null, LockPositionMenu_Click, "LockPositionMenu"));
        menu.Items.Add(new ToolStripMenuItem(Resources.DisableEditing, null, DisableEnableEditingMenu_Click,
            "DisableEnableEditingMenu"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem(Resources.About, null, AboutMenu_Click, AboutMenuName));
        menu.Items.Add(new ToolStripMenuItem(Resources.CheckForUpdates, null, CheckForUpdates_Click,
            CheckForUpdatesMenuName));
        menu.Items.Add(new ToolStripMenuItem(Resources.RestoreDefaults, null, ResetConfigMenu_Click, ResetConfigMenuName));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem(Resources.Exit, null, ExitMenu_Click, "ExitMenu"));
        return menu;
    }

    internal static ToolStripMenuItem CreateInstanceSubmenu(ContextMenuStrip menu, string instanceName)
    {
        if (!menu.Items.ContainsKey(instanceName))
        {
            menu.Items.Insert(0, new ToolStripMenuItem(instanceName)
            {
                Name = instanceName,
                BackColor = Color.Gainsboro
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

    private void ReorderBottomMenuItems()
    {
        ReorderBottomMenuItems(
            trayIcon.ContextMenuStrip,
            () => new ToolStripMenuItem(Resources.About, null, AboutMenu_Click, AboutMenuName),
            () => new ToolStripMenuItem(Resources.CheckForUpdates, null, CheckForUpdates_Click,
                CheckForUpdatesMenuName));
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

    private void ChangeTrayIconDate()
    {
        var today = DateTime.Now;

        // the timer ticks every second; only rebuild the icon when the day actually changes.
        if (_currentTrayIconDay == today.Day)
        {
            return;
        }

        // get new instance of the resource manager.  This will allow us to look up a resource by name.
        var resourceManager = new ResourceManager("OotD.Properties.Resources", typeof(Resources).Assembly);

        // find the icon for the today's day of the month and replace the tray icon with it, compensate for user's DPI settings.
        using var dateIcon = (Icon)resourceManager.GetObject("_" + today.Date.Day, CultureInfo.CurrentCulture)!;

        // dispose the outgoing icon so its GDI handle isn't leaked.
        var previousIcon = trayIcon.Icon;

        trayIcon.Icon = _graphics.DpiX < 96f
            ? new Icon(dateIcon, new Size(16, 16))
            : new Icon(dateIcon, new Size(32, 32));

        previousIcon?.Dispose();
        _currentTrayIconDay = today.Day;
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
        // update day of the month in the tray
        ChangeTrayIconDate();
    }

    private void ExitMenu_Click(object? sender, EventArgs e)
    {
        foreach (var formInstance in _mainFormInstances)
        {
            formInstance.Value.SaveCurrentViewSettings();
            formInstance.Value.Dispose();
        }

        Startup.DisposeOutlookObjects();
        Application.Exit();
    }

    private void AddInstanceMenu_Click(object? sender, EventArgs e)
    {
        var result = InputBox.Show(this, "", Resources.NewInstanceName, string.Empty, InputBox_Validating);
        if (!result.Ok)
        {
            return;
        }

        // trigger the tray icon context menu to show the second instance
        var mainForm = new MainForm(result.Text);
        mainForm.Dispose();

        LoadInstances();

        var newInstance = _mainFormInstances[result.Text];
        var occupiedBounds = _mainFormInstances
            .Where(instance => instance.Key != result.Text)
            .Select(instance => instance.Value.Bounds)
            .ToArray();

        var currentWorkingArea = Screen.FromHandle(Handle).WorkingArea;
        var orderedWorkingAreas = OrderWorkingAreas(currentWorkingArea,
            Screen.AllScreens.Select(screen => screen.WorkingArea));
        var preferredStart = GetCascadedStartPoint(currentWorkingArea, newInstance.Size, occupiedBounds);

        Point? selectedLocation = null;
        foreach (var area in orderedWorkingAreas)
        {
            var preferredForArea = area.Contains(preferredStart ?? Point.Empty)
                ? preferredStart
                : null;

            var candidate = FindNonOverlappingLocation(area, newInstance.Size, occupiedBounds, preferredForArea);
            var candidateBounds = new Rectangle(candidate, newInstance.Size);
            if (occupiedBounds.Any(existing => existing.IntersectsWith(candidateBounds)))
            {
                continue;
            }

            selectedLocation = candidate;
            break;
        }

        selectedLocation ??= FindNonOverlappingLocation(currentWorkingArea, newInstance.Size, occupiedBounds,
            preferredStart);

        newInstance.Left = selectedLocation.Value.X;
        newInstance.Top = selectedLocation.Value.Y;

        // Make sure the newly added instance is visible above existing windows.
        newInstance.BringToFront();
        newInstance.Activate();

        // Save the new position so that it's correctly loaded on next run
        newInstance.Preferences.Left = newInstance.Left;
        newInstance.Preferences.Top = newInstance.Top;
    }

    internal static Point FindNonOverlappingLocation(Rectangle workingArea, Size windowSize,
        IReadOnlyCollection<Rectangle> occupiedBounds, Point? preferredStart = null)
    {
        var maxX = Math.Max(workingArea.Left, workingArea.Right - windowSize.Width);
        var maxY = Math.Max(workingArea.Top, workingArea.Bottom - windowSize.Height);

        const int PlacementStep = 30;

        var bestLocation = new Point(workingArea.Left, workingArea.Top);
        var smallestOverlapArea = int.MaxValue;

        if (preferredStart.HasValue)
        {
            var preferred = new Point(
                Math.Min(Math.Max(preferredStart.Value.X, workingArea.Left), maxX),
                Math.Min(Math.Max(preferredStart.Value.Y, workingArea.Top), maxY));

            if (IsNonOverlappingCandidate(new Rectangle(preferred, windowSize), occupiedBounds, out var overlapArea))
            {
                return preferred;
            }

            smallestOverlapArea = overlapArea;
            bestLocation = preferred;
        }

        for (var y = workingArea.Top; y <= maxY; y += PlacementStep)
        {
            for (var x = workingArea.Left; x <= maxX; x += PlacementStep)
            {
                var candidate = new Rectangle(x, y, windowSize.Width, windowSize.Height);

                if (IsNonOverlappingCandidate(candidate, occupiedBounds, out var overlapArea))
                {
                    return candidate.Location;
                }

                if (overlapArea >= smallestOverlapArea)
                {
                    continue;
                }

                smallestOverlapArea = overlapArea;
                bestLocation = candidate.Location;
            }
        }

        return bestLocation;
    }

    internal static IReadOnlyList<Rectangle> OrderWorkingAreas(Rectangle currentWorkingArea,
        IEnumerable<Rectangle> allWorkingAreas)
    {
        return [.. allWorkingAreas.OrderByDescending(area => area == currentWorkingArea)];
    }

    internal static Point? GetCascadedStartPoint(Rectangle workingArea, Size windowSize,
        IReadOnlyCollection<Rectangle> occupiedBounds)
    {
        if (occupiedBounds.Count == 0)
        {
            return null;
        }

        var anchor = occupiedBounds
            .OrderByDescending(rect => rect.Top)
            .ThenByDescending(rect => rect.Left)
            .First();

        var maxX = Math.Max(workingArea.Left, workingArea.Right - windowSize.Width);
        var maxY = Math.Max(workingArea.Top, workingArea.Bottom - windowSize.Height);

        var x = Math.Min(Math.Max(anchor.Left + CascadeOffset, workingArea.Left), maxX);
        var y = Math.Min(Math.Max(anchor.Top + CascadeOffset, workingArea.Top), maxY);

        return new Point(x, y);
    }

    private static bool IsNonOverlappingCandidate(Rectangle candidate, IReadOnlyCollection<Rectangle> occupiedBounds,
        out int overlapArea)
    {
        overlapArea = 0;
        var intersectsExisting = false;

        foreach (var existing in occupiedBounds)
        {
            if (!candidate.IntersectsWith(existing))
            {
                continue;
            }

            intersectsExisting = true;
            var intersection = Rectangle.Intersect(candidate, existing);
            overlapArea += intersection.Width * intersection.Height;
        }

        return !intersectsExisting;
    }

    private static void InputBox_Validating(object? sender, InputBoxValidatingEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Text))
        {
            return;
        }

        e.Cancel = true;
        e.Message = Resources.ResourceManager.GetString("Required")!;
    }

    private static void AboutMenu_Click(object? sender, EventArgs e)
    {
        var aboutForm = new AboutBox();
        aboutForm.ShowDialog();
    }

    private void CheckForUpdates_Click(object? sender, EventArgs e)
    {
        _sparkle.UpdateDetected -= OnSparkleOnUpdateDetectedShowWithToast;
        _sparkle.UpdateDetected += OnSparkleOnUpdateDetectedShowWithoutToast;

        _sparkle.CheckForUpdatesAtUserRequest();
    }

    private void StartWithWindowsMenu_Click(object? sender, EventArgs e)
    {
        var startWithWindowsMenu = trayIcon.ContextMenuStrip!.Items["StartWithWindows"] as ToolStripMenuItem;
        if (startWithWindowsMenu is { Checked: true })
        {
            GlobalPreferences.StartWithWindows = false;
            startWithWindowsMenu.Checked = false;
        }
        else
        {
            GlobalPreferences.StartWithWindows = true;
            startWithWindowsMenu?.Checked = true;
        }
    }

    private void HideShowAllMenu_Click(object? sender, EventArgs e)
    {
        ShowHideAllInstances();
    }

    private void ShowHideAllInstances()
    {
        ShowHideInstances(trayIcon.ContextMenuStrip!,
            _mainFormInstances.Values.Select(instance => ((Form)instance, instance.TrayMenu)).ToArray());
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

    private void LockPositionMenu_Click(object? sender, EventArgs e)
    {
        var lockPositionMenu = trayIcon.ContextMenuStrip!.Items["LockPositionMenu"] as ToolStripMenuItem;
        if (lockPositionMenu is { Checked: true })
        {
            lockPositionMenu.Checked = false;
            LockOrUnlock(false);
        }
        else
        {
            lockPositionMenu?.Checked = true;

            LockOrUnlock(true);
        }
    }

    private void LockOrUnlock(bool @lock)
    {
        GlobalPreferences.LockPosition = @lock;

        foreach (var (_, formInstance) in _mainFormInstances)
        {
            formInstance.HeaderPanel.Visible = !@lock;
        }
    }

    private void DisableEnableEditingMenu_Click(object? sender, EventArgs e)
    {
        DisableEnableAllInstances();
    }

    private void ResetConfigMenu_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(this,
            Resources.RestoreDefaultsConfirmation,
            Resources.ConfirmationCaption,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            foreach (var (_, formInstance) in _mainFormInstances)
            {
                formInstance.Dispose();
            }

            _mainFormInstances.Clear();

            Registry.CurrentUser.DeleteSubKeyTree(ProductRegistryPath, false);

            LoadInstances();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error resetting configuration.");
            MessageBox.Show(this,
                Resources.ErrorInitializingApp + Environment.NewLine + ex.Message,
                Resources.ErrorCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void DisableEnableAllInstances()
    {
        var disableEditingMenu = trayIcon.ContextMenuStrip!.Items["DisableEnableEditingMenu"] as ToolStripMenuItem;

        if (disableEditingMenu is { Checked: true })
        {
            foreach (var (_, mainForm) in _mainFormInstances)
            {
                mainForm.Enabled = true;

                if (mainForm.TrayMenu.Items["DisableEnableEditingMenu"] is ToolStripMenuItem instanceDisableEditingMenu)
                {
                    instanceDisableEditingMenu.Checked = !instanceDisableEditingMenu.Checked;
                }
            }

            disableEditingMenu.Checked = false;
        }
        else
        {
            foreach (var (_, mainForm) in _mainFormInstances)
            {
                mainForm.Enabled = false;

                if (mainForm.TrayMenu.Items["DisableEnableEditingMenu"] is ToolStripMenuItem instanceDisableEditingMenu)
                {
                    instanceDisableEditingMenu.Checked = !instanceDisableEditingMenu.Checked;
                }
            }

            disableEditingMenu?.Checked = true;
        }
    }

    private void InstanceRemovedEventHandler(object? sender, InstanceRemovedEventArgs e)
    {
        // remove the menu item for the removed instance.
        trayIcon.ContextMenuStrip!.Items.RemoveByKey(e.InstanceName);
        _mainFormInstances.Remove(e.InstanceName);

        // if we only have one instance left, reload everything so that the context
        // menu only shows the one instances' menu items.
        if (_mainFormInstances.Count == 1)
        {
            LoadInstances();
        }
    }

    private void InstanceRenamedEventHandler(object? sender, InstanceRenamedEventArgs e)
    {
        var menuItem = trayIcon.ContextMenuStrip?.Items[e.OldInstanceName];
        if (menuItem != null)
        {
            menuItem.Text = e.NewInstanceName;
            menuItem.Name = e.NewInstanceName;
        }
    }

    private void InstanceContextMenu_DropDownOpened(object? sender, EventArgs e)
    {
        var item = sender as ToolStripDropDownItem;

        // flash the form so the user knows which one they're working with
        if (!backgroundWorker.IsBusy)
        {
            backgroundWorker.RunWorkerAsync(item);
        }
    }

    private void FlashForm(ToolStripDropDownItem dropDownItem)
    {
        // the instance may have been renamed or removed since the flash was queued.
        if (!_mainFormInstances.TryGetValue(dropDownItem.DropDownItems[0].Text!, out var formInstance))
        {
            return;
        }

        for (var i = 0; i < 2; i++)
        {
            var currentOpacity = formInstance.Opacity;
            formInstance.InvokeEx(_ => formInstance.Opacity = .3);
            Thread.Sleep(250);
            formInstance.InvokeEx(_ => formInstance.Opacity = currentOpacity);
            Thread.Sleep(250);
        }
    }

    private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ShowHideAllInstances();
        }
    }

    private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        var item = (ToolStripDropDownItem)e.Argument!;
        FlashForm(item);
    }

    private static string ProductRegistryPath => PreferencesRegistry.RootPath;
}
