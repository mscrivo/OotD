using System;
using System.Windows.Forms;
using Microsoft.Win32;
using BitFactory.Logging;

namespace OutlookDesktop
{
    internal static class GlobalPreferences
    {
        // never instantiated, only contains static methods

        /// <summary>
        /// Returns true if there is a registry entry that makes Outlook on the Desktop start
        /// when Windows starts. On set, we save or delete that registry value
        /// accordingly.
        /// </summary>
        public static bool StartWithWindows
        {
            get
            {
                using (
                    RegistryKey key =
                        Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run"))
                {
                    if (key != null)
                    {
                        var val = (string)key.GetValue("OutlookOnDesktop");
                        return (!string.IsNullOrEmpty(val));
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
                        catch (Exception ex)
                        {
                            ConfigLogger.Instance.LogError(String.Format("Exception caught setting Start with Windows Key: {0}", ex));
                        }
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
                        bool isFirstRun;
                        if (bool.TryParse((string)key.GetValue("FirstRun", "true"), out isFirstRun))
                        {
                            if (isFirstRun)
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