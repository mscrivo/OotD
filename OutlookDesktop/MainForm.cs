using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using Microsoft.Win32;
using OutlookDesktop.Properties;

namespace OutlookDesktop
{
    public enum FolderViewType
    {
        Inbox,
        Calendar,
        Contacts,
        Notes,
        Tasks,
    }

    public partial class MainForm : Form
    {
        private const int SWP_DRAWFRAME = 0x20;
        private const int SWP_NOMOVE = 0x2;
        private const int SWP_NOSIZE = 0x1;
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_NOACTIVATE = 0x10;
        private const int HWND_BOTTOM = 0x1;

        private Microsoft.Office.Interop.Outlook.Application _outlookApplication;
        private Microsoft.Office.Interop.Outlook.NameSpace _outlookNamespace;
        private DateTime _previousDate;
        private String _customFolder;

        public event EventHandler<InstanceRemovedEventArgs> InstanceRemoved;
        public event EventHandler<InstanceRenamedEventArgs> InstanceRenamed;

        public Boolean IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }
        private Boolean _isInitialized;

        public InstancePreferences Preferences
        {
            get
            {
                return _preferences;
            }
        }
        private InstancePreferences _preferences;

        public String InstanceName
        {
            get
            {
                return _instanceName;
            }
        }
        private String _instanceName;

        public MainForm(String instanceName)
        {
            try
            {
                InitializeComponent();

                _outlookApplication = new Microsoft.Office.Interop.Outlook.Application();
                _outlookNamespace = _outlookApplication.GetNamespace("MAPI");
                axOutlookViewControl.Folder = FolderViewType.Calendar.ToString();
                axOutlookViewControl.View = "Day/Week/Month";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Resources.ErrorInitializingApp + " " + ex.ToString(), Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isInitialized = false;
                return;
            }

            _instanceName = instanceName;
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
            catch (Exception ex)
            {
                MessageBox.Show(this, Resources.ErrorInitializingApp + " " + ex.ToString(), Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isInitialized = false;
                return;
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Loads user preferences from registry and applies them.
        /// </summary>
        public void LoadSettings()
        {
            // create a new instance of the preferences class
            _preferences = new InstancePreferences(InstanceName);

            try
            {
                this.Opacity = _preferences.Opacity;
            }
            catch (Exception)
            {
                // use default if there was a problem
                this.Opacity = InstancePreferences.DefaultOpacity;
                MessageBox.Show(this, Resources.ErrorSettingOpacity, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            try
            {
                this.Left = _preferences.Left;
                this.Top = _preferences.Top;
                this.Width = _preferences.Width;
                this.Height = _preferences.Height;
            }
            catch (Exception)
            {
                // use defaults if there was a problem
                this.Left = InstancePreferences.DefaultTopPosition;
                this.Top = InstancePreferences.DefaultLeftPosition;
                this.Width = InstancePreferences.DefaultWidth;
                this.Height = InstancePreferences.DefaultHeight;
                MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            if (_preferences.FolderViewType == FolderViewType.Calendar.ToString())
            {
                CalendarMenu.Checked = true;
            }
            else if (_preferences.FolderViewType == FolderViewType.Contacts.ToString())
            {
                ContactsMenu.Checked = true;
            }
            else if (_preferences.FolderViewType == FolderViewType.Inbox.ToString())
            {
                InboxMenu.Checked = true;
            }
            else if (_preferences.FolderViewType == FolderViewType.Notes.ToString())
            {
                NotesMenu.Checked = true;
            }
            else if (_preferences.FolderViewType == FolderViewType.Tasks.ToString())
            {
                TasksMenu.Checked = true;
            }
            else
            {
                // custom folder
                _customFolder = _preferences.FolderViewType;
                String folderName = GetFolderNameFromFullPath(_customFolder, _outlookNamespace.Folders);
                trayMenu.Items.Insert(0, new ToolStripMenuItem(folderName, null, new System.EventHandler(this.CustomFolderMenu_Click)));
                ToolStripMenuItem customMenu = (ToolStripMenuItem)trayMenu.Items[0];
                customMenu.Checked = true;
            }

            axOutlookViewControl.Folder = _preferences.FolderViewType;
        }

        private String GetFolderNameFromFullPath(String fullPath, Microsoft.Office.Interop.Outlook.Folders oFolders)
        {
            String tempName = "";

            foreach (Microsoft.Office.Interop.Outlook.Folder oFolder in oFolders)
            {
                if (String.Compare(GetFolderPath(oFolder.FullFolderPath), fullPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return oFolder.Name;
                }

                tempName = GetFolderNameFromFullPath(fullPath, oFolder.Folders);
                if (!String.IsNullOrEmpty(tempName)) return tempName;
            }

            return "";
        }

        private static string GetFolderPath(string folderPath)
        {
            return folderPath.Replace("\\\\Personal Folders\\", "");
        }

        private void ShowHideDesktopComponent()
        {
            if (this.Visible == true)
            {
                HideShowMenu.Text = Resources.Show;
                this.Visible = false;
            }
            else
            {
                HideShowMenu.Text = Resources.Hide;
                this.Visible = true;
            }
        }

        private void SelectFolderMenu_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Outlook.MAPIFolder oFolder = _outlookNamespace.PickFolder();
            if (oFolder != null)
            {
                if (trayMenu.Items[0].Text != SelectFolderMenu.Text)
                {
                    trayMenu.Items.RemoveAt(0);
                }

                _preferences.FolderViewType = GetFolderPath(oFolder.FullFolderPath);
                _customFolder = _preferences.FolderViewType;
                axOutlookViewControl.Folder = GetFolderPath(oFolder.FullFolderPath);
                trayMenu.Items.Insert(0, new ToolStripMenuItem(oFolder.Name, null, new System.EventHandler(this.CustomFolderMenu_Click)));
                ToolStripMenuItem customMenu = (ToolStripMenuItem)trayMenu.Items[0];
                customMenu.Checked = true;
                CalendarMenu.Checked = false;
                ContactsMenu.Checked = false;
                InboxMenu.Checked = false;
                NotesMenu.Checked = false;
                TasksMenu.Checked = false;
            }
        }

        private void CustomFolderMenu_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = _customFolder;
            ToolStripMenuItem customMenu = (ToolStripMenuItem)trayMenu.Items[0];
            customMenu.Checked = true;
            CalendarMenu.Checked = false;
            ContactsMenu.Checked = false;
            InboxMenu.Checked = false;
            NotesMenu.Checked = false;
            TasksMenu.Checked = false;
        }

        private void CalendarMenu_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = FolderViewType.Calendar.ToString();
            _preferences.FolderViewType = FolderViewType.Calendar.ToString();
            ToolStripMenuItem customMenu = (ToolStripMenuItem)trayMenu.Items[0];
            customMenu.Checked = false;
            CalendarMenu.Checked = true;
            ContactsMenu.Checked = false;
            InboxMenu.Checked = false;
            NotesMenu.Checked = false;
            TasksMenu.Checked = false;
        }

        private void ContactsMenu_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = FolderViewType.Contacts.ToString();
            _preferences.FolderViewType = FolderViewType.Contacts.ToString();
            ToolStripMenuItem customMenu = (ToolStripMenuItem)trayMenu.Items[0];
            customMenu.Checked = false;
            CalendarMenu.Checked = false;
            ContactsMenu.Checked = true;
            InboxMenu.Checked = false;
            NotesMenu.Checked = false;
            TasksMenu.Checked = false;
        }

        private void InboxMenu_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = FolderViewType.Inbox.ToString();
            _preferences.FolderViewType = FolderViewType.Inbox.ToString();
            ToolStripMenuItem customMenu = (ToolStripMenuItem)trayMenu.Items[0];
            customMenu.Checked = false;
            CalendarMenu.Checked = false;
            ContactsMenu.Checked = false;
            InboxMenu.Checked = true;
            NotesMenu.Checked = false;
            TasksMenu.Checked = false;
        }

        private void NotesMenu_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = FolderViewType.Notes.ToString();
            _preferences.FolderViewType = FolderViewType.Notes.ToString();
            ToolStripMenuItem customMenu = (ToolStripMenuItem)trayMenu.Items[0];
            customMenu.Checked = false;
            CalendarMenu.Checked = false;
            ContactsMenu.Checked = false;
            InboxMenu.Checked = false;
            NotesMenu.Checked = true;
            TasksMenu.Checked = false;
        }

        private void TasksMenu_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = FolderViewType.Tasks.ToString();
            _preferences.FolderViewType = FolderViewType.Tasks.ToString();
            ToolStripMenuItem customMenu = (ToolStripMenuItem)trayMenu.Items[0];
            customMenu.Checked = false;
            CalendarMenu.Checked = false;
            ContactsMenu.Checked = false;
            InboxMenu.Checked = false;
            NotesMenu.Checked = false;
            TasksMenu.Checked = true;
        }

