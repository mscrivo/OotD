using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using OutlookDesktop.Properties;

namespace OutlookDesktop
{
    public enum FolderViewTypes
    {
        Inbox,
        Calendar,
        Contacts,
        Notes,
        Tasks,
    }

    /// <summary>
    /// The main form for this app.  A borderless window which hosts the Microsoft Outlook View Control ActiveX component.
    /// </summary>
    public class FormMain : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.ContextMenu trayMenu;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuPrefs;
        private System.Windows.Forms.MenuItem menuExit;
        private Timer updateTimer;
        private MenuItem menuInbox;
        private MenuItem menuCalendar;
        private MenuItem menuItem4;
        private MenuItem menuContacts;
        private MenuItem menuTasks;
        private MenuItem menuNotes;
        private MenuItem menuAbout;
        private MenuItem menuSelectFolder;

        private Preferences prefs;

        private const int SWP_DRAWFRAME = 0x20;
        private const int SWP_NOMOVE = 0x2;
        private const int SWP_NOSIZE = 0x1;
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_NOACTIVATE = 0x10;
        private const int HWND_BOTTOM = 0x1;

        private MenuItem menuHide;
        private Microsoft.Office.Interop.Outlook.Application oApp;
        private Microsoft.Office.Interop.Outlook.NameSpace oNsp;
        private DateTime prevDateTime;
        private AxMicrosoft.Office.Interop.OutlookViewCtl.AxViewCtl axOutlookViewControl;
        private String customFolder;

        public FormMain()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            oApp = new Microsoft.Office.Interop.Outlook.Application();
            oNsp = oApp.GetNamespace("MAPI");
            axOutlookViewControl.Folder = FolderViewTypes.Calendar.ToString();
            axOutlookViewControl.View = "Day/Week/Month";

            this.LoadSettings();

