using System;
using AxOLXLib;
using MACTrackBarLib;
using OutlookDesktop.Properties;

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
                OutlookFolderViews = null;
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
            this.TrayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CalendarMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ContactsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.InboxMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.NotesMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.TasksMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SelectFolderMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator2 = new System.Windows.Forms.ToolStripSeparator();
            this.OutlookViewsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.DefaultOutlookViewSubMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator3 = new System.Windows.Forms.ToolStripSeparator();
            this.RenameInstanceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator4 = new System.Windows.Forms.ToolStripSeparator();
            this.HideShowMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.DisableEnableEditingMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator5 = new System.Windows.Forms.ToolStripSeparator();
            this.RemoveInstanceMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Separator6 = new System.Windows.Forms.ToolStripSeparator();
            this.ExitMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.HeaderPanel = new System.Windows.Forms.Panel();
            this.TransparencySlider = new MACTrackBarLib.MACTrackBar();
            this.LabelCurrentDate = new System.Windows.Forms.Label();
            this.ButtonPrevious = new System.Windows.Forms.Button();
            this.ButtonNext = new System.Windows.Forms.Button();
            this.TodayButton = new System.Windows.Forms.Button();
            this.WorkWeekButton = new System.Windows.Forms.Button();
            this.MonthButton = new System.Windows.Forms.Button();
            this.WeekButton = new System.Windows.Forms.Button();
            this.DayButton = new System.Windows.Forms.Button();
            this.ViewControlHostPanel = new System.Windows.Forms.Panel();
            this.OutlookViewControl = new AxOLXLib.AxViewCtl();
            this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.WindowMessageTimer = new System.Windows.Forms.Timer(this.components);
            this.TrayMenu.SuspendLayout();
            this.HeaderPanel.SuspendLayout();
            this.ViewControlHostPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OutlookViewControl)).BeginInit();
            this.SuspendLayout();
            // 
            // TrayMenu
            // 
            this.TrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
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
            this.Separator6,
            this.ExitMenu});
            this.TrayMenu.Name = "trayMenu";
            this.TrayMenu.Size = new System.Drawing.Size(187, 304);
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
            this.DefaultOutlookViewSubMenu});
            this.OutlookViewsMenu.Name = "OutlookViewsMenu";
            this.OutlookViewsMenu.Size = new System.Drawing.Size(186, 22);
            this.OutlookViewsMenu.Text = "Outlook Views";
            // 
            // DefaultOutlookViewSubMenu
            // 
            this.DefaultOutlookViewSubMenu.Name = "DefaultOutlookViewSubMenu";
            this.DefaultOutlookViewSubMenu.Size = new System.Drawing.Size(140, 22);
            this.DefaultOutlookViewSubMenu.Text = "Default View";
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
            // Separator6
            // 
            this.Separator6.Name = "Separator6";
            this.Separator6.Size = new System.Drawing.Size(183, 6);
            // 
            // ExitMenu
            // 
            this.ExitMenu.Name = "ExitMenu";
            this.ExitMenu.Size = new System.Drawing.Size(186, 22);
            this.ExitMenu.Text = "Exit";
            this.ExitMenu.Click += new System.EventHandler(this.ExitMenu_Click);
            // 
            // UpdateTimer
            // 
            this.UpdateTimer.Enabled = true;
            this.UpdateTimer.Interval = 1000;
            this.UpdateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.HeaderPanel.Controls.Add(this.TransparencySlider);
            this.HeaderPanel.Controls.Add(this.LabelCurrentDate);
            this.HeaderPanel.Controls.Add(this.ButtonPrevious);
            this.HeaderPanel.Controls.Add(this.ButtonNext);
            this.HeaderPanel.Controls.Add(this.TodayButton);
            this.HeaderPanel.Controls.Add(this.WorkWeekButton);
            this.HeaderPanel.Controls.Add(this.MonthButton);
            this.HeaderPanel.Controls.Add(this.WeekButton);
            this.HeaderPanel.Controls.Add(this.DayButton);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = new System.Drawing.Point(4, 4);
            this.HeaderPanel.Margin = new System.Windows.Forms.Padding(4);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Padding = new System.Windows.Forms.Padding(3);
            this.HeaderPanel.Size = new System.Drawing.Size(507, 19);
            this.HeaderPanel.TabIndex = 3;
            this.ToolTip.SetToolTip(this.HeaderPanel, global::OutlookDesktop.Properties.Resources.Form_Move_Help_Message);
            this.HeaderPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HeaderPanel_MouseDown);
            this.HeaderPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HeaderPanel_MouseMove);
            // 
            // TransparencySlider
            // 
            this.TransparencySlider.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.TransparencySlider.BorderColor = System.Drawing.SystemColors.ActiveCaption;
            this.TransparencySlider.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TransparencySlider.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(123)))), ((int)(((byte)(125)))), ((int)(((byte)(123)))));
            this.TransparencySlider.IndentHeight = 6;
            this.TransparencySlider.LargeChange = 15;
            this.TransparencySlider.Location = new System.Drawing.Point(0, -3);
            this.TransparencySlider.Margin = new System.Windows.Forms.Padding(0);
            this.TransparencySlider.Maximum = 100;
            this.TransparencySlider.MaximumSize = new System.Drawing.Size(200, 28);
            this.TransparencySlider.Minimum = 30;
            this.TransparencySlider.MinimumSize = new System.Drawing.Size(100, 14);
            this.TransparencySlider.Name = "TransparencySlider";
            this.TransparencySlider.Size = new System.Drawing.Size(100, 24);
            this.TransparencySlider.SmallChange = 5;
            this.TransparencySlider.TabIndex = 10;
            this.TransparencySlider.TextTickStyle = System.Windows.Forms.TickStyle.None;
            this.TransparencySlider.TickColor = System.Drawing.Color.DarkGray;
            this.TransparencySlider.TickHeight = 4;
            this.TransparencySlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.ToolTip.SetToolTip(this.TransparencySlider, global::OutlookDesktop.Properties.Resources.Transparency_Slider_Help_Message);
            this.TransparencySlider.TrackerColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(130)))), ((int)(((byte)(198)))));
            this.TransparencySlider.TrackerSize = new System.Drawing.Size(12, 12);
            this.TransparencySlider.TrackLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(93)))), ((int)(((byte)(90)))));
            this.TransparencySlider.TrackLineHeight = 2;
            this.TransparencySlider.Value = 50;
            this.TransparencySlider.ValueChanged += new MACTrackBarLib.ValueChangedHandler(this.TransparencySlider_ValueChanged);
            this.TransparencySlider.MouseHover += new System.EventHandler(this.TransparencySlider_MouseHover);
            // 
            // LabelCurrentDate
            // 
            this.LabelCurrentDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelCurrentDate.Location = new System.Drawing.Point(167, -1);
            this.LabelCurrentDate.Name = "LabelCurrentDate";
            this.LabelCurrentDate.Size = new System.Drawing.Size(178, 20);
            this.LabelCurrentDate.TabIndex = 9;
            this.LabelCurrentDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ToolTip.SetToolTip(this.LabelCurrentDate, global::OutlookDesktop.Properties.Resources.Form_Move_Help_Message);
            this.LabelCurrentDate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LabelCurrentDate_MouseDown);
            this.LabelCurrentDate.MouseHover += new System.EventHandler(this.LabelCurrentDate_MouseHover);
            // 
            // ButtonPrevious
            // 
            this.ButtonPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonPrevious.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonPrevious.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.ButtonPrevious.Image = global::OutlookDesktop.Properties.Resources.Previous;
            this.ButtonPrevious.Location = new System.Drawing.Point(461, -1);
            this.ButtonPrevious.Name = "ButtonPrevious";
            this.ButtonPrevious.Size = new System.Drawing.Size(22, 19);
            this.ButtonPrevious.TabIndex = 6;
            this.ToolTip.SetToolTip(this.ButtonPrevious, global::OutlookDesktop.Properties.Resources.Move_Previous);
            this.ButtonPrevious.UseVisualStyleBackColor = true;
            this.ButtonPrevious.Click += new System.EventHandler(this.ButtonPrevious_Click);
            this.ButtonPrevious.MouseHover += new System.EventHandler(this.ButtonPrevious_MouseHover);
            // 
            // ButtonNext
            // 
            this.ButtonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonNext.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.ButtonNext.Image = global::OutlookDesktop.Properties.Resources.Next;
            this.ButtonNext.Location = new System.Drawing.Point(483, -1);
            this.ButtonNext.Name = "ButtonNext";
            this.ButtonNext.Size = new System.Drawing.Size(22, 19);
            this.ButtonNext.TabIndex = 7;
            this.ToolTip.SetToolTip(this.ButtonNext, global::OutlookDesktop.Properties.Resources.Move_Next);
            this.ButtonNext.UseVisualStyleBackColor = true;
            this.ButtonNext.Click += new System.EventHandler(this.ButtonNext_Click);
            this.ButtonNext.MouseHover += new System.EventHandler(this.ButtonNext_MouseHover);
            // 
            // TodayButton
            // 
            this.TodayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TodayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TodayButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TodayButton.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.TodayButton.Image = global::OutlookDesktop.Properties.Resources.Today;
            this.TodayButton.Location = new System.Drawing.Point(349, -1);
            this.TodayButton.Name = "TodayButton";
            this.TodayButton.Size = new System.Drawing.Size(20, 19);
            this.TodayButton.TabIndex = 1;
            this.ToolTip.SetToolTip(this.TodayButton, global::OutlookDesktop.Properties.Resources.Go_to_Today);
            this.TodayButton.UseVisualStyleBackColor = true;
            this.TodayButton.Click += new System.EventHandler(this.TodayButton_Click);
            this.TodayButton.MouseHover += new System.EventHandler(this.TodayButton_MouseHover);
            // 
            // WorkWeekButton
            // 
            this.WorkWeekButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.WorkWeekButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.WorkWeekButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WorkWeekButton.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.WorkWeekButton.Image = global::OutlookDesktop.Properties.Resources.FiveDays;
            this.WorkWeekButton.Location = new System.Drawing.Point(393, -1);
            this.WorkWeekButton.Name = "WorkWeekButton";
            this.WorkWeekButton.Size = new System.Drawing.Size(20, 19);
            this.WorkWeekButton.TabIndex = 3;
            this.ToolTip.SetToolTip(this.WorkWeekButton, global::OutlookDesktop.Properties.Resources.Toggle_Work_Week_View);
            this.WorkWeekButton.UseVisualStyleBackColor = true;
            this.WorkWeekButton.Click += new System.EventHandler(this.WorkWeekButton_Click);
            this.WorkWeekButton.MouseHover += new System.EventHandler(this.WorkWeekButton_MouseHover);
            // 
            // MonthButton
            // 
            this.MonthButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MonthButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MonthButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MonthButton.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.MonthButton.Image = ((System.Drawing.Image)(resources.GetObject("MonthButton.Image")));
            this.MonthButton.Location = new System.Drawing.Point(438, -1);
            this.MonthButton.Name = "MonthButton";
            this.MonthButton.Size = new System.Drawing.Size(20, 19);
            this.MonthButton.TabIndex = 5;
            this.ToolTip.SetToolTip(this.MonthButton, global::OutlookDesktop.Properties.Resources.Toggle_Month_View);
            this.MonthButton.UseVisualStyleBackColor = true;
            this.MonthButton.Click += new System.EventHandler(this.MonthButton_Click);
            this.MonthButton.MouseHover += new System.EventHandler(this.MonthButton_MouseHover);
            // 
            // WeekButton
            // 
            this.WeekButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.WeekButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.WeekButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WeekButton.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.WeekButton.Image = global::OutlookDesktop.Properties.Resources.SevenDays;
            this.WeekButton.Location = new System.Drawing.Point(416, -1);
            this.WeekButton.Name = "WeekButton";
            this.WeekButton.Size = new System.Drawing.Size(20, 19);
            this.WeekButton.TabIndex = 4;
            this.ToolTip.SetToolTip(this.WeekButton, global::OutlookDesktop.Properties.Resources.Toggle_Full_Week_View);
            this.WeekButton.UseVisualStyleBackColor = true;
            this.WeekButton.Click += new System.EventHandler(this.WeekButton_Click);
            this.WeekButton.MouseHover += new System.EventHandler(this.WeekButton_MouseHover);
            // 
            // DayButton
            // 
            this.DayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DayButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DayButton.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.DayButton.Image = global::OutlookDesktop.Properties.Resources.OneDay;
            this.DayButton.Location = new System.Drawing.Point(371, -1);
            this.DayButton.Name = "DayButton";
            this.DayButton.Size = new System.Drawing.Size(20, 19);
            this.DayButton.TabIndex = 2;
            this.ToolTip.SetToolTip(this.DayButton, global::OutlookDesktop.Properties.Resources.Toggle_Day_View);
            this.DayButton.UseVisualStyleBackColor = true;
            this.DayButton.Click += new System.EventHandler(this.DayButton_Click);
            this.DayButton.MouseHover += new System.EventHandler(this.DayButton_MouseHover);
            // 
            // ViewControlHostPanel
            // 
            this.ViewControlHostPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewControlHostPanel.Controls.Add(this.OutlookViewControl);
            this.ViewControlHostPanel.Location = new System.Drawing.Point(4, 28);
            this.ViewControlHostPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ViewControlHostPanel.Name = "ViewControlHostPanel";
            this.ViewControlHostPanel.Size = new System.Drawing.Size(507, 368);
            this.ViewControlHostPanel.TabIndex = 4;
            // 
            // OutlookViewControl
            // 
            this.OutlookViewControl.CausesValidation = false;
            this.OutlookViewControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OutlookViewControl.Enabled = true;
            this.OutlookViewControl.Location = new System.Drawing.Point(0, 0);
            this.OutlookViewControl.Margin = new System.Windows.Forms.Padding(0);
            this.OutlookViewControl.Name = "OutlookViewControl";
            this.OutlookViewControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("OutlookViewControl.OcxState")));
            this.OutlookViewControl.Size = new System.Drawing.Size(507, 368);
            this.OutlookViewControl.TabIndex = 3;
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.Text = "notifyIcon";
            this.NotifyIcon.Visible = true;
            // 
            // ToolTip
            // 
            this.ToolTip.AutoPopDelay = 5000;
            this.ToolTip.InitialDelay = 1000;
            this.ToolTip.IsBalloon = true;
            this.ToolTip.ReshowDelay = 100;
            // 
            // WindowMessageTimer
            // 
            this.WindowMessageTimer.Tick += new System.EventHandler(this.WindowMessageTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(515, 400);
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
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
            this.TrayMenu.ResumeLayout(false);
            this.HeaderPanel.ResumeLayout(false);
            this.ViewControlHostPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OutlookViewControl)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
       
        internal System.Windows.Forms.ContextMenuStrip TrayMenu;
        private System.Windows.Forms.Timer UpdateTimer;
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
        private System.Windows.Forms.ToolStripMenuItem DefaultOutlookViewSubMenu;
        private System.Windows.Forms.ToolStripSeparator Separator3;
        private System.Windows.Forms.ToolStripSeparator Separator2;
        private System.Windows.Forms.ToolStripMenuItem DisableEnableEditingMenu;
        internal System.Windows.Forms.Panel HeaderPanel;
        private System.Windows.Forms.Panel ViewControlHostPanel;
        private AxViewCtl OutlookViewControl;
        private System.Windows.Forms.Button DayButton;
        private System.Windows.Forms.NotifyIcon NotifyIcon;
        private System.Windows.Forms.Button MonthButton;
        private System.Windows.Forms.Button WeekButton;
        private System.Windows.Forms.Button WorkWeekButton;
        public System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Timer WindowMessageTimer;
        private System.Windows.Forms.ToolStripSeparator Separator6;
        private System.Windows.Forms.Button TodayButton;
        private System.Windows.Forms.Button ButtonNext;
        private System.Windows.Forms.Button ButtonPrevious;
        public System.Windows.Forms.Label LabelCurrentDate;
#pragma warning disable CS3003 // Type is not CLS-compliant
        public MACTrackBar TransparencySlider;
#pragma warning restore CS3003 // Type is not CLS-compliant
    }
}