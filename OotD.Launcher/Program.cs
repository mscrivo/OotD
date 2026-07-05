// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using NLog;
using NLog.Targets;
using OotD.Properties;
using OotD.Utility;

namespace OotD;

public static class Program
{
    // ReSharper disable once UnusedMember.Local
    private const string DebugArg = " -d";
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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

        var installation = OutlookInstallationDetector.Detect(new RegistryOutlookEnvironment(_logger));

        if (!installation.IsUsable)
        {
            ShowInstallationError(installation.Error);
            return;
        }

        var exeName = installation.Bitness.Equals("x64", StringComparison.OrdinalIgnoreCase)
            ? "OotD.x64.exe"
            : "OotD.x86.exe";

        try
        {
            var processStartInfo = SetupRunCommand(new ProcessStartInfo(exeName), args);
            Process.Start(processStartInfo);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error starting child process.");
            MessageBox.Show(
                string.Format(Resources.ChildProcessErrorMessage, GetLoggerFileName()),
                Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void ShowInstallationError(OutlookDetectionError error)
    {
        switch (error)
        {
            case OutlookDetectionError.OfficeNotInstalled:
                _logger.Info("Could not find Office key.");
                MessageBox.Show(Resources.OutlookKeyNotFoundError, Resources.ErrorCaption, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                break;
            case OutlookDetectionError.UnsupportedVersion:
                _logger.Debug("Outlook is not available or installed.");
                MessageBox.Show(
                    Resources.Office2010Requirement + Environment.NewLine + Resources.InstallOutlookMsg,
                    Resources.MissingRequirementsCapation, MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;
            case OutlookDetectionError.OutlookExecutableNotFound:
                _logger.Error("Outlook executable not found in the reported install path.");
                MessageBox.Show(Resources.OutlookExeNotFoundError, Resources.ErrorCaption, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                break;
            case OutlookDetectionError.OutlookLocationNotFound:
                _logger.Error("Unable to find Outlook exe location in registry");
                MessageBox.Show(Resources.OutlookLocationKeyNotFoundError, Resources.ErrorCaption,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;
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
    ///     Reads the machine registry and file system to locate Outlook. All of the decision logic
    ///     lives in <see cref="OutlookInstallationDetector" />; this just supplies the raw reads.
    /// </summary>
    private sealed class RegistryOutlookEnvironment(Logger logger) : IOutlookEnvironment
    {
        public IReadOnlyList<double> GetInstalledOfficeVersions()
        {
            var versions = new List<double>();

            using var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office");
            if (key == null)
            {
                return versions;
            }

            foreach (var subKey in key.GetSubKeyNames())
            {
                logger.Info($"Found {subKey} key");

                if (double.TryParse(subKey, NumberStyles.Float, new NumberFormatInfo(), out var versionKey))
                {
                    versions.Add(versionKey);
                }
            }

            return versions;
        }

        public string? GetOutlookInstallPath()
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\OUTLOOK.EXE");
            return key?.GetValue("Path")?.ToString();
        }

        public bool OutlookExecutableExists(string installPath)
        {
            var fullPath = Path.Combine(installPath, "Outlook.exe");
            var exists = File.Exists(fullPath);
            if (!exists)
            {
                logger.Error($"Outlook executable not found at {fullPath}");
            }

            return exists;
        }

        public string? GetBitness(double officeVersion)
        {
            using var outlookKey = Registry.LocalMachine.OpenSubKey(
                                       $"SOFTWARE\\Microsoft\\Office\\{officeVersion}.0\\Outlook")
                                   ?? OpenWowOutlookKey(officeVersion);

            var bitness = outlookKey?.GetValue("Bitness")?.ToString();

            if (!string.IsNullOrWhiteSpace(bitness))
            {
                logger.Info($"Outlook Bitness is: {bitness}");
            }

            return bitness;
        }

        private RegistryKey? OpenWowOutlookKey(double officeVersion)
        {
            logger.Info($"Trying SOFTWARE\\Wow6432Node\\Microsoft\\Office\\{officeVersion}.0\\Outlook");
            return Registry.LocalMachine.OpenSubKey(
                $"SOFTWARE\\Wow6432Node\\Microsoft\\Office\\{officeVersion}.0\\Outlook");
        }
    }

    private static string GetLoggerFileName()
    {
        if (LogManager.Configuration?.FindTargetByName("f") is not FileTarget fileTarget)
        {
            throw new InvalidOperationException("Could not find file logging target");
        }
        var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
        return fileTarget.FileName.Render(logEventInfo).Replace("/", "\\").Replace("\\\\", "\\");
    }
}