            // Make window "Always on Bottom" i.e. pinned to desktop, so that
            // other windows don't get trapped behind it.  We do this, by attaching
            // the form to the "Progman" window, which is the main window in Windows.
            try
            {
                if (System.Environment.OSVersion.Version.Major < 6)
                {
                    // Older version of windows or DWM is not enabled

                    this.SendToBack();
                    IntPtr pWnd = UnsafeNativeMethods.FindWindow("Progman", null);
                    UnsafeNativeMethods.SetParent(this.Handle, pWnd);
                }
                else
                {
                    // Vista or Above
                    // TODO: Find a better way, this sucks!
                    UnsafeNativeMethods.SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                }
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(this, ex.Message, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        public Preferences Prefs
        {
            get
            {
                return prefs;
            }
        }

        /// <summary>
        /// Loads user preferences from registry and applies them.
        /// </summary>
        public void LoadSettings()
        {
            // create a new instance of the preferences class
            this.prefs = new Preferences();

            try
            {
                this.Opacity = prefs.Opacity;
            }
            catch (Exception)
            {
                // use default if there was a problem
                this.Opacity = Preferences.DefaultOpacity;
                MessageBox.Show(this, Resources.ErrorSettingOpacity, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            try
            {
                this.Left = prefs.Left;
                this.Top = prefs.Top;
                this.Width = prefs.Width;
                this.Height = prefs.Height;
            }
            catch (Exception)
            {
                // use defaults if there was a problem
                this.Left = Preferences.DefaultTopPosition;
                this.Top = Preferences.DefaultLeftPosition;
                this.Width = Preferences.DefaultWidth;
                this.Height = Preferences.DefaultHeight;
                MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }


            if (prefs.FolderViewType == FolderViewTypes.Calendar.ToString())
            {
                menuCalendar.Checked = true;
            }
            else if (prefs.FolderViewType == FolderViewTypes.Contacts.ToString())
            {
                menuContacts.Checked = true;
            }
            else if (prefs.FolderViewType == FolderViewTypes.Inbox.ToString())
            {
                menuInbox.Checked = true;
            }
            else if (prefs.FolderViewType == FolderViewTypes.Notes.ToString())
            {
                menuNotes.Checked = true;
            }
            else if (prefs.FolderViewType == FolderViewTypes.Tasks.ToString())
            {
                menuTasks.Checked = true;
            }
            else
            {
                // custom folder
                customFolder = prefs.FolderViewType;
                String folderName = GetFolderNameFromFullPath(customFolder, oNsp.Folders);
                MenuItem item = new MenuItem(folderName, new System.EventHandler(this.menuCustomFolder_Click));
                trayMenu.MenuItems.Add(0, item);
                trayMenu.MenuItems[0].Checked = true;
            }

            axOutlookViewControl.Folder = prefs.FolderViewType;
        }

        private String GetFolderNameFromFullPath(String fullPath, Microsoft.Office.Interop.Outlook.Folders oFolders)
        {
            String tempName = "";
 
            foreach (Microsoft.Office.Interop.Outlook.Folder oFolder in oFolders)
            {
                if (String.Compare(GetFolderPath(oFolder.FullFolderPath), fullPath) == 0)
                {
                    return oFolder.Name;
                }
                
                tempName = GetFolderNameFromFullPath(fullPath, oFolder.Folders);
                if (!String.IsNullOrEmpty(tempName)) return tempName;
            }
 
            return "";
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                // Ensure we cleanup the Outlook resources, but do not call Quit() on the Outlook
                // app object or we will inadvertantly close any full blown Outlook instances 
                // that are open.
                oApp = null;
                oNsp = null;
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu = new System.Windows.Forms.ContextMenu();
            this.menuSelectFolder = new System.Windows.Forms.MenuItem();
            this.menuCalendar = new System.Windows.Forms.MenuItem();
            this.menuContacts = new System.Windows.Forms.MenuItem();
            this.menuInbox = new System.Windows.Forms.MenuItem();
            this.menuNotes = new System.Windows.Forms.MenuItem();
            this.menuTasks = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuAbout = new System.Windows.Forms.MenuItem();
            this.menuPrefs = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuHide = new System.Windows.Forms.MenuItem();
            this.menuExit = new System.Windows.Forms.MenuItem();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.axOutlookViewControl = new AxMicrosoft.Office.Interop.OutlookViewCtl.AxViewCtl();
            ((System.ComponentModel.ISupportInitialize)(this.axOutlookViewControl)).BeginInit();
            this.SuspendLayout();
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenu = this.trayMenu;
            this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
            this.trayIcon.Text = Resources.ProductName;
            this.trayIcon.Visible = true;
            this.trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.trayIcon_MouseDoubleClick);
            // 
            // trayMenu
            // 
            this.trayMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuSelectFolder,
            this.menuCalendar,
            this.menuContacts,
            this.menuInbox,
            this.menuNotes,
            this.menuTasks,
            this.menuItem4,
            this.menuAbout,
            this.menuPrefs,
            this.menuItem2,
            this.menuHide,
            this.menuExit});
            // 
            // menuSelectFolder
            // 
            this.menuSelectFolder.Index = 0;
            this.menuSelectFolder.Text = Resources.SelectFolder;
            this.menuSelectFolder.Click += new System.EventHandler(this.menuOtherFolders_Click);
            // 
            // menuCalendar
            // 
            this.menuCalendar.Index = 1;
            this.menuCalendar.Text = Resources.Calendar;
            this.menuCalendar.Click += new System.EventHandler(this.menuCalendar_Click);
            // 
            // menuContacts
            // 
            this.menuContacts.Index = 2;
            this.menuContacts.Text = Resources.Contacts;
            this.menuContacts.Click += new System.EventHandler(this.menuContacts_Click);
            // 
            // menuInbox
            // 
            this.menuInbox.Index = 3;
            this.menuInbox.Text = Resources.Inbox;
            this.menuInbox.Click += new System.EventHandler(this.menuInbox_Click);
            // 
            // menuNotes
            // 
            this.menuNotes.Index = 4;
            this.menuNotes.Text = Resources.Notes;
            this.menuNotes.Click += new System.EventHandler(this.menuNotes_Click);
            // 
            // menuTasks
            // 
            this.menuTasks.Index = 5;
            this.menuTasks.Text = Resources.Tasks;
            this.menuTasks.Click += new System.EventHandler(this.menuTasks_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 6;
            this.menuItem4.Text = global::OutlookDesktop.Properties.Resources.Separator;
            // 
            // menuAbout
            // 
            this.menuAbout.Index = 7;
            this.menuAbout.Text = Resources.About;
            this.menuAbout.Click += new System.EventHandler(this.menuAbout_Click);
            // 
            // menuPrefs
            // 
            this.menuPrefs.Index = 8;
            this.menuPrefs.Text = Resources.Preferences;
            this.menuPrefs.Click += new System.EventHandler(this.menuPrefs_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 9;
            this.menuItem2.Text = global::OutlookDesktop.Properties.Resources.Separator;
            // 
            // menuHide
            // 
            this.menuHide.Index = 10;
            this.menuHide.Text = Resources.Hide;
            this.menuHide.Click += new System.EventHandler(this.menuHide_Click);
            // 
            // menuExit
            // 
            this.menuExit.Index = 11;
            this.menuExit.Text = Resources.Close;
            this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Interval = 1000;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // axOutlookViewControl
            // 
            this.axOutlookViewControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axOutlookViewControl.Enabled = true;
            this.axOutlookViewControl.Location = new System.Drawing.Point(0, 0);
            this.axOutlookViewControl.Name = "axOutlookViewControl";
            this.axOutlookViewControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axOutlookViewControl.OcxState")));
            this.axOutlookViewControl.Size = new System.Drawing.Size(292, 271);
            this.axOutlookViewControl.TabIndex = 0;
            // 
            // FormMain
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 271);
            this.Controls.Add(this.axOutlookViewControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.Name = "FormMain";
            this.Opacity = 0.5;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = Resources.ProductName;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormMain_Paint);
            this.Activated += new System.EventHandler(this.FormMain_Activated);
            ((System.ComponentModel.ISupportInitialize)(this.axOutlookViewControl)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();

            // create main form instance
            FormMain fMainForm = new FormMain();

            Application.Run(fMainForm);
        }

        private void menuPrefs_Click(object sender, System.EventArgs e)
        {
            FormPrefs fPrefs = new FormPrefs(this);
            fPrefs.Show();
        }

        private void menuExit_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void menuCalendar_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = FolderViewTypes.Calendar.ToString();
            prefs.FolderViewType = FolderViewTypes.Calendar.ToString();
            trayMenu.MenuItems[0].Checked = false;
            menuCalendar.Checked = true;
            menuContacts.Checked = false;
            menuInbox.Checked = false;
            menuNotes.Checked = false;
            menuTasks.Checked = false;
        }

        private void menuContacts_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = FolderViewTypes.Contacts.ToString();
            prefs.FolderViewType = FolderViewTypes.Contacts.ToString();
            trayMenu.MenuItems[0].Checked = false;
            menuCalendar.Checked = false;
            menuContacts.Checked = true;
            menuInbox.Checked = false;
            menuNotes.Checked = false;
            menuTasks.Checked = false;
        }

        private void menuInbox_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = FolderViewTypes.Inbox.ToString();
            prefs.FolderViewType = FolderViewTypes.Inbox.ToString();
            trayMenu.MenuItems[0].Checked = false;
            menuCalendar.Checked = false;
            menuContacts.Checked = false;
            menuInbox.Checked = true;
            menuNotes.Checked = false;
            menuTasks.Checked = false;
        }

