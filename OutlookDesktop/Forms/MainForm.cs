using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using Blue.Windows;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;
using NLog;
using OutlookDesktop.Enums;
using OutlookDesktop.Events;
using OutlookDesktop.Preferences;
using OutlookDesktop.Properties;
using OutlookDesktop.Utility;
using Application = System.Windows.Forms.Application;
using Exception = System.Exception;
using View = Microsoft.Office.Interop.Outlook.View;

namespace OutlookDesktop.Forms
{
    /// <summary>
    /// This is the form that hosts the outlook view control. One of these will exist for each instance.
    /// </summary>
    public partial class MainForm : Form
    {
        private const int ResizeBorderWidth = 4;
        private string _customFolder;
        private ToolStripMenuItem _customMenu;
        private MAPIFolder _outlookFolder;
        private DateTime _previousDate;
        private OutlookFolderDefinition _customFolderDefinition;
        private bool _outlookContextMenuActivated;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly StickyWindow _stickyWindow;

        // To avoid flicker when moving or resizing, this variable is set when a move or resize is started
        // and then reset when the move or resize is done.  SetWindowPos inside of WndProc will not fire
        // when this is true.
        private bool _movingOrResizing;

        /// <summary>
        /// Sets up the form for the current instance.
        /// </summary>
        /// <param name="instanceName">The name of the instance to display.</param>
        public MainForm(string instanceName)
        {
            try
            {
                InitializeComponent();
            }
            catch (COMException loE)
            {
                Logger.Error("Error initializing main view: {0}", loE);
                if ((uint)loE.ErrorCode == 0x80040154)
                {
                    MessageBox.Show(this, Resources.Incorrect_bittedness_of_OotD, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }

            try
            {
                InstanceName = instanceName;

                // Uniquely identify the previous/next buttons for use in an ugly hack below.
                ButtonNext.Tag = Guid.NewGuid();
                ButtonPrevious.Tag = Guid.NewGuid();

                SuspendLayout();
                LoadSettings();
                ResumeLayout();
                SendWindowToBack();

                // hook up sticky window instance and events to let us know when resizing/moving
                // has ended so we can update the form dimensions in the preferences.
                _stickyWindow = new StickyWindow(this);
                _stickyWindow.MoveEnded += (sender, args) =>
                {
                    _movingOrResizing = false;
                    SaveFormDimensions();
                };
                _stickyWindow.ResizeEnded += (sender, args) =>
                {
                    _movingOrResizing = false;
                    SaveFormDimensions();
                };

                // hook up event to keep the date in the header bar up to date
                OutlookViewControl.SelectionChange += OnAxOutlookViewControlOnSelectionChange;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error initializing window.");
                MessageBox.Show(this, Resources.ErrorInitializingApp + Environment.NewLine + ex.Message, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void OnAxOutlookViewControlOnSelectionChange(object sender, EventArgs args)
        {
            try
            {
                LabelCurrentDate.Text = OutlookViewControl.SelectedDate.ToLongDateString();
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Error setting date in header.");
            }
        }

        private void SendWindowToBack()
        {
            if (Environment.OSVersion.Version.Major < 6 || !UnsafeNativeMethods.DwmIsCompositionEnabled())
                // Windows XP or higher with DWM window composition disabled
                UnsafeNativeMethods.PinWindowToDesktop(this);
            else if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 0)
                // Vista
                UnsafeNativeMethods.SendWindowToBack(this);
            else
            {
                // Windows 7 or above
                UnsafeNativeMethods.RemoveWindowFromAeroPeek(this);
                UnsafeNativeMethods.SendWindowToBack(this);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;  // Turn on WS_EX_TOOLWINDOW style bit to hide window from alt-tab
                return cp;
            }
        }

        #region Events

        public EventHandler<InstanceRemovedEventArgs> InstanceRemoved;
        public EventHandler<InstanceRenamedEventArgs> InstanceRenamed;

        private void OnInstanceRemoved(object sender, InstanceRemovedEventArgs e)
        {
            InstanceRemoved?.Invoke(sender, e);
        }

        private void OnInstanceRenamed(object sender, InstanceRenamedEventArgs e)
        {
            InstanceRenamed?.Invoke(sender, e);
        }

        #endregion

        /// <summary>
        /// Get the location of the Select folder menu in the tray context menu. 
        /// </summary>
        /// <returns></returns>
        private int GetSelectFolderMenuLocation()
        {
            return TrayMenu.Items.IndexOf(SelectFolderMenu);
        }

        /// <summary>
        /// Loads user preferences from registry and applies them.
        /// </summary>
        private void LoadSettings()
        {
            // create a new instance of the preferences class
            Preferences = new InstancePreferences(InstanceName);

            // There should be no reason other than first run as to why the Store and Entry IDs are 
            // empty. 
            if (string.IsNullOrEmpty(Preferences.OutlookFolderStoreId))
            {
                // Set the MAPI Folder Details and the IDs.
                Preferences.OutlookFolderName = FolderViewType.Calendar.ToString();
                Preferences.OutlookFolderStoreId = GetFolderFromViewType(FolderViewType.Calendar).StoreID;
                Preferences.OutlookFolderEntryId = GetFolderFromViewType(FolderViewType.Calendar).EntryID;
            }

            SetMAPIFolder();

            // Sets the opacity of the instance. 
            try
            {
                Opacity = Preferences.Opacity;
            }
            catch (Exception ex)
            {
                // use default if there was a problem
                Opacity = InstancePreferences.DefaultOpacity;
                Logger.Error(ex, "Error setting opacity.");
                MessageBox.Show(this, Resources.ErrorSettingOpacity, Resources.ErrorCaption, MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            TransparencySlider.Value = (int)(Preferences.Opacity * 100);

            // Sets the position of the instance. 
            try
            {
                Left = Preferences.Left;
                Top = Preferences.Top;
                Width = Preferences.Width;
                Height = Preferences.Height;
            }
            catch (Exception ex)
            {
                // use defaults if there was a problem
                Left = InstancePreferences.DefaultTopPosition;
                Top = InstancePreferences.DefaultLeftPosition;
                Width = InstancePreferences.DefaultWidth;
                Height = InstancePreferences.DefaultHeight;
                Logger.Error(ex, "Error setting window position.");
                MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            // Checks the menuitem of the current folder.
            if (Preferences.OutlookFolderName == GetFolderFromViewType(FolderViewType.Calendar).Name)
            {
                CalendarMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == GetFolderFromViewType(FolderViewType.Contacts).Name)
            {
                ContactsMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == GetFolderFromViewType(FolderViewType.Inbox).Name)
            {
                InboxMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == GetFolderFromViewType(FolderViewType.Notes).Name)
            {
                NotesMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == GetFolderFromViewType(FolderViewType.Tasks).Name)
            {
                TasksMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == GetFolderFromViewType(FolderViewType.Todo).Name)
            {
                TodosMenu.Checked = true;
            }
            else
            {
                // custom folder
                _customFolder = Preferences.OutlookFolderName;
                string folderName = GetFolderNameFromFullPath(_customFolder);
                TrayMenu.Items.Insert(GetSelectFolderMenuLocation() + 1, new ToolStripMenuItem(folderName, null, CustomFolderMenu_Click));
                _customMenu = (ToolStripMenuItem)TrayMenu.Items[GetSelectFolderMenuLocation() + 1];
                _customMenu.Checked = true;

                // store the custom folder definition in case the user wants to switch back to it and we need to reload it.
                _customFolderDefinition.OutlookFolderName = Preferences.OutlookFolderName;
                _customFolderDefinition.OutlookFolderStoreId = Preferences.OutlookFolderStoreId;
                _customFolderDefinition.OutlookFolderEntryId = Preferences.OutlookFolderEntryId;
            }

            // Sets the viewcontrol folder from preferences. 
            OutlookViewControl.Folder = Preferences.OutlookFolderName;

            // Sets the selected view from preferences. 
            try
            {
                OutlookViewControl.View = Preferences.OutlookFolderView;
            }
            catch
            {
                // if we get an exception here, it means the view stored doesn't apply to the current folder view,
                // so just reset it.
                Preferences.OutlookFolderView = string.Empty;
            }

            // If the view is a calendar view, use the stored ViewXML to restore their day/week/month view setting.
            if (Preferences.OutlookFolderName == FolderViewType.Calendar.ToString())
            {
                if (!string.IsNullOrEmpty(Preferences.ViewXml))
                {
                    OutlookViewControl.ViewXML = Preferences.ViewXml;
                }
                else
                {
                    SetViewXml(Resources.MonthXML);
                }
            }

            // Get a copy of the possible outlook views for the selected folder and populate the context menu for this instance. 
            UpdateOutlookViewsList();

            // Sets whether the instance is allowed to be edited or not
            if (Preferences.DisableEditing)
            {
                DisableEnableEditing();
            }
        }

        /// <summary>
        /// This will populate the _outlookFolder object with the MapiFolder for the EntryID and StoreId stored
        /// in the registry. 
        /// </summary>
        private void SetMAPIFolder()
        {
            // Load up the MAPI Folder from Entry / Store IDs 
            if (!string.IsNullOrEmpty(Preferences.OutlookFolderEntryId) && !string.IsNullOrEmpty(Preferences.OutlookFolderStoreId))
            {
                try
                {
                    _outlookFolder = Startup.OutlookNameSpace.GetFolderFromID(Preferences.OutlookFolderEntryId, Preferences.OutlookFolderStoreId);
                    ShowToolbarButtonsFor(_outlookFolder.FolderPath);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error setting MAPI folder.");
                }
            }
            else
            {
                _outlookFolder = null;
            }
        }

        /// <summary>
        /// This will populate a dropdown off the instance context menu with the available
        /// views in outlook, it will also associate the menuitem with the event handler. 
        /// </summary>
        private void UpdateOutlookViewsList()
        {
            OutlookViewsMenu.DropDownItems.Clear();
            OutlookFolderViews = new List<View>();

            if (_outlookFolder != null)
            {
                foreach (View view in _outlookFolder.Views)
                {
                    var viewItem = new ToolStripMenuItem(view.Name) { Tag = view };

                    viewItem.Click += ViewItem_Click;

                    if (view.Name == Preferences.OutlookFolderView)
                        viewItem.Checked = true;

                    OutlookViewsMenu.DropDownItems.Add(viewItem);

                    OutlookFolderViews.Add(view);
                }
            }
        }

        /// <summary>
        /// Will select the passed menu item in the views dropdown list. 
        /// </summary>
        /// <param name="viewItem">ToolStripMenuItem that is to be checked.</param>
        private void CheckSelectedView(ToolStripMenuItem viewItem)
        {
            CheckSelectedMenuItemInCollection(viewItem, OutlookViewsMenu.DropDownItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private static string GetFolderNameFromFullPath(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf("\\", StringComparison.Ordinal) + 1, fullPath.Length - fullPath.LastIndexOf("\\", StringComparison.Ordinal) - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oFolder"></param>
        /// <returns></returns>
        private static string GenerateFolderPathFromObject(MAPIFolder oFolder)
        {
            string fullFolderPath = "\\\\";
            var subfolders = new List<string> { oFolder.Name };

            while (oFolder?.Parent != null)
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

        private static string GetFolderPath(string folderPath)
        {
            return folderPath.Replace("\\\\Personal Folders\\", "");
        }

        private void ShowHideDesktopComponent()
        {
            if (Visible)
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

        private void DisableEnableEditing()
        {
            if (Enabled)
            {
                DisableEnableEditingMenu.Checked = true;
                Preferences.DisableEditing = true;
                Enabled = false;
            }
            else
            {
                DisableEnableEditingMenu.Checked = false;
                Preferences.DisableEditing = false;
                Enabled = true;
            }
        }

        /// <summary>
        /// Returns a MAPI Folder for the passes FolderViewType.
        /// </summary>
        /// <param name="folderViewType"></param>
        /// <returns></returns>
        private static MAPIFolder GetFolderFromViewType(FolderViewType folderViewType)
        {
            switch (folderViewType)
            {
                case FolderViewType.Inbox:
                    return Startup.OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                case FolderViewType.Calendar:
                    return Startup.OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                case FolderViewType.Contacts:
                    return Startup.OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderContacts);
                case FolderViewType.Notes:
                    return Startup.OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderNotes);
                case FolderViewType.Tasks:
                    return Startup.OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderTasks);
                case FolderViewType.Todo:
                    return Startup.OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Checks the passed menu item and unchecks the rest.
        /// This is used only for the folder types menu. 
        /// </summary>
        /// <param name="itemToCheck"></param>
        private void CheckSelectedMenuItem(ToolStripMenuItem itemToCheck)
        {
            var menuItems = new List<ToolStripMenuItem> { CalendarMenu, ContactsMenu, InboxMenu, NotesMenu, TasksMenu, TodosMenu };

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
                menuItem.Checked = menuItem == itemToCheck;
            }
        }

        /// <summary>
        /// Generic function to deal with menu check items for selecting the folders to view. 
        /// </summary>
        /// <param name="folderViewType"></param>
        /// <param name="itemToCheck"></param>
        private void DefaultFolderTypesClicked(FolderViewType folderViewType, ToolStripMenuItem itemToCheck)
        {
            if (folderViewType != FolderViewType.Calendar)
            {
                SetViewXml(string.Empty);
            }

            try
            {
                OutlookViewControl.Folder = GetFolderFromViewType(folderViewType).Name;

                Preferences.OutlookFolderName = GetFolderFromViewType(folderViewType).Name;
                Preferences.OutlookFolderStoreId = GetFolderFromViewType(folderViewType).StoreID;
                Preferences.OutlookFolderEntryId = GetFolderFromViewType(folderViewType).EntryID;

                SetMAPIFolder();

                UpdateOutlookViewsList();

                CheckSelectedMenuItem(itemToCheck);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.ViewTypeNotSupported, Resources.ErrorCaption, MessageBoxButtons.OK);
                Logger.Error(ex, "Unable to set Outlook folder.");
            }
        }

        /// <summary>
        /// Given a folderPath, show the appropriate buttons in the toolbar for that view.
        /// </summary>
        /// <param name="folderPath"></param>
        private void ShowToolbarButtonsFor(string folderPath)
        {
            switch (folderPath)
            {
                case string c when c.Contains("\\Calendar"):
                    {
                        TodayButton.Visible = true;
                        DayButton.Visible = true;
                        WeekButton.Visible = true;
                        WorkWeekButton.Visible = true;
                        MonthButton.Visible = true;
                        ButtonPrevious.Visible = true;
                        ButtonNext.Visible = true;
                        LabelCurrentDate.Visible = true;

                        NewEmailButton.Visible = false;
                        break;
                    }
                case string s when s.Contains("\\Inbox"):
                    {
                        TodayButton.Visible = false;
                        DayButton.Visible = false;
                        WeekButton.Visible = false;
                        WorkWeekButton.Visible = false;
                        MonthButton.Visible = false;
                        ButtonPrevious.Visible = false;
                        ButtonNext.Visible = false;
                        LabelCurrentDate.Visible = false;

                        NewEmailButton.Left = ButtonNext.Left;
                        NewEmailButton.Visible = true;
                        break;
                    }
                default:
                    {
                        TodayButton.Visible = false;
                        DayButton.Visible = false;
                        WeekButton.Visible = false;
                        WorkWeekButton.Visible = false;
                        MonthButton.Visible = false;
                        ButtonPrevious.Visible = false;
                        ButtonNext.Visible = false;
                        LabelCurrentDate.Visible = false;

                        NewEmailButton.Visible = false;
                        break;
                    }
            }
        }

        private void UpdateCustomFolder(MAPIFolder oFolder)
        {
            if (oFolder == null) return;

            try
            {
                // Remove old item (selectmenu+1)
                if (TrayMenu.Items.Contains(_customMenu))
                {
                    TrayMenu.Items.Remove(_customMenu);
                }

                string folderPath = GetFolderPath(GenerateFolderPathFromObject(oFolder));
                OutlookViewControl.Folder = folderPath;

                // Save the EntryId and the StoreId for this folder in the preferences. 
                Preferences.OutlookFolderEntryId = oFolder.EntryID;
                Preferences.OutlookFolderStoreId = oFolder.StoreID;
                Preferences.OutlookFolderName = folderPath;

                // store the custom folder details in memory as well, in case the user wants to switch back to it.
                _customFolderDefinition.OutlookFolderName = Preferences.OutlookFolderName;
                _customFolderDefinition.OutlookFolderStoreId = Preferences.OutlookFolderStoreId;
                _customFolderDefinition.OutlookFolderEntryId = Preferences.OutlookFolderEntryId;

                _customFolder = Preferences.OutlookFolderName;

                // Update the UI to reflect the new settings. 
                TrayMenu.Items.Insert(GetSelectFolderMenuLocation() + 1, new ToolStripMenuItem(oFolder.Name, null, CustomFolderMenu_Click));
                _customMenu = (ToolStripMenuItem)TrayMenu.Items[GetSelectFolderMenuLocation() + 1];

                SetMAPIFolder();
                CheckSelectedMenuItem(_customMenu);
                UpdateOutlookViewsList();
            }
            catch (Exception)
            {
                MessageBox.Show(this, Resources.ErrorSettingFolder, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResizeForm(ResizeDirection direction)
        {
            if (GlobalPreferences.LockPosition) return;

            _movingOrResizing = true;
        
            int dir = -1;
            switch (direction)
            {
                case ResizeDirection.Left:
                    dir = UnsafeNativeMethods.HTLEFT;
                    break;
                case ResizeDirection.TopLeft:
                    dir = UnsafeNativeMethods.HTTOPLEFT;
                    break;
                case ResizeDirection.Top:
                    dir = UnsafeNativeMethods.HTTOP;
                    break;
                case ResizeDirection.TopRight:
                    dir = UnsafeNativeMethods.HTTOPRIGHT;
                    break;
                case ResizeDirection.Right:
                    dir = UnsafeNativeMethods.HTRIGHT;
                    break;
                case ResizeDirection.BottomRight:
                    dir = UnsafeNativeMethods.HTBOTTOMRIGHT;
                    break;
                case ResizeDirection.Bottom:
                    dir = UnsafeNativeMethods.HTBOTTOM;
                    break;
                case ResizeDirection.BottomLeft:
                    dir = UnsafeNativeMethods.HTBOTTOMLEFT;
                    break;
            }

            if (dir != -1)
            {
                UnsafeNativeMethods.ReleaseCapture();
                UnsafeNativeMethods.SendMessage(Handle, UnsafeNativeMethods.WM_NCLBUTTONDOWN, (IntPtr)dir, IntPtr.Zero);
            }
        }

        private void MoveForm()
        {
            if (GlobalPreferences.LockPosition)
                return;

            _movingOrResizing = true;
                 
            UnsafeNativeMethods.ReleaseCapture();
            UnsafeNativeMethods.SendMessage(Handle, UnsafeNativeMethods.WM_NCLBUTTONDOWN, (IntPtr)UnsafeNativeMethods.HTCAPTION, IntPtr.Zero);
        }

        #region Event Handlers

        /// <summary>
        /// When a view is selected this will change the view control view to it, save it in the 
        /// preferences and then check the box next to the view in the drop down list. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewItem_Click(object sender, EventArgs e)
        {
            var viewItem = sender as ToolStripMenuItem;
            var view = viewItem?.Tag as View;

            if (view != null)
            {
                OutlookViewControl.View = view.Name;

                Preferences.OutlookFolderView = view.Name;
            }

            CheckSelectedView(viewItem);
        }

        /// <summary>
        /// This handler will select a custom folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectFolderMenu_Click(object sender, EventArgs e)
        {
            MAPIFolder oFolder = Startup.OutlookNameSpace.PickFolder();
            if (oFolder != null)
            {
                UpdateCustomFolder(oFolder);
            }
        }

        private void CustomFolderMenu_Click(object sender, EventArgs e)
        {
            OutlookViewControl.Folder = _customFolder;
            CheckSelectedMenuItem(_customMenu);

            Preferences.OutlookFolderName = _customFolderDefinition.OutlookFolderName;
            Preferences.OutlookFolderStoreId = _customFolderDefinition.OutlookFolderStoreId;
            Preferences.OutlookFolderEntryId = _customFolderDefinition.OutlookFolderEntryId;

            SetMAPIFolder();
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

        private void TodosMenu_Click(object sender, EventArgs e)
        {
            DefaultFolderTypesClicked(FolderViewType.Todo, TodosMenu);
        }

        private void HideMenu_Click(object sender, EventArgs e)
        {
            ShowHideDesktopComponent();
        }

        private void DisableEnableEditingMenu_Click(object sender, EventArgs e)
        {
            DisableEnableEditing();
        }

        private void RemoveInstanceMenu_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(Parent, Resources.RemoveInstanceConfirmation,
                                               Resources.ConfirmationCaption, MessageBoxButtons.YesNo,
                                               MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                using (var appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
                {
                    appReg?.DeleteSubKeyTree(InstanceName);
                }

                OnInstanceRemoved(this, new InstanceRemovedEventArgs(InstanceName));
                Dispose();
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // increment day in outlook's calendar if we've crossed over into a new day
            if (DateTime.Now.Day != _previousDate.Day)
            {
                try
                {
                    OutlookViewControl.GoToToday();
                }
                catch (Exception ex)
                {
                    // no big deal if we can't set the day, just ignore and go on.
                    Logger.Warn(ex, "Unable to go to today on calendar.");
                }
            }
            _previousDate = DateTime.Now;
        }

        private void SaveFormDimensions()
        {
            Debug.WriteLine("Saving window position");
            Debug.WriteLine("Left: {0}", Left);
            Debug.WriteLine("Top: {0}", Top);
            Debug.WriteLine("Width: {0}", Width);
            Debug.WriteLine("Height: {0}", Height);
            Preferences.Left = Left;
            Preferences.Top = Top;
            Preferences.Width = Width;
            Preferences.Height = Height;
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Dispose();
            Startup.DisposeOutlookObjects();

            Application.Exit();
        }

        private void RenameInstanceMenu_Click(object sender, EventArgs e)
        {
            var result = InputBox.Show(this, "", "Rename Instance", InstanceName, InputBox_Validating);
            if (!result.Ok) return;

            using (var parentKey = Registry.CurrentUser.OpenSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName, true))
            {
                if (parentKey == null) return;

                RegistryHelper.RenameSubKey(parentKey, InstanceName, result.Text);
                string oldInstanceName = InstanceName;
                InstanceName = result.Text;
                Preferences = new InstancePreferences(InstanceName);

                OnInstanceRenamed(this, new InstanceRenamedEventArgs(oldInstanceName, InstanceName));
            }
        }

        private static void InputBox_Validating(object sender, InputBoxValidatingEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text.Trim()))
            {
                e.Cancel = true;
                e.Message = "Required";
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && WindowState != FormWindowState.Maximized)
            {
                // temporarily hide Outlook View Control because it makes resizing really slow
                ViewControlHostPanel.Visible = false;
                ResizeForm(ResizeDir);
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            // restore Outlook View Control visibily
            ViewControlHostPanel.Visible = true;
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (GlobalPreferences.LockPosition)
                return;

            if (e.Location.X < ResizeBorderWidth && e.Location.Y < ResizeBorderWidth)
                ResizeDir = ResizeDirection.TopLeft;

            else if (e.Location.X < ResizeBorderWidth && e.Location.Y > Height - ResizeBorderWidth)
                ResizeDir = ResizeDirection.BottomLeft;

            else if (e.Location.X > Width - ResizeBorderWidth && e.Location.Y > Height - ResizeBorderWidth)
                ResizeDir = ResizeDirection.BottomRight;

            else if (e.Location.X > Width - ResizeBorderWidth && e.Location.Y < ResizeBorderWidth)
                ResizeDir = ResizeDirection.TopRight;

            else if (e.Location.X < ResizeBorderWidth)
                ResizeDir = ResizeDirection.Left;

            else if (e.Location.X > Width - ResizeBorderWidth)
                ResizeDir = ResizeDirection.Right;

            else if (e.Location.Y < ResizeBorderWidth)
                ResizeDir = ResizeDirection.Top;

            else if (e.Location.Y > Height - ResizeBorderWidth)
                ResizeDir = ResizeDirection.Bottom;

            else
                ResizeDir = ResizeDirection.None;
        }

        private void HeaderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && WindowState != FormWindowState.Maximized)
            {
                MoveForm();
            }
        }

        private void LabelBackground_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && WindowState != FormWindowState.Maximized)
            {
                MoveForm();
            }
        }

        private void LabelCurrentDate_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && WindowState != FormWindowState.Maximized)
            {
                MoveForm();
            }
        }

        #region Resize Cursor Reset Events
        private void HeaderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            ResizeDir = ResizeDirection.None;
            Cursor = Cursors.SizeAll;
        }

        private void WorkWeekButton_MouseHover(object sender, EventArgs e)
        {
            ResizeDir = ResizeDirection.None;
            Cursor = Cursors.Default;
        }

        private void DayButton_MouseHover(object sender, EventArgs e)
        {
            ResizeDir = ResizeDirection.None;
            Cursor = Cursors.Default;
        }

        private void TodayButton_MouseHover(object sender, EventArgs e)
        {
            ResizeDir = ResizeDirection.None;
            Cursor = Cursors.Default;
        }

        private void WeekButton_MouseHover(object sender, EventArgs e)
        {
            ResizeDir = ResizeDirection.None;
            Cursor = Cursors.Default;
        }

        private void MonthButton_MouseHover(object sender, EventArgs e)
        {
            ResizeDir = ResizeDirection.None;
            Cursor = Cursors.Default;
        }

        private void ButtonPrevious_MouseHover(object sender, EventArgs e)
        {
            ResizeDir = ResizeDirection.None;
            Cursor = Cursors.Default;
        }

        private void ButtonNext_MouseHover(object sender, EventArgs e)
        {
            ResizeDir = ResizeDirection.None;
            Cursor = Cursors.Default;
        }

        private void TransparencySlider_MouseHover(object sender, EventArgs e)
        {
            ResizeDir = ResizeDirection.None;
            Cursor = Cursors.Default;
        }


        private void NewEmailButton_MouseHover(object sender, EventArgs e)
        {
            ResizeDir = ResizeDirection.None;
            Cursor = Cursors.Default;
        }

        private void LabelCurrentDate_MouseHover(object sender, EventArgs e)
        {
            Cursor = Cursors.SizeAll;
        }

        private void LabelBackground_MouseHover(object sender, EventArgs e)
        {
            Cursor = Cursors.SizeAll;
        }
        #endregion

        private void SetViewXml(string value)
        {
            OutlookViewControl.ViewXML = value;
            Preferences.ViewXml = value;
        }

        private void DayButton_Click(object sender, EventArgs e)
        {
            SetViewXml(Resources.DayXML);
        }

        private void WorkWeekButton_Click(object sender, EventArgs e)
        {
            SetViewXml(Resources.WorkWeekXML);
        }

        private void WeekButton_Click(object sender, EventArgs e)
        {
            SetViewXml(Resources.WeekXML);
        }

        private void MonthButton_Click(object sender, EventArgs e)
        {
            SetViewXml(Resources.MonthXML);
        }

        private void TodayButton_Click(object sender, EventArgs e)
        {
            OutlookViewControl.GoToToday();
        }

        private void NewEmailButton_Click(object sender, EventArgs e)
        {
            OutlookViewControl.NewMessage();
        }

        private void ButtonPrevious_Click(object sender, EventArgs e)
        {
            // get the view mode from the current ViewXML, this will tell us what calendar view we're in
            var mode = GetCurrentCalendarViewMode();

            SetCurrentViewControlAsActiveIfNecessary(mode, ButtonPrevious, ref Startup.LastPreviousButtonClicked);

            var offset = GetNextPreviousOffsetBasedOnCalendarViewMode(mode);

            OutlookViewControl.GoToDate(OutlookViewControl.SelectedDate.AddDays(offset * -1).ToString(CultureInfo.InvariantCulture));
        }

        private void ButtonNext_Click(object sender, EventArgs e)
        {
            // get the view mode from the current ViewXML, this will tell us what calendar view we're in
            var mode = GetCurrentCalendarViewMode();

            SetCurrentViewControlAsActiveIfNecessary(mode, ButtonNext, ref Startup.LastNextButtonClicked);

            var offset = GetNextPreviousOffsetBasedOnCalendarViewMode(mode);

            OutlookViewControl.GoToDate(OutlookViewControl.SelectedDate.AddDays(offset).ToString(CultureInfo.InvariantCulture));
        }

        private static double GetNextPreviousOffsetBasedOnCalendarViewMode(CurrentCalendarView mode)
        {
            double offset;

            switch (mode)
            {
                case CurrentCalendarView.Day:
                    offset = 1;
                    break;
                case CurrentCalendarView.Week:
                case CurrentCalendarView.WorkWeek:
                    offset = 7;
                    break;
                case CurrentCalendarView.Month:
                    offset = 31;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            return offset;
        }

        // Terrible hack to get around a bug in the Outlook View Control where if you have more than one
        // calendar view active, GoToDate will not work on the instance it's called on, instead it will 
        // work on the last "active" view of the calendar, which may or may not be the current one.  
        // So to get around that, if the last clicked next button was not this one, we reset the 
        // calendar view to make it active, before using GoToDate.            
        private void SetCurrentViewControlAsActiveIfNecessary(CurrentCalendarView mode, Button button, ref Guid lastButtonGuidClicked)
        {
            // we don't need to do this if we only have one instance, so bail right away.
            if (InstanceManager.InstanceCount == 1) return;

            // we can bail if we know the last button clicked was the one on this form.
            if ((Guid)button.Tag == lastButtonGuidClicked) return;

            var currentDate = OutlookViewControl.SelectedDate;

            switch (mode)
            {
                case CurrentCalendarView.Day:
                    SetViewXml(Resources.DayXML);
                    break;
                case CurrentCalendarView.Week:
                    SetViewXml(Resources.WeekXML);
                    break;
                case CurrentCalendarView.WorkWeek:
                    SetViewXml(Resources.WorkWeekXML);
                    break;
                case CurrentCalendarView.Month:
                    SetViewXml(Resources.MonthXML);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            OutlookViewControl.GoToDate(currentDate.ToString(CultureInfo.InvariantCulture));
            lastButtonGuidClicked = (Guid)button.Tag;
        }

        private CurrentCalendarView GetCurrentCalendarViewMode()
        {
            var mode = CurrentCalendarView.Day;

            var xElement = XDocument.Parse(OutlookViewControl.ViewXML).Element("view");
            var element = xElement?.Element("mode");
            if (element != null)
            {
                mode = (CurrentCalendarView)Convert.ToInt32(element.Value);
            }
            return mode;
        }

        private void TransparencySlider_ValueChanged(object sender, EventArgs e, decimal value)
        {
            var opacityVal = (double)(value / 100);
            if (Math.Abs(opacityVal - 1) < double.Epsilon)
            {
                opacityVal = 0.99;
            }
            Opacity = opacityVal;
            Preferences.Opacity = opacityVal;
        }

        private void WindowMessageTimer_Tick(object sender, EventArgs e)
        {
            WindowMessageTimer.Enabled = false;
        }
        #endregion

        /// <summary>
        /// Standard windows message handler.  The main reason this exists is to ensure 
        /// the OotD window always stays behind other windows.  A side affect of that is that even
        /// context menus from OotD show up behind the main window, so we have to do some trickery below
        /// to handle that case and make sure that the outlook view control context menu shows up in front of 
        /// the main window.  Since we don't have access to the context menu directly, we have to bring the 
        /// window to the front temporarily while the context menu is visible.  Terrible hack.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == UnsafeNativeMethods.WM_PARENTNOTIFY)
            {
                if (m.WParam.ToInt32() == UnsafeNativeMethods.WM_RBUTTONDOWN)
                {
                    _outlookContextMenuActivated = true;
                    UnsafeNativeMethods.SendWindowToTop(this);
                    WindowMessageTimer.Start();
                    m.Result = IntPtr.Zero;
                }
            }
            else if (m.Msg == UnsafeNativeMethods.WM_NCACTIVATE)
            {
                if (m.WParam.ToInt32() == 1 && _outlookContextMenuActivated && !WindowMessageTimer.Enabled)
                {
                    _outlookContextMenuActivated = false;
                    UnsafeNativeMethods.SendWindowToBack(this);
                    m.Result = IntPtr.Zero;
                }
            }
            else if (m.Msg == UnsafeNativeMethods.WM_WINDOWPOSCHANGING && !_outlookContextMenuActivated && !Startup.UpdateDetected && !_movingOrResizing)
            {
                var mwp = (UnsafeNativeMethods.WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(UnsafeNativeMethods.WINDOWPOS));
                mwp.flags = mwp.flags | UnsafeNativeMethods.SWP_NOZORDER;
                Marshal.StructureToPtr(mwp, m.LParam, true);
                UnsafeNativeMethods.SendWindowToBack(this);
                m.Result = IntPtr.Zero;
            }

            base.WndProc(ref m);
        }

        #region Properties

        private ResizeDirection _resizeDir = ResizeDirection.None;
        private List<View> OutlookFolderViews { get; set; }
        public InstancePreferences Preferences { get; private set; }
        private string InstanceName { get; set; }

        private ResizeDirection ResizeDir
        {
            get { return _resizeDir; }
            set
            {
                _resizeDir = value;

                switch (value)
                {
                    case ResizeDirection.Left:
                    case ResizeDirection.Right:
                        Cursor = Cursors.SizeWE;
                        break;
                    case ResizeDirection.Top:
                    case ResizeDirection.Bottom:
                        Cursor = Cursors.SizeNS;
                        break;
                    case ResizeDirection.BottomLeft:
                    case ResizeDirection.TopRight:
                        Cursor = Cursors.SizeNESW;
                        break;
                    case ResizeDirection.BottomRight:
                    case ResizeDirection.TopLeft:
                        Cursor = Cursors.SizeNWSE;
                        break;
                    default:
                        Cursor = Cursors.Default;
                        break;
                }
            }
        }

        #endregion

        #region Nested type: ResizeDirection

        private enum ResizeDirection
        {
            None = 0,
            Left = 1,
            TopLeft = 2,
            Top = 3,
            TopRight = 4,
            Right = 5,
            BottomRight = 6,
            Bottom = 7,
            BottomLeft = 8
        }
        #endregion        
    }
}