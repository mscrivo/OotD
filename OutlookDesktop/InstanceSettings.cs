using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OutlookDesktop.Properties;

namespace OutlookDesktop
{
    public partial class InstanceSettings : UserControl
    {

        /// <summary>
        /// The standard Logging object. Yay for logging!
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The instance whoes settings we are viewing and modifying. 
        /// </summary>
        private MainForm _instance;

        private bool _dirty;

        public InstanceSettings(MainForm passedInstance)
        {
            log.DebugFormat("Setting up control for {0}", passedInstance.InstanceName);

            log.Debug("Standard Init");
            InitializeComponent();


            _instance = passedInstance;

            log.Debug("Setting the max and min dimensions for this desktop");
            AdjustMinMaxDimensions();

            LoadSettings();
            InitializeComponent();
        }

        #region Private Methods
        /// <summary>
        /// Setup up mins and maxes for the dimensions of the window
        /// depending on the user's resolution.
        /// </summary>
        private void AdjustMinMaxDimensions()
        {
            verticalPosition.Minimum = SystemInformation.VirtualScreen.Top;
            verticalPosition.Maximum = SystemInformation.VirtualScreen.Top + SystemInformation.VirtualScreen.Height - _instance.Height + verticalPosition.SmallChange;
            log.DebugFormat("Vertical Min: {0}, Vertical Max: {1}", verticalPosition.Minimum, verticalPosition.Maximum);

            horizontalPosition.Minimum = SystemInformation.VirtualScreen.Left;
            horizontalPosition.Maximum = SystemInformation.VirtualScreen.Left + SystemInformation.VirtualScreen.Width - _instance.Width + horizontalPosition.SmallChange;
            log.DebugFormat("Horizontal Min: {0}, Horizontal Max: {1}", horizontalPosition.Minimum, horizontalPosition.Maximum);

            heightSlider.Minimum = SystemInformation.MinimumWindowSize.Height;
            heightSlider.Maximum = SystemInformation.VirtualScreen.Height;
            log.DebugFormat("Height Min: {0}, Height Max: {1}", heightSlider.Minimum, heightSlider.Maximum);

            widthSlider.Minimum = SystemInformation.MinimumWindowSize.Width;
            widthSlider.Maximum = SystemInformation.VirtualScreen.Width;
            log.DebugFormat("Width Min: {0}, Width Max: {1}", widthSlider.Minimum, widthSlider.Maximum);
        }


