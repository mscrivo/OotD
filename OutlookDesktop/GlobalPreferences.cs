using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using OutlookDesktop.Properties;
using System.Globalization;

namespace OutlookDesktop
{
    class GlobalPreferences
    {
        // never instantiated, only contains static methods
        private GlobalPreferences()
        {
        }

        /// <summary>
        /// Returns true if there is a registry entry that makes Outlook on the Desktop start
        /// when Windows starts. On set, we save or delete that registry value
        /// accordingly.
        /// </summary>
        public static bool StartWithWindows
        {
            get
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run"))
                {
                    if (key != null)
                    {
                        string val = (string)key.GetValue("OutlookOnDesktop");
                        return (val != null && val.Length > 0);
                    }
                }
                return false;
            }
            set
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        try
                        {
                            if (value)
                            {
                                key.SetValue("OutlookOnDesktop", Application.ExecutablePath);
                            }
                            else
                            {
                                if (key.GetValue("OutlookOnDesktop") != null)
                                    key.DeleteValue("OutlookOnDesktop");
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
        }


        public static bool IsFirstRun
        {
            get
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
                {
                    if (key != null)
                    {
                        bool _isFirstRun;
                        if (bool.TryParse((string)key.GetValue("FirstRun", "true"), out _isFirstRun))
                        {
                            if (_isFirstRun)
                            {
                                key.SetValue("FirstRun", false);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }
    }
}
