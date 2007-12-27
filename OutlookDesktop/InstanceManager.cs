using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using OutlookDesktop.Properties;

namespace OutlookDesktop
{
    public partial class InstanceManager : Form
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();

            InstanceManager instanceManager = new InstanceManager();
            instanceManager.LoadInstances();

            Application.Run(instanceManager);
        }

        private MainForm[] _mainFormInstances;

        public InstanceManager()
        {
            InitializeComponent();
        }

        private void LoadInstances()
        {
            // Close any open instances first.
            if (_mainFormInstances != null && _mainFormInstances.Length > 0)
            {
                foreach (MainForm form in _mainFormInstances)
                {
                    form.Dispose();
                }
            }

            // Each subkey in our main registry key represents an instance. 
            // Read each subkey and load the instance.
            using (RegistryKey appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
            {
                if (appReg.SubKeyCount > 1)
                {
                    // There are multiple instances defined, so we build the context menu strip dynamically.
                    trayIcon.ContextMenuStrip = new ContextMenuStrip();
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Add Instance", null, AddInstanceMenu_Click));
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                    _mainFormInstances = new MainForm[appReg.SubKeyCount];
                    ToolStripMenuItem[] instanceSubmenu = new ToolStripMenuItem[appReg.SubKeyCount];
                    int count = 0;

                    // each instance will get it's own submenu in the main context menu.
                    foreach (string instanceName in appReg.GetSubKeyNames())
                    {
                        _mainFormInstances[count] = new MainForm(instanceName);

                        // hook up the instance removed/renamed event handlers so that we can
                        // remove/rename the appropriate menu item from the context menu.
                        _mainFormInstances[count].InstanceRemoved += InstanceRemovedEventHandler;
                        _mainFormInstances[count].InstanceRenamed += InstanceRenamedEventHandler;

                        // create the submenu for the instance
                        instanceSubmenu[count] = new ToolStripMenuItem(instanceName, null, null, instanceName);
                        trayIcon.ContextMenuStrip.Items.Add(instanceSubmenu[count]);

                        // add the name of the instance to the top of the submenu so it's clear which
                        // instance the submenu belongs to.
                        _mainFormInstances[count].TrayMenu.Items.Insert(0, new ToolStripMenuItem(instanceName));
                        _mainFormInstances[count].TrayMenu.Items[0].BackColor = Color.Gainsboro;

                        // the submenu items are set to the contenxt menu defined in the form's instance.
                        instanceSubmenu[count].DropDown = _mainFormInstances[count].TrayMenu;
                        
                        _mainFormInstances[count].TrayMenu.Items["ExitMenu"].Visible = false;

                        // finally, show the form.
                        _mainFormInstances[count].Show();
                        count++;
                    }

                    // add the rest of the necessary menu items to the main context menu.
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Start With Windows", null, StartWithWindowsMenu_Click, "StartWithWindows"));
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Hide", null, HideShowAllMenu_Click, "HideShowMenu"));
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("About", null, AboutMenu_Click, "AboutMenu"));
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Exit", null, ExitMenu_Click, "ExitMenu"));
                }
                else
                {
                    // this is a first run, or there is only 1 instance defined.
                    String instanceName;
                    if (appReg.SubKeyCount == 1)
                        instanceName = appReg.GetSubKeyNames()[0];
                    else
                        instanceName = "Default Instance";

                    // create our instance and set the context menu to one defined in the form instance.
                    _mainFormInstances = new MainForm[1];
                    _mainFormInstances[0] = new MainForm(instanceName);
                    trayIcon.ContextMenuStrip = _mainFormInstances[0].TrayMenu;

                    // remove unnecessary menu items
                    trayIcon.ContextMenuStrip.Items["RemoveInstanceMenu"].Visible = false;
                    trayIcon.ContextMenuStrip.Items["RenameInstanceMenu"].Visible = false;

                    // add global menu items that don't apply to the instance.
                    trayIcon.ContextMenuStrip.Items.Insert(0, new ToolStripMenuItem("Add Instance", null, AddInstanceMenu_Click));
                    trayIcon.ContextMenuStrip.Items.Insert(1, new ToolStripSeparator());
                    trayIcon.ContextMenuStrip.Items.Insert(12, new ToolStripMenuItem("Start With Windows", null, StartWithWindowsMenu_Click, "StartWithWindows"));
                    trayIcon.ContextMenuStrip.Items.Insert(17, new ToolStripMenuItem("About", null, AboutMenu_Click, "AboutMenu"));

                    // finally, show the form.
                    _mainFormInstances[0].Show();
                }
            }

            ToolStripMenuItem startWithWindowsMenu = (ToolStripMenuItem)trayIcon.ContextMenuStrip.Items["StartWithWindows"];

            if (GlobalPreferences.StartWithWindows)
                startWithWindowsMenu.Checked = true;
            else
                startWithWindowsMenu.Checked = false;

        }

        private void ChangeTrayIconDate()
        {
            // get new instance of the resource manager.  This will allow us to look up a resource by name.
            System.Resources.ResourceManager resourceManager = new global::System.Resources.ResourceManager("OutlookDesktop.Properties.Resources", typeof(Resources).Assembly);

            DateTime today = DateTime.Now;

            // find the icon for the today's day of the month and replace the tray icon with it.
            trayIcon.Icon = (Icon)resourceManager.GetObject("_" + today.Date.Day, CultureInfo.CurrentCulture);
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            // update day of the month in the tray
            ChangeTrayIconDate();
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AddInstanceMenu_Click(object sender, EventArgs e)
        {
            InputBoxResult result = InputBox.Show(this, "", "New Instance Name", String.Empty, new InputBoxValidatingEventHandler(inputBox_Validating));
            if (result.Ok)
            {
                MainForm mainForm = new MainForm(result.Text);
                mainForm.Dispose();
                LoadInstances();
            }
        }

        private void inputBox_Validating(object sender, InputBoxValidatingEventArgs e)
        {
            if (String.IsNullOrEmpty(e.Text.Trim()))
            {
                e.Cancel = true;
                e.Message = "Required";
            }
        }

        private void AboutMenu_Click(object sender, EventArgs e)
        {
            AboutBox aboutForm = new AboutBox();
            aboutForm.ShowDialog();
        }

        private void StartWithWindowsMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem startWithWindowsMenu = (ToolStripMenuItem)trayIcon.ContextMenuStrip.Items["StartWithWindows"];
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
            if (trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text == "Hide")
            {
                foreach (MainForm form in _mainFormInstances)
                {
                    form.Visible = false;
                    form.TrayMenu.Items["HideShowMenu"].Text = "Show";
                }
                trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text = "Show";
            }
            else if (trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text == "Show")
            {
                foreach (MainForm form in _mainFormInstances)
                {
                    form.Visible = true;
                    form.TrayMenu.Items["HideShowMenu"].Text = "Hide";
                }
                trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text = "Hide";
            }
        }

        private void InstanceRemovedEventHandler(Object sender, InstanceRemovedEventArgs e)
        {
            // remove the menu item for the removed instance.
            trayIcon.ContextMenuStrip.Items.RemoveByKey(e.InstanceName);

            // if we only have one instance left, reload everything so that the context
            // menu only shows the one instances' menu items.
            LoadInstances();
        }

        private void InstanceRenamedEventHandler(Object sender, InstanceRenamedEventArgs e)
        {
            // remove the menu item for the removed instance.
            trayIcon.ContextMenuStrip.Items[e.OldInstanceName].Text = e.NewInstanceName;
            trayIcon.ContextMenuStrip.Items[e.OldInstanceName].Name = e.NewInstanceName;
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ShowHideAllInstances();                                 
        }
    }
}
