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
        private MainForm _parentMainForm;
        private bool _dirty;

        public PreferencesForm(MainForm parentMainForm)
		{
            InitializeComponent();

            _parentMainForm = parentMainForm;
            
            AdjustMinMaxDimensions();
			LoadSettings();
		}

        private void AdjustMinMaxDimensions() {
            // Setup up mins and maxes for the dimensions of the window
			// depending on the user's resolution.            
            verticalPosition.Minimum = SystemInformation.VirtualScreen.Top;
            verticalPosition.Maximum = SystemInformation.VirtualScreen.Top + SystemInformation.VirtualScreen.Height - _parentMainForm.Height + verticalPosition.SmallChange;

            horizontalPosition.Minimum = SystemInformation.VirtualScreen.Left;
            horizontalPosition.Maximum = SystemInformation.VirtualScreen.Left + SystemInformation.VirtualScreen.Width - _parentMainForm.Width + horizontalPosition.SmallChange;
			heightSlider.Minimum = SystemInformation.MinimumWindowSize.Height;
            heightSlider.Maximum = SystemInformation.VirtualScreen.Height;

			widthSlider.Minimum = SystemInformation.MinimumWindowSize.Width;
            widthSlider.Maximum = SystemInformation.VirtualScreen.Width;
        }

		/// <summary>
		/// Loads values from registry into their corresponding controls.
		/// </summary>
		private void LoadSettings()
		{
			try 
			{
                if (_parentMainForm.Preferences.Opacity >= 0 && _parentMainForm.Preferences.Opacity <= 1)
                {
                    this.transparencySlider.Value = (int)(_parentMainForm.Preferences.Opacity * 100);
                }
                else
                {
                    this.transparencySlider.Value = (int) (InstancePreferences.DefaultOpacity * 100);
                    MessageBox.Show(this, Resources.ErrorSettingOpacity, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (_parentMainForm.Preferences.Left >= this.horizontalPosition.Minimum && _parentMainForm.Preferences.Left <= this.horizontalPosition.Maximum) {
                    this.horizontalPosition.Value = _parentMainForm.Preferences.Left;
                } 
                else
                {
                    this.horizontalPosition.Value = InstancePreferences.DefaultLeftPosition;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (_parentMainForm.Preferences.Top >= this.verticalPosition.Minimum && _parentMainForm.Preferences.Top <= this.verticalPosition.Maximum)
                {
                    this.verticalPosition.Value = _parentMainForm.Preferences.Top;
                }
                else
                {
                    this.verticalPosition.Value = InstancePreferences.DefaultTopPosition;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (_parentMainForm.Preferences.Height >= this.heightSlider.Minimum && _parentMainForm.Preferences.Height <= this.heightSlider.Maximum)
                {
                    this.heightSlider.Value = _parentMainForm.Preferences.Height;
                }
                else
                {
                    this.heightSlider.Value = InstancePreferences.DefaultHeight;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                if (_parentMainForm.Preferences.Width >= this.widthSlider.Minimum && _parentMainForm.Preferences.Width <= this.widthSlider.Maximum)
                {
                    this.widthSlider.Value = _parentMainForm.Preferences.Width;
                }
                else
                {
                    this.widthSlider.Value = InstancePreferences.DefaultWidth;
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }


                // Load up the Outlook views.
                //_parentMainForm.OutlookNameSpace.get
                _dirty = false;
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
                _parentMainForm.Preferences.Opacity = opacityVal;
                _parentMainForm.Preferences.Left = this.horizontalPosition.Value;
                _parentMainForm.Preferences.Top = this.verticalPosition.Value;
                _parentMainForm.Preferences.Width = this.widthSlider.Value;
                _parentMainForm.Preferences.Height = this.heightSlider.Value;
                _dirty = false;
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

			msgResult = MessageBox.Show(this, Resources.CancelConfirmation, Resources.ConfirmationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

			if (msgResult == DialogResult.Yes) 
			{
				// restore settings
                _parentMainForm.Opacity = _parentMainForm.Preferences.Opacity;
                _parentMainForm.Left = _parentMainForm.Preferences.Left;
                _parentMainForm.Top = _parentMainForm.Preferences.Top;
                _parentMainForm.Height = _parentMainForm.Preferences.Height;
                _parentMainForm.Width = _parentMainForm.Preferences.Width;
				
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
            if (_dirty && !Confirm_Cancel())
            {
                e.Cancel = true;
            }
        }

        private void transparencySlider_Scroll(object sender, EventArgs e)
        {
            // update main form in real time so the user can gauage how they want it.
            double opacityVal = (double)this.transparencySlider.Value / 100;
            uxOpacityValue.Text = opacityVal.ToString();
            if (opacityVal == 1) { opacityVal = 0.99; }
            _parentMainForm.Opacity = opacityVal;
            _dirty = true;
        }

        private void widthSlider_Scroll(object sender, EventArgs e)
        {
            // update main form in real time so the user can gauage how they want it.
            _parentMainForm.Width = widthSlider.Value;
            uxWidthValue.Text = widthSlider.Value.ToString();
            _dirty = true;
            AdjustMinMaxDimensions();
            Debug.Print(_parentMainForm.Top + " " + _parentMainForm.Left);
        }

        private void heightSlider_Scroll(object sender, EventArgs e)
        {
            // update main form in real time so the user can gauage how they want it.
            _parentMainForm.Height = heightSlider.Value;
            uxHeightValue.Text = heightSlider.Value.ToString();
            _dirty = true;
            AdjustMinMaxDimensions();
            Debug.Print(_parentMainForm.Top + " " + _parentMainForm.Left);
        }

        private void horizontalPosition_Scroll(object sender, ScrollEventArgs e)
        {
            // update main form in real time so the user can gauage how they want it.
            _parentMainForm.Left = horizontalPosition.Value;
            _dirty = true;
            Debug.Print(_parentMainForm.Top + " " + _parentMainForm.Left);
        }

        private void verticalPosition_Scroll(object sender, ScrollEventArgs e)
        {
            // update main form in real time so the user can gauage how they want it.
            _parentMainForm.Top = verticalPosition.Value;
            _dirty = true;
            Debug.Print(_parentMainForm.Top + " " + _parentMainForm.Left);
        }
    }
}
