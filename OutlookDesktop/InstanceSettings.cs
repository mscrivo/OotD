using System;
using System.Windows.Forms;

using OutlookDesktop.Properties;

namespace OutlookDesktop
{
    public partial class InstanceSettings : UserControl
    {

        /// <summary>
        /// The standard Logging object. Yay for logging!
        /// </summary>
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The instance whoes settings we are viewing and modifying. 
        /// </summary>
        private readonly MainForm _instance;

        public InstanceSettings(MainForm passedInstance)
        {
            Log.DebugFormat("Setting up control for {0}", passedInstance.InstanceName);

            Log.Debug("Standard Init");
            InitializeComponent();

            _instance = passedInstance;

            Log.Debug("Setting the max and min dimensions for this desktop");
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
            Log.DebugFormat("Vertical Min: {0}, Vertical Max: {1}", verticalPosition.Minimum, verticalPosition.Maximum);

            horizontalPosition.Minimum = SystemInformation.VirtualScreen.Left;
            horizontalPosition.Maximum = SystemInformation.VirtualScreen.Left + SystemInformation.VirtualScreen.Width - _instance.Width + horizontalPosition.SmallChange;
            Log.DebugFormat("Horizontal Min: {0}, Horizontal Max: {1}", horizontalPosition.Minimum, horizontalPosition.Maximum);

            heightSlider.Minimum = SystemInformation.MinimumWindowSize.Height;
            heightSlider.Maximum = SystemInformation.VirtualScreen.Height;
            Log.DebugFormat("Height Min: {0}, Height Max: {1}", heightSlider.Minimum, heightSlider.Maximum);

            widthSlider.Minimum = SystemInformation.MinimumWindowSize.Width;
            widthSlider.Maximum = SystemInformation.VirtualScreen.Width;
            Log.DebugFormat("Width Min: {0}, Width Max: {1}", widthSlider.Minimum, widthSlider.Maximum);
        }


