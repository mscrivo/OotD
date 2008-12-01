using System;
using System.Windows.Forms;
using Microsoft.Win32;
using OutlookDesktop.Properties;
using System.Globalization;

namespace OutlookDesktop
{
    public class InstancePreferences
    {
        public const double DefaultOpacity = 0.5;
        public const int DefaultLeftPosition = 100;
        public const int DefaultTopPosition = 100;
        public const int DefaultHeight = 500;
        public const int DefaultWidth = 700;

        private RegistryKey appReg;

        public InstancePreferences(String instanceName)
        {
            try
            {
                appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + instanceName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        ~InstancePreferences()
        {
            appReg.Close();
        }

        /// <summary>
        /// Main Window Opacity.
        /// </summary>
        public double Opacity
        {
            get
            {
                double opacity = DefaultOpacity;

                if (double.TryParse((string)appReg.GetValue("Opacity", opacity.ToString("G", CultureInfo.CurrentCulture)), out opacity))
                    return opacity;
                else
                    return DefaultOpacity;
            }
            set
            {
                appReg.SetValue("Opacity", value);
            }
        }

        /// <summary>
        /// Main Window Left.
        /// </summary>
        public int Left
        {
            get
            {
                return (int)appReg.GetValue("Left", DefaultLeftPosition);
            }
            set
            {
                appReg.SetValue("Left", value);
            }
        }

        /// <summary>
        /// Main Window Top.
        /// </summary>
        public int Top
        {
            get
            {
                return (int)appReg.GetValue("Top", DefaultTopPosition);
            }
            set
            {
                appReg.SetValue("Top", value);
            }
        }

        /// <summary>
        /// Main Window Width.
        /// </summary>
        public int Width
        {
            get
            {
                return (int)appReg.GetValue("Width", DefaultWidth);
            }
            set
            {
                appReg.SetValue("Width", value);
            }
        }

        /// <summary>
        /// Main Window Height.
        /// </summary>
        public int Height
        {
            get
            {
                return (int)appReg.GetValue("Height", DefaultHeight);
            }
            set
            {
                appReg.SetValue("Height", value);
            }
        }

        public string OutlookFolderName
        {
            get
            {
                return (string)appReg.GetValue("CurrentViewType", "Calendar");
            }
            set
            {
                appReg.SetValue("CurrentViewType", value);
            }
        }

        public string OutlookFolderView
        {
            get
            {
                return (string)appReg.GetValue("OutlookView", "Day/Week/Month");
            }
            set
            {
                appReg.SetValue("OutlookView", value);
            }
        }


        public string OutlookFolderEntryId
        {
            get
            {
                return (string)appReg.GetValue("FolderEntryId", "");
            }
            set
            {
                appReg.SetValue("FolderEntryId", value);
            }
        }


        public string OutlookFolderStoreId
        {
            get
            {
                return (string)appReg.GetValue("FolderStoreId", "");
            }
            set
            {
                appReg.SetValue("FolderStoreId", value);
            }
        }
    
    }
}