        /// <summary>
        /// Loads values from registry into their corresponding controls.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                log.DebugFormat("Setting Opacity: {0}", _instance.Preferences.Opacity);
                if (_instance.Preferences.Opacity >= 0 && _instance.Preferences.Opacity <= 1)
                {
                    this.transparencySlider.Value = (int)(_instance.Preferences.Opacity * 100);
                    uxOpacityValue.Text = _instance.Preferences.Opacity.ToString();
                }
                else
                {
                    this.transparencySlider.Value = (int)(InstancePreferences.DefaultOpacity * 100);
                    log.Error(Resources.ErrorSettingOpacity);
                    MessageBox.Show(this, Resources.ErrorSettingOpacity, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                log.DebugFormat("Setting Horizontal Position: {0}", _instance.Preferences.Opacity);
                if (_instance.Preferences.Left >= this.horizontalPosition.Minimum && _instance.Preferences.Left <= this.horizontalPosition.Maximum)
                {
                    this.horizontalPosition.Value = _instance.Preferences.Left;
                }
                else
                {
                    this.horizontalPosition.Value = InstancePreferences.DefaultLeftPosition;
                    log.Error(Resources.ErrorSettingDimensions);
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                log.DebugFormat("Setting Vertical Position: {0}", _instance.Preferences.Top);
                if (_instance.Preferences.Top >= this.verticalPosition.Minimum && _instance.Preferences.Top <= this.verticalPosition.Maximum)
                {
                    this.verticalPosition.Value = _instance.Preferences.Top;
                }
                else
                {
                    this.verticalPosition.Value = InstancePreferences.DefaultTopPosition;
                    log.Error(Resources.ErrorSettingDimensions);
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                log.DebugFormat("Setting Height: {0}", _instance.Preferences.Top);
                if (_instance.Preferences.Height >= this.heightSlider.Minimum && _instance.Preferences.Height <= this.heightSlider.Maximum)
                {
                    this.heightSlider.Value = _instance.Preferences.Height;
                }
                else
                {
                    this.heightSlider.Value = InstancePreferences.DefaultHeight;
                    log.Error(Resources.ErrorSettingDimensions);
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                log.DebugFormat("Setting Width Position: {0}", _instance.Preferences.Top);
                if (_instance.Preferences.Width >= this.widthSlider.Minimum && _instance.Preferences.Width <= this.widthSlider.Maximum)
                {
                    this.widthSlider.Value = _instance.Preferences.Width;
                }
                else
                {
                    this.widthSlider.Value = InstancePreferences.DefaultWidth;
                    log.Error(Resources.ErrorSettingDimensions);
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                log.Debug("Setting Folder Selection");

                uxDefaultFolderTypes.Text = _instance.Preferences.OutlookFolderName;


                _dirty = false;
            }
            catch (Exception ex)
            {
                log.Error(Resources.ErrorCaption, ex);
                MessageBox.Show(this, ex.Message, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
                double opacityVal;
                opacityVal = (double)this.transparencySlider.Value / 100;
                if (opacityVal == 1) { opacityVal = 0.99; }

                log.DebugFormat("Saving Settings: Opacity - {0}, Left - {1}, Top - {2}, Width - {3}, Height - {4}", new Object[] { opacityVal, horizontalPosition.Value, verticalPosition.Value, widthSlider.Value, heightSlider.Value });

                _instance.Preferences.Opacity = opacityVal;
                _instance.Preferences.Left = this.horizontalPosition.Value;
                _instance.Preferences.Top = this.verticalPosition.Value;
                _instance.Preferences.Width = this.widthSlider.Value;
                _instance.Preferences.Height = this.heightSlider.Value;
                _dirty = false;
            }
            catch (Exception ex)
            {
                log.Error("Error saving settings", ex);
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
                log.DebugFormat("Restore Settings: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Preferences.Opacity, _instance.Preferences.Left, _instance.Preferences.Top, _instance.Preferences.Height, _instance.Preferences.Width });
                _instance.Opacity = _instance.Preferences.Opacity;
                _instance.Left = _instance.Preferences.Left;
                _instance.Top = _instance.Preferences.Top;
                _instance.Height = _instance.Preferences.Height;
                _instance.Width = _instance.Preferences.Width;

                return true;
            }
            else
            {
                return false;
            }
        }


        private void ChangeOpacity()
        {
            // update main form in real time so the user can gauage how they want it.
            double opacityVal = (double)this.transparencySlider.Value / 100;
            uxOpacityValue.Text = opacityVal.ToString();
            if (opacityVal == 1) { opacityVal = 0.99; }
            _instance.Opacity = opacityVal;
            _dirty = true;
            log.DebugFormat("Settings Change: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Opacity, _instance.Left, _instance.Top, _instance.Height, _instance.Width });

        }

        private void Changewidth()
        {
            // update main form in real time so the user can gauage how they want it.
            _instance.Width = widthSlider.Value;
            uxWidthValue.Text = widthSlider.Value.ToString();
            _dirty = true;
            AdjustMinMaxDimensions();
            log.DebugFormat("Settings Change: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Opacity, _instance.Left, _instance.Top, _instance.Height, _instance.Width });
        }

        private void ChangeHeight()
        {
            // update main form in real time so the user can gauage how they want it.
            _instance.Height = heightSlider.Value;
            uxHeightValue.Text = heightSlider.Value.ToString();
            _dirty = true;
            AdjustMinMaxDimensions();
            log.DebugFormat("Settings Change: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Opacity, _instance.Left, _instance.Top, _instance.Height, _instance.Width });
        }

        private void ChangeLeft()
        {
            // update main form in real time so the user can gauage how they want it.
            _instance.Left = horizontalPosition.Value;
            _dirty = true;
            log.DebugFormat("Settings Change: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Opacity, _instance.Left, _instance.Top, _instance.Height, _instance.Width });
        }

        private void ChangeTop()
        {
            // update main form in real time so the user can gauage how they want it.
            _instance.Top = verticalPosition.Value;
            _dirty = true;
            log.DebugFormat("Settings Change: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Opacity, _instance.Left, _instance.Top, _instance.Height, _instance.Width });
        }


        #endregion

        #region Event Handlers

        private void preferencesLabel_Click(object sender, EventArgs e)
        {

        }

        private void uxSaveSettings_Click(object sender, EventArgs e)
        {
            if (!Save_Settings())
            {
                log.Error(Resources.ErrorSavingSettings);
                MessageBox.Show(this, Resources.ErrorSavingSettings, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

        }

        private void uxCancelSettingsChange_Click(object sender, EventArgs e)
        {

        }

        private void transparencySlider_Scroll(object sender, EventArgs e)
        {
            ChangeOpacity();
        }

        private void widthSlider_Scroll(object sender, EventArgs e)
        {
            Changewidth();
        }

        private void heightSlider_Scroll(object sender, EventArgs e)
        {
            ChangeHeight();
        }

        private void verticalPosition_Scroll(object sender, ScrollEventArgs e)
        {
            ChangeTop();
        }

        private void horizontalPosition_Scroll(object sender, ScrollEventArgs e)
        {
            ChangeLeft();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void uxSelectCustomFolder_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Outlook.MAPIFolder oFolder = _instance.OutlookNameSpace.PickFolder();
            if (oFolder != null)
            {
                log.DebugFormat("Updating Folder to {0}",oFolder.Name);
                _instance.UpdateCustomFolder(oFolder);

                uxDefaultFolderTypes.Text = _instance.Preferences.OutlookFolderName;

                uxFolderViews.Items.AddRange(_instance.OulookFolderViews.ToArray());
            }
        }

        private void uxDefaultFolderTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Custom folder will not exist as a dropdown so we are selecting a existing folder. 
            string selectedFolder = uxDefaultFolderTypes.SelectedValue as String;

            if (selectedFolder == null)
            {
                log.Error("Selected folder is a null, not good.");
                return;
            }

            _instance.UpdateDefaultFolder(selectedFolder);

            uxFolderViews.Items.AddRange(_instance.OulookFolderViews.ToArray());
        }
    
        #endregion
    }
}
