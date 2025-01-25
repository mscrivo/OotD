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

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly Graphics _graphics;
    private readonly Dictionary<string, MainForm> _mainFormInstances = [];
    private readonly Sparkle _sparkle;

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

            using var appReg =
                Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" +
                                                  Application.ProductName);

            // ReSharper disable once UselessBinaryOperation
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

        // Each subkey in our main registry key represents an instance. 
        // Read each subkey and load the instance.
        using (var appReg =
               Registry.CurrentUser.CreateSubKey(
                   "Software\\" + Application.CompanyName + "\\" + Application.ProductName))
        {
            _logger.Debug("Settings Found.");

            if (InstanceCount > 1)
            {
                _logger.Debug("Multiple instances to load");

                // There are multiple instances defined, so we build the context menu strip dynamically.
                trayIcon.ContextMenuStrip = new ContextMenuStrip();
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.AddInstance, null,
                    AddInstanceMenu_Click, "AddInstanceMenu"));
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                var instanceSubmenu = new ToolStripMenuItem[InstanceCount];
                var count = 0;

                // each instance will get it's own submenu in the main context menu.
                foreach (var instanceName in appReg.GetSubKeyNames())
                {
                    // Skip the key named "AutoUpdate" since it's the settings for the updater component
                    if (instanceName == AutoUpdateInstanceName)
                    {
                        continue;
                    }

                    var newlyAdded = false;
                    if (!_mainFormInstances.TryGetValue(instanceName, out var value))
                    {
                        _logger.Debug($"Instantiating instance {instanceName}");
                        value = new MainForm(instanceName);
                        _mainFormInstances.Add(instanceName, value);
                        newlyAdded = true;
                    }

                    value.InstanceRemoved += InstanceRemovedEventHandler;
                    value.InstanceRenamed += InstanceRenamedEventHandler;

                    // create the submenu for the instance
                    instanceSubmenu[count] = new ToolStripMenuItem(instanceName, null, null, instanceName);
                    trayIcon.ContextMenuStrip.Items.Add(instanceSubmenu[count]);

                    // add the name of the instance to the top of the submenu so it's clear which
                    // instance the submenu belongs to.
                    if (!value.TrayMenu.Items.ContainsKey(instanceName))
                    {
                        value.TrayMenu.Items.Insert(0,
                            new ToolStripMenuItem(instanceName) { Name = instanceName });
                        value.TrayMenu.Items[0].BackColor = Color.Gainsboro;

                        if (!value.TrayMenu.Items.ContainsKey("AddInstanceSeparator"))
                        {
                            value.TrayMenu.Items.Insert(1,
                                new ToolStripSeparator { Name = "AddInstanceSeparator" });
                        }
                    }

                    // the submenu items are set to the context menu defined in the form's instance.
                    instanceSubmenu[count].DropDown = value.TrayMenu;
                    instanceSubmenu[count].DropDownOpened += InstanceContextMenu_DropDownOpened;
                    value.TrayMenu.Items["RemoveInstanceMenu"]!.Visible = true;
                    value.TrayMenu.Items["RenameInstanceMenu"]!.Visible = true;

                    if (value.TrayMenu.Items.ContainsKey("AddInstanceMenu"))
                    {
                        value.TrayMenu.Items["AddInstanceMenu"]!.Visible = false;
                    }

                    if (value.TrayMenu.Items.ContainsKey("AboutMenu"))
                    {
                        value.TrayMenu.Items["AboutMenu"]!.Visible = false;
                    }

                    if (value.TrayMenu.Items.ContainsKey("StartWithWindows"))
                    {
                        value.TrayMenu.Items["StartWithWindows"]!.Visible = false;
                    }

                    if (value.TrayMenu.Items.ContainsKey("LockPositionMenu"))
                    {
                        value.TrayMenu.Items["LockPositionMenu"]!.Visible = false;
                    }

                    if (value.TrayMenu.Items.ContainsKey("CheckForUpdatesMenu"))
                    {
                        value.TrayMenu.Items["CheckForUpdatesMenu"]!.Visible = false;
                    }

                    value.TrayMenu.Items["Separator6"]!.Visible = false;
                    value.TrayMenu.Items["ExitMenu"]!.Visible = false;

                    // finally, show the form
                    if (newlyAdded)
                    {
                        _logger.Debug($"Showing Instance {instanceName}");
                        value.Show();
                        UnsafeNativeMethods.SendWindowToBack(value);
                    }

                    count++;
                }

                // add the rest of the necessary menu items to the main context menu.
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.StartWithWindows, null,
                    StartWithWindowsMenu_Click, "StartWithWindows"));

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.HideAll, null,
                    HideShowAllMenu_Click, "HideShowMenu"));
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.LockPosition, null,
                    LockPositionMenu_Click, "LockPositionMenu"));
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.DisableEditing, null,
                    DisableEnableEditingMenu_Click, "DisableEnableEditingMenu"));

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.About, null, AboutMenu_Click,
                    "AboutMenu"));
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.CheckForUpdates, null,
                    CheckForUpdates_Click, "CheckForUpdatesMenu"));

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.Exit, null, ExitMenu_Click,
                    "ExitMenu"));
            }
            else
            {
                // this is a first run, or there is only 1 instance defined.
                const string DefaultInstanceName = "Default Instance";

                var instanceName = InstanceCount == 1 ? appReg.GetSubKeyNames()[0] : DefaultInstanceName;

                if (instanceName == AutoUpdateInstanceName)
                {
                    instanceName = DefaultInstanceName;
                }

                // create our instance and set the context menu to one defined in the form instance.
                var newlyAdded = false;
                if (!_mainFormInstances.TryGetValue(instanceName, out var value))
                {
                    value = new MainForm(instanceName);
                    _mainFormInstances.Add(instanceName, value);
                    newlyAdded = true;
                }

                trayIcon.ContextMenuStrip = value.TrayMenu;

                // remove unnecessary menu items
                while (trayIcon.ContextMenuStrip.Items.Count > 0 &&
                       trayIcon.ContextMenuStrip.Items[0].Text != Resources.Calendar)
                {
                    trayIcon.ContextMenuStrip.Items.RemoveAt(0);
                }

                trayIcon.ContextMenuStrip.Items["RemoveInstanceMenu"]!.Visible = false;
                trayIcon.ContextMenuStrip.Items["RenameInstanceMenu"]!.Visible = false;

                if (value.TrayMenu.Items.ContainsKey("AddInstanceMenu"))
                {
                    value.TrayMenu.Items["AddInstanceMenu"]!.Visible = true;
                }

                if (value.TrayMenu.Items.ContainsKey("AboutMenu"))
                {
                    value.TrayMenu.Items["AboutMenu"]!.Visible = true;
                }

                if (value.TrayMenu.Items.ContainsKey("StartWithWindows"))
                {
                    value.TrayMenu.Items["StartWithWindows"]!.Visible = true;
                }

                if (value.TrayMenu.Items.ContainsKey("LockPositionMenu"))
                {
                    value.TrayMenu.Items["LockPositionMenu"]!.Visible = true;
                }

                if (value.TrayMenu.Items.ContainsKey("CheckForUpdatesMenu"))
                {
                    value.TrayMenu.Items["CheckForUpdatesMenu"]!.Visible = true;
                }

                // add global menu items that don't apply to the instance.
                if (!trayIcon.ContextMenuStrip.Items.ContainsKey("AddInstanceMenu"))
                {
                    trayIcon.ContextMenuStrip.Items.Insert(0,
                        new ToolStripMenuItem(Resources.AddInstance, null, AddInstanceMenu_Click, "AddInstanceMenu"));

                    trayIcon.ContextMenuStrip.Items.Insert(1, new ToolStripSeparator { Name = "AddInstanceSeparator" });
                }

                if (!trayIcon.ContextMenuStrip.Items.ContainsKey("StartWithWindows"))
                {
                    trayIcon.ContextMenuStrip.Items.Insert(12,
                        new ToolStripMenuItem(Resources.StartWithWindows, null, StartWithWindowsMenu_Click,
                            "StartWithWindows"));
                }

                if (!trayIcon.ContextMenuStrip.Items.ContainsKey("LockPositionMenu"))
                {
                    trayIcon.ContextMenuStrip.Items.Insert(15,
                        new ToolStripMenuItem(Resources.LockPosition, null, LockPositionMenu_Click,
                            "LockPositionMenu"));
                }

                if (!trayIcon.ContextMenuStrip.Items.ContainsKey("CheckForUpdatesMenu"))
                {
                    trayIcon.ContextMenuStrip.Items.Insert(20,
                        new ToolStripMenuItem(Resources.CheckForUpdates, null, CheckForUpdates_Click,
                            "CheckForUpdatesMenu"));
                }

                if (!trayIcon.ContextMenuStrip.Items.ContainsKey("AboutMenu"))
                {
                    trayIcon.ContextMenuStrip.Items.Insert(20,
                        new ToolStripMenuItem(Resources.About, null, AboutMenu_Click, "AboutMenu"));
                }

                value.TrayMenu.Items["Separator6"]!.Visible = true;
                value.TrayMenu.Items["ExitMenu"]!.Visible = true;

                if (newlyAdded)
                {
                    value.Show();
                    UnsafeNativeMethods.SendWindowToBack(value);
                }
            }
        }

        var startWithWindowsMenu = trayIcon.ContextMenuStrip.Items["StartWithWindows"] as ToolStripMenuItem;
        startWithWindowsMenu!.Checked = GlobalPreferences.StartWithWindows;

        var lockPositionMenu = trayIcon.ContextMenuStrip.Items["LockPositionMenu"] as ToolStripMenuItem;
        lockPositionMenu!.Checked = GlobalPreferences.LockPosition;
        LockOrUnlock(GlobalPreferences.LockPosition);
    }

    private void ChangeTrayIconDate()
    {
        // get new instance of the resource manager.  This will allow us to look up a resource by name.
        var resourceManager = new ResourceManager("OotD.Properties.Resources", typeof(Resources).Assembly);

        var today = DateTime.Now;

        // find the icon for the today's day of the month and replace the tray icon with it, compensate for user's DPI settings.
        var dateIcon = (Icon)resourceManager.GetObject("_" + today.Date.Day, CultureInfo.CurrentCulture)!;

        trayIcon.Icon = _graphics.DpiX < 96f
            ? new Icon(dateIcon, new Size(16, 16))
            : new Icon(dateIcon, new Size(32, 32));
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

        // reposition the newly added instance so that it's not directly on top of the previous one
        var rnd = new Random();
        var xLoc = rnd.Next(0, Screen.FromHandle(Handle).WorkingArea.Width - _mainFormInstances[result.Text].Width);
        var yLoc = rnd.Next(0, Screen.FromHandle(Handle).WorkingArea.Height - _mainFormInstances[result.Text].Height);
        _mainFormInstances[result.Text].Left = xLoc;
        _mainFormInstances[result.Text].Top = yLoc;

        // Save the new position so that it's correctly loaded on next run
        _mainFormInstances[result.Text].Preferences.Left = _mainFormInstances[result.Text].Left;
        _mainFormInstances[result.Text].Preferences.Top = _mainFormInstances[result.Text].Top;
    }

    private static void InputBox_Validating(object? sender, InputBoxValidatingEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Text))
        {
            return;
        }

        e.Cancel = true;
        e.Message = "Required";
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
            if (startWithWindowsMenu != null)
            {
                startWithWindowsMenu.Checked = true;
            }
        }
    }

    private void HideShowAllMenu_Click(object? sender, EventArgs e)
    {
        ShowHideAllInstances();
    }

    private void ShowHideAllInstances()
    {
        var hideShowMenuText = trayIcon.ContextMenuStrip!.Items["HideShowMenu"]!.Text;

        if (hideShowMenuText == Resources.HideAll || hideShowMenuText == Resources.Hide)
        {
            foreach (var (_, mainForm) in _mainFormInstances)
            {
                mainForm.Visible = false;
                mainForm.TrayMenu.Items["HideShowMenu"]!.Text = Resources.Show;
            }

            trayIcon.ContextMenuStrip.Items["HideShowMenu"]!.Text =
                _mainFormInstances.Count == 1 ? Resources.Show : Resources.ShowAll;
        }
        else if (hideShowMenuText == Resources.ShowAll || hideShowMenuText == Resources.Show)
        {
            foreach (var (_, mainForm) in _mainFormInstances)
            {
                mainForm.Visible = true;
                mainForm.TrayMenu.Items["HideShowMenu"]!.Text = Resources.Hide;
            }

            trayIcon.ContextMenuStrip.Items["HideShowMenu"]!.Text =
                _mainFormInstances.Count == 1 ? Resources.Hide : Resources.HideAll;
        }
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
            if (lockPositionMenu != null)
            {
                lockPositionMenu.Checked = true;
            }

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

            if (disableEditingMenu != null)
            {
                disableEditingMenu.Checked = true;
            }
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
        // remove the menu item for the removed instance.
        trayIcon.ContextMenuStrip!.Items[e.OldInstanceName]!.Text = e.NewInstanceName;
        trayIcon.ContextMenuStrip.Items[e.OldInstanceName]!.Name = e.NewInstanceName;
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
        for (var i = 0; i < 2; i++)
        {
            var formInstance = _mainFormInstances[dropDownItem.DropDownItems[0].Text!];

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
}
