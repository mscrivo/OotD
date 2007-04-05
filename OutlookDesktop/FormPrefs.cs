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
		private System.Windows.Forms.CheckBox chkLaunchOnStartup;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TrackBar sldTrans;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.HScrollBar horizontal;
		private System.Windows.Forms.VScrollBar vertical;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TrackBar sldWidth;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TrackBar sldHeight;

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
            vertical.Minimum = SystemInformation.VirtualScreen.Top;
            vertical.Maximum = SystemInformation.VirtualScreen.Top + SystemInformation.VirtualScreen.Height - fOwnerForm.Height + vertical.SmallChange;

            horizontal.Minimum = SystemInformation.VirtualScreen.Left;
            horizontal.Maximum = SystemInformation.VirtualScreen.Left + SystemInformation.VirtualScreen.Width - fOwnerForm.Width + horizontal.SmallChange;
			sldHeight.Minimum = SystemInformation.MinimumWindowSize.Height;
            sldHeight.Maximum = SystemInformation.VirtualScreen.Height;

			sldWidth.Minimum = SystemInformation.MinimumWindowSize.Width;
            sldWidth.Maximum = SystemInformation.VirtualScreen.Width;
        }

		/// <summary>
		/// Loads values from registry into their corresponding controls.
		/// </summary>
		private void Load_Settings()
		{
            chkLaunchOnStartup.Checked = Preferences.StartWithWindows;
            
			try 
			{
                if (fOwnerForm.Prefs.Opacity >= 0 && fOwnerForm.Prefs.Opacity <= 1)
                {
                    this.sldTrans.Value = (int)(fOwnerForm.Prefs.Opacity * 100);
                }
                else
                {
                    this.sldTrans.Value = (int) (Preferences.DEFAULT_OPACITY * 100);
                    MessageBox.Show(this, Resources.ErrorSettingOpacity, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Prefs.Left >= this.horizontal.Minimum && fOwnerForm.Prefs.Left <= this.horizontal.Maximum) {
                    this.horizontal.Value = fOwnerForm.Prefs.Left;
                } 
                else
                {
                    this.horizontal.Value = Preferences.DEFAULT_LEFT;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Prefs.Top >= this.vertical.Minimum && fOwnerForm.Prefs.Top <= this.vertical.Maximum)
                {
                    this.vertical.Value = fOwnerForm.Prefs.Top;
                }
                else
                {
                    this.vertical.Value = Preferences.DEFAULT_TOP;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Prefs.Height >= this.sldHeight.Minimum && fOwnerForm.Prefs.Height <= this.sldHeight.Maximum)
                {
                    this.sldHeight.Value = fOwnerForm.Prefs.Height;
                }
                else
                {
                    this.sldHeight.Value = Preferences.DEFAULT_HEIGHT;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Prefs.Width >= this.sldWidth.Minimum && fOwnerForm.Prefs.Width <= this.sldWidth.Maximum)
                {
                    this.sldWidth.Value = fOwnerForm.Prefs.Width;
                }
                else
                {
                    this.sldWidth.Value = Preferences.DEFAULT_WIDTH;
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
                Preferences.StartWithWindows = this.chkLaunchOnStartup.Checked;
                double opacityVal;
                opacityVal = (double)this.sldTrans.Value / 100;
                if (opacityVal == 1) { opacityVal = 0.99; }
                fOwnerForm.Prefs.Opacity = opacityVal;
                fOwnerForm.Prefs.Left = this.horizontal.Value;
                fOwnerForm.Prefs.Top = this.vertical.Value;
                fOwnerForm.Prefs.Width = this.sldWidth.Value;
                fOwnerForm.Prefs.Height = this.sldHeight.Value;
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
            this.label1 = new System.Windows.Forms.Label();
            this.sldTrans = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.chkLaunchOnStartup = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.horizontal = new System.Windows.Forms.HScrollBar();
            this.vertical = new System.Windows.Forms.VScrollBar();
            this.label5 = new System.Windows.Forms.Label();
            this.sldWidth = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.sldHeight = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.sldTrans)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sldWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sldHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.SteelBlue;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(5, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(467, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "Preferences";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // sldTrans
            // 
            this.sldTrans.Location = new System.Drawing.Point(88, 88);
            this.sldTrans.Maximum = 100;
            this.sldTrans.Name = "sldTrans";
            this.sldTrans.Size = new System.Drawing.Size(368, 45);
            this.sldTrans.SmallChange = 5;
            this.sldTrans.TabIndex = 3;
            this.sldTrans.TickFrequency = 10;
            this.sldTrans.TickStyle = System.Windows.Forms.TickStyle.None;
            this.sldTrans.Value = 50;
            this.sldTrans.Scroll += new System.EventHandler(this.sldTrans_Scroll);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(24, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 18);
            this.label2.TabIndex = 4;
            this.label2.Text = "Opacity:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(405, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 16);
            this.label3.TabIndex = 5;
            this.label3.Text = "Opaque";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(96, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Transparent";
            // 
            // cmdOK
            // 
            this.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdOK.Location = new System.Drawing.Point(142, 298);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(88, 24);
            this.cmdOK.TabIndex = 7;
            this.cmdOK.Text = global::OutlookDesktop.Properties.Resources.OKButton;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdCancel.Location = new System.Drawing.Point(249, 298);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(88, 24);
            this.cmdCancel.TabIndex = 8;
            this.cmdCancel.Text = global::OutlookDesktop.Properties.Resources.CancelButton;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // chkLaunchOnStartup
            // 
            this.chkLaunchOnStartup.Location = new System.Drawing.Point(96, 47);
            this.chkLaunchOnStartup.Name = "chkLaunchOnStartup";
            this.chkLaunchOnStartup.Size = new System.Drawing.Size(299, 27);
            this.chkLaunchOnStartup.TabIndex = 11;
            this.chkLaunchOnStartup.Text = global::OutlookDesktop.Properties.Resources.StartWithWindows;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(66, 194);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(20, 20);
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            // 
            // horizontal
            // 
            this.horizontal.Location = new System.Drawing.Point(49, 194);
            this.horizontal.Name = "horizontal";
            this.horizontal.Size = new System.Drawing.Size(54, 20);
            this.horizontal.SmallChange = 10;
            this.horizontal.TabIndex = 13;
            this.horizontal.Value = 100;
            this.horizontal.Scroll += new System.Windows.Forms.ScrollEventHandler(this.horizontal_Scroll);
            // 
            // vertical
            // 
            this.vertical.Location = new System.Drawing.Point(65, 178);
            this.vertical.Name = "vertical";
            this.vertical.Size = new System.Drawing.Size(22, 51);
            this.vertical.SmallChange = 10;
            this.vertical.TabIndex = 12;
            this.vertical.Value = 100;
            this.vertical.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vertical_Scroll);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(55, 237);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 14);
            this.label5.TabIndex = 15;
            this.label5.Text = "Position";
            // 
            // sldWidth
            // 
            this.sldWidth.Location = new System.Drawing.Point(206, 163);
            this.sldWidth.Maximum = 800;
            this.sldWidth.Name = "sldWidth";
            this.sldWidth.Size = new System.Drawing.Size(250, 45);
            this.sldWidth.SmallChange = 5;
            this.sldWidth.TabIndex = 16;
            this.sldWidth.TickFrequency = 10;
            this.sldWidth.TickStyle = System.Windows.Forms.TickStyle.None;
            this.sldWidth.Value = 50;
            this.sldWidth.Scroll += new System.EventHandler(this.sldWidth_Scroll);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(141, 165);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 18);
            this.label6.TabIndex = 17;
            this.label6.Text = "Width";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(212, 189);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 16);
            this.label7.TabIndex = 18;
            this.label7.Text = "Narrow";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(416, 189);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 17);
            this.label8.TabIndex = 19;
            this.label8.Text = "Wide";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(422, 249);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(44, 17);
            this.label9.TabIndex = 23;
            this.label9.Text = "Tall";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(212, 249);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(72, 16);
            this.label10.TabIndex = 22;
            this.label10.Text = "Short";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(142, 225);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(63, 18);
            this.label11.TabIndex = 21;
            this.label11.Text = "Height";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // sldHeight
            // 
            this.sldHeight.Location = new System.Drawing.Point(206, 223);
            this.sldHeight.Maximum = 600;
            this.sldHeight.Name = "sldHeight";
            this.sldHeight.Size = new System.Drawing.Size(250, 45);
            this.sldHeight.SmallChange = 5;
            this.sldHeight.TabIndex = 20;
            this.sldHeight.TickFrequency = 10;
            this.sldHeight.TickStyle = System.Windows.Forms.TickStyle.None;
            this.sldHeight.Value = 50;
            this.sldHeight.Scroll += new System.EventHandler(this.sldHeight_Scroll);
            // 
            // FormPrefs
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(478, 333);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.sldHeight);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.sldWidth);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.horizontal);
            this.Controls.Add(this.vertical);
            this.Controls.Add(this.chkLaunchOnStartup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.sldTrans);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormPrefs";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Outlook on the Desktop Preferences";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmPrefs_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.sldTrans)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sldWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sldHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void sldTrans_Scroll(object sender, System.EventArgs e)
		{		
			// update main form in real time so the user can gauage how they want it.
            double opacityVal = (double)this.sldTrans.Value / 100;
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
			fOwnerForm.Top = vertical.Value;
            bDirty = true;
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
		}

		private void horizontal_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			// update main form in real time so the user can gauage how they want it.
			fOwnerForm.Left = horizontal.Value;
            bDirty = true;
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
		}

		private void sldWidth_Scroll(object sender, System.EventArgs e)
		{
			// update main form in real time so the user can gauage how they want it.
			fOwnerForm.Width = sldWidth.Value;
            bDirty = true;
            AdjustMinMaxDimensions();
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
		}

		private void sldHeight_Scroll(object sender, System.EventArgs e)
		{
			// update main form in real time so the user can gauage how they want it.
			fOwnerForm.Height = sldHeight.Value;
            bDirty = true;
            AdjustMinMaxDimensions();
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
		}
	}
}
