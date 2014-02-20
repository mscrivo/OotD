using System;
using System.Windows.Forms;
using Microsoft.Win32;
using NLog;

namespace OutlookDesktop.Preferences
{
    internal static class GlobalPreferences
    {
        // never instantiated, only contains static methods

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                    var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run"))
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
                using (var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
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
                            Logger.ErrorException("Exception caught setting Start with Windows Key", ex);
                        }
                    }
                }
            }
        }

        public static bool LockPosition
        {
            get
            {
                using (var key = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
                {
                    if (key != null)
                    {
                        bool lockPositions;
                        if (bool.TryParse((string)key.GetValue("LockPosition", "false"), out lockPositions) &&
                            lockPositions)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
                {
                    if (key != null)
                    {
                        key.SetValue("LockPosition", value);
                    }
                }
            }
        }

        public static bool IsFirstRun
        {
            get
            {
                if (_isFirstRun.HasValue) return _isFirstRun.Value;

                using (var key = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
                {
                    if (key != null)
                    {
                        bool isFirstRun;
                        if (bool.TryParse((string)key.GetValue("FirstRun", "true"), out isFirstRun))
                        {
                            if (isFirstRun)
                            {
                                key.SetValue("FirstRun", false);
                                _isFirstRun = true;
                                return _isFirstRun.Value;
                            }
                        }
                    }
                }
                _isFirstRun = false;

                return _isFirstRun.Value;
            }            
        }
        private static bool? _isFirstRun;
    }
}