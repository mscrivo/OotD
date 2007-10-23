using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using OutlookDesktop.Properties;

namespace OutlookDesktop
{
	/// <summary>
	/// Summary description for frmPrefs.
	/// </summary>
	public class FormPrefs : System.Windows.Forms.Form
	{
		private FormMain fOwnerForm;
		private bool bDirty;
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.CheckBox launchOnStartupCheckbox;
		private System.Windows.Forms.Label preferencesLabel;
		private System.Windows.Forms.TrackBar transparencySlider;
		private System.Windows.Forms.Label opacityLabel;
		private System.Windows.Forms.Label opaqueLabel;
		private System.Windows.Forms.Label transparentLabel;
		private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.HScrollBar horizontalPosition;
		private System.Windows.Forms.VScrollBar verticalPosition;
		private System.Windows.Forms.Label positionLabel;
		private System.Windows.Forms.Label widthLabel;
		private System.Windows.Forms.Label narrowLabel;
		private System.Windows.Forms.Label wideLabel;
		private System.Windows.Forms.TrackBar widthSlider;
		private System.Windows.Forms.Label tallLabel;
		private System.Windows.Forms.Label shortLabel;
		private System.Windows.Forms.Label heightLabel;
		private System.Windows.Forms.TrackBar heightSlider;

		public FormPrefs(FormMain fOwner)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			fOwnerForm = fOwner;

            AdjustMinMaxDimensions();

			Load_Settings();
		}

        private void AdjustMinMaxDimensions() {
            // Setup up mins and maxes for the dimensions of the window
			// depending on the user's resolution.            
            verticalPosition.Minimum = SystemInformation.WorkingArea.Top;
            verticalPosition.Maximum = SystemInformation.WorkingArea.Top + SystemInformation.WorkingArea.Height - fOwnerForm.Height + verticalPosition.SmallChange;

            horizontalPosition.Minimum = SystemInformation.WorkingArea.Left;
            horizontalPosition.Maximum = SystemInformation.WorkingArea.Left + SystemInformation.WorkingArea.Width - fOwnerForm.Width + horizontalPosition.SmallChange;
			heightSlider.Minimum = SystemInformation.MinimumWindowSize.Height;
            heightSlider.Maximum = SystemInformation.WorkingArea.Height;

			widthSlider.Minimum = SystemInformation.MinimumWindowSize.Width;
            widthSlider.Maximum = SystemInformation.WorkingArea.Width;
        }

