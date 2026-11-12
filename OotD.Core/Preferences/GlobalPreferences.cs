using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Win32;
using NLog;

namespace OotD.Preferences;

internal static class GlobalPreferences
{
    // never instantiated, only contains static methods

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static bool? _isFirstRun;

    /// <summary>
    ///     Returns true if there is a registry entry that makes Outlook on the Desktop start
    ///     when Windows starts. On set, we save or delete that registry value
    ///     accordingly.
    /// </summary>
    public static bool StartWithWindows
    {
        get => TaskScheduling.OotDScheduledTaskExists();
        set
        {
            try
            {
                if (value)
                {
                    TaskScheduling.CreateOotDStartupTask(_logger);
                }
                else
                {
                    TaskScheduling.RemoveOotDStartupTask(_logger);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Tasks registered by an elevated installer before the ACL fix can only be changed by an admin.
                RunElevated(value ? "--createStartupEntry" : "--removeStartupEntry");
            }
        }
    }

    internal static Action<string> RunElevated { get; set; } = args =>
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(Environment.ProcessPath!, args)
            {
                UseShellExecute = true,
                Verb = "runas"
            });
            process?.WaitForExit();
        }
        catch (Win32Exception e) when (e.NativeErrorCode == 1223) // ERROR_CANCELLED: UAC prompt declined
        {
            _logger.Info("Elevation declined; startup task left unchanged");
        }
    };

    public static bool LockPosition
    {
        get
        {
            using var key = Registry.CurrentUser.CreateSubKey(PreferencesRegistry.RootPath);
            return bool.TryParse(key.GetValue("LockPosition", "false").ToString(), out var lockPositions) &&
                   lockPositions;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(PreferencesRegistry.RootPath);
            key.SetValue("LockPosition", value);
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

            using (var key = Registry.CurrentUser.CreateSubKey(PreferencesRegistry.RootPath))
            {
                if (bool.TryParse(key.GetValue("FirstRun", "true").ToString(), out var isFirstRun))
                {
                    if (isFirstRun)
                    {
                        key.SetValue("FirstRun", false);
                        _isFirstRun = true;
                        return _isFirstRun.Value;
                    }
                }
            }

            _isFirstRun = false;

            return _isFirstRun.Value;
        }
    }
}
