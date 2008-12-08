using System;
using System.Collections.Generic;
using System.Collections;
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

    /// <summary>
    /// Standard Outlook folder types. 
    /// </summary>
    public enum FolderViewType
    {
        Inbox,
        Calendar,
        Contacts,
        Notes,
        Tasks,
    }


    /// <summary>
    /// This is the form that hosts the outlook view control. One of these will
    /// exist for each instance.
    /// </summary>
    public partial class MainForm : Form
    {

        /// <summary>
        /// Outlook Application
        /// </summary>
        private Microsoft.Office.Interop.Outlook.Application _outlookApplication;

        /// <summary>
        /// The active namespace for the current outlook session. 
        /// </summary>
        private Microsoft.Office.Interop.Outlook.NameSpace _outlookNamespace;

        /// <summary>
        /// The MAPIFolder for the currently selected folder to show. 
        /// </summary>
        private Microsoft.Office.Interop.Outlook.MAPIFolder _outlookFolder;

        /// <summary>
        /// Contains the current views avaliable for the folder. 
        /// </summary>
        private List<Microsoft.Office.Interop.Outlook.View> _oulookFolderViews;


        private DateTime _previousDate;
        private String _customFolder;
        private Boolean _isInitialized;
        private ToolStripMenuItem _customMenu;
        private String _instanceName;
        private InstancePreferences _preferences;

        public event EventHandler<InstanceRemovedEventArgs> InstanceRemoved;
        public event EventHandler<InstanceRenamedEventArgs> InstanceRenamed;

        /// <summary>
        /// Standard logging block.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);



        #region Public access to private variables.
        public Microsoft.Office.Interop.Outlook.Application OutlookApplication
        {
            get
            {
                if (_outlookApplication != null)
                    return _outlookApplication;
                else
                    return null;
            }
        }

        public Microsoft.Office.Interop.Outlook.NameSpace OutlookNameSpace
        {
            get
            {
                if (_outlookNamespace != null)
                    return _outlookNamespace;
                else
                    return null;
            }
        }

        public List<Microsoft.Office.Interop.Outlook.View> OulookFolderViews
        {
            get
            {
                if (_oulookFolderViews != null)
                    return _oulookFolderViews;
                else
                    return null;
            }
        }

        public Boolean IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }


        public InstancePreferences Preferences
        {
            get
            {
                return _preferences;
            }
        }


        public String InstanceName
        {
            get
            {
                return _instanceName;
            }
        }


        public ToolStripMenuItem CustomMenu
        {
            get
            {
                return _customMenu;
            }
            set
            {
                _customMenu = value;
            }
        }


        #endregion

        /// <summary>
        /// Sets up the form for the current instance.
        /// </summary>
        /// <param name="instanceName">The name of the instance to display.</param>
        public MainForm(String instanceName)
        {
            try
            {
                InitializeComponent();

                // Get or create a instance of the Outlook Applciation.
                _outlookApplication = new Microsoft.Office.Interop.Outlook.Application();
                _outlookNamespace = _outlookApplication.GetNamespace("MAPI");

                // Set the default viewcontrol to the calendar and Day/Week/Month view.
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

            if (System.Environment.OSVersion.Version.Major < 6)
                _isInitialized = UnsafeNativeMethods.PinWindowToDesktop(this);
            else
                _isInitialized = UnsafeNativeMethods.SendWindowToBack(this);

            if (!_isInitialized) return;
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

        /// <summary>
        /// Get the location of the Select folder menu in the tray context menu. 
        /// </summary>
        /// <returns></returns>
        private int GetSelectFolderMenuLocation()
        {
            return trayMenu.Items.IndexOf(SelectFolderMenu);
        }


        /// <summary>
        /// Loads user preferences from registry and applies them.
        /// </summary>
        public void LoadSettings()
        {
            // create a new instance of the preferences class
            _preferences = new InstancePreferences(InstanceName);


            // There should ne no reason other than first run as to why the Store and Entry IDs are 
            //empty. 
            if (String.IsNullOrEmpty(_preferences.OutlookFolderStoreId))
            {
                // Set the Mapi Folder Details and the IDs.
                _preferences.OutlookFolderName = FolderViewType.Calendar.ToString();
                _preferences.OutlookFolderStoreId = GetFolderFromViewType(FolderViewType.Calendar).StoreID;
                _preferences.OutlookFolderEntryId = GetFolderFromViewType(FolderViewType.Calendar).EntryID;
            }


            SetMapiFolder();

            // Sets the opacity of the instance. 
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

            // Sets the position of the instance. 
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


            // Checks the menuitem ofr the current folder.
            if (_preferences.OutlookFolderName == FolderViewType.Calendar.ToString())
            {
                CalendarMenu.Checked = true;
            }
            else if (_preferences.OutlookFolderName == FolderViewType.Contacts.ToString())
            {
                ContactsMenu.Checked = true;
            }
            else if (_preferences.OutlookFolderName == FolderViewType.Inbox.ToString())
            {
                InboxMenu.Checked = true;
            }
            else if (_preferences.OutlookFolderName == FolderViewType.Notes.ToString())
            {
                NotesMenu.Checked = true;
            }
            else if (_preferences.OutlookFolderName == FolderViewType.Tasks.ToString())
            {
                TasksMenu.Checked = true;
            }
            else
            {
                // custom folder
                _customFolder = _preferences.OutlookFolderName;
                String folderName = GetFolderNameFromFullPath(_customFolder, _outlookNamespace.Folders);
                trayMenu.Items.Insert(GetSelectFolderMenuLocation() + 1, new ToolStripMenuItem(folderName, null, new System.EventHandler(this.CustomFolderMenu_Click)));
                CustomMenu = (ToolStripMenuItem)trayMenu.Items[GetSelectFolderMenuLocation() + 1];
                CustomMenu.Checked = true;
            }


            // Sets the viewcontrol folder from preferences. 
            axOutlookViewControl.Folder = _preferences.OutlookFolderName;

            // Sets the selected view from preferences. 
            axOutlookViewControl.View = _preferences.OutlookFolderView;

            // Get a copy of the possible outlook views for the selected folder and populate the context menu for this instance. 
            UpdateOutlookViewsList();

        }


        /// <summary>
        /// This will populate the _outlookFolder object with the MapiFolder for the EntryID and StoreId stored
        /// in the registry. 
        /// </summary>
        private void SetMapiFolder()
        {
            // Load up the MAPI Folder from Entry / Store IDs 
            if (_preferences.OutlookFolderEntryId != "" && _preferences.OutlookFolderStoreId != "")
                _outlookFolder = _outlookNamespace.GetFolderFromID(_preferences.OutlookFolderEntryId, _preferences.OutlookFolderStoreId);
            else
                _outlookFolder = null;
        }


        /// <summary>
        /// This will populate a dropdown off the instance context menu with the avaliable
        /// views in outlook, it will also assoicate the menuitem with the event handler. 
        /// </summary>
        private void UpdateOutlookViewsList()
        {
            uxOutlookViews.DropDownItems.Clear();
            _oulookFolderViews = new List<Microsoft.Office.Interop.Outlook.View>();

            //uxOutlookViews.DropDownItems.Add(uxDefaultOutlookView);

            if (_outlookFolder != null)
            {
                //NOTE: Issue with the update of views in this instance of Outlook.
                //      Will have to spawn a new instance... Sigh.
                //SetMapiFolder();

                foreach (Microsoft.Office.Interop.Outlook.View view in _outlookFolder.Views)
                {
                    ToolStripMenuItem viewItem = new ToolStripMenuItem(view.Name);
                    viewItem.Tag = view;

                    viewItem.Click += new EventHandler(viewItem_Click);

                    if (view.Name == _preferences.OutlookFolderView)
                        viewItem.Checked = true;

                    uxOutlookViews.DropDownItems.Add(viewItem);

                    _oulookFolderViews.Add(view);
                }
            }
        }

        /// <summary>
        /// Will select the passed menu item in the views dropdown list. 
        /// </summary>
        /// <param name="viewItem">ToolStripMenuItem that is to be checked.</param>
        private void CheckSelectedView(ToolStripMenuItem viewItem)
        {
            CheckSelectedMenuItemInCollection(viewItem, uxOutlookViews.DropDownItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="oFolders"></param>
        /// <returns></returns>
        private String GetFolderNameFromFullPath(String fullPath, Microsoft.Office.Interop.Outlook.Folders oFolders)
        {
            //TODO: Revert back and deal with online/offline better!
            return fullPath.Substring(fullPath.LastIndexOf("\\") + 1, fullPath.Length - fullPath.LastIndexOf("\\") - 1);
            String tempName = "";
            Microsoft.Office.Interop.Outlook.MAPIFolder mapiFld = null;

            try
            {
                if (oFolders != null && oFolders.GetFirst() != null) mapiFld = oFolders.GetFirst();

                //if (mapiFld.FolderPath.StartsWith("\\\\Public Folders")) return "";

                while (mapiFld != null)
                {
                    if (String.Compare(GetFolderPath(GenerateFolderPathFromObject(mapiFld)), fullPath, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return mapiFld.Name;
                    }

                    try
                    {
                        tempName = GetFolderNameFromFullPath(fullPath, mapiFld.Folders);
                    }
                    catch (Exception offlineException)
                    {
                        //-659291883
                        //if(offlineException.
                        if (offlineException.Message == "The connection to Microsoft Exchange is unavailable. Outlook must be online or connected to complete this action.")
                        {
                            // This is a allowed message, just hit a error with the public folders. Needs to continue. 
                        }
                        else
                            throw (offlineException);
                    }

                    mapiFld = oFolders.GetNext();

                    if (!String.IsNullOrEmpty(tempName)) return tempName;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(this, Resources.ErrorSettingFolder, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return "";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="oFolder"></param>
        /// <returns></returns>
        private string GenerateFolderPathFromObject(Microsoft.Office.Interop.Outlook.MAPIFolder oFolder)
        {
            string fullFolderPath = "\\\\";
            List<string> subfolders = new List<string>();

            subfolders.Add(oFolder.Name);

            while (oFolder != null && oFolder.Parent != null)
            {
                oFolder = oFolder.Parent as Microsoft.Office.Interop.Outlook.MAPIFolder;
                if (oFolder != null) subfolders.Add(oFolder.Name);
            }

            for (int i = subfolders.Count - 1; i >= 0; i--)
            {
                fullFolderPath += subfolders[i] + "\\";
            }

            if (fullFolderPath.EndsWith("\\"))
            {
                fullFolderPath = fullFolderPath.Substring(0, fullFolderPath.Length - 1);
            }

            return fullFolderPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private static string GetFolderPath(string folderPath)
        {
            return folderPath.Replace("\\\\Personal Folders\\", "");
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// Returns a MAPI Folder for the passes FolderViewType.
        /// </summary>
        /// <param name="folderViewType"></param>
        /// <returns></returns>
        private Microsoft.Office.Interop.Outlook.MAPIFolder GetFolderFromViewType(FolderViewType folderViewType)
        {
            switch (folderViewType)
            {
                case FolderViewType.Inbox:
                    return _outlookNamespace.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderInbox);
                case FolderViewType.Calendar:
                    return _outlookNamespace.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderCalendar);
                case FolderViewType.Contacts:
                    return _outlookNamespace.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderContacts);
                case FolderViewType.Notes:
                    return _outlookNamespace.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderNotes);
                case FolderViewType.Tasks:
                    return _outlookNamespace.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderTasks);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Checks the passed menu item and unchecks the rest.
        /// 
        /// This is used only for the folder types menu. 
        /// </summary>
        /// <param name="itemToCheck"></param>
        private void CheckSelectedFolder(ToolStripMenuItem itemToCheck)
        {
            List<ToolStripMenuItem> menuItems = new List<ToolStripMenuItem>();

            menuItems.Add(CalendarMenu);
            menuItems.Add(ContactsMenu);
            menuItems.Add(InboxMenu);
            menuItems.Add(NotesMenu);
            menuItems.Add(TasksMenu);
            if (CustomMenu != null) menuItems.Add(CustomMenu);

            CheckSelectedMenuItemInCollection(itemToCheck, menuItems);
        }

        /// <summary>
        /// For a given collection of MenuItems this function will iterate through them and then check the passed item. 
        /// </summary>
        /// <param name="itemToCheck">Item to check in the list</param>
        /// <param name="menuItems">IList of the menuitems to check</param>
        private void CheckSelectedMenuItemInCollection(ToolStripMenuItem itemToCheck, IList menuItems)
        {
            foreach (ToolStripMenuItem menuItem in menuItems)
            {
                if (menuItem == itemToCheck)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }
        }

        /// <summary>
        /// Generic function to deal with menue check items for selecting the folders to view. 
        /// </summary>
        /// <param name="folderViewType"></param>
        /// <param name="itemToCheck"></param>
        private void DefaultFolderTypesClicked(FolderViewType folderViewType, ToolStripMenuItem itemToCheck)
        {
            axOutlookViewControl.Folder = folderViewType.ToString();
            _preferences.OutlookFolderName = folderViewType.ToString();
            _preferences.OutlookFolderStoreId = GetFolderFromViewType(folderViewType).StoreID;
            _preferences.OutlookFolderEntryId = GetFolderFromViewType(folderViewType).EntryID;

            SetMapiFolder();

            UpdateOutlookViewsList();

            CheckSelectedFolder(itemToCheck);
        }

        public void ChangeDefaultFolderType(FolderViewType folderViewType)
        {

            ToolStripMenuItem itemToCheck;
            switch (folderViewType)
            {
                case FolderViewType.Inbox:
                    itemToCheck = InboxMenu;
                    break;
                case FolderViewType.Calendar:
                    itemToCheck = CalendarMenu;
                    break;
                case FolderViewType.Contacts:
                    itemToCheck = ContactsMenu;
                    break;
                case FolderViewType.Notes:
                    itemToCheck = NotesMenu;
                    break;
                case FolderViewType.Tasks:
                    itemToCheck = TasksMenu;
                    break;
                default:
                    itemToCheck = null;
                    break;
            }

            DefaultFolderTypesClicked(folderViewType, itemToCheck);
        }

        public void UpdateDefaultFolder(string folderName)
        {
            switch (folderName.ToLower())
            {
                case "inbox":
                    ChangeDefaultFolderType(FolderViewType.Inbox);
                    break;
                case "calendar":
                    ChangeDefaultFolderType(FolderViewType.Calendar);
                    break;
                case "contacts":
                    ChangeDefaultFolderType(FolderViewType.Contacts);
                    break;
                case "notes":
                    ChangeDefaultFolderType(FolderViewType.Notes);
                    break;
                case "tasks":
                    ChangeDefaultFolderType(FolderViewType.Tasks);
                    break;
                default:
                    return;
            }
        }

        public void UpdateCustomFolder(Microsoft.Office.Interop.Outlook.MAPIFolder oFolder)
        {
            if (oFolder == null) return;

            try
            {
                // Remove old item (selectmenu+1)
                if (trayMenu.Items.Contains(CustomMenu))
                {
                    trayMenu.Items.Remove(CustomMenu);
                }

                String folderPath = GetFolderPath(GenerateFolderPathFromObject(oFolder));
                axOutlookViewControl.Folder = folderPath;

                // Save the EntryId and the StoreId for this folder in the prefrences. 
                _preferences.OutlookFolderEntryId = oFolder.EntryID;
                _preferences.OutlookFolderStoreId = oFolder.StoreID;

                _preferences.OutlookFolderName = folderPath;
                _customFolder = _preferences.OutlookFolderName;

                // Update the UI to reflect the new settings. 
                trayMenu.Items.Insert(GetSelectFolderMenuLocation() + 1, new ToolStripMenuItem(oFolder.Name, null, new System.EventHandler(this.CustomFolderMenu_Click)));
                CustomMenu = (ToolStripMenuItem)trayMenu.Items[GetSelectFolderMenuLocation() + 1];

                SetMapiFolder();
                CheckSelectedFolder(CustomMenu);
                UpdateOutlookViewsList();

            }
            catch (Exception)
            {
                MessageBox.Show(this, Resources.ErrorSettingFolder, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Event Handlers

        /// <summary>
        /// When a view is selected this will change the view control view to it, save it in the 
        /// preferences and then check the box next to the view in the drop down list. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void viewItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem viewItem = sender as ToolStripMenuItem;
            Microsoft.Office.Interop.Outlook.View view = viewItem.Tag as Microsoft.Office.Interop.Outlook.View;

            axOutlookViewControl.View = view.Name;

            _preferences.OutlookFolderView = view.Name;

            CheckSelectedView(viewItem);
        }

        /// <summary>
        /// This handler will select a custom folder.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectFolderMenu_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Outlook.MAPIFolder oFolder = _outlookNamespace.PickFolder();
            UpdateCustomFolder(oFolder);
        }



        private void CustomFolderMenu_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = _customFolder;

            CheckSelectedFolder(CustomMenu);
        }

        private void CalendarMenu_Click(object sender, EventArgs e)
        {
            DefaultFolderTypesClicked(FolderViewType.Calendar, CalendarMenu);
        }

        private void ContactsMenu_Click(object sender, EventArgs e)
        {
            DefaultFolderTypesClicked(FolderViewType.Contacts, ContactsMenu);
        }

        private void InboxMenu_Click(object sender, EventArgs e)
        {
            DefaultFolderTypesClicked(FolderViewType.Inbox, InboxMenu);
        }

        private void NotesMenu_Click(object sender, EventArgs e)
        {
            DefaultFolderTypesClicked(FolderViewType.Notes, NotesMenu);
        }

        private void TasksMenu_Click(object sender, EventArgs e)
        {
            DefaultFolderTypesClicked(FolderViewType.Tasks, TasksMenu);
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
                try
                {
                    axOutlookViewControl.GoToToday();
                }
                catch (Exception)
                {
                    // no big deal if we can't set the day, just ignore and go on.
                }
            }
            _previousDate = DateTime.Now;
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

        private void trayMenu_Click(object sender, EventArgs e)
        {
            //TODO: Setup event process for dealing with the view changes.
            //UpdateOutlookViewsList();
        }

        #endregion

        private void MainForm_Activated(object sender, EventArgs e)
        {
            UnsafeNativeMethods.SendWindowToBack(this);
        }

        private void MainForm_Layout(object sender, LayoutEventArgs e)
        {
            UnsafeNativeMethods.SendWindowToBack(this);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void MainForm_Validated(object sender, EventArgs e)
        {
        }

    }
}
