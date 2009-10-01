using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;
using OutlookDesktop.Properties;
using Application = System.Windows.Forms.Application;
using Exception = System.Exception;

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
        private DateTime _previousDate;
        private String _customFolder;

        public event EventHandler<InstanceRemovedEventArgs> InstanceRemoved;
        public event EventHandler<InstanceRenamedEventArgs> InstanceRenamed;

        #region Public Properties

        [CLSCompliant(false)]
        /// <summary>
        /// Outlook Application
        /// </summary>
        private Microsoft.Office.Interop.Outlook.Application OutlookApplication { get; set; }

        [CLSCompliant(false)]
        public NameSpace OutlookNameSpace { get; private set; }

        [CLSCompliant(false)]
        /// <summary>
        /// Contains the current views avaliable for the folder. 
        /// </summary>
        public List<Microsoft.Office.Interop.Outlook.View> OulookFolderViews { get; private set; }

        private MAPIFolder _outlookFolder;

        private readonly Boolean _isInitialized;

        public InstancePreferences Preferences { get; private set; }

        public string InstanceName { get; private set; }

        private ToolStripMenuItem _customMenu;

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
                OutlookApplication = new Microsoft.Office.Interop.Outlook.Application();
                OutlookNameSpace = OutlookApplication.GetNamespace("MAPI");

                // Set the default viewcontrol to the calendar and Day/Week/Month view.
                axOutlookViewControl.Folder = FolderViewType.Calendar.ToString();
                axOutlookViewControl.View = "Day/Week/Month";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Resources.ErrorInitializingApp + " " + ex, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isInitialized = false;
                return;
            }

            InstanceName = instanceName;
            LoadSettings();

            if (Environment.OSVersion.Version.Major < 6)
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
        private void LoadSettings()
        {
            // create a new instance of the preferences class
            Preferences = new InstancePreferences(InstanceName);


            // There should ne no reason other than first run as to why the Store and Entry IDs are 
            //empty. 
            if (String.IsNullOrEmpty(Preferences.OutlookFolderStoreId))
            {
                // Set the Mapi Folder Details and the IDs.
                Preferences.OutlookFolderName = FolderViewType.Calendar.ToString();
                Preferences.OutlookFolderStoreId = GetFolderFromViewType(FolderViewType.Calendar).StoreID;
                Preferences.OutlookFolderEntryId = GetFolderFromViewType(FolderViewType.Calendar).EntryID;
            }

            SetMapiFolder();

            // Sets the opacity of the instance. 
            try
            {
                Opacity = Preferences.Opacity;
            }
            catch (Exception)
            {
                // use default if there was a problem
                Opacity = InstancePreferences.DefaultOpacity;
                MessageBox.Show(this, Resources.ErrorSettingOpacity, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            // Sets the position of the instance. 
            try
            {
                Left = Preferences.Left;
                Top = Preferences.Top;
                Width = Preferences.Width;
                Height = Preferences.Height;
            }
            catch (Exception)
            {
                // use defaults if there was a problem
                Left = InstancePreferences.DefaultTopPosition;
                Top = InstancePreferences.DefaultLeftPosition;
                Width = InstancePreferences.DefaultWidth;
                Height = InstancePreferences.DefaultHeight;
                MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            // Checks the menuitem ofr the current folder.
            if (Preferences.OutlookFolderName == FolderViewType.Calendar.ToString())
            {
                CalendarMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == FolderViewType.Contacts.ToString())
            {
                ContactsMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == FolderViewType.Inbox.ToString())
            {
                InboxMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == FolderViewType.Notes.ToString())
            {
                NotesMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == FolderViewType.Tasks.ToString())
            {
                TasksMenu.Checked = true;
            }
            else
            {
                // custom folder
                _customFolder = Preferences.OutlookFolderName;
                String folderName = GetFolderNameFromFullPath(_customFolder, OutlookNameSpace.Folders);
                trayMenu.Items.Insert(GetSelectFolderMenuLocation() + 1, new ToolStripMenuItem(folderName, null, new EventHandler(CustomFolderMenu_Click)));
                _customMenu = (ToolStripMenuItem)trayMenu.Items[GetSelectFolderMenuLocation() + 1];
                _customMenu.Checked = true;
            }

            // Sets the viewcontrol folder from preferences. 
            axOutlookViewControl.Folder = Preferences.OutlookFolderName;

            // Sets the selected view from preferences. 
            axOutlookViewControl.View = Preferences.OutlookFolderView;

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
            if (Preferences.OutlookFolderEntryId != "" && Preferences.OutlookFolderStoreId != "")
                _outlookFolder = OutlookNameSpace.GetFolderFromID(Preferences.OutlookFolderEntryId, Preferences.OutlookFolderStoreId);
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
            OulookFolderViews = new List<Microsoft.Office.Interop.Outlook.View>();

            //uxOutlookViews.DropDownItems.Add(uxDefaultOutlookView);

            if (_outlookFolder != null)
            {
                //NOTE: Issue with the update of views in this instance of Outlook.
                //      Will have to spawn a new instance... Sigh.
                //SetMapiFolder();

                foreach (Microsoft.Office.Interop.Outlook.View view in _outlookFolder.Views)
                {
                    ToolStripMenuItem viewItem = new ToolStripMenuItem(view.Name) {Tag = view};

                    viewItem.Click += new EventHandler(viewItem_Click);

                    if (view.Name == Preferences.OutlookFolderView)
                        viewItem.Checked = true;

                    uxOutlookViews.DropDownItems.Add(viewItem);

                    OulookFolderViews.Add(view);
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oFolder"></param>
        /// <returns></returns>
        private static string GenerateFolderPathFromObject(MAPIFolder oFolder)
        {
            string fullFolderPath = "\\\\";
            List<string> subfolders = new List<string>();

            subfolders.Add(oFolder.Name);

            while (oFolder != null && oFolder.Parent != null)
            {
                oFolder = oFolder.Parent as MAPIFolder;
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
            if (Visible == true)
            {
                HideShowMenu.Text = Resources.Show;
                Visible = false;
            }
            else
            {
                HideShowMenu.Text = Resources.Hide;
                Visible = true;
            }
        }

        /// <summary>
        /// Returns a MAPI Folder for the passes FolderViewType.
        /// </summary>
        /// <param name="folderViewType"></param>
        /// <returns></returns>
        private MAPIFolder GetFolderFromViewType(FolderViewType folderViewType)
        {
            switch (folderViewType)
            {
                case FolderViewType.Inbox:
                    return OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                case FolderViewType.Calendar:
                    return OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                case FolderViewType.Contacts:
                    return OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderContacts);
                case FolderViewType.Notes:
                    return OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderNotes);
                case FolderViewType.Tasks:
                    return OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderTasks);
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

            if (_customMenu != null) menuItems.Add(_customMenu);

            CheckSelectedMenuItemInCollection(itemToCheck, menuItems);
        }

        /// <summary>
        /// For a given collection of MenuItems this function will iterate through them and then check the passed item. 
        /// </summary>
        /// <param name="itemToCheck">Item to check in the list</param>
        /// <param name="menuItems">IList of the menuitems to check</param>
        private static void CheckSelectedMenuItemInCollection(ToolStripMenuItem itemToCheck, IList menuItems)
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
            Preferences.OutlookFolderName = folderViewType.ToString();
            Preferences.OutlookFolderStoreId = GetFolderFromViewType(folderViewType).StoreID;
            Preferences.OutlookFolderEntryId = GetFolderFromViewType(folderViewType).EntryID;

            SetMapiFolder();

            UpdateOutlookViewsList();

            CheckSelectedFolder(itemToCheck);
        }

        private void ChangeDefaultFolderType(FolderViewType folderViewType)
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

        [CLSCompliant(false)]
        public void UpdateCustomFolder(MAPIFolder oFolder)
        {
            if (oFolder == null) return;

            try
            {
                // Remove old item (selectmenu+1)
                if (trayMenu.Items.Contains(_customMenu))
                {
                    trayMenu.Items.Remove(_customMenu);
                }

                String folderPath = GetFolderPath(GenerateFolderPathFromObject(oFolder));
                axOutlookViewControl.Folder = folderPath;

                // Save the EntryId and the StoreId for this folder in the prefrences. 
                Preferences.OutlookFolderEntryId = oFolder.EntryID;
                Preferences.OutlookFolderStoreId = oFolder.StoreID;

                Preferences.OutlookFolderName = folderPath;
                _customFolder = Preferences.OutlookFolderName;

                // Update the UI to reflect the new settings. 
                trayMenu.Items.Insert(GetSelectFolderMenuLocation() + 1, new ToolStripMenuItem(oFolder.Name, null, new EventHandler(CustomFolderMenu_Click)));
                _customMenu = (ToolStripMenuItem)trayMenu.Items[GetSelectFolderMenuLocation() + 1];

                SetMapiFolder();
                CheckSelectedFolder(_customMenu);
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
            if (viewItem != null)
            {
                Microsoft.Office.Interop.Outlook.View view = viewItem.Tag as Microsoft.Office.Interop.Outlook.View;

                if (view != null)
                {
                    axOutlookViewControl.View = view.Name;

                    Preferences.OutlookFolderView = view.Name;
                }
            }

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
            MAPIFolder oFolder = OutlookNameSpace.PickFolder();
            UpdateCustomFolder(oFolder);
        }

        private void CustomFolderMenu_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = _customFolder;

            CheckSelectedFolder(_customMenu);
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
                    if (appReg != null) appReg.DeleteSubKeyTree(InstanceName);
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
                catch
                {
                    // no big deal if we can't set the day, just ignore and go on.
                }
            }
            _previousDate = DateTime.Now;
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Dispose();

            Application.Exit();
        }

        private void RenameInstanceMenu_Click(object sender, EventArgs e)
        {
            InputBoxResult result = InputBox.InputBox.Show(this, "", "Rename Instance", InstanceName, inputBox_Validating);
            if (result.Ok)
            {
                using (RegistryKey parentKey = Registry.CurrentUser.OpenSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName, true))
                {
                    if (parentKey != null)
                    {
                        RegistryHelper.RenameSubKey(parentKey, InstanceName, result.Text);
                        String oldInstanceName = InstanceName;
                        InstanceName = result.Text;
                        Preferences = new InstancePreferences(InstanceName);

                        InstanceRenamed(this, new InstanceRenamedEventArgs(oldInstanceName, InstanceName));
                    }
                }
            }
        }

        private static void inputBox_Validating(object sender, InputBoxValidatingEventArgs e)
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
    }
}
