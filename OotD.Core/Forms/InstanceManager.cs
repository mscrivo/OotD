// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32;
using NetSparkle;
using NLog;
using OotD.Events;
using OotD.Preferences;
using OotD.Properties;
using OotD.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace OotD.Forms
{
    public partial class InstanceManager : Form
    {
        private const string AppCastUrl = "https://outlookonthedesktop.com/ootdAppcast.xml";
        private const string AutoUpdateInstanceName = "AutoUpdate";

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, MainForm> _mainFormInstances = new Dictionary<string, MainForm>();
        private readonly Graphics _graphics;
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

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;             // Turn on WS_EX_TOOLWINDOW style bit to hide window from alt-tab
                cp.ExStyle |= 0x02000000;       // Turn on WS_EX_COMPOSITED to turn on double-buffering for the entire form and controls.
                return cp;
            }
        }

        public static int InstanceCount
        {
            get
            {
                var instanceCount = 0;

                using var appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName);
                if (appReg == null)
                {
                    return instanceCount;
                }

                // ReSharper disable once UselessBinaryOperation
                instanceCount += appReg.GetSubKeyNames().Count(instanceName => instanceName != AutoUpdateInstanceName);

                return instanceCount;
            }
        }

        public void LoadInstances()
        {
            _logger.Debug("Loading app settings from registry");

            // Each subkey in our main registry key represents an instance. 
            // Read each subkey and load the instance.
            using (var appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
            {
                _logger.Debug("Settings Found.");

                if (appReg != null)
                {
                    if (InstanceCount > 1)
                    {
                        _logger.Debug("Multiple instances to load");

                        // There are multiple instances defined, so we build the context menu strip dynamically.
                        trayIcon.ContextMenuStrip = new ContextMenuStrip();
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.AddInstance, null, AddInstanceMenu_Click, "AddInstanceMenu"));
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
                            if (!_mainFormInstances.ContainsKey(instanceName))
                            {
                                _logger.Debug($"Instantiating instance {instanceName}");
                                _mainFormInstances.Add(instanceName, new MainForm(instanceName));
                                newlyAdded = true;
                            }

                            // hook up the instance removed/renamed event handlers so that we can
                            // remove/rename the appropriate menu item from the context menu.
                            _mainFormInstances[instanceName].InstanceRemoved += InstanceRemovedEventHandler;
                            _mainFormInstances[instanceName].InstanceRenamed += InstanceRenamedEventHandler;

                            // create the submenu for the instance
                            instanceSubmenu[count] = new ToolStripMenuItem(instanceName, null, null, instanceName);
                            trayIcon.ContextMenuStrip.Items.Add(instanceSubmenu[count]);

                            // add the name of the instance to the top of the submenu so it's clear which
                            // instance the submenu belongs to.
                            if (!_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey(instanceName))
                            {
                                _mainFormInstances[instanceName].TrayMenu.Items.Insert(0, new ToolStripMenuItem(instanceName) { Name = instanceName });
                                _mainFormInstances[instanceName].TrayMenu.Items[0].BackColor = Color.Gainsboro;

                                if (!_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("AddInstanceSeparator"))
                                {
                                    _mainFormInstances[instanceName].TrayMenu.Items.Insert(1, new ToolStripSeparator { Name = "AddInstanceSeparator" });
                                }
                            }

                            // the submenu items are set to the context menu defined in the form's instance.
                            instanceSubmenu[count].DropDown = _mainFormInstances[instanceName].TrayMenu;
                            instanceSubmenu[count].DropDownOpened += InstanceContextMenu_DropDownOpened;

                            _mainFormInstances[instanceName].TrayMenu.Items["RemoveInstanceMenu"].Visible = true;
                            _mainFormInstances[instanceName].TrayMenu.Items["RenameInstanceMenu"].Visible = true;

                            if (_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("AddInstanceMenu"))
                            {
                                _mainFormInstances[instanceName].TrayMenu.Items["AddInstanceMenu"].Visible = false;
                            }
                            if (_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("AboutMenu"))
                            {
                                _mainFormInstances[instanceName].TrayMenu.Items["AboutMenu"].Visible = false;
                            }
                            if (_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("StartWithWindows"))
                            {
                                _mainFormInstances[instanceName].TrayMenu.Items["StartWithWindows"].Visible = false;
                            }
                            if (_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("LockPositionMenu"))
                            {
                                _mainFormInstances[instanceName].TrayMenu.Items["LockPositionMenu"].Visible = false;
                            }
                            if (_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("CheckForUpdatesMenu"))
                            {
                                _mainFormInstances[instanceName].TrayMenu.Items["CheckForUpdatesMenu"].Visible = false;
                            }

                            _mainFormInstances[instanceName].TrayMenu.Items["Separator6"].Visible = false;
                            _mainFormInstances[instanceName].TrayMenu.Items["ExitMenu"].Visible = false;

                            // finally, show the form
                            if (newlyAdded)
                            {
                                _logger.Debug($"Showing Instance {instanceName}");
                                _mainFormInstances[instanceName].Show();
                                UnsafeNativeMethods.SendWindowToBack(_mainFormInstances[instanceName]);
                            }
                            count++;
                        }

                        // add the rest of the necessary menu items to the main context menu.
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.StartWithWindows, null, StartWithWindowsMenu_Click, "StartWithWindows"));

                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.HideAll, null, HideShowAllMenu_Click, "HideShowMenu"));
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.LockPosition, null, LockPositionMenu_Click, "LockPositionMenu"));
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.DisableEditing, null, DisableEnableEditingMenu_Click, "DisableEnableEditingMenu"));

                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.About, null, AboutMenu_Click, "AboutMenu"));
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.CheckForUpdates, null, CheckForUpdates_Click, "CheckForUpdatesMenu"));

                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.Exit, null, ExitMenu_Click, "ExitMenu"));
                    }
                    else
                    {
                        // this is a first run, or there is only 1 instance defined.
                        const string defaultInstanceName = "Default Instance";

                        var instanceName = InstanceCount == 1 ? appReg.GetSubKeyNames()[0] : defaultInstanceName;

                        if (instanceName == AutoUpdateInstanceName)
                        {
                            instanceName = defaultInstanceName;
                        }

                        // create our instance and set the context menu to one defined in the form instance.
                        var newlyAdded = false;
                        if (!_mainFormInstances.ContainsKey(instanceName))
                        {
                            _mainFormInstances.Add(instanceName, new MainForm(instanceName));
                            newlyAdded = true;
                        }

                        trayIcon.ContextMenuStrip = _mainFormInstances[instanceName].TrayMenu;

                        // remove unnecessary menu items
                        while (trayIcon.ContextMenuStrip.Items.Count > 0 && trayIcon.ContextMenuStrip.Items[0].Text != Resources.Calendar)
                        {
                            trayIcon.ContextMenuStrip.Items.RemoveAt(0);
                        }

                        trayIcon.ContextMenuStrip.Items["RemoveInstanceMenu"].Visible = false;
                        trayIcon.ContextMenuStrip.Items["RenameInstanceMenu"].Visible = false;

                        if (_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("AddInstanceMenu"))
                        {
                            _mainFormInstances[instanceName].TrayMenu.Items["AddInstanceMenu"].Visible = true;
                        }
                        if (_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("AboutMenu"))
                        {
                            _mainFormInstances[instanceName].TrayMenu.Items["AboutMenu"].Visible = true;
                        }
                        if (_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("StartWithWindows"))
                        {
                            _mainFormInstances[instanceName].TrayMenu.Items["StartWithWindows"].Visible = true;
                        }
                        if (_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("LockPositionMenu"))
                        {
                            _mainFormInstances[instanceName].TrayMenu.Items["LockPositionMenu"].Visible = true;
                        }
                        if (_mainFormInstances[instanceName].TrayMenu.Items.ContainsKey("CheckForUpdatesMenu"))
                        {
                            _mainFormInstances[instanceName].TrayMenu.Items["CheckForUpdatesMenu"].Visible = true;
                        }

                        // add global menu items that don't apply to the instance.
                        if (!trayIcon.ContextMenuStrip.Items.ContainsKey("AddInstanceMenu"))
                        {
                            trayIcon.ContextMenuStrip.Items.Insert(0, new ToolStripMenuItem(Resources.AddInstance, null, AddInstanceMenu_Click, "AddInstanceMenu"));

                            trayIcon.ContextMenuStrip.Items.Insert(1, new ToolStripSeparator { Name = "AddInstanceSeparator" });
                        }

                        if (!trayIcon.ContextMenuStrip.Items.ContainsKey("StartWithWindows"))
                        {
                            trayIcon.ContextMenuStrip.Items.Insert(12, new ToolStripMenuItem(Resources.StartWithWindows, null, StartWithWindowsMenu_Click, "StartWithWindows"));
                        }

                        if (!trayIcon.ContextMenuStrip.Items.ContainsKey("LockPositionMenu"))
                        {
                            trayIcon.ContextMenuStrip.Items.Insert(15, new ToolStripMenuItem(Resources.LockPosition, null, LockPositionMenu_Click, "LockPositionMenu"));
                        }

                        if (!trayIcon.ContextMenuStrip.Items.ContainsKey("CheckForUpdatesMenu"))
                        {
                            trayIcon.ContextMenuStrip.Items.Insert(20, new ToolStripMenuItem(Resources.CheckForUpdates, null, CheckForUpdates_Click, "CheckForUpdatesMenu"));
                        }

                        if (!trayIcon.ContextMenuStrip.Items.ContainsKey("AboutMenu"))
                        {
                            trayIcon.ContextMenuStrip.Items.Insert(20, new ToolStripMenuItem(Resources.About, null, AboutMenu_Click, "AboutMenu"));
                        }

                        _mainFormInstances[instanceName].TrayMenu.Items["Separator6"].Visible = true;
                        _mainFormInstances[instanceName].TrayMenu.Items["ExitMenu"].Visible = true;

                        if (newlyAdded)
                        {
                            _mainFormInstances[instanceName].Show();
                            UnsafeNativeMethods.SendWindowToBack(_mainFormInstances[instanceName]);
                        }
                    }
                }
            }

            var startWithWindowsMenu = (ToolStripMenuItem)trayIcon.ContextMenuStrip.Items["StartWithWindows"];
            startWithWindowsMenu.Checked = GlobalPreferences.StartWithWindows;

            var lockPositionMenu = (ToolStripMenuItem)trayIcon.ContextMenuStrip.Items["LockPositionMenu"];
            lockPositionMenu.Checked = GlobalPreferences.LockPosition;
            LockOrUnlock(GlobalPreferences.LockPosition);
        }

        private void ChangeTrayIconDate()
        {
            // get new instance of the resource manager.  This will allow us to look up a resource by name.
            var resourceManager = new ResourceManager("OotD.Properties.Resources", typeof(Resources).Assembly);

            var today = DateTime.Now;

            // find the icon for the today's day of the month and replace the tray icon with it, compensate for user's DPI settings.
            var dateIcon = (Icon)resourceManager.GetObject("_" + today.Date.Day, CultureInfo.CurrentCulture)!;

            trayIcon.Icon = _graphics.DpiX.Equals(96f)
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
                formInstance.Value.Dispose();
            }
            Startup.DisposeOutlookObjects();
            Application.Exit();
        }

        private void AddInstanceMenu_Click(object? sender, EventArgs e)
        {
            var result = InputBox.Show(this, "", Resources.NewInstanceName, string.Empty, InputBox_Validating);
            if (result.Ok)
            {
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
                _mainFormInstances[result.Text].Preferences!.Left = _mainFormInstances[result.Text].Left;
                _mainFormInstances[result.Text].Preferences!.Top = _mainFormInstances[result.Text].Top;
            }
        }

        private static void InputBox_Validating(object? sender, InputBoxValidatingEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Text))
            {
                e.Cancel = true;
                e.Message = "Required";
            }
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
            var startWithWindowsMenu = (ToolStripMenuItem)trayIcon.ContextMenuStrip.Items["StartWithWindows"];
            if (startWithWindowsMenu.Checked)
            {
                GlobalPreferences.StartWithWindows = false;
                startWithWindowsMenu.Checked = false;
            }
            else
            {
                GlobalPreferences.StartWithWindows = true;
                startWithWindowsMenu.Checked = true;
            }
        }

        private void HideShowAllMenu_Click(object? sender, EventArgs e)
        {
            ShowHideAllInstances();
        }

        private void ShowHideAllInstances()
        {
            var hideShowMenuText = trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text;

            if (hideShowMenuText == Resources.HideAll || hideShowMenuText == Resources.Hide)
            {
                foreach (var formInstance in _mainFormInstances)
                {
                    formInstance.Value.Visible = false;
                    formInstance.Value.TrayMenu.Items["HideShowMenu"].Text = Resources.Show;
                }

                trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text = _mainFormInstances.Count == 1 ? Resources.Show : Resources.ShowAll;
            }
            else if (hideShowMenuText == Resources.ShowAll || hideShowMenuText == Resources.Show)
            {
                foreach (var formInstance in _mainFormInstances)
                {
                    formInstance.Value.Visible = true;
                    formInstance.Value.TrayMenu.Items["HideShowMenu"].Text = Resources.Hide;
                }

                trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text = _mainFormInstances.Count == 1 ? Resources.Hide : Resources.HideAll;
            }
        }

        private void LockPositionMenu_Click(object? sender, EventArgs e)
        {
            var lockPositionMenu = (ToolStripMenuItem)trayIcon.ContextMenuStrip.Items["LockPositionMenu"];
            if (lockPositionMenu.Checked)
            {
                lockPositionMenu.Checked = false;
                LockOrUnlock(false);
            }
            else
            {
                lockPositionMenu.Checked = true;
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
            var disableEditingMenu = (ToolStripMenuItem)trayIcon.ContextMenuStrip.Items["DisableEnableEditingMenu"];

            if (disableEditingMenu.Checked)
            {
                foreach (var formInstance in _mainFormInstances)
                {
                    formInstance.Value.Enabled = true;

                    var instanceDisableEditingMenu = (ToolStripMenuItem)formInstance.Value.TrayMenu.Items["DisableEnableEditingMenu"];
                    instanceDisableEditingMenu.Checked = !instanceDisableEditingMenu.Checked;
                }
                disableEditingMenu.Checked = false;
            }
            else
            {
                foreach (var formInstance in _mainFormInstances)
                {
                    formInstance.Value.Enabled = false;

                    var instanceDisableEditingMenu = (ToolStripMenuItem)formInstance.Value.TrayMenu.Items["DisableEnableEditingMenu"];
                    instanceDisableEditingMenu.Checked = !instanceDisableEditingMenu.Checked;
                }
                disableEditingMenu.Checked = true;
            }
        }

        private void InstanceRemovedEventHandler(object? sender, InstanceRemovedEventArgs e)
        {
            // remove the menu item for the removed instance.
            trayIcon.ContextMenuStrip.Items.RemoveByKey(e.InstanceName);
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
            trayIcon.ContextMenuStrip.Items[e.OldInstanceName].Text = e.NewInstanceName;
            trayIcon.ContextMenuStrip.Items[e.OldInstanceName].Name = e.NewInstanceName;
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
                var formInstance = _mainFormInstances[dropDownItem.DropDownItems[0].Text];

                var currentOpacity = formInstance.Opacity;
                formInstance.InvokeEx(f => formInstance.Opacity = .3);
                Thread.Sleep(250);
                formInstance.InvokeEx(f => formInstance.Opacity = currentOpacity);
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
            var item = (ToolStripDropDownItem)e.Argument;
            FlashForm(item);
        }
    }
}
