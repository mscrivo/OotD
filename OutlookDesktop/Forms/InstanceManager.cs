using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using NLog;
using OutlookDesktop.Properties;

namespace OutlookDesktop.Forms
{
    public partial class InstanceManager : Form
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, MainForm> _mainFormInstances = new Dictionary<string, MainForm>();

        public InstanceManager()
        {
            InitializeComponent();

            if (GlobalPreferences.IsFirstRun)
            {
                trayIcon.ShowBalloonTip(2000, Resources.OotdRunning, Resources.RightClickToConfigure, ToolTipIcon.Info);

                Logger.Debug("First Run");
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Turn on WS_EX_TOOLWINDOW style bit to hide window from alt-tab
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        public void LoadInstances()
        {
            Logger.Debug("Loading app settings from registry");

            // Each subkey in our main registry key represents an instance. 
            // Read each subkey and load the instance.
            using (var appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
            {
                Logger.Debug("Settings Found.");
                if (appReg != null)
                {
                    if (appReg.SubKeyCount > 1)
                    {
                        Logger.Debug("Multiple instances to load");

                        // There are multiple instances defined, so we build the context menu strip dynamically.
                        trayIcon.ContextMenuStrip = new ContextMenuStrip();
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.AddInstance, null,
                                                                                  AddInstanceMenu_Click,
                                                                                  "AddInstanceMenu"));
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                        var instanceSubmenu = new ToolStripMenuItem[appReg.SubKeyCount];
                        int count = 0;

                        // each instance will get it's own submenu in the main context menu.
                        foreach (string instanceName in appReg.GetSubKeyNames())
                        {
                            bool newlyAdded = false;
                            if (!_mainFormInstances.ContainsKey(instanceName))
                            {
                                Logger.Debug(String.Format("Instantiating instance {0}", instanceName));
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
                            _mainFormInstances[instanceName].TrayMenu.Items["ExitMenu"].Visible = false;

                            // finally, show the form
                            if (newlyAdded)
                            {
                                Logger.Debug(string.Format("Showing Instance {0}", instanceName));
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
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.Exit, null, ExitMenu_Click, "ExitMenu"));
                    }
                    else
                    {
                        // this is a first run, or there is only 1 instance defined.
                        string instanceName = appReg.SubKeyCount == 1 ? appReg.GetSubKeyNames()[0] : "Default Instance";

                        // create our instance and set the context menu to one defined in the form instance.
                        bool newlyAdded = false;
                        if (!_mainFormInstances.ContainsKey(instanceName))
                        {
                            _mainFormInstances.Add(instanceName, new MainForm(instanceName));
                            newlyAdded = true;
                        }

                        trayIcon.ContextMenuStrip = _mainFormInstances[instanceName].TrayMenu;

                        // remove unnecessary menu items
                        while (trayIcon.ContextMenuStrip.Items.Count > 0 &&
                               trayIcon.ContextMenuStrip.Items[0].Text != Resources.Calendar)
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

                        if (!trayIcon.ContextMenuStrip.Items.ContainsKey("AboutMenu"))
                        {
                            trayIcon.ContextMenuStrip.Items.Insert(19, new ToolStripMenuItem(Resources.About, null, AboutMenu_Click, "AboutMenu"));
                        }
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
        }

        private void ChangeTrayIconDate()
        {
            // get new instance of the resource manager.  This will allow us to look up a resource by name.
            var resourceManager = new ResourceManager("OutlookDesktop.Properties.Resources", typeof(Resources).Assembly);

            DateTime today = DateTime.Now;

            // find the icon for the today's day of the month and replace the tray icon with it.
            trayIcon.Icon = (Icon)resourceManager.GetObject("_" + today.Date.Day, CultureInfo.CurrentCulture);
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // update day of the month in the tray
            ChangeTrayIconDate();
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            foreach (var formInstance in _mainFormInstances)
            {
                formInstance.Value.Dispose();
            }
            Application.Exit();
        }

        private void AddInstanceMenu_Click(object sender, EventArgs e)
        {
            InputBoxResult result = InputBox.Show(this, "", Resources.NewInstanceName, String.Empty, InputBox_Validating);
            if (result.Ok)
            {
                // trigger the tray icon context menu to show the second instance
                var mainForm = new MainForm(result.Text);
                mainForm.Dispose();

                LoadInstances();

                // reposition the newly added instance so that it's not directly on top of the previous one
                _mainFormInstances[result.Text].Left = _mainFormInstances[result.Text].Left + 400;
                _mainFormInstances[result.Text].Top = _mainFormInstances[result.Text].Top + 200;
            }
        }

        private static void InputBox_Validating(object sender, InputBoxValidatingEventArgs e)
        {
            if (String.IsNullOrEmpty(e.Text.Trim()))
            {
                e.Cancel = true;
                e.Message = "Required";
            }
        }

        private static void AboutMenu_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutBox();
            aboutForm.ShowDialog();
        }

        private void StartWithWindowsMenu_Click(object sender, EventArgs e)
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

        private void HideShowAllMenu_Click(object sender, EventArgs e)
        {
            ShowHideAllInstances();
        }

        private void ShowHideAllInstances()
        {
            string hideShowMenuText = trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text;

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

        private void LockPositionMenu_Click(object sender, EventArgs e)
        {
            var lockPositionMenu = (ToolStripMenuItem)trayIcon.ContextMenuStrip.Items["LockPositionMenu"];
            if (lockPositionMenu.Checked)
            {
                GlobalPreferences.LockPosition = false;
                lockPositionMenu.Checked = false;

                foreach (var formInstance in _mainFormInstances)
                {
                    formInstance.Value.TransparencySlider.Enabled = true;
                }
            }
            else
            {
                GlobalPreferences.LockPosition = true;
                lockPositionMenu.Checked = true;

                foreach (var formInstance in _mainFormInstances)
                {
                    formInstance.Value.TransparencySlider.Enabled = false;
                }
            }
        }

        private void DisableEnableEditingMenu_Click(object sender, EventArgs e)
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

        private void InstanceRemovedEventHandler(Object sender, InstanceRemovedEventArgs e)
        {
            // remove the menu item for the removed instance.
            trayIcon.ContextMenuStrip.Items.RemoveByKey(e.InstanceName);
            _mainFormInstances.Remove(e.InstanceName);

            // if we only have one instance left, reload everything so that the context
            // menu only shows the one instances' menu items.
            if (_mainFormInstances.Count == 1)
                LoadInstances();
        }

        private void InstanceRenamedEventHandler(Object sender, InstanceRenamedEventArgs e)
        {
            // remove the menu item for the removed instance.
            trayIcon.ContextMenuStrip.Items[e.OldInstanceName].Text = e.NewInstanceName;
            trayIcon.ContextMenuStrip.Items[e.OldInstanceName].Name = e.NewInstanceName;
        }

        private void InstanceContextMenu_DropDownOpened(object sender, EventArgs e)
        {
            var item = sender as ToolStripDropDownItem;

            // flash the form so the user knows which one they're working with
            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync(item);
            }
        }

        private void FlashForm(ToolStripDropDownItem dropDownitem)
        {
            if (dropDownitem != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    MainForm formInstance = _mainFormInstances[dropDownitem.DropDownItems[0].Text];

                    double currentOpacity = formInstance.Opacity;
                    formInstance.InvokeEx(f => formInstance.Opacity = .3);
                    Thread.Sleep(250);
                    formInstance.InvokeEx(f => formInstance.Opacity = currentOpacity);
                    Thread.Sleep(250);
                }
            }
        }

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ShowHideAllInstances();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var item = (ToolStripDropDownItem)e.Argument;
            FlashForm(item);
        }
    }
}