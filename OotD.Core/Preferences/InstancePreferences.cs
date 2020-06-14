using System;
using Microsoft.Win32;
using System.Globalization;
using System.Windows.Forms;

namespace OotD.Preferences
{
    public class InstancePreferences
    {
        public const int DefaultHeight = 500;
        public const int DefaultLeftPosition = 100;
        public const double DefaultOpacity = 0.5;
        public const int DefaultTopPosition = 100;
        public const int DefaultWidth = 700;

        private readonly RegistryKey _appReg;

        public InstancePreferences(string instanceName)
        {
            _appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + instanceName) ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// Main Window Opacity.
        /// </summary>
        public double Opacity
        {
            get
            {
                var opacity = DefaultOpacity;

                if (
                    double.TryParse((string)_appReg.GetValue("Opacity", opacity.ToString("G", CultureInfo.CurrentCulture)), out opacity))
                {
                    return opacity;
                }

                return DefaultOpacity;
            }
            set => _appReg.SetValue("Opacity", value);
        }

        /// <summary>
        /// Main Window Left.
        /// </summary>
        public int Left
        {
            get => (int)_appReg.GetValue("Left", DefaultLeftPosition);
            set => _appReg.SetValue("Left", value);
        }

        /// <summary>
        /// Main Window Top.
        /// </summary>
        public int Top
        {
            get => (int)_appReg.GetValue("Top", DefaultTopPosition);
            set => _appReg.SetValue("Top", value);
        }

        /// <summary>
        /// Main Window Width.
        /// </summary>
        public int Width
        {
            get => (int)_appReg.GetValue("Width", DefaultWidth);
            set => _appReg.SetValue("Width", value);
        }

        /// <summary>
        /// Main Window Height.
        /// </summary>
        public int Height
        {
            get => (int)_appReg.GetValue("Height", DefaultHeight);
            set => _appReg.SetValue("Height", value);
        }

        public string? OutlookFolderName
        {
            get => (string)_appReg.GetValue("CurrentViewType", "Calendar");
            set
            {
                if (value != null)
                {
                    _appReg.SetValue("CurrentViewType", value);
                }

                OutlookFolderView = string.Empty;
            }
        }

        public string? OutlookFolderView
        {
            get => (string)_appReg.GetValue("OutlookView", "Day/Week/Month");
            set
            {
                if (value != null)
                {
                    _appReg.SetValue("OutlookView", value);
                }
            }
        }


        public string? OutlookFolderEntryId
        {
            get => (string)_appReg.GetValue("FolderEntryId", "");
            set
            {
                if (value != null)
                {
                    _appReg.SetValue("FolderEntryId", value);
                }
            }
        }


        public string? OutlookFolderStoreId
        {
            get => (string)_appReg.GetValue("FolderStoreId", "");
            set
            {
                if (value != null)
                {
                    _appReg.SetValue("FolderStoreId", value);
                }
            }
        }

        public bool DisableEditing
        {
            get
            {
                bool.TryParse(_appReg.GetValue("DisableEditing", "False").ToString(), out var retVal);
                return retVal;
            }
            set => _appReg.SetValue("DisableEditing", value);
        }

        public string ViewXml
        {
            get => (string)_appReg.GetValue("ViewXML", "");
            set => _appReg.SetValue("ViewXML", value);
        }

        ~InstancePreferences()
        {
            _appReg.Close();
        }
    }
}
