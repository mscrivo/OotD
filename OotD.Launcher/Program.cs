// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using NLog;
using NLog.Targets;
using OotD.Properties;

namespace OotD
{
    public static class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // ReSharper disable once UnusedMember.Local
        private const string DebugArg = " -d";

        // ReSharper disable once ConvertToConstant.Local
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once RedundantDefaultMemberInitializer
        private static bool _isDebug = false;

        [STAThread]
        public static void Main(string[] args)
        {
            _logger.Info($"Command Line Args: {string.Join(" ", args)}");

#if DEBUG
            _isDebug = true;
#endif

            var bitness = ValidateOutlookInstallation();

            try
            {
                switch (bitness.ToLowerInvariant())
                {
                    case "x64":
                        {
                            var processStartInfo = new ProcessStartInfo("OotD.x64.exe");
                            processStartInfo = SetupRunCommand(processStartInfo, args);
                            Process.Start(processStartInfo);
                            break;
                        }
                    case "x86":
                        {
                            var processStartInfo = new ProcessStartInfo("OotD.x86.exe");
                            processStartInfo = SetupRunCommand(processStartInfo, args);
                            Process.Start(processStartInfo);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error starting child process.");
                MessageBox.Show(
                    string.Format(Resources.ChildProcessErrorMessage, GetLoggerFileName()),
                    Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private static ProcessStartInfo SetupRunCommand(ProcessStartInfo startInfo, string[] args)
        {
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            startInfo.LoadUserProfile = true;
            startInfo.Arguments = string.Join(" ", args);

            if (_isDebug)
            {
                startInfo.Arguments += DebugArg;
            }

            return startInfo;
        }


        /// <summary>
        /// Validates that the minimum supported version of Outlook is installed and returns the bitness of the installed version.
        /// </summary>
        /// <returns></returns>
        private static string ValidateOutlookInstallation()
        {
            var outlookFolder = string.Empty;
            double version = 0;

            // first make sure they have Office/Outlook 2000 (9.0) or higher installed by looking for 
            // the version subkeys in HKLM.
            using (var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office"))
            {
                if (key != null)
                {
                    var subKeys = key.GetSubKeyNames();

                    foreach (var subKey in subKeys)
                    {
                        _logger.Info($"Found {subKey} key");

                        if (double.TryParse(subKey, NumberStyles.Float, new NumberFormatInfo(), out var versionKey))
                        {
                            if (versionKey > version)
                            {
                                version = versionKey;
                            }
                        }
                    }
                }
            }

            switch (version)
            {
                case <= 0:
                    _logger.Info("Could not find Office key.");

                    MessageBox.Show(Resources.OutlookKeyNotFoundError, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return string.Empty;
                case < 14:
                    _logger.Debug("Outlook is not available or installed.");
                    MessageBox.Show(
                        Resources.Office2010Requirement + Environment.NewLine +
                        Resources.InstallOutlookMsg, Resources.MissingRequirementsCapation, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return string.Empty;
            }

            _logger.Info($"Office version {version} detected");

            // now check for the existence of the actual Outlook.exe.
            using (var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\OUTLOOK.EXE"))
            {
                if (key != null)
                {
                    outlookFolder = key.GetValue("Path")?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(outlookFolder))
                {
                    var fullPath = Path.Combine(outlookFolder, "Outlook.exe");

                    if (!File.Exists(fullPath))
                    {
                        _logger.Error($"Outlook executable not found at {fullPath}");
                        MessageBox.Show(Resources.OutlookExeNotFoundError, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return string.Empty;
                    }
                }
            }

            _logger.Info($"Outlook path reported as {outlookFolder} and Outlook.exe found in that path");

            if (string.IsNullOrEmpty(outlookFolder))
            {
                _logger.Error("Unable to find Outlook exe location in registry");
                MessageBox.Show(Resources.OutlookLocationKeyNotFoundError, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            // now check for bitness, if we can't find it with the latest version, try finding it under the previous 
            // version numbers.
            string? bitness = null;

            while (string.IsNullOrWhiteSpace(bitness) && version - 1 >= 14)
            {
                bitness = GetBitness(version);
                if (string.IsNullOrWhiteSpace(bitness))
                {
                    _logger.Info($"Could not find bitness key for Outlook under subkey {version}.0, trying {version - 1}.0");
                    version--;
                }
                else
                {
                    break;
                }
            }

            return bitness ?? "x86";
        }

        private static string? GetBitness(double versionSubKey)
        {
            var outlookKey = Registry.LocalMachine.OpenSubKey($"SOFTWARE\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");
            string? bitness = null;

            if (outlookKey != null)
            {
                bitness = outlookKey.GetValue("Bitness")?.ToString();
            }
            else
            {
                _logger.Info($"Unable to find key SOFTWARE\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");
                _logger.Info($"Trying SOFTWARE\\Wow6432Node\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");

                outlookKey = Registry.LocalMachine.OpenSubKey($"SOFTWARE\\Wow6432Node\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");

                if (outlookKey != null)
                {
                    bitness = outlookKey.GetValue("Bitness")?.ToString();
                }
            }

            if (outlookKey == null)
            {
                _logger.Info($"Unable to find key SOFTWARE\\Wow6432Node\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");
            }

            if (!string.IsNullOrWhiteSpace(bitness))
            {
                _logger.Info($"Outlook Bitness is: {bitness}");
            }

            return bitness;
        }

        private static string GetLoggerFileName()
        {
            var fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("f");
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            return fileTarget.FileName.Render(logEventInfo).Replace("/", "\\").Replace("\\\\", "\\");
        }
    }
}
