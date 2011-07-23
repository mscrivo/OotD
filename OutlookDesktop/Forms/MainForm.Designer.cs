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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CalendarMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ContactsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.InboxMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.NotesMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.TasksMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.SelectFolderMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.uxOutlookViews = new System.Windows.Forms.ToolStripMenuItem();
            this.uxDefaultOutlookView = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.RenameInstanceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.HideShowMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.DisableEnableEditingMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.RemoveInstanceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.HeaderPanel = new System.Windows.Forms.Panel();
            this.transparencySlider = new System.Windows.Forms.TrackBar();
            this.workWeekButton = new System.Windows.Forms.Button();
            this.monthButton = new System.Windows.Forms.Button();
            this.weekButton = new System.Windows.Forms.Button();
            this.dayButton = new System.Windows.Forms.Button();
            this.ViewControlHostPanel = new System.Windows.Forms.Panel();
            this.axOutlookViewControl = new AxOLXLib.AxViewCtl();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.trayMenu.SuspendLayout();
            this.HeaderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.transparencySlider)).BeginInit();
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
            this.toolStripMenuItem2,
            this.SelectFolderMenu,
            this.toolStripSeparator2,
            this.uxOutlookViews,
            this.toolStripSeparator1,
            this.RenameInstanceMenu,
            this.toolStripMenuItem3,
            this.HideShowMenu,
            this.DisableEnableEditingMenu,
            this.toolStripMenuItem4,
            this.RemoveInstanceMenu,
            this.ExitMenu});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(187, 320);
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
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(183, 6);
            // 
            // SelectFolderMenu
            // 
            this.SelectFolderMenu.Name = "SelectFolderMenu";
            this.SelectFolderMenu.Size = new System.Drawing.Size(186, 22);
            this.SelectFolderMenu.Text = "Select Folder ...";
            this.SelectFolderMenu.Click += new System.EventHandler(this.SelectFolderMenu_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(183, 6);
            // 
            // uxOutlookViews
            // 
            this.uxOutlookViews.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uxDefaultOutlookView});
            this.uxOutlookViews.Name = "uxOutlookViews";
            this.uxOutlookViews.Size = new System.Drawing.Size(186, 22);
            this.uxOutlookViews.Text = "Outlook Views";
            // 
            // uxDefaultOutlookView
            // 
            this.uxDefaultOutlookView.Name = "uxDefaultOutlookView";
            this.uxDefaultOutlookView.Size = new System.Drawing.Size(140, 22);
            this.uxDefaultOutlookView.Text = "Default View";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(183, 6);
            // 
            // RenameInstanceMenu
            // 
            this.RenameInstanceMenu.Name = "RenameInstanceMenu";
            this.RenameInstanceMenu.Size = new System.Drawing.Size(186, 22);
            this.RenameInstanceMenu.Text = "Rename this Instance";
            this.RenameInstanceMenu.Click += new System.EventHandler(this.RenameInstanceMenu_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(183, 6);
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
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(183, 6);
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
            this.HeaderPanel.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.HeaderPanel.Controls.Add(this.transparencySlider);
            this.HeaderPanel.Controls.Add(this.workWeekButton);
            this.HeaderPanel.Controls.Add(this.monthButton);
            this.HeaderPanel.Controls.Add(this.weekButton);
            this.HeaderPanel.Controls.Add(this.dayButton);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = new System.Drawing.Point(4, 4);
            this.HeaderPanel.Margin = new System.Windows.Forms.Padding(4);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Padding = new System.Windows.Forms.Padding(3);
            this.HeaderPanel.Size = new System.Drawing.Size(392, 20);
            this.HeaderPanel.TabIndex = 3;
            this.HeaderPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HeaderPanel_MouseDown);
            this.HeaderPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HeaderPanel_MouseMove);
            // 
            // transparencySlider
            // 
            this.transparencySlider.AutoSize = false;
            this.transparencySlider.Location = new System.Drawing.Point(6, 3);
            this.transparencySlider.Maximum = 100;
            this.transparencySlider.Minimum = 25;
            this.transparencySlider.Name = "transparencySlider";
            this.transparencySlider.Size = new System.Drawing.Size(89, 14);
            this.transparencySlider.SmallChange = 5;
            this.transparencySlider.TabIndex = 5;
            this.transparencySlider.TickFrequency = 10;
            this.transparencySlider.Value = 25;
            this.transparencySlider.Scroll += new System.EventHandler(this.transparencySlider_Scroll);
            this.transparencySlider.MouseHover += new System.EventHandler(this.transparencySlider_MouseHover);
            // 
            // workWeekButton
            // 
            this.workWeekButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.workWeekButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.workWeekButton.Location = new System.Drawing.Point(216, 0);
            this.workWeekButton.Name = "workWeekButton";
            this.workWeekButton.Size = new System.Drawing.Size(68, 20);
            this.workWeekButton.TabIndex = 4;
            this.workWeekButton.Text = "Work Week";
            this.workWeekButton.UseVisualStyleBackColor = true;
            this.workWeekButton.Click += new System.EventHandler(this.workWeekButton_Click);
            // 
            // monthButton
            // 
            this.monthButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.monthButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.monthButton.Location = new System.Drawing.Point(339, 0);
            this.monthButton.Name = "monthButton";
            this.monthButton.Size = new System.Drawing.Size(52, 20);
            this.monthButton.TabIndex = 2;
            this.monthButton.Text = "Month";
            this.monthButton.UseVisualStyleBackColor = true;
            this.monthButton.Click += new System.EventHandler(this.monthButton_Click);
            // 
            // weekButton
            // 
            this.weekButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.weekButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.weekButton.Location = new System.Drawing.Point(286, 0);
            this.weekButton.Name = "weekButton";
            this.weekButton.Size = new System.Drawing.Size(52, 20);
            this.weekButton.TabIndex = 1;
            this.weekButton.Text = "Week";
            this.weekButton.UseVisualStyleBackColor = true;
            this.weekButton.Click += new System.EventHandler(this.weekButton_Click);
            // 
            // dayButton
            // 
            this.dayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dayButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dayButton.Location = new System.Drawing.Point(162, 0);
            this.dayButton.Name = "dayButton";
            this.dayButton.Size = new System.Drawing.Size(52, 20);
            this.dayButton.TabIndex = 0;
            this.dayButton.Text = "Day";
            this.dayButton.UseVisualStyleBackColor = true;
            this.dayButton.Click += new System.EventHandler(this.dayButton_Click);
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
            this.axOutlookViewControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axOutlookViewControl.OcxState")));
            this.axOutlookViewControl.Size = new System.Drawing.Size(392, 368);
            this.axOutlookViewControl.TabIndex = 3;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
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
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Layout += new System.Windows.Forms.LayoutEventHandler(this.MainForm_Layout);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
            this.trayMenu.ResumeLayout(false);
            this.HeaderPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.transparencySlider)).EndInit();
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
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem HideShowMenu;
        private System.Windows.Forms.ToolStripMenuItem RemoveInstanceMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem RenameInstanceMenu;
        private System.Windows.Forms.ToolStripMenuItem ExitMenu;
        private System.Windows.Forms.ToolStripMenuItem uxOutlookViews;
        private System.Windows.Forms.ToolStripMenuItem uxDefaultOutlookView;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem DisableEnableEditingMenu;
        internal System.Windows.Forms.Panel HeaderPanel;
        private System.Windows.Forms.Panel ViewControlHostPanel;
        private AxOLXLib.AxViewCtl axOutlookViewControl;
        private System.Windows.Forms.Button dayButton;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button monthButton;
        private System.Windows.Forms.Button weekButton;
        private System.Windows.Forms.Button workWeekButton;
        private System.Windows.Forms.TrackBar transparencySlider;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}