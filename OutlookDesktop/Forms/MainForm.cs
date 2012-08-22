using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BitFactory.Logging;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;
using OutlookDesktop.Properties;
using Application = System.Windows.Forms.Application;
using Exception = System.Exception;
using View = Microsoft.Office.Interop.Outlook.View;

namespace OutlookDesktop.Forms
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
        private const int _resizeBorderWidth = 4;

        private String _customFolder;
        private ToolStripMenuItem _customMenu;
        private MAPIFolder _outlookFolder;
        private DateTime _previousDate;

        /// <summary>
        /// Sets up the form for the current instance.
        /// </summary>
        /// <param name="instanceName">The name of the instance to display.</param>
        public MainForm(String instanceName)
        {
            InitializeComponent();

            InstanceName = instanceName;

            SuspendLayout();
            LoadSettings();
            ResumeLayout();

            if (Environment.OSVersion.Version.Major < 6 || !UnsafeNativeMethods.DwmIsCompositionEnabled())
                // Windows XP or higher with DWM window composition disabled
                UnsafeNativeMethods.PinWindowToDesktop(this);
            else if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 0)
                // Vista
                UnsafeNativeMethods.SendWindowToBack(this);
            else
            {
                // Windows 7 or above
                UnsafeNativeMethods.SendWindowToBack(this);
                UnsafeNativeMethods.RemoveWindowFromAeroPeek(this);
            }
        }

        #region Events

        private EventHandler<InstanceRemovedEventArgs> _instanceRemoved;
        private EventHandler<InstanceRenamedEventArgs> _instanceRenamed;

        public event EventHandler<InstanceRemovedEventArgs> InstanceRemoved
        {
            // The following ensures that the event can only be subscribed too at most once
            add
            {
                if (_instanceRemoved == null || !_instanceRemoved.GetInvocationList().Contains(value))
                {
                    _instanceRemoved += value;
                }
            }
            remove { _instanceRemoved -= value; }
        }

        private void OnInstanceRemoved(object sender, InstanceRemovedEventArgs e)
        {
            if (_instanceRemoved != null)
            {
                _instanceRemoved(sender, e);
            }
        }

        public event EventHandler<InstanceRenamedEventArgs> InstanceRenamed
        {
            // The following ensures that the event can only be subscribed too at most once
            add
            {
                if (_instanceRenamed == null || !_instanceRenamed.GetInvocationList().Contains(value))
                {
                    _instanceRenamed += value;
                }
            }
            remove { _instanceRenamed -= value; }
        }

        private void OnInstanceRenamed(object sender, InstanceRenamedEventArgs e)
        {
            if (_instanceRenamed != null)
            {
                _instanceRenamed(sender, e);
            }
        }

        #endregion

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
            catch (Exception)
            {
                // use defaults if there was a problem
                Left = InstancePreferences.DefaultTopPosition;
                Top = InstancePreferences.DefaultLeftPosition;
                Width = InstancePreferences.DefaultWidth;
                Height = InstancePreferences.DefaultHeight;
                MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            // Checks the menuitem of the current folder.
            if (Preferences.OutlookFolderName == FolderViewType.Calendar.ToString())
            {
                CalendarMenu.Checked = true;
                ShowCalendarButtons(true);
            }
            else if (Preferences.OutlookFolderName == FolderViewType.Contacts.ToString())
            {
                ShowCalendarButtons(false);
                ContactsMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == FolderViewType.Inbox.ToString())
            {
                ShowCalendarButtons(false);
                InboxMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == FolderViewType.Notes.ToString())
            {
                ShowCalendarButtons(false);
                NotesMenu.Checked = true;
            }
            else if (Preferences.OutlookFolderName == FolderViewType.Tasks.ToString())
            {
                ShowCalendarButtons(false);
                TasksMenu.Checked = true;
            }
            else
            {
                // custom folder
                _customFolder = Preferences.OutlookFolderName;
                string folderName = GetFolderNameFromFullPath(_customFolder);
                trayMenu.Items.Insert(GetSelectFolderMenuLocation() + 1,
                                      new ToolStripMenuItem(folderName, null, CustomFolderMenu_Click));
                _customMenu = (ToolStripMenuItem)trayMenu.Items[GetSelectFolderMenuLocation() + 1];
                _customMenu.Checked = true;
            }

            // Sets the viewcontrol folder from preferences. 
            axOutlookViewControl.Folder = Preferences.OutlookFolderName;

            // Sets the selected view from preferences. 
            try
            {
                axOutlookViewControl.View = Preferences.OutlookFolderView;
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
                axOutlookViewControl.ViewXML = Preferences.ViewXml;
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
        private void SetMapiFolder()
        {
            // Load up the MAPI Folder from Entry / Store IDs 
            if (Preferences.OutlookFolderEntryId != "" && Preferences.OutlookFolderStoreId != "")
                try
                {
                    _outlookFolder = Startup.OutlookNameSpace.GetFolderFromID(Preferences.OutlookFolderEntryId,
                                                                              Preferences.OutlookFolderStoreId);

                    ShowCalendarButtonsFor(_outlookFolder);
                }
                catch (Exception ex)
                {
                    ConfigLogger.Instance.LogError(ex);
                }
            else
                _outlookFolder = null;
        }

        /// <summary>
        /// This will populate a dropdown off the instance context menu with the avaliable
        /// views in outlook, it will also assoicate the menuitem with the event handler. 
        /// </summary>
        private void UpdateOutlookViewsList()
        {
            OutlookViewsMenu.DropDownItems.Clear();
            OulookFolderViews = new List<View>();

            if (_outlookFolder != null)
            {
                foreach (View view in _outlookFolder.Views)
                {
                    var viewItem = new ToolStripMenuItem(view.Name) { Tag = view };

                    viewItem.Click += ViewItem_Click;

                    if (view.Name == Preferences.OutlookFolderView)
                        viewItem.Checked = true;

                    OutlookViewsMenu.DropDownItems.Add(viewItem);

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
            CheckSelectedMenuItemInCollection(viewItem, OutlookViewsMenu.DropDownItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private static string GetFolderNameFromFullPath(string fullPath)
        {
            //TODO: Revert back and deal with online/offline better!
            return fullPath.Substring(fullPath.LastIndexOf("\\", StringComparison.Ordinal) + 1,
                                      fullPath.Length - fullPath.LastIndexOf("\\", StringComparison.Ordinal) - 1);
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
            var menuItems = new List<ToolStripMenuItem> { CalendarMenu, ContactsMenu, InboxMenu, NotesMenu, TasksMenu };

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
                SetViewXML(string.Empty);
                ShowCalendarButtons(false);
            }
            else
            {
                ShowCalendarButtons(true);
            }

            axOutlookViewControl.Folder = folderViewType.ToString();

            Preferences.OutlookFolderName = folderViewType.ToString();
            Preferences.OutlookFolderStoreId = GetFolderFromViewType(folderViewType).StoreID;
            Preferences.OutlookFolderEntryId = GetFolderFromViewType(folderViewType).EntryID;

            SetMapiFolder();

            UpdateOutlookViewsList();

            CheckSelectedFolder(itemToCheck);
        }

        private void ShowCalendarButtons(bool show)
        {
            DayButton.Visible = show;
            WeekButton.Visible = show;
            WorkWeekButton.Visible = show;
            MonthButton.Visible = show;
        }

        private void UpdateCustomFolder(MAPIFolder oFolder)
        {
            if (oFolder == null) return;

            try
            {
                // Remove old item (selectmenu+1)
                if (trayMenu.Items.Contains(_customMenu))
                {
                    trayMenu.Items.Remove(_customMenu);
                }

                string folderPath = GetFolderPath(GenerateFolderPathFromObject(oFolder));
                axOutlookViewControl.Folder = folderPath;

                // Save the EntryId and the StoreId for this folder in the prefrences. 
                Preferences.OutlookFolderEntryId = oFolder.EntryID;
                Preferences.OutlookFolderStoreId = oFolder.StoreID;

                Preferences.OutlookFolderName = folderPath;
                _customFolder = Preferences.OutlookFolderName;

                // Update the UI to reflect the new settings. 
                trayMenu.Items.Insert(GetSelectFolderMenuLocation() + 1,
                                      new ToolStripMenuItem(oFolder.Name, null, CustomFolderMenu_Click));
                _customMenu = (ToolStripMenuItem)trayMenu.Items[GetSelectFolderMenuLocation() + 1];

                SetMapiFolder();
                CheckSelectedFolder(_customMenu);
                UpdateOutlookViewsList();
            }
            catch (Exception)
            {
                MessageBox.Show(this, Resources.ErrorSettingFolder, Resources.ErrorCaption, MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void MoveForm()
        {
            if (GlobalPreferences.LockPosition)
                return;

            UnsafeNativeMethods.ReleaseCapture();
            UnsafeNativeMethods.SendMessage(Handle, UnsafeNativeMethods.WM_NCLBUTTONDOWN,
                                            (IntPtr)UnsafeNativeMethods.HTCAPTION, IntPtr.Zero);

            // update the values stored in the registry
            Preferences.Left = Left;
            Preferences.Top = Top;
        }

        private void ResizeForm(ResizeDirection direction)
        {
            if (GlobalPreferences.LockPosition) return;

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

        #region Event Handlers

        private void MainForm_Layout(object sender, LayoutEventArgs e)
        {
            // Update the settings stored in the registry
            Preferences.Width = Width;
            Preferences.Height = Height;
        }

        /// <summary>
        /// When a view is selected this will change the view control view to it, save it in the 
        /// preferences and then check the box next to the view in the drop down list. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewItem_Click(object sender, EventArgs e)
        {
            var viewItem = sender as ToolStripMenuItem;
            if (viewItem != null)
            {
                var view = viewItem.Tag as View;

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
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectFolderMenu_Click(object sender, EventArgs e)
        {
            MAPIFolder oFolder = Startup.OutlookNameSpace.PickFolder();
            UpdateCustomFolder(oFolder);

            ShowCalendarButtonsFor(oFolder);
        }

        /// <summary>
        /// Checks to see if the specified folder contains an appointment object, is so, we assume its a calendar
        /// so we should show the calendar buttons.
        /// </summary>
        /// <param name="oFolder"></param>
        private void ShowCalendarButtonsFor(MAPIFolder oFolder)
        {
            if (oFolder.Items != null && oFolder.Items.Count > 0)
            {
                var appt = oFolder.Items[1] as AppointmentItem;
                if (appt != null)
                {
                    ShowCalendarButtons(true);
                    return;
                }
            }
            ShowCalendarButtons(false);
        }

        private void CustomFolderMenu_Click(object sender, EventArgs e)
        {
            axOutlookViewControl.Folder = _customFolder;
            CheckSelectedFolder(_customMenu);
            ShowCalendarButtons(false);
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
            DialogResult result = MessageBox.Show(this, Resources.RemoveInstanceConfirmation,
                                                  Resources.ConfirmationCaption, MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                using (
                    RegistryKey appReg =
                        Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName +
                                                          "\\" + Application.ProductName))
                {
                    if (appReg != null) appReg.DeleteSubKeyTree(InstanceName);
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
                    axOutlookViewControl.GoToToday();
                }
                catch (Exception ex)
                {
                    // no big deal if we can't set the day, just ignore and go on.
                    ConfigLogger.Instance.LogError(ex);
                }
            }
            _previousDate = DateTime.Now;
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Dispose();

            Startup.OutlookExplorer.Close();
            Startup.OutlookExplorer = null;
            Startup.OutlookFolder = null;
            Startup.OutlookNameSpace = null;
            Startup.OutlookApp = null;

            Application.Exit();
        }

        private void RenameInstanceMenu_Click(object sender, EventArgs e)
        {
            InputBoxResult result = InputBox.Show(this, "", "Rename Instance", InstanceName, InputBox_Validating);
            if (result.Ok)
            {
                using (
                    RegistryKey parentKey =
                        Registry.CurrentUser.OpenSubKey(
                            "Software\\" + Application.CompanyName + "\\" +
                            Application.ProductName, true))
                {
                    if (parentKey != null)
                    {
                        RegistryHelper.RenameSubKey(parentKey, InstanceName, result.Text);
                        String oldInstanceName = InstanceName;
                        InstanceName = result.Text;
                        Preferences = new InstancePreferences(InstanceName);

                        OnInstanceRenamed(this, new InstanceRenamedEventArgs(oldInstanceName, InstanceName));
                    }
                }
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

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && WindowState != FormWindowState.Maximized)
            {
                ResizeForm(ResizeDir);
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (GlobalPreferences.LockPosition)
                return;

            if (e.Location.X < _resizeBorderWidth && e.Location.Y < _resizeBorderWidth)
                ResizeDir = ResizeDirection.TopLeft;

            else if (e.Location.X < _resizeBorderWidth && e.Location.Y > Height - _resizeBorderWidth)
                ResizeDir = ResizeDirection.BottomLeft;

            else if (e.Location.X > Width - _resizeBorderWidth && e.Location.Y > Height - _resizeBorderWidth)
                ResizeDir = ResizeDirection.BottomRight;

            else if (e.Location.X > Width - _resizeBorderWidth && e.Location.Y < _resizeBorderWidth)
                ResizeDir = ResizeDirection.TopRight;

            else if (e.Location.X < _resizeBorderWidth)
                ResizeDir = ResizeDirection.Left;

            else if (e.Location.X > Width - _resizeBorderWidth)
                ResizeDir = ResizeDirection.Right;

            else if (e.Location.Y < _resizeBorderWidth)
                ResizeDir = ResizeDirection.Top;

            else if (e.Location.Y > Height - _resizeBorderWidth)
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

        private void HeaderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            ResizeDir = ResizeDirection.None;
        }

        private void SetViewXML(string value)
        {
            axOutlookViewControl.ViewXML = value;
            Preferences.ViewXml = value;
        }

        private void DayButton_Click(object sender, EventArgs e)
        {
            SetViewXML(Resources.day);
        }

        private void WorkWeekButton_Click(object sender, EventArgs e)
        {
            SetViewXML(Resources.WorkWeek);
        }

        private void MonthButton_Click(object sender, EventArgs e)
        {
            SetViewXML(Resources.month);
        }

        private void WeekButton_Click(object sender, EventArgs e)
        {
            SetViewXML(Resources.week);
        }

        private void TransparencySlider_Scroll(object sender, EventArgs e)
        {
            double opacityVal = (double)TransparencySlider.Value / 100;
            if (Math.Abs(opacityVal - 1) < double.Epsilon)
            {
                opacityVal = 0.99;
            }
            Opacity = opacityVal;
            Preferences.Opacity = opacityVal;
        }

        private void TransparencySlider_MouseHover(object sender, EventArgs e)
        {
            ToolTip.SetToolTip(TransparencySlider,
                               GlobalPreferences.LockPosition
                                   ? "You must unlock the form before attempting to change the transparency level."
                                   : "Slide to change this windows transparency level");
        }

        private void HeaderPanel_MouseHover(object sender, EventArgs e)
        {
            ToolTip.SetToolTip(HeaderPanel,
                               GlobalPreferences.LockPosition
                                   ? "You must unlock the form before attempting to move the window."
                                   : "Click and hold this header to move the window");
        }

        #endregion

        #region Properties

        private ResizeDirection _resizeDir = ResizeDirection.None;
        private List<View> OulookFolderViews { get; set; }
        private InstancePreferences Preferences { get; set; }
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