        /// <summary>
        /// Loads values from registry into their corresponding controls.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                Log.DebugFormat("Setting Opacity: {0}", _instance.Preferences.Opacity);
                if (_instance.Preferences.Opacity >= 0 && _instance.Preferences.Opacity <= 1)
                {
                    transparencySlider.Value = (int)(_instance.Preferences.Opacity * 100);
                    uxOpacityValue.Text = _instance.Preferences.Opacity.ToString();
                }
                else
                {
                    transparencySlider.Value = (int)(InstancePreferences.DefaultOpacity * 100);
                    Log.Error(Resources.ErrorSettingOpacity);
                    MessageBox.Show(this, Resources.ErrorSettingOpacity, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                Log.DebugFormat("Setting Horizontal Position: {0}", _instance.Preferences.Opacity);
                if (_instance.Preferences.Left >= horizontalPosition.Minimum && _instance.Preferences.Left <= horizontalPosition.Maximum)
                {
                    horizontalPosition.Value = _instance.Preferences.Left;
                }
                else
                {
                    horizontalPosition.Value = InstancePreferences.DefaultLeftPosition;
                    Log.Error(Resources.ErrorSettingDimensions);
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                Log.DebugFormat("Setting Vertical Position: {0}", _instance.Preferences.Top);
                if (_instance.Preferences.Top >= verticalPosition.Minimum && _instance.Preferences.Top <= verticalPosition.Maximum)
                {
                    verticalPosition.Value = _instance.Preferences.Top;
                }
                else
                {
                    verticalPosition.Value = InstancePreferences.DefaultTopPosition;
                    Log.Error(Resources.ErrorSettingDimensions);
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                Log.DebugFormat("Setting Height: {0}", _instance.Preferences.Top);
                if (_instance.Preferences.Height >= heightSlider.Minimum && _instance.Preferences.Height <= heightSlider.Maximum)
                {
                    heightSlider.Value = _instance.Preferences.Height;
                }
                else
                {
                    heightSlider.Value = InstancePreferences.DefaultHeight;
                    Log.Error(Resources.ErrorSettingDimensions);
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                Log.DebugFormat("Setting Width Position: {0}", _instance.Preferences.Top);
                if (_instance.Preferences.Width >= widthSlider.Minimum && _instance.Preferences.Width <= widthSlider.Maximum)
                {
                    widthSlider.Value = _instance.Preferences.Width;
                }
                else
                {
                    widthSlider.Value = InstancePreferences.DefaultWidth;
                    Log.Error(Resources.ErrorSettingDimensions);
                    MessageBox.Show(this, Resources.ErrorSettingDimensions, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                Log.Debug("Setting Folder Selection");

                uxDefaultFolderTypes.Text = _instance.Preferences.OutlookFolderName;
            }
            catch (Exception ex)
            {
                Log.Error(Resources.ErrorCaption, ex);
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
                double opacityVal = (double)this.transparencySlider.Value / 100;
                if (opacityVal == 1) { opacityVal = 0.99; }

                Log.DebugFormat("Saving Settings: Opacity - {0}, Left - {1}, Top - {2}, Width - {3}, Height - {4}", new Object[] { opacityVal, horizontalPosition.Value, verticalPosition.Value, widthSlider.Value, heightSlider.Value });

                _instance.Preferences.Opacity = opacityVal;
                _instance.Preferences.Left = horizontalPosition.Value;
                _instance.Preferences.Top = verticalPosition.Value;
                _instance.Preferences.Width = widthSlider.Value;
                _instance.Preferences.Height = heightSlider.Value;
            }
            catch (Exception ex)
            {
                Log.Error("Error saving settings", ex);
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
            DialogResult msgResult = MessageBox.Show(this, Resources.CancelConfirmation, Resources.ConfirmationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

            if (msgResult == DialogResult.Yes)
            {
                // restore settings
                Log.DebugFormat("Restore Settings: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Preferences.Opacity, _instance.Preferences.Left, _instance.Preferences.Top, _instance.Preferences.Height, _instance.Preferences.Width });
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
            double opacityVal = (double)transparencySlider.Value / 100;
            uxOpacityValue.Text = opacityVal.ToString();
            if (opacityVal == 1) { opacityVal = 0.99; }
            _instance.Opacity = opacityVal;
            Log.DebugFormat("Settings Change: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Opacity, _instance.Left, _instance.Top, _instance.Height, _instance.Width });

        }

        private void Changewidth()
        {
            // update main form in real time so the user can gauage how they want it.
            _instance.Width = widthSlider.Value;
            uxWidthValue.Text = widthSlider.Value.ToString();
            AdjustMinMaxDimensions();
            Log.DebugFormat("Settings Change: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Opacity, _instance.Left, _instance.Top, _instance.Height, _instance.Width });
        }

        private void ChangeHeight()
        {
            // update main form in real time so the user can gauage how they want it.
            _instance.Height = heightSlider.Value;
            uxHeightValue.Text = heightSlider.Value.ToString();
            AdjustMinMaxDimensions();
            Log.DebugFormat("Settings Change: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Opacity, _instance.Left, _instance.Top, _instance.Height, _instance.Width });
        }

        private void ChangeLeft()
        {
            // update main form in real time so the user can gauage how they want it.
            _instance.Left = horizontalPosition.Value;
            Log.DebugFormat("Settings Change: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Opacity, _instance.Left, _instance.Top, _instance.Height, _instance.Width });
        }

        private void ChangeTop()
        {
            // update main form in real time so the user can gauage how they want it.
            _instance.Top = verticalPosition.Value;
            Log.DebugFormat("Settings Change: Opacity - {0}, Left - {1}, Top - {2}, Height - {3}, Width - {4}", new Object[] { _instance.Opacity, _instance.Left, _instance.Top, _instance.Height, _instance.Width });
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
                Log.Error(Resources.ErrorSavingSettings);
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
                Log.DebugFormat("Updating Folder to {0}",oFolder.Name);
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
                Log.Error("Selected folder is a null, not good.");
                return;
            }

            _instance.UpdateDefaultFolder(selectedFolder);

            uxFolderViews.Items.AddRange(_instance.OulookFolderViews.ToArray());
        }
    
        #endregion
    }
}