        private void menuTasks_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = FolderViewTypes.Tasks.ToString();
            prefs.FolderViewType = FolderViewTypes.Tasks.ToString();
            trayMenu.MenuItems[0].Checked = false;
            menuCalendar.Checked = false;
            menuContacts.Checked = false;
            menuInbox.Checked = false;
            menuNotes.Checked = false;
            menuTasks.Checked = true;
        }

        private void menuNotes_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = FolderViewTypes.Notes.ToString();
            prefs.FolderViewType = FolderViewTypes.Notes.ToString();
            trayMenu.MenuItems[0].Checked = false;
            menuCalendar.Checked = false;
            menuContacts.Checked = false;
            menuInbox.Checked = false;
            menuNotes.Checked = true;
            menuTasks.Checked = false;
        }

        private void menuCustomFolder_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = customFolder;
            trayMenu.MenuItems[0].Checked = true;
            menuCalendar.Checked = false;
            menuContacts.Checked = false;
            menuInbox.Checked = false;
            menuNotes.Checked = false;
            menuTasks.Checked = false;
        }

        private void menuOtherFolders_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Outlook.MAPIFolder oFolder = oNsp.PickFolder();
            if (oFolder != null)
            {
                if (trayMenu.MenuItems[0].Text != menuSelectFolder.Text)
                {
                    trayMenu.MenuItems.RemoveAt(0);
                }

                prefs.FolderViewType = GetFolderPath(oFolder.FullFolderPath);
                customFolder = prefs.FolderViewType;
                axOutlookViewControl.Folder = GetFolderPath(oFolder.FullFolderPath);
                MenuItem item = new MenuItem(oFolder.Name, new System.EventHandler(this.menuCustomFolder_Click));
                trayMenu.MenuItems.Add(0, item);
                trayMenu.MenuItems[0].Checked = true;
                menuCalendar.Checked = false;
                menuContacts.Checked = false;
                menuInbox.Checked = false;
                menuNotes.Checked = false;
                menuTasks.Checked = false;
            }
        }

        private static string GetFolderPath(string folderPath)
        {
            return folderPath.Replace("\\\\Personal Folders\\", "");
        }

        private void FormMain_Activated(object sender, EventArgs e)
        {
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                UnsafeNativeMethods.SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            }
        }

        private void FormMain_Paint(object sender, PaintEventArgs e)
        {
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                UnsafeNativeMethods.SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            }
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

            // increment day in outlook's calendar if we've crossed over into a new day
            if (DateTime.Now.Day != prevDateTime.Day)
            {
                axOutlookViewControl.GoToToday();
            }
            prevDateTime = DateTime.Now;
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            AboutBox aboutForm = new AboutBox();
            aboutForm.ShowDialog();
        }

        private void menuHide_Click(object sender, EventArgs e)
        {
            showHideDesktopComponent();
        }

        private void showHideDesktopComponent()
        {
            if (this.Visible == true)
            {
                menuHide.Text = Resources.Show;
                this.Visible = false;
            }
            else
            {
                menuHide.Text = Resources.Hide;
                this.Visible = true;
            }
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                showHideDesktopComponent();
        }
    }
}
