using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using OutlookDesktop.Properties;

namespace OutlookDesktop
{
    public partial class PreferencesForm : Form
    {
        public PreferencesForm(MainForm fOwner)
		{
			InitializeComponent();

			fOwnerForm = fOwner;

            AdjustMinMaxDimensions();

			Load_Settings();
		}

        private void AdjustMinMaxDimensions() {
            // Setup up mins and maxes for the dimensions of the window
			// depending on the user's resolution.            
            verticalPosition.Minimum = SystemInformation.VirtualScreen.Top;
            verticalPosition.Maximum = SystemInformation.VirtualScreen.Top + SystemInformation.VirtualScreen.Height - fOwnerForm.Height + verticalPosition.SmallChange;

            horizontalPosition.Minimum = SystemInformation.VirtualScreen.Left;
            horizontalPosition.Maximum = SystemInformation.VirtualScreen.Left + SystemInformation.VirtualScreen.Width - fOwnerForm.Width + horizontalPosition.SmallChange;
			heightSlider.Minimum = SystemInformation.MinimumWindowSize.Height;
            heightSlider.Maximum = SystemInformation.VirtualScreen.Height;

			widthSlider.Minimum = SystemInformation.MinimumWindowSize.Width;
            widthSlider.Maximum = SystemInformation.VirtualScreen.Width;
        }

		/// <summary>
		/// Loads values from registry into their corresponding controls.
		/// </summary>
		private void Load_Settings()
		{
            //launchOnStartupCheckbox.Checked = InstancePreferences.StartWithWindows;
            
			try 
			{
                if (fOwnerForm.Preferences.Opacity >= 0 && fOwnerForm.Preferences.Opacity <= 1)
                {
                    this.transparencySlider.Value = (int)(fOwnerForm.Preferences.Opacity * 100);
                }
                else
                {
                    this.transparencySlider.Value = (int) (InstancePreferences.DefaultOpacity * 100);
                    MessageBox.Show(this, Resources.ErrorSettingOpacity, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Preferences.Left >= this.horizontalPosition.Minimum && fOwnerForm.Preferences.Left <= this.horizontalPosition.Maximum) {
                    this.horizontalPosition.Value = fOwnerForm.Preferences.Left;
                } 
                else
                {
                    this.horizontalPosition.Value = InstancePreferences.DefaultLeftPosition;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Preferences.Top >= this.verticalPosition.Minimum && fOwnerForm.Preferences.Top <= this.verticalPosition.Maximum)
                {
                    this.verticalPosition.Value = fOwnerForm.Preferences.Top;
                }
                else
                {
                    this.verticalPosition.Value = InstancePreferences.DefaultTopPosition;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Preferences.Height >= this.heightSlider.Minimum && fOwnerForm.Preferences.Height <= this.heightSlider.Maximum)
                {
                    this.heightSlider.Value = fOwnerForm.Preferences.Height;
                }
                else
                {
                    this.heightSlider.Value = InstancePreferences.DefaultHeight;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (fOwnerForm.Preferences.Width >= this.widthSlider.Minimum && fOwnerForm.Preferences.Width <= this.widthSlider.Maximum)
                {
                    this.widthSlider.Value = fOwnerForm.Preferences.Width;
                }
                else
                {
                    this.widthSlider.Value = InstancePreferences.DefaultWidth;
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
		/// Saves the user's settings to the windows registry.
		/// </summary>
		/// <returns>true if successful, false otherwise.</returns>
		private bool Save_Settings()
		{
			try 
			{
                //InstancePreferences.StartWithWindows = this.launchOnStartupCheckbox.Checked;
                double opacityVal;
                opacityVal = (double)this.transparencySlider.Value / 100;
                if (opacityVal == 1) { opacityVal = 0.99; }
                fOwnerForm.Preferences.Opacity = opacityVal;
                fOwnerForm.Preferences.Left = this.horizontalPosition.Value;
                fOwnerForm.Preferences.Top = this.verticalPosition.Value;
                fOwnerForm.Preferences.Width = this.widthSlider.Value;
                fOwnerForm.Preferences.Height = this.heightSlider.Value;
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
                fOwnerForm.Opacity = fOwnerForm.Preferences.Opacity;
                fOwnerForm.Left = fOwnerForm.Preferences.Left;
                fOwnerForm.Top = fOwnerForm.Preferences.Top;
                fOwnerForm.Height = fOwnerForm.Preferences.Height;
                fOwnerForm.Width = fOwnerForm.Preferences.Width;
				
				return true;
			}
			else 
			{
				return false;
			}
		}

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (!Save_Settings())
            {
                MessageBox.Show(this, Resources.ErrorSavingSettings, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PreferencesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bDirty && !Confirm_Cancel())
            {
                e.Cancel = true;
            }
        }

        private void transparencySlider_Scroll(object sender, EventArgs e)
        {
            // update main form in real time so the user can gauage how they want it.
            double opacityVal = (double)this.transparencySlider.Value / 100;
            if (opacityVal == 1) { opacityVal = 0.99; }
            fOwnerForm.Opacity = opacityVal;
            bDirty = true;
        }

        private void widthSlider_Scroll(object sender, EventArgs e)
        {
            // update main form in real time so the user can gauage how they want it.
            fOwnerForm.Width = widthSlider.Value;
            bDirty = true;
            AdjustMinMaxDimensions();
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
        }

        private void heightSlider_Scroll(object sender, EventArgs e)
        {
            // update main form in real time so the user can gauage how they want it.
            fOwnerForm.Height = heightSlider.Value;
            bDirty = true;
            AdjustMinMaxDimensions();
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
        }

        private void horizontalPosition_Scroll(object sender, ScrollEventArgs e)
        {
            // update main form in real time so the user can gauage how they want it.
            fOwnerForm.Left = horizontalPosition.Value;
            bDirty = true;
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
        }

        private void verticalPosition_Scroll(object sender, ScrollEventArgs e)
        {
            // update main form in real time so the user can gauage how they want it.
            fOwnerForm.Top = verticalPosition.Value;
            bDirty = true;
            Debug.Print(fOwnerForm.Top + " " + fOwnerForm.Left);
        }
    }
}