		/// <summary>
		/// Loads values from registry into their corresponding controls.
		/// </summary>
		private void Load_Settings()
		{
            launchOnStartupCheckbox.Checked = Preferences.StartWithWindows;
            
			try 
			{
                if (fOwnerForm.Prefs.Opacity >= 0 && fOwnerForm.Prefs.Opacity <= 1)
                {
                    this.transparencySlider.Value = (int)(fOwnerForm.Prefs.Opacity * 100);
                }
                else
                {
                    this.transparencySlider.Value = (int) (Preferences.DefaultOpacity * 100);
                    MessageBox.Show(this, Resources.ErrorSettingOpacity, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Prefs.Left >= this.horizontalPosition.Minimum && fOwnerForm.Prefs.Left <= this.horizontalPosition.Maximum) {
                    this.horizontalPosition.Value = fOwnerForm.Prefs.Left;
                } 
                else
                {
                    this.horizontalPosition.Value = Preferences.DefaultLeftPosition;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Prefs.Top >= this.verticalPosition.Minimum && fOwnerForm.Prefs.Top <= this.verticalPosition.Maximum)
                {
                    this.verticalPosition.Value = fOwnerForm.Prefs.Top;
                }
                else
                {
                    this.verticalPosition.Value = Preferences.DefaultTopPosition;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Prefs.Height >= this.heightSlider.Minimum && fOwnerForm.Prefs.Height <= this.heightSlider.Maximum)
                {
                    this.heightSlider.Value = fOwnerForm.Prefs.Height;
                }
                else
                {
                    this.heightSlider.Value = Preferences.DefaultHeight;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Prefs.Width >= this.widthSlider.Minimum && fOwnerForm.Prefs.Width <= this.widthSlider.Maximum)
                {
                    this.widthSlider.Value = fOwnerForm.Prefs.Width;
                }
                else
                {
                    this.widthSlider.Value = Preferences.DefaultWidth;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                bDirty = false;
			} 
			catch (Exception ex)
			{
                MessageBox.Show(this, ex.Message, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error,MessageBoxDefaultButton.Button1);
			}
		}
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Saves the user's settings to the windows registry.
		/// </summary>
		/// <returns>true if successful, false otherwise.</returns>
		private bool Save_Settings()
		{
			try 
			{
                Preferences.StartWithWindows = this.launchOnStartupCheckbox.Checked;
                double opacityVal;
                opacityVal = (double)this.transparencySlider.Value / 100;
                if (opacityVal == 1) { opacityVal = 0.99; }
                fOwnerForm.Prefs.Opacity = opacityVal;
                fOwnerForm.Prefs.Left = this.horizontalPosition.Value;
                fOwnerForm.Prefs.Top = this.verticalPosition.Value;
                fOwnerForm.Prefs.Width = this.widthSlider.Value;
                fOwnerForm.Prefs.Height = this.heightSlider.Value;
                bDirty = false;
			} 
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Verfies if the user wants to cancel without saving or not.
		/// </summary>
		/// <returns>true if the user wants to cancel, false otherwise.</returns>
		private bool Confirm_Cancel()
		{
			DialogResult msgResult = new DialogResult();

			msgResult = MessageBox.Show(this, Resources.CancelConfirmation, Resources.CancelCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

			if (msgResult == DialogResult.Yes) 
			{
				// restore settings
                fOwnerForm.Opacity = fOwnerForm.Prefs.Opacity;
                fOwnerForm.Left = fOwnerForm.Prefs.Left;
                fOwnerForm.Top = fOwnerForm.Prefs.Top;
                fOwnerForm.Height = fOwnerForm.Prefs.Height;
                fOwnerForm.Width = fOwnerForm.Prefs.Width;
				
				return true;
			}
			else 
			{
				return false;
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrefs));
            this.preferencesLabel = new System.Windows.Forms.Label();
            this.transparencySlider = new System.Windows.Forms.TrackBar();
            this.opacityLabel = new System.Windows.Forms.Label();
            this.opaqueLabel = new System.Windows.Forms.Label();
            this.transparentLabel = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.launchOnStartupCheckbox = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.horizontalPosition = new System.Windows.Forms.HScrollBar();
            this.verticalPosition = new System.Windows.Forms.VScrollBar();
            this.positionLabel = new System.Windows.Forms.Label();
            this.widthSlider = new System.Windows.Forms.TrackBar();
            this.widthLabel = new System.Windows.Forms.Label();
            this.narrowLabel = new System.Windows.Forms.Label();
            this.wideLabel = new System.Windows.Forms.Label();
            this.tallLabel = new System.Windows.Forms.Label();
            this.shortLabel = new System.Windows.Forms.Label();
            this.heightLabel = new System.Windows.Forms.Label();
            this.heightSlider = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.transparencySlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.widthSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // preferencesLabel
            // 
            this.preferencesLabel.BackColor = System.Drawing.Color.SteelBlue;
            this.preferencesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.preferencesLabel.ForeColor = System.Drawing.Color.White;
            this.preferencesLabel.Location = new System.Drawing.Point(5, 4);
            this.preferencesLabel.Name = "preferencesLabel";
            this.preferencesLabel.Size = new System.Drawing.Size(467, 27);
            this.preferencesLabel.TabIndex = 0;
            this.preferencesLabel.Text = "Preferences";
            this.preferencesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // transparencySlider
            // 
            this.transparencySlider.Location = new System.Drawing.Point(88, 88);
            this.transparencySlider.Maximum = 100;
            this.transparencySlider.Name = "transparencySlider";
            this.transparencySlider.Size = new System.Drawing.Size(368, 45);
            this.transparencySlider.SmallChange = 5;
            this.transparencySlider.TabIndex = 3;
            this.transparencySlider.TickFrequency = 10;
            this.transparencySlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.transparencySlider.Value = 50;
            this.transparencySlider.Scroll += new System.EventHandler(this.sldTrans_Scroll);
            // 
            // opacityLabel
            // 
            this.opacityLabel.Location = new System.Drawing.Point(24, 90);
            this.opacityLabel.Name = "opacityLabel";
            this.opacityLabel.Size = new System.Drawing.Size(63, 18);
            this.opacityLabel.TabIndex = 4;
            this.opacityLabel.Text = "Opacity:";
            this.opacityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // opaqueLabel
            // 
            this.opaqueLabel.Location = new System.Drawing.Point(405, 112);
            this.opaqueLabel.Name = "opaqueLabel";
            this.opaqueLabel.Size = new System.Drawing.Size(48, 16);
            this.opaqueLabel.TabIndex = 5;
            this.opaqueLabel.Text = "Opaque";
            // 
            // transparentLabel
            // 
            this.transparentLabel.Location = new System.Drawing.Point(96, 112);
            this.transparentLabel.Name = "transparentLabel";
            this.transparentLabel.Size = new System.Drawing.Size(72, 16);
            this.transparentLabel.TabIndex = 6;
            this.transparentLabel.Text = "Transparent";
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.OKButton.Location = new System.Drawing.Point(142, 298);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(88, 24);
            this.OKButton.TabIndex = 7;
            this.OKButton.Text = global::OutlookDesktop.Properties.Resources.OKButton;
            this.OKButton.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Cancel.Location = new System.Drawing.Point(249, 298);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(88, 24);
            this.Cancel.TabIndex = 8;
            this.Cancel.Text = global::OutlookDesktop.Properties.Resources.CancelButton;
            this.Cancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // launchOnStartupCheckbox
            // 
            this.launchOnStartupCheckbox.Location = new System.Drawing.Point(96, 47);
            this.launchOnStartupCheckbox.Name = "launchOnStartupCheckbox";
            this.launchOnStartupCheckbox.Size = new System.Drawing.Size(299, 27);
            this.launchOnStartupCheckbox.TabIndex = 11;
            this.launchOnStartupCheckbox.Text = global::OutlookDesktop.Properties.Resources.StartWithWindows;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(66, 194);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(20, 20);
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            // 
            // horizontalPosition
            // 
            this.horizontalPosition.Location = new System.Drawing.Point(49, 194);
            this.horizontalPosition.Name = "horizontalPosition";
            this.horizontalPosition.Size = new System.Drawing.Size(54, 20);
            this.horizontalPosition.SmallChange = 10;
            this.horizontalPosition.TabIndex = 13;
            this.horizontalPosition.Value = 100;
            this.horizontalPosition.Scroll += new System.Windows.Forms.ScrollEventHandler(this.horizontal_Scroll);
            // 
            // verticalPosition
            // 
            this.verticalPosition.Location = new System.Drawing.Point(65, 178);
            this.verticalPosition.Name = "verticalPosition";
            this.verticalPosition.Size = new System.Drawing.Size(22, 51);
            this.verticalPosition.SmallChange = 10;
            this.verticalPosition.TabIndex = 12;
            this.verticalPosition.Value = 100;
            this.verticalPosition.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vertical_Scroll);
            // 
            // positionLabel
            // 
            this.positionLabel.Location = new System.Drawing.Point(55, 237);
            this.positionLabel.Name = "positionLabel";
            this.positionLabel.Size = new System.Drawing.Size(49, 14);
            this.positionLabel.TabIndex = 15;
            this.positionLabel.Text = "Position";
            // 
            // widthSlider
            // 
            this.widthSlider.Location = new System.Drawing.Point(206, 163);
            this.widthSlider.Maximum = 800;
            this.widthSlider.Name = "widthSlider";
            this.widthSlider.Size = new System.Drawing.Size(250, 45);
            this.widthSlider.SmallChange = 5;
            this.widthSlider.TabIndex = 16;
            this.widthSlider.TickFrequency = 10;
            this.widthSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.widthSlider.Value = 50;
            this.widthSlider.Scroll += new System.EventHandler(this.sldWidth_Scroll);
            // 
            // widthLabel
            // 
            this.widthLabel.Location = new System.Drawing.Point(141, 165);
            this.widthLabel.Name = "widthLabel";
            this.widthLabel.Size = new System.Drawing.Size(63, 18);
            this.widthLabel.TabIndex = 17;
            this.widthLabel.Text = "Width";
            this.widthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // narrowLabel
            // 
            this.narrowLabel.Location = new System.Drawing.Point(212, 189);
            this.narrowLabel.Name = "narrowLabel";
            this.narrowLabel.Size = new System.Drawing.Size(72, 16);
            this.narrowLabel.TabIndex = 18;
            this.narrowLabel.Text = "Narrow";
            // 
            // wideLabel
            // 
            this.wideLabel.Location = new System.Drawing.Point(416, 189);
            this.wideLabel.Name = "wideLabel";
            this.wideLabel.Size = new System.Drawing.Size(44, 17);
            this.wideLabel.TabIndex = 19;
            this.wideLabel.Text = "Wide";
            // 
            // tallLabel
            // 
            this.tallLabel.Location = new System.Drawing.Point(422, 249);
            this.tallLabel.Name = "tallLabel";
            this.tallLabel.Size = new System.Drawing.Size(44, 17);
            this.tallLabel.TabIndex = 23;
            this.tallLabel.Text = "Tall";
            // 
            // shortLabel
            // 
            this.shortLabel.Location = new System.Drawing.Point(212, 249);
            this.shortLabel.Name = "shortLabel";
            this.shortLabel.Size = new System.Drawing.Size(72, 16);
            this.shortLabel.TabIndex = 22;
            this.shortLabel.Text = "Short";
            // 
            // heightLabel
            // 
            this.heightLabel.Location = new System.Drawing.Point(142, 225);
            this.heightLabel.Name = "heightLabel";
            this.heightLabel.Size = new System.Drawing.Size(63, 18);
            this.heightLabel.TabIndex = 21;
            this.heightLabel.Text = "Height";
            this.heightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // heightSlider
            // 
            this.heightSlider.Location = new System.Drawing.Point(206, 223);
            this.heightSlider.Maximum = 600;
            this.heightSlider.Name = "heightSlider";
            this.heightSlider.Size = new System.Drawing.Size(250, 45);
            this.heightSlider.SmallChange = 5;
            this.heightSlider.TabIndex = 20;
            this.heightSlider.TickFrequency = 10;
            this.heightSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.heightSlider.Value = 50;
            this.heightSlider.Scroll += new System.EventHandler(this.sldHeight_Scroll);
            // 
            // FormPrefs
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(478, 333);
            this.Controls.Add(this.tallLabel);
            this.Controls.Add(this.shortLabel);
            this.Controls.Add(this.heightLabel);
            this.Controls.Add(this.heightSlider);
            this.Controls.Add(this.wideLabel);
            this.Controls.Add(this.narrowLabel);
            this.Controls.Add(this.widthLabel);
            this.Controls.Add(this.widthSlider);
            this.Controls.Add(this.positionLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.horizontalPosition);
            this.Controls.Add(this.verticalPosition);
            this.Controls.Add(this.launchOnStartupCheckbox);
            this.Controls.Add(this.preferencesLabel);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.transparentLabel);
            this.Controls.Add(this.opaqueLabel);
            this.Controls.Add(this.opacityLabel);
            this.Controls.Add(this.transparencySlider);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormPrefs";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Outlook on the Destkop Preferences";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmPrefs_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.transparencySlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.widthSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightSlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void sldTrans_Scroll(object sender, System.EventArgs e)
		{		
			// update main form in real time so the user can gauage how they want it.
            double opacityVal = (double)this.transparencySlider.Value / 100;
            if (opacityVal == 1) { opacityVal = 0.99; }
            fOwnerForm.Opacity = opacityVal;
            bDirty = true;
		}

		private void cmdOK_Click(object sender, System.EventArgs e)
		{
			if (!Save_Settings())
			{
				MessageBox.Show(this, Resources.ErrorSavingSettings, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}

			this.Close();
		}

		private void cmdCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void frmPrefs_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (bDirty && !Confirm_Cancel())
			{
				e.Cancel = true;
			}
		}

		private void vertical_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{		
			// update main form in real time so the user can gauage how they want it.
			fOwnerForm.Top = verticalPosition.Value;
            bDirty = true;
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
		}

		private void horizontal_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			// update main form in real time so the user can gauage how they want it.
			fOwnerForm.Left = horizontalPosition.Value;
            bDirty = true;
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
		}

		private void sldWidth_Scroll(object sender, System.EventArgs e)
		{
			// update main form in real time so the user can gauage how they want it.
			fOwnerForm.Width = widthSlider.Value;
            bDirty = true;
            AdjustMinMaxDimensions();
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
		}

		private void sldHeight_Scroll(object sender, System.EventArgs e)
		{
			// update main form in real time so the user can gauage how they want it.
			fOwnerForm.Height = heightSlider.Value;
            bDirty = true;
            AdjustMinMaxDimensions();
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
		}
	}
}
