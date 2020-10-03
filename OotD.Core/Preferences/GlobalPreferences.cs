// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32;
using NLog;
using System.Windows.Forms;

namespace OotD.Preferences
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
            get => TaskScheduling.OotDScheduledTaskExists();
            set
            {
                if (value)
                {
                    TaskScheduling.CreateOotDStartupTask(Logger);
                }
                else
                {
                    TaskScheduling.RemoveOotDStartupTask(Logger);
                }
            }
        }

        public static bool LockPosition
        {
            get
            {
                using var key = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName);
                if (key != null)
                {
                    if (bool.TryParse(key.GetValue("LockPosition", "false")?.ToString(), out var lockPositions) &&
                        lockPositions)
                    {
                        return true;
                    }
                }

                return false;
            }
            set
            {
                using var key = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName);
                key?.SetValue("LockPosition", value);
            }
        }

        public static bool IsFirstRun
        {
            get
            {
                if (_isFirstRun.HasValue)
                {
                    return _isFirstRun.Value;
                }

                using (var key = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
                {
                    if (key != null)
                    {
                        if (bool.TryParse(key.GetValue("FirstRun", "true")?.ToString(), out var isFirstRun))
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
