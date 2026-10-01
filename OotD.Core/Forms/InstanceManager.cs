using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using NetSparkle;
using NLog;
using OotD.Events;
using OotD.Preferences;
using OotD.Properties;
using OotD.Utility;
using static OotD.Forms.InstanceManagerTrayPolicy;

namespace OotD.Forms;

/// <summary>
///     Owns the tray icon and every <see cref="MainForm" /> instance. The testable menu, tray icon and
///     placement logic lives in <see cref="InstanceManagerTrayPolicy" /> and
///     <see cref="InstanceManagerPlacementPolicy" />; what remains here wires Outlook-hosted forms, the
///     update checker and modal dialogs together.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Shell that hosts Outlook-backed MainForms, NetSparkle and modal dialogs.")]
public partial class InstanceManager : Form
{
    private const string AppCastUrl = "https://outlookonthedesktop.com/ootdAppcast.xml";

    // Release installers are Authenticode-signed through the SignPath Foundation's open source program. The updater
    // refuses to run a downloaded installer that isn't validly signed by this publisher.
    private const string TrustedInstallerPublisher = "SignPath Foundation";

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
        _sparkle.TrustedInstallerPublisher = TrustedInstallerPublisher;

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
        return InstanceManagerTrayPolicy.CreateMultipleInstanceMenu(new Dictionary<string, EventHandler>
        {
            ["AddInstanceMenu"] = AddInstanceMenu_Click,
            ["StartWithWindows"] = StartWithWindowsMenu_Click,
            ["HideShowMenu"] = HideShowAllMenu_Click,
            ["LockPositionMenu"] = LockPositionMenu_Click,
            ["DisableEnableEditingMenu"] = DisableEnableEditingMenu_Click,
            [AboutMenuName] = AboutMenu_Click,
            [CheckForUpdatesMenuName] = CheckForUpdates_Click,
            [ResetConfigMenuName] = ResetConfigMenu_Click,
            ["ExitMenu"] = ExitMenu_Click
        });
    }

    private void ReorderBottomMenuItems()
    {
        InstanceManagerTrayPolicy.ReorderBottomMenuItems(
            trayIcon.ContextMenuStrip,
            () => new ToolStripMenuItem(Resources.About, null, AboutMenu_Click, AboutMenuName),
            () => new ToolStripMenuItem(Resources.CheckForUpdates, null, CheckForUpdates_Click,
                CheckForUpdatesMenuName));
    }

    private void ChangeTrayIconDate()
    {
        var today = DateTime.Now;

        // the timer ticks every second; only rebuild the icon when the day actually changes.
        if (_currentTrayIconDay == today.Day)
        {
            return;
        }

        // dispose the outgoing icon so its GDI handle isn't leaked.
        var previousIcon = trayIcon.Icon;

        trayIcon.Icon = CreateDateIcon(today.Day, _graphics.DpiX);

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
        var result = InputBox.Show(this, "", Resources.NewInstanceName, string.Empty,
            CreateInstanceNameValidator(PreferencesRegistry.GetSubKeyNames()));
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

        var selectedLocation = InstanceManagerPlacementPolicy.SelectNewInstanceLocation(
            Screen.FromHandle(Handle).WorkingArea,
            Screen.AllScreens.Select(screen => screen.WorkingArea),
            newInstance.Size,
            occupiedBounds);

        newInstance.Left = selectedLocation.X;
        newInstance.Top = selectedLocation.Y;

        // Make sure the newly added instance is visible above existing windows.
        newInstance.BringToFront();
        newInstance.Activate();

        // Save the new position so that it's correctly loaded on next run
        newInstance.Preferences.Left = newInstance.Left;
        newInstance.Preferences.Top = newInstance.Top;
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
        ToggleMenuCheck(trayIcon.ContextMenuStrip!.Items["StartWithWindows"],
            check => GlobalPreferences.StartWithWindows = check);
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

    private void LockPositionMenu_Click(object? sender, EventArgs e)
    {
        ToggleMenuCheck(trayIcon.ContextMenuStrip!.Items["LockPositionMenu"], LockOrUnlock);
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
        ToggleEditing(trayIcon.ContextMenuStrip!.Items["DisableEnableEditingMenu"],
            _mainFormInstances.Values.Select(mainForm => ((Control)mainForm, mainForm.TrayMenu)));
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
        // Re-key the instance so LoadInstances finds it under its new name instead of opening a duplicate.
        if (_mainFormInstances.Remove(e.OldInstanceName, out var instance))
        {
            _mainFormInstances[e.NewInstanceName] = instance;
        }

        RenameInstanceMenuItem(trayIcon.ContextMenuStrip, e.OldInstanceName, e.NewInstanceName);
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
