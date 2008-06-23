namespace OutlookDesktop
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
                _outlookApplication = null;
                _outlookNamespace = null;
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
            this.SelectFolderMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.CalendarMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ContactsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.InboxMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.NotesMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.TasksMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.RenameInstanceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.PreferencesMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.HideShowMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.RemoveInstanceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.axOutlookViewControl = new AxMicrosoft.Office.Interop.OutlookViewCtl.AxViewCtl();
            this.trayMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axOutlookViewControl)).BeginInit();
            this.SuspendLayout();
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SelectFolderMenu,
            this.toolStripMenuItem1,
            this.CalendarMenu,
            this.ContactsMenu,
            this.InboxMenu,
            this.NotesMenu,
            this.TasksMenu,
            this.toolStripMenuItem2,
            this.RenameInstanceMenu,
            this.PreferencesMenu,
            this.toolStripMenuItem3,
            this.HideShowMenu,
            this.toolStripMenuItem4,
            this.RemoveInstanceMenu,
            this.ExitMenu});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(187, 292);
            // 
            // SelectFolderMenu
            // 
            this.SelectFolderMenu.Name = "SelectFolderMenu";
            this.SelectFolderMenu.Size = new System.Drawing.Size(186, 22);
            this.SelectFolderMenu.Text = "Select Folder ...";
            this.SelectFolderMenu.Click += new System.EventHandler(this.SelectFolderMenu_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(183, 6);
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
            // RenameInstanceMenu
            // 
            this.RenameInstanceMenu.Name = "RenameInstanceMenu";
            this.RenameInstanceMenu.Size = new System.Drawing.Size(186, 22);
            this.RenameInstanceMenu.Text = "Rename this Instance";
            this.RenameInstanceMenu.Click += new System.EventHandler(this.RenameInstanceMenu_Click);
            // 
            // PreferencesMenu
            // 
            this.PreferencesMenu.Name = "PreferencesMenu";
            this.PreferencesMenu.Size = new System.Drawing.Size(186, 22);
            this.PreferencesMenu.Text = "Preferences";
            this.PreferencesMenu.Click += new System.EventHandler(this.PreferencesMenu_Click);
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
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // axOutlookViewControl
            // 
            this.axOutlookViewControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axOutlookViewControl.Enabled = true;
            this.axOutlookViewControl.Location = new System.Drawing.Point(0, 0);
            this.axOutlookViewControl.Name = "axOutlookViewControl";
            this.axOutlookViewControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axOutlookViewControl.OcxState")));
            this.axOutlookViewControl.Size = new System.Drawing.Size(284, 264);
            this.axOutlookViewControl.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 264);
            this.Controls.Add(this.axOutlookViewControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Opacity = 0.5;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Outlook on the Desktop";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.trayMenu.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem CalendarMenu;
        private System.Windows.Forms.ToolStripMenuItem ContactsMenu;
        private System.Windows.Forms.ToolStripMenuItem InboxMenu;
        private System.Windows.Forms.ToolStripMenuItem NotesMenu;
        private System.Windows.Forms.ToolStripMenuItem TasksMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem PreferencesMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem HideShowMenu;
        private System.Windows.Forms.ToolStripMenuItem RemoveInstanceMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem RenameInstanceMenu;
        private System.Windows.Forms.ToolStripMenuItem ExitMenu;
        private AxMicrosoft.Office.Interop.OutlookViewCtl.AxViewCtl axOutlookViewControl;
    }
}