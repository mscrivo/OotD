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

        private MainForm[] mainFormInstances;

        public InstanceManager()
        {
            InitializeComponent();
        }

        private void LoadInstances()
        {
            // close any open instances first.
            if (mainFormInstances != null && mainFormInstances.Length > 0)
            {
                foreach (MainForm form in mainFormInstances)
                {
                    form.Dispose();
                }
            }

            using (RegistryKey appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
            {
                if (appReg.SubKeyCount > 1)
                {
                    // multiple instances defined, built context menu strip dynamically.
                    trayIcon.ContextMenuStrip = new ContextMenuStrip();
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Add Instance", null, AddInstanceMenu_Click));
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                    mainFormInstances = new MainForm[appReg.SubKeyCount];
                    ToolStripMenuItem[] instanceSubmenu = new ToolStripMenuItem[appReg.SubKeyCount];
                    int count = 0;

                    // create instance for each key we found in the registry,
                    // also create the context menu for each.
                    foreach (string instanceName in appReg.GetSubKeyNames())
                    {
                        mainFormInstances[count] = new MainForm(instanceName);

                        // hook up our instance removed event handler so that we can remove the 
                        // appropriate menu item from the context menu.
                        mainFormInstances[count].InstanceRemoved += InstanceRemovedEventHandler;
                        instanceSubmenu[count] = new ToolStripMenuItem(instanceName, null, null, instanceName);
                        trayIcon.ContextMenuStrip.Items.Add(instanceSubmenu[count]);
                        mainFormInstances[count].TrayMenu.Items.Insert(0, new ToolStripMenuItem(instanceName));
                        mainFormInstances[count].TrayMenu.Items[0].BackColor = Color.Gainsboro;
                        instanceSubmenu[count].DropDown = mainFormInstances[count].TrayMenu;
                        mainFormInstances[count].TrayMenu.Items["ExitMenu"].Visible = false;
                        mainFormInstances[count].Show();
                        count++;
                    }

                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("About", null, AboutMenu_Click, "AboutMenu"));
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Start With Windows", null, StartWithWindowsMenu_Click, "StartWithWindows"));
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Hide All", null, HideShowAllMenu_Click, "HideShowAllMenu"));
                    trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Exit", null, ExitMenu_Click, "ExitMenu"));
                }
                else
                {
                    // first run, or only 1 instance defined.
                    String instanceName;
                    if (appReg.SubKeyCount == 1)
                        instanceName = appReg.GetSubKeyNames()[0];
                    else
                        instanceName = "Default Instance";

                    mainFormInstances = new MainForm[1];
                    mainFormInstances[0] = new MainForm(instanceName);
                    trayIcon.ContextMenuStrip = mainFormInstances[0].TrayMenu;
                    mainFormInstances[0].InstanceRemoved += InstanceRemovedEventHandler;
                    trayIcon.ContextMenuStrip.Items["RemoveInstanceMenu"].Visible = false;
                    trayIcon.ContextMenuStrip.Items["RenameInstanceMenu"].Visible = false;
                    trayIcon.ContextMenuStrip.Items.Insert(0, new ToolStripMenuItem("Add Instance", null, AddInstanceMenu_Click));
                    trayIcon.ContextMenuStrip.Items.Insert(1, new ToolStripSeparator());
                    trayIcon.ContextMenuStrip.Items.Insert(12, new ToolStripMenuItem("Start With Windows", null, StartWithWindowsMenu_Click, "StartWithWindows"));
                    mainFormInstances[0].Show();
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
            InputBoxResult result = InputBox.Show(this, "", "New Instance Name", String.Empty, new InputBoxValidatingHandler(inputBox_Validating));
            if (result.OK)
            {
                MainForm mainForm = new MainForm(result.Text);
                mainForm.Dispose();
                LoadInstances();
            }
        }

        private void inputBox_Validating(object sender, InputBoxValidatingArgs e)
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
            if (trayIcon.ContextMenuStrip.Items["HideShowAllMenu"].Text == "Hide All")
            {
                foreach (MainForm form in mainFormInstances)
                {
                    form.Visible = false;
                    form.TrayMenu.Items["HideMenu"].Text = "Show";
                }
                trayIcon.ContextMenuStrip.Items["HideShowAllMenu"].Text = "Show All";
            }
            else if (trayIcon.ContextMenuStrip.Items["HideShowAllMenu"].Text == "Show All")
            {
                foreach (MainForm form in mainFormInstances)
                {
                    form.Visible = true;
                    form.TrayMenu.Items["HideMenu"].Text = "Hide";
                }
                trayIcon.ContextMenuStrip.Items["HideShowAllMenu"].Text = "Hide All";
            }
        }

        /// <summary>
        /// Event handler for when an instance is removed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstanceRemovedEventHandler(Object sender, InstanceRemovedEventArgs e)
        {
            // remove the menu item for the removed instance.
            trayIcon.ContextMenuStrip.Items.RemoveByKey(e.InstanceName);

            // if we only have one instance left, reload everything so that the context
            // menu only shows the one instances' menu items.
            LoadInstances();
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ShowHideAllInstances();
        }
    }
}