        private void PreferencesMenu_Click(object sender, EventArgs e)
        {
            PreferencesForm preferencesForm = new PreferencesForm(this);
            preferencesForm.Show();
        }

        private void HideMenu_Click(object sender, EventArgs e)
        {
            ShowHideDesktopComponent();
        }

        private void RemoveInstanceMenu_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this, Resources.RemoveInstanceConfirmation, Resources.ConfirmationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                using (RegistryKey appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
                {
                    appReg.DeleteSubKeyTree(InstanceName);
                }

                InstanceRemoved(this, new InstanceRemovedEventArgs(InstanceName));
                Dispose();
            }
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            // increment day in outlook's calendar if we've crossed over into a new day
            if (DateTime.Now.Day != _previousDate.Day)
            {
                axOutlookViewControl.GoToToday();
            }
            _previousDate = DateTime.Now;
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                UnsafeNativeMethods.SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            }
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                UnsafeNativeMethods.SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            }
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void RenameInstanceMenu_Click(object sender, EventArgs e)
        {
            InputBoxResult result = InputBox.Show(this, "", "Rename Instance", _instanceName, inputBox_Validating);
            if (result.Ok)
            {
                using (RegistryKey parentKey = Registry.CurrentUser.OpenSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName, true))
                {
                    if (parentKey != null)
                    {
                        RegistryHelper.RenameSubKey(parentKey, _instanceName, result.Text);
                        String oldInstanceName = _instanceName;
                        _instanceName = result.Text;
                        _preferences = new InstancePreferences(_instanceName);

                        InstanceRenamed(this, new InstanceRenamedEventArgs(oldInstanceName, _instanceName));
                    }
                }
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
    }
}
