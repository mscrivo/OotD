using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using OutlookDesktop.Properties;

namespace OutlookDesktop
{
    public enum folderViewTypes
    {
        Inbox,
        Calendar,
        Contacts,
        Notes,
        Tasks,
    }

    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class FormMain : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.ContextMenu trayMenu;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuPrefs;
        private System.Windows.Forms.MenuItem menuExit;
        private WebBrowser webBrowser;
        private Timer updateTimer;
        private MenuItem menuInbox;
        private MenuItem menuCalendar;
        private MenuItem menuItem4;
        private MenuItem menuContacts;
        private MenuItem menuTasks;
        private MenuItem menuNotes;
        private MenuItem menuAbout;

        private Preferences prefs;

        private const int SWP_DRAWFRAME = 0x20;
        private const int SWP_NOMOVE = 0x2;
        private const int SWP_NOSIZE = 0x1;
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_NOACTIVATE = 0x10;
        private MenuItem menuHide;
        private const int HWND_BOTTOM = 0x1;
                
        public FormMain()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

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
                this.Opacity = Preferences.DEFAULT_OPACITY;
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
                this.Left = Preferences.DEFAULT_TOP;
                this.Top = Preferences.DEFAULT_LEFT;
                this.Width = Preferences.DEFAULT_WIDTH;
                this.Height = Preferences.DEFAULT_HEIGHT;
                MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }


            if (prefs.FolderViewType == folderViewTypes.Calendar.ToString()) 
            {
                menuCalendar.Checked = true;
            }
            else if (prefs.FolderViewType == folderViewTypes.Contacts.ToString())
            {
                menuContacts.Checked = true;
            }
            else if (prefs.FolderViewType == folderViewTypes.Inbox.ToString())
            {
                menuInbox.Checked = true;
            }
            else if (prefs.FolderViewType == folderViewTypes.Notes.ToString()) 
            {
                menuNotes.Checked = true;
            }
            else if (prefs.FolderViewType == folderViewTypes.Tasks.ToString())
            {
                menuTasks.Checked = true;
            }
            else
            {
                menuCalendar.Checked = true;
            }
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
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenu = this.trayMenu;
            this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
            this.trayIcon.Text = "Outlook on the Desktop";
            this.trayIcon.Visible = true;
            this.trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.trayIcon_MouseDoubleClick);
            // 
            // trayMenu
            // 
            this.trayMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
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
            // menuCalendar
            // 
            this.menuCalendar.Index = 0;
            this.menuCalendar.Text = "Calendar";
            this.menuCalendar.Click += new System.EventHandler(this.menuCalendar_Click);
            // 
            // menuContacts
            // 
            this.menuContacts.Index = 1;
            this.menuContacts.Text = "Contacts";
            this.menuContacts.Click += new System.EventHandler(this.menuContacts_Click);
            // 
            // menuInbox
            // 
            this.menuInbox.Index = 2;
            this.menuInbox.Text = "Inbox";
            this.menuInbox.Click += new System.EventHandler(this.menuInbox_Click);
            // 
            // menuNotes
            // 
            this.menuNotes.Index = 3;
            this.menuNotes.Text = "Notes";
            this.menuNotes.Click += new System.EventHandler(this.menuNotes_Click);
            // 
            // menuTasks
            // 
            this.menuTasks.Index = 4;
            this.menuTasks.Text = "Tasks";
            this.menuTasks.Click += new System.EventHandler(this.menuTasks_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 5;
            this.menuItem4.Text = "-";
            // 
            // menuAbout
            // 
            this.menuAbout.Index = 6;
            this.menuAbout.Text = "About";
            this.menuAbout.Click += new System.EventHandler(this.menuAbout_Click);
            // 
            // menuPrefs
            // 
            this.menuPrefs.Index = 7;
            this.menuPrefs.Text = "Preferences";
            this.menuPrefs.Click += new System.EventHandler(this.menuPrefs_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 8;
            this.menuItem2.Text = global::OutlookDesktop.Properties.Resources.Separator;
            // 
            // menuHide
            // 
            this.menuHide.Index = 9;
            this.menuHide.Text = "Hide";
            this.menuHide.Click += new System.EventHandler(this.menuHide_Click);
            // 
            // menuExit
            // 
            this.menuExit.Index = 10;
            this.menuExit.Text = "Close";
            this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // webBrowser
            // 
            this.webBrowser.AllowNavigation = false;
            this.webBrowser.AllowWebBrowserDrop = false;
            this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser.Location = new System.Drawing.Point(0, 0);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScrollBarsEnabled = false;
            this.webBrowser.Size = new System.Drawing.Size(292, 271);
            this.webBrowser.TabIndex = 2;
            this.webBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Interval = 1000;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // FormMain
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 271);
            this.Controls.Add(this.webBrowser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimizeBox = false;
            this.Name = "FormMain";
            this.Opacity = 0.5;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Outlook on the Desktop";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormMain_Paint);
            this.Activated += new System.EventHandler(this.FormMain_Activated);
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

            // load the html file that loads the Outlook ActiveX control
            fMainForm.webBrowser.Navigate(String.Concat(Application.StartupPath, "\\outlookAX.html"));

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

        public void ChangeFolderType(folderViewTypes folderType)
        {
            string fileContents = "";
            TextReader tr;
            TextWriter tw;

            try
            {
                //this.webBrowser.Navigate(String.Concat(Application.StartupPath, ""));
                tr = new StreamReader(String.Concat(Application.StartupPath, "\\outlookAX.html"));
                fileContents = tr.ReadToEnd();
                tr.Close();

                Regex re = new Regex("VALUE=\"[a-zA-Z]*\"", RegexOptions.IgnoreCase);
                fileContents = re.Replace(fileContents, "VALUE=\"" + folderType.ToString() + "\"");

                try
                {
                    tw = new StreamWriter(String.Concat(Application.StartupPath, "\\outlookAX.html"));
                    tw.Write(fileContents);
                    tw.Close();
                    prefs.FolderViewType = folderType.ToString();
                    this.webBrowser.Refresh();
                }
                catch (IOException ex)
                {
                    MessageBox.Show(this, ex.Message, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(this, ex.Message, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void menuCalendar_Click(object sender, EventArgs e)
        {
            ChangeFolderType(folderViewTypes.Calendar);
            menuCalendar.Checked = true;
            menuContacts.Checked = false;
            menuInbox.Checked = false;
            menuNotes.Checked = false;
            menuTasks.Checked = false;
        }

        private void menuContacts_Click(object sender, EventArgs e)
        {
            ChangeFolderType(folderViewTypes.Contacts);
            menuCalendar.Checked = false;
            menuContacts.Checked = true;
            menuInbox.Checked = false;
            menuNotes.Checked = false;
            menuTasks.Checked = false;
        }

        private void menuInbox_Click(object sender, EventArgs e)
        {
            ChangeFolderType(folderViewTypes.Inbox);
            menuCalendar.Checked = false;
            menuContacts.Checked = false;
            menuInbox.Checked = true;
            menuNotes.Checked = false;
            menuTasks.Checked = false;
        }

        private void menuTasks_Click(object sender, EventArgs e)
        {
            ChangeFolderType(folderViewTypes.Tasks);
            menuCalendar.Checked = false;
            menuContacts.Checked = false;
            menuInbox.Checked = false;
            menuNotes.Checked = false;
            menuTasks.Checked = true;
        }

        private void menuNotes_Click(object sender, EventArgs e)
        {
            ChangeFolderType(folderViewTypes.Notes);
            menuCalendar.Checked = false;
            menuContacts.Checked = false;
            menuInbox.Checked = false;
            menuNotes.Checked = true;
            menuTasks.Checked = false;
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
            
            DateTime today = DateTime.Now;
            switch (today.Date.Day)
            {
                case 1:
                    trayIcon.Icon = Resources._1;
                    break;
                case 2:
                    trayIcon.Icon = Resources._2;
                    break;
                case 3:
                    trayIcon.Icon = Resources._3;
                    break;
                case 4:
                    trayIcon.Icon = Resources._4;
                    break;
                case 5:
                    trayIcon.Icon = Resources._5;
                    break;
                case 6:
                    trayIcon.Icon = Resources._6;
                    break;
                case 7:
                    trayIcon.Icon = Resources._7;
                    break;
                case 8:
                    trayIcon.Icon = Resources._8;
                    break;
                case 9:
                    trayIcon.Icon = Resources._9;
                    break;
                case 10:
                    trayIcon.Icon = Resources._10;
                    break;
                case 11:
                    trayIcon.Icon = Resources._11;
                    break;
                case 12:
                    trayIcon.Icon = Resources._12;
                    break;
                case 13:
                    trayIcon.Icon = Resources._13;
                    break;
                case 14:
                    trayIcon.Icon = Resources._14;
                    break;
                case 15:
                    trayIcon.Icon = Resources._15;
                    break;
                case 16:
                    trayIcon.Icon = Resources._16;
                    break;
                case 17:
                    trayIcon.Icon = Resources._17;
                    break;
                case 18:
                    trayIcon.Icon = Resources._18;
                    break;
                case 19:
                    trayIcon.Icon = Resources._19;
                    break;
                case 20:
                    trayIcon.Icon = Resources._20;
                    break;
                case 21:
                    trayIcon.Icon = Resources._21;
                    break;
                case 22:
                    trayIcon.Icon = Resources._22;
                    break;
                case 23:
                    trayIcon.Icon = Resources._23;
                    break;
                case 24:
                    trayIcon.Icon = Resources._24;
                    break;
                case 25:
                    trayIcon.Icon = Resources._25;
                    break;
                case 26:
                    trayIcon.Icon = Resources._26;
                    break;
                case 27:
                    trayIcon.Icon = Resources._27;
                    break;
                case 28:
                    trayIcon.Icon = Resources._28;
                    break;
                case 29:
                    trayIcon.Icon = Resources._29;
                    break;
                case 30:
                    trayIcon.Icon = Resources._30;
                    break;
                case 31:
                    trayIcon.Icon = Resources._31;
                    break;
            }
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            ChangeTrayIconDate();
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
                menuHide.Text = "Show";
                this.Visible = false;
            }
            else
            {
                menuHide.Text = "Hide";
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
