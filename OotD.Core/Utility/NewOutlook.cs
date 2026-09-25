// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace OotD.Utility;

/// <summary>
///     Detects the new Outlook for Windows, which OotD can't work with because it doesn't provide the
///     Outlook View Control or COM automation that classic Outlook does.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Thin registry and file system reads.")]
public static class NewOutlook
{
    private const string PackagesKey =
        @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages";

    private const string PackageNamePrefix = "Microsoft.OutlookForWindows_";

    /// <summary>
    ///     True when the new Outlook for Windows (olk.exe) is installed for the current user.
    /// </summary>
    public static bool IsInstalled()
    {
        try
        {
            var alias = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft", "WindowsApps", "olk.exe");
            if (File.Exists(alias))
            {
                return true;
            }

            using var packages = Registry.CurrentUser.OpenSubKey(PackagesKey);
            return packages?.GetSubKeyNames()
                .Any(name => name.StartsWith(PackageNamePrefix, StringComparison.OrdinalIgnoreCase)) == true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     True when classic Outlook's "New Outlook" toggle is switched on, which redirects Outlook to the
    ///     new Outlook for Windows.
    /// </summary>
    public static bool IsEnabled()
    {
        try
        {
            using var preferences =
                Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\16.0\Outlook\Preferences");
            return preferences?.GetValue("UseNewOutlook") is int useNewOutlook && useNewOutlook != 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
