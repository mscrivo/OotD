using AxOLXLib;

namespace OutlookDesktop.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();

                // Ensure we cleanup the Outlook resources, but do not call Quit() on the Outlook
                // app object or we will inadvertantly close any full blown Outlook instances 
                // that are open.
                OulookFolderViews = null;
                _outlookFolder = null;
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
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CalendarMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ContactsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.InboxMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.NotesMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.TasksMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SelectFolderMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator2 = new System.Windows.Forms.ToolStripSeparator();
            this.OutlookViewsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.uxDefaultOutlookView = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator3 = new System.Windows.Forms.ToolStripSeparator();
            this.RenameInstanceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator4 = new System.Windows.Forms.ToolStripSeparator();
            this.HideShowMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.DisableEnableEditingMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator5 = new System.Windows.Forms.ToolStripSeparator();
            this.RemoveInstanceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.HeaderPanel = new System.Windows.Forms.Panel();
            this.TransparencySlider = new System.Windows.Forms.TrackBar();
            this.WorkWeekButton = new System.Windows.Forms.Button();
            this.MonthButton = new System.Windows.Forms.Button();
            this.WeekButton = new System.Windows.Forms.Button();
            this.DayButton = new System.Windows.Forms.Button();
            this.ViewControlHostPanel = new System.Windows.Forms.Panel();
            this.axOutlookViewControl = new AxOLXLib.AxViewCtl();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.WindowMessageTimer = new System.Windows.Forms.Timer(this.components);
            this.trayMenu.SuspendLayout();
            this.HeaderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TransparencySlider)).BeginInit();
            this.ViewControlHostPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axOutlookViewControl)).BeginInit();
            this.SuspendLayout();
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CalendarMenu,
            this.ContactsMenu,
            this.InboxMenu,
            this.NotesMenu,
            this.TasksMenu,
            this.Separator1,
            this.SelectFolderMenu,
            this.Separator2,
            this.OutlookViewsMenu,
            this.Separator3,
            this.RenameInstanceMenu,
            this.Separator4,
            this.HideShowMenu,
            this.DisableEnableEditingMenu,
            this.Separator5,
            this.RemoveInstanceMenu,
            this.ExitMenu});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(187, 298);
            // 
            // CalendarMenu
            // 
            this.CalendarMenu.Name = "CalendarMenu";
            this.CalendarMenu.Size = new System.Drawing.Size(186, 22);
            this.CalendarMenu.Text = "Calendar";
            this.CalendarMenu.Click += new System.EventHandler(this.CalendarMenu_Click);
            // 
            // ContactsMenu
            // 
            this.ContactsMenu.Name = "ContactsMenu";
            this.ContactsMenu.Size = new System.Drawing.Size(186, 22);
            this.ContactsMenu.Text = "Contacts";
            this.ContactsMenu.Click += new System.EventHandler(this.ContactsMenu_Click);
            // 
            // InboxMenu
            // 
            this.InboxMenu.Name = "InboxMenu";
            this.InboxMenu.Size = new System.Drawing.Size(186, 22);
            this.InboxMenu.Text = "Inbox";
            this.InboxMenu.Click += new System.EventHandler(this.InboxMenu_Click);
            // 
            // NotesMenu
            // 
            this.NotesMenu.Name = "NotesMenu";
            this.NotesMenu.Size = new System.Drawing.Size(186, 22);
            this.NotesMenu.Text = "Notes";
            this.NotesMenu.Click += new System.EventHandler(this.NotesMenu_Click);
            // 
            // TasksMenu
            // 
            this.TasksMenu.Name = "TasksMenu";
            this.TasksMenu.Size = new System.Drawing.Size(186, 22);
            this.TasksMenu.Text = "Tasks";
            this.TasksMenu.Click += new System.EventHandler(this.TasksMenu_Click);
            // 
            // Separator1
            // 
            this.Separator1.Name = "Separator1";
            this.Separator1.Size = new System.Drawing.Size(183, 6);
            // 
            // SelectFolderMenu
            // 
            this.SelectFolderMenu.Name = "SelectFolderMenu";
            this.SelectFolderMenu.Size = new System.Drawing.Size(186, 22);
            this.SelectFolderMenu.Text = "Select Folder ...";
            this.SelectFolderMenu.Click += new System.EventHandler(this.SelectFolderMenu_Click);
            // 
            // Separator2
            // 
            this.Separator2.Name = "Separator2";
            this.Separator2.Size = new System.Drawing.Size(183, 6);
            // 
            // OutlookViewsMenu
            // 
            this.OutlookViewsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uxDefaultOutlookView});
            this.OutlookViewsMenu.Name = "OutlookViewsMenu";
            this.OutlookViewsMenu.Size = new System.Drawing.Size(186, 22);
            this.OutlookViewsMenu.Text = "Outlook Views";
            // 
            // uxDefaultOutlookView
            // 
            this.uxDefaultOutlookView.Name = "uxDefaultOutlookView";
            this.uxDefaultOutlookView.Size = new System.Drawing.Size(140, 22);
            this.uxDefaultOutlookView.Text = "Default View";
            // 
            // Separator3
            // 
            this.Separator3.Name = "Separator3";
            this.Separator3.Size = new System.Drawing.Size(183, 6);
            // 
            // RenameInstanceMenu
            // 
            this.RenameInstanceMenu.Name = "RenameInstanceMenu";
            this.RenameInstanceMenu.Size = new System.Drawing.Size(186, 22);
            this.RenameInstanceMenu.Text = "Rename this Instance";
            this.RenameInstanceMenu.Click += new System.EventHandler(this.RenameInstanceMenu_Click);
            // 
            // Separator4
            // 
            this.Separator4.Name = "Separator4";
            this.Separator4.Size = new System.Drawing.Size(183, 6);
            // 
            // HideShowMenu
            // 
            this.HideShowMenu.Name = "HideShowMenu";
            this.HideShowMenu.Size = new System.Drawing.Size(186, 22);
            this.HideShowMenu.Text = "Hide";
            this.HideShowMenu.Click += new System.EventHandler(this.HideMenu_Click);
            // 
            // DisableEnableEditingMenu
            // 
            this.DisableEnableEditingMenu.Name = "DisableEnableEditingMenu";
            this.DisableEnableEditingMenu.Size = new System.Drawing.Size(186, 22);
            this.DisableEnableEditingMenu.Text = "Disable Editing";
            this.DisableEnableEditingMenu.Click += new System.EventHandler(this.DisableEnableEditingMenu_Click);
            // 
            // Separator5
            // 
            this.Separator5.Name = "Separator5";
            this.Separator5.Size = new System.Drawing.Size(183, 6);
            // 
            // RemoveInstanceMenu
            // 
            this.RemoveInstanceMenu.Name = "RemoveInstanceMenu";
            this.RemoveInstanceMenu.Size = new System.Drawing.Size(186, 22);
            this.RemoveInstanceMenu.Text = "Remove this Instance";
            this.RemoveInstanceMenu.Click += new System.EventHandler(this.RemoveInstanceMenu_Click);
            // 
            // ExitMenu
            // 
            this.ExitMenu.Name = "ExitMenu";
            this.ExitMenu.Size = new System.Drawing.Size(186, 22);
            this.ExitMenu.Text = "Exit";
            this.ExitMenu.Click += new System.EventHandler(this.ExitMenu_Click);
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Interval = 1000;
            this.updateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.HeaderPanel.Controls.Add(this.TransparencySlider);
            this.HeaderPanel.Controls.Add(this.WorkWeekButton);
            this.HeaderPanel.Controls.Add(this.MonthButton);
            this.HeaderPanel.Controls.Add(this.WeekButton);
            this.HeaderPanel.Controls.Add(this.DayButton);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = new System.Drawing.Point(4, 4);
            this.HeaderPanel.Margin = new System.Windows.Forms.Padding(4);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Padding = new System.Windows.Forms.Padding(3);
            this.HeaderPanel.Size = new System.Drawing.Size(392, 20);
            this.HeaderPanel.TabIndex = 3;
            this.HeaderPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HeaderPanel_MouseDown);
            this.HeaderPanel.MouseHover += new System.EventHandler(this.HeaderPanel_MouseHover);
            this.HeaderPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HeaderPanel_MouseMove);
            // 
            // TransparencySlider
            // 
            this.TransparencySlider.AutoSize = false;
            this.TransparencySlider.Location = new System.Drawing.Point(2, 5);
            this.TransparencySlider.Maximum = 100;
            this.TransparencySlider.Minimum = 25;
            this.TransparencySlider.Name = "TransparencySlider";
            this.TransparencySlider.Size = new System.Drawing.Size(89, 14);
            this.TransparencySlider.SmallChange = 5;
            this.TransparencySlider.TabIndex = 5;
            this.TransparencySlider.TickFrequency = 10;
            this.TransparencySlider.Value = 25;
            this.TransparencySlider.Scroll += new System.EventHandler(this.TransparencySlider_Scroll);
            this.TransparencySlider.MouseHover += new System.EventHandler(this.TransparencySlider_MouseHover);
            // 
            // WorkWeekButton
            // 
            this.WorkWeekButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.WorkWeekButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WorkWeekButton.Location = new System.Drawing.Point(216, 0);
            this.WorkWeekButton.Name = "WorkWeekButton";
            this.WorkWeekButton.Size = new System.Drawing.Size(68, 20);
            this.WorkWeekButton.TabIndex = 4;
            this.WorkWeekButton.Text = "Work Week";
            this.WorkWeekButton.UseVisualStyleBackColor = true;
            this.WorkWeekButton.Click += new System.EventHandler(this.WorkWeekButton_Click);
            // 
            // MonthButton
            // 
            this.MonthButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MonthButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MonthButton.Location = new System.Drawing.Point(339, 0);
            this.MonthButton.Name = "MonthButton";
            this.MonthButton.Size = new System.Drawing.Size(52, 20);
            this.MonthButton.TabIndex = 2;
            this.MonthButton.Text = "Month";
            this.MonthButton.UseVisualStyleBackColor = true;
            this.MonthButton.Click += new System.EventHandler(this.MonthButton_Click);
            // 
            // WeekButton
            // 
            this.WeekButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.WeekButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WeekButton.Location = new System.Drawing.Point(286, 0);
            this.WeekButton.Name = "WeekButton";
            this.WeekButton.Size = new System.Drawing.Size(52, 20);
            this.WeekButton.TabIndex = 1;
            this.WeekButton.Text = "Week";
            this.WeekButton.UseVisualStyleBackColor = true;
            this.WeekButton.Click += new System.EventHandler(this.WeekButton_Click);
            // 
            // DayButton
            // 
            this.DayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DayButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DayButton.Location = new System.Drawing.Point(162, 0);
            this.DayButton.Name = "DayButton";
            this.DayButton.Size = new System.Drawing.Size(52, 20);
            this.DayButton.TabIndex = 0;
            this.DayButton.Text = "Day";
            this.DayButton.UseVisualStyleBackColor = true;
            this.DayButton.Click += new System.EventHandler(this.DayButton_Click);
            // 
            // ViewControlHostPanel
            // 
            this.ViewControlHostPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewControlHostPanel.Controls.Add(this.axOutlookViewControl);
            this.ViewControlHostPanel.Location = new System.Drawing.Point(4, 28);
            this.ViewControlHostPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ViewControlHostPanel.Name = "ViewControlHostPanel";
            this.ViewControlHostPanel.Size = new System.Drawing.Size(392, 368);
            this.ViewControlHostPanel.TabIndex = 4;
            // 
            // axOutlookViewControl
            // 
            this.axOutlookViewControl.CausesValidation = false;
            this.axOutlookViewControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axOutlookViewControl.Enabled = true;
            this.axOutlookViewControl.Location = new System.Drawing.Point(0, 0);
            this.axOutlookViewControl.Margin = new System.Windows.Forms.Padding(0);
            this.axOutlookViewControl.Name = "axOutlookViewControl";
            this.axOutlookViewControl.Size = new System.Drawing.Size(392, 368);
            this.axOutlookViewControl.TabIndex = 3;
            // 
            // notifyIcon
            // 
            this.notifyIcon.Text = "notifyIcon";
            this.notifyIcon.Visible = true;
            // 
            // ToolTip
            // 
            this.ToolTip.IsBalloon = true;
            // 
            // WindowMessageTimer
            // 
            this.WindowMessageTimer.Tick += new System.EventHandler(this.WindowMessageTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 400);
            this.Controls.Add(this.ViewControlHostPanel);
            this.Controls.Add(this.HeaderPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(325, 125);
            this.Name = "MainForm";
            this.Opacity = 0.5D;
            this.Padding = new System.Windows.Forms.Padding(4);
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Outlook on the Desktop";
            this.Layout += new System.Windows.Forms.LayoutEventHandler(this.MainForm_Layout);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
            this.trayMenu.ResumeLayout(false);
            this.HeaderPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TransparencySlider)).EndInit();
            this.ViewControlHostPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.axOutlookViewControl)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ContextMenuStrip TrayMenu 
        {
            get
            {
                return trayMenu;
            }           
        }
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.ToolStripMenuItem SelectFolderMenu;
        private System.Windows.Forms.ToolStripMenuItem CalendarMenu;
        private System.Windows.Forms.ToolStripMenuItem ContactsMenu;
        private System.Windows.Forms.ToolStripMenuItem InboxMenu;
        private System.Windows.Forms.ToolStripMenuItem NotesMenu;
        private System.Windows.Forms.ToolStripMenuItem TasksMenu;
        private System.Windows.Forms.ToolStripSeparator Separator1;
        private System.Windows.Forms.ToolStripSeparator Separator4;
        private System.Windows.Forms.ToolStripMenuItem HideShowMenu;
        private System.Windows.Forms.ToolStripMenuItem RemoveInstanceMenu;
        private System.Windows.Forms.ToolStripSeparator Separator5;
        private System.Windows.Forms.ToolStripMenuItem RenameInstanceMenu;
        private System.Windows.Forms.ToolStripMenuItem ExitMenu;
        private System.Windows.Forms.ToolStripMenuItem OutlookViewsMenu;
        private System.Windows.Forms.ToolStripMenuItem uxDefaultOutlookView;
        private System.Windows.Forms.ToolStripSeparator Separator3;
        private System.Windows.Forms.ToolStripSeparator Separator2;
        private System.Windows.Forms.ToolStripMenuItem DisableEnableEditingMenu;
        internal System.Windows.Forms.Panel HeaderPanel;
        private System.Windows.Forms.Panel ViewControlHostPanel;
        private AxOLXLib.AxViewCtl axOutlookViewControl;
        private System.Windows.Forms.Button DayButton;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Button MonthButton;
        private System.Windows.Forms.Button WeekButton;
        private System.Windows.Forms.Button WorkWeekButton;
        public System.Windows.Forms.TrackBar TransparencySlider;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Timer WindowMessageTimer;
    }
}