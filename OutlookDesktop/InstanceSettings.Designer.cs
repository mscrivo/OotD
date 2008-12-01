namespace OutlookDesktop
{
    partial class InstanceSettings
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
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.uxHeightValue = new System.Windows.Forms.Label();
            this.uxWidthValue = new System.Windows.Forms.Label();
            this.preferencesLabel = new System.Windows.Forms.Label();
            this.uxOpacityValue = new System.Windows.Forms.Label();
            this.uxCancelSettingsChange = new System.Windows.Forms.Button();
            this.heightSlider = new System.Windows.Forms.TrackBar();
            this.uxSaveSettings = new System.Windows.Forms.Button();
            this.tallLabel = new System.Windows.Forms.Label();
            this.opacityLabel = new System.Windows.Forms.Label();
            this.transparencySlider = new System.Windows.Forms.TrackBar();
            this.positionLabel = new System.Windows.Forms.Label();
            this.shortLabel = new System.Windows.Forms.Label();
            this.widthSlider = new System.Windows.Forms.TrackBar();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.heightLabel = new System.Windows.Forms.Label();
            this.widthLabel = new System.Windows.Forms.Label();
            this.opaqueLabel = new System.Windows.Forms.Label();
            this.horizontalPosition = new System.Windows.Forms.HScrollBar();
            this.transparentLabel = new System.Windows.Forms.Label();
            this.narrowLabel = new System.Windows.Forms.Label();
            this.wideLabel = new System.Windows.Forms.Label();
            this.verticalPosition = new System.Windows.Forms.VScrollBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.uxDefaultFolderTypes = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.uxSelectCustomFolder = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.uxFolderViews = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.heightSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.transparencySlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.widthSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // uxHeightValue
            // 
            this.uxHeightValue.AutoSize = true;
            this.uxHeightValue.Location = new System.Drawing.Point(300, 191);
            this.uxHeightValue.Name = "uxHeightValue";
            this.uxHeightValue.Size = new System.Drawing.Size(19, 13);
            this.uxHeightValue.TabIndex = 68;
            this.uxHeightValue.Text = "50";
            // 
            // uxWidthValue
            // 
            this.uxWidthValue.AutoSize = true;
            this.uxWidthValue.Location = new System.Drawing.Point(300, 131);
            this.uxWidthValue.Name = "uxWidthValue";
            this.uxWidthValue.Size = new System.Drawing.Size(19, 13);
            this.uxWidthValue.TabIndex = 67;
            this.uxWidthValue.Text = "50";
            // 
            // preferencesLabel
            // 
            this.preferencesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.preferencesLabel.BackColor = System.Drawing.Color.SteelBlue;
            this.preferencesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.preferencesLabel.ForeColor = System.Drawing.Color.White;
            this.preferencesLabel.Location = new System.Drawing.Point(3, 0);
            this.preferencesLabel.Name = "preferencesLabel";
            this.preferencesLabel.Size = new System.Drawing.Size(502, 27);
            this.preferencesLabel.TabIndex = 47;
            this.preferencesLabel.Text = "Size and Position";
            this.preferencesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.preferencesLabel.Click += new System.EventHandler(this.preferencesLabel_Click);
            // 
            // uxOpacityValue
            // 
            this.uxOpacityValue.AutoSize = true;
            this.uxOpacityValue.Location = new System.Drawing.Point(235, 54);
            this.uxOpacityValue.Name = "uxOpacityValue";
            this.uxOpacityValue.Size = new System.Drawing.Size(22, 13);
            this.uxOpacityValue.TabIndex = 66;
            this.uxOpacityValue.Text = "0.5";
            // 
            // uxCancelSettingsChange
            // 
            this.uxCancelSettingsChange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.uxCancelSettingsChange.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.uxCancelSettingsChange.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.uxCancelSettingsChange.Location = new System.Drawing.Point(239, 478);
            this.uxCancelSettingsChange.Name = "uxCancelSettingsChange";
            this.uxCancelSettingsChange.Size = new System.Drawing.Size(88, 24);
            this.uxCancelSettingsChange.TabIndex = 53;
            this.uxCancelSettingsChange.Text = global::OutlookDesktop.Properties.Resources.CancelButton;
            this.uxCancelSettingsChange.Click += new System.EventHandler(this.uxCancelSettingsChange_Click);
            // 
            // heightSlider
            // 
            this.heightSlider.Location = new System.Drawing.Point(186, 165);
            this.heightSlider.Maximum = 600;
            this.heightSlider.Name = "heightSlider";
            this.heightSlider.Size = new System.Drawing.Size(250, 45);
            this.heightSlider.SmallChange = 5;
            this.heightSlider.TabIndex = 62;
            this.heightSlider.TickFrequency = 10;
            this.heightSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.heightSlider.Value = 50;
            this.heightSlider.Scroll += new System.EventHandler(this.heightSlider_Scroll);
            // 
            // uxSaveSettings
            // 
            this.uxSaveSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.uxSaveSettings.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.uxSaveSettings.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.uxSaveSettings.Location = new System.Drawing.Point(132, 478);
            this.uxSaveSettings.Name = "uxSaveSettings";
            this.uxSaveSettings.Size = new System.Drawing.Size(88, 24);
            this.uxSaveSettings.TabIndex = 52;
            this.uxSaveSettings.Text = "&Save";
            this.uxSaveSettings.Click += new System.EventHandler(this.uxSaveSettings_Click);
            // 
            // tallLabel
            // 
            this.tallLabel.Location = new System.Drawing.Point(402, 191);
            this.tallLabel.Name = "tallLabel";
            this.tallLabel.Size = new System.Drawing.Size(44, 17);
            this.tallLabel.TabIndex = 65;
            this.tallLabel.Text = "Tall";
            // 
            // opacityLabel
            // 
            this.opacityLabel.Location = new System.Drawing.Point(4, 32);
            this.opacityLabel.Name = "opacityLabel";
            this.opacityLabel.Size = new System.Drawing.Size(63, 18);
            this.opacityLabel.TabIndex = 49;
            this.opacityLabel.Text = "Opacity:";
            this.opacityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // transparencySlider
            // 
            this.transparencySlider.Location = new System.Drawing.Point(68, 30);
            this.transparencySlider.Maximum = 100;
            this.transparencySlider.Name = "transparencySlider";
            this.transparencySlider.Size = new System.Drawing.Size(368, 45);
            this.transparencySlider.SmallChange = 5;
            this.transparencySlider.TabIndex = 48;
            this.transparencySlider.TickFrequency = 10;
            this.transparencySlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.transparencySlider.Value = 50;
            this.transparencySlider.Scroll += new System.EventHandler(this.transparencySlider_Scroll);
            // 
            // positionLabel
            // 
            this.positionLabel.Location = new System.Drawing.Point(35, 179);
            this.positionLabel.Name = "positionLabel";
            this.positionLabel.Size = new System.Drawing.Size(49, 14);
            this.positionLabel.TabIndex = 57;
            this.positionLabel.Text = "Position";
            // 
            // shortLabel
            // 
            this.shortLabel.Location = new System.Drawing.Point(192, 191);
            this.shortLabel.Name = "shortLabel";
            this.shortLabel.Size = new System.Drawing.Size(72, 16);
            this.shortLabel.TabIndex = 64;
            this.shortLabel.Text = "Short";
            // 
            // widthSlider
            // 
            this.widthSlider.Location = new System.Drawing.Point(186, 105);
            this.widthSlider.Maximum = 800;
            this.widthSlider.Name = "widthSlider";
            this.widthSlider.Size = new System.Drawing.Size(250, 45);
            this.widthSlider.SmallChange = 5;
            this.widthSlider.TabIndex = 58;
            this.widthSlider.TickFrequency = 10;
            this.widthSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.widthSlider.Value = 50;
            this.widthSlider.Scroll += new System.EventHandler(this.widthSlider_Scroll);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(46, 136);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(20, 20);
            this.pictureBox1.TabIndex = 56;
            this.pictureBox1.TabStop = false;
            // 
            // heightLabel
            // 
            this.heightLabel.Location = new System.Drawing.Point(122, 167);
            this.heightLabel.Name = "heightLabel";
            this.heightLabel.Size = new System.Drawing.Size(63, 18);
            this.heightLabel.TabIndex = 63;
            this.heightLabel.Text = "Height";
            this.heightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // widthLabel
            // 
            this.widthLabel.Location = new System.Drawing.Point(121, 107);
            this.widthLabel.Name = "widthLabel";
            this.widthLabel.Size = new System.Drawing.Size(63, 18);
            this.widthLabel.TabIndex = 59;
            this.widthLabel.Text = "Width";
            this.widthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // opaqueLabel
            // 
            this.opaqueLabel.Location = new System.Drawing.Point(385, 54);
            this.opaqueLabel.Name = "opaqueLabel";
            this.opaqueLabel.Size = new System.Drawing.Size(48, 16);
            this.opaqueLabel.TabIndex = 50;
            this.opaqueLabel.Text = "Opaque";
            // 
            // horizontalPosition
            // 
            this.horizontalPosition.Location = new System.Drawing.Point(29, 136);
            this.horizontalPosition.Name = "horizontalPosition";
            this.horizontalPosition.Size = new System.Drawing.Size(54, 20);
            this.horizontalPosition.SmallChange = 10;
            this.horizontalPosition.TabIndex = 55;
            this.horizontalPosition.Value = 100;
            this.horizontalPosition.Scroll += new System.Windows.Forms.ScrollEventHandler(this.horizontalPosition_Scroll);
            // 
            // transparentLabel
            // 
            this.transparentLabel.Location = new System.Drawing.Point(76, 54);
            this.transparentLabel.Name = "transparentLabel";
            this.transparentLabel.Size = new System.Drawing.Size(72, 16);
            this.transparentLabel.TabIndex = 51;
            this.transparentLabel.Text = "Transparent";
            // 
            // narrowLabel
            // 
            this.narrowLabel.Location = new System.Drawing.Point(192, 131);
            this.narrowLabel.Name = "narrowLabel";
            this.narrowLabel.Size = new System.Drawing.Size(72, 16);
            this.narrowLabel.TabIndex = 60;
            this.narrowLabel.Text = "Narrow";
            // 
            // wideLabel
            // 
            this.wideLabel.Location = new System.Drawing.Point(396, 131);
            this.wideLabel.Name = "wideLabel";
            this.wideLabel.Size = new System.Drawing.Size(44, 17);
            this.wideLabel.TabIndex = 61;
            this.wideLabel.Text = "Wide";
            // 
            // verticalPosition
            // 
            this.verticalPosition.Location = new System.Drawing.Point(45, 120);
            this.verticalPosition.Name = "verticalPosition";
            this.verticalPosition.Size = new System.Drawing.Size(22, 51);
            this.verticalPosition.SmallChange = 10;
            this.verticalPosition.TabIndex = 54;
            this.verticalPosition.Value = 100;
            this.verticalPosition.Scroll += new System.Windows.Forms.ScrollEventHandler(this.verticalPosition_Scroll);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.Color.SteelBlue;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(3, 215);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(502, 27);
            this.label1.TabIndex = 69;
            this.label1.Text = "Instance Name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BackColor = System.Drawing.Color.SteelBlue;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(3, 297);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(502, 27);
            this.label2.TabIndex = 70;
            this.label2.Text = "Folder and View";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // uxDefaultFolderTypes
            // 
            this.uxDefaultFolderTypes.FormattingEnabled = true;
            this.uxDefaultFolderTypes.Items.AddRange(new object[] {
            "Inbox",
            "Calendar",
            "Contacts",
            "Notes",
            "Tasks",
            "Custom"});
            this.uxDefaultFolderTypes.Location = new System.Drawing.Point(7, 349);
            this.uxDefaultFolderTypes.Name = "uxDefaultFolderTypes";
            this.uxDefaultFolderTypes.Size = new System.Drawing.Size(463, 21);
            this.uxDefaultFolderTypes.TabIndex = 71;
            this.uxDefaultFolderTypes.SelectedIndexChanged += new System.EventHandler(this.uxDefaultFolderTypes_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 333);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 72;
            this.label3.Text = "Folder";
            // 
            // uxSelectCustomFolder
            // 
            this.uxSelectCustomFolder.Location = new System.Drawing.Point(476, 349);
            this.uxSelectCustomFolder.Name = "uxSelectCustomFolder";
            this.uxSelectCustomFolder.Size = new System.Drawing.Size(29, 23);
            this.uxSelectCustomFolder.TabIndex = 73;
            this.uxSelectCustomFolder.Text = "...";
            this.uxSelectCustomFolder.UseVisualStyleBackColor = true;
            this.uxSelectCustomFolder.Click += new System.EventHandler(this.uxSelectCustomFolder_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 373);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 74;
            this.label4.Text = "Folder View";
            // 
            // uxFolderViews
            // 
            this.uxFolderViews.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.uxFolderViews.FormattingEnabled = true;
            this.uxFolderViews.Location = new System.Drawing.Point(7, 389);
            this.uxFolderViews.Name = "uxFolderViews";
            this.uxFolderViews.Size = new System.Drawing.Size(463, 21);
            this.uxFolderViews.TabIndex = 75;
            this.uxFolderViews.SelectedIndexChanged += new System.EventHandler(this.uxFolderViews_SelectedIndexChanged);
            // 
            // InstanceSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uxFolderViews);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.uxSelectCustomFolder);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.uxDefaultFolderTypes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.uxHeightValue);
            this.Controls.Add(this.uxWidthValue);
            this.Controls.Add(this.preferencesLabel);
            this.Controls.Add(this.uxOpacityValue);
            this.Controls.Add(this.uxCancelSettingsChange);
            this.Controls.Add(this.heightSlider);
            this.Controls.Add(this.uxSaveSettings);
            this.Controls.Add(this.tallLabel);
            this.Controls.Add(this.opacityLabel);
            this.Controls.Add(this.transparencySlider);
            this.Controls.Add(this.positionLabel);
            this.Controls.Add(this.shortLabel);
            this.Controls.Add(this.widthSlider);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.heightLabel);
            this.Controls.Add(this.widthLabel);
            this.Controls.Add(this.opaqueLabel);
            this.Controls.Add(this.horizontalPosition);
            this.Controls.Add(this.transparentLabel);
            this.Controls.Add(this.narrowLabel);
            this.Controls.Add(this.wideLabel);
            this.Controls.Add(this.verticalPosition);
            this.Name = "InstanceSettings";
            this.Size = new System.Drawing.Size(508, 509);
            ((System.ComponentModel.ISupportInitialize)(this.heightSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.transparencySlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.widthSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label uxHeightValue;
        private System.Windows.Forms.Label uxWidthValue;
        private System.Windows.Forms.Label preferencesLabel;
        private System.Windows.Forms.Label uxOpacityValue;
        private System.Windows.Forms.Button uxCancelSettingsChange;
        private System.Windows.Forms.TrackBar heightSlider;
        private System.Windows.Forms.Button uxSaveSettings;
        private System.Windows.Forms.Label tallLabel;
        private System.Windows.Forms.Label opacityLabel;
        private System.Windows.Forms.TrackBar transparencySlider;
        private System.Windows.Forms.Label positionLabel;
        private System.Windows.Forms.Label shortLabel;
        private System.Windows.Forms.TrackBar widthSlider;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label heightLabel;
        private System.Windows.Forms.Label widthLabel;
        private System.Windows.Forms.Label opaqueLabel;
        private System.Windows.Forms.HScrollBar horizontalPosition;
        private System.Windows.Forms.Label transparentLabel;
        private System.Windows.Forms.Label narrowLabel;
        private System.Windows.Forms.Label wideLabel;
        private System.Windows.Forms.VScrollBar verticalPosition;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox uxDefaultFolderTypes;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button uxSelectCustomFolder;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox uxFolderViews;
    }
}
