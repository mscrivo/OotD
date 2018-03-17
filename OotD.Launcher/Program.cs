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
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // ReSharper disable once UnusedMember.Local
        private const string debugArg = " -d";

        // ReSharper disable once ConvertToConstant.Local
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static bool IsDebug = false;

        public static void Main(string[] args)
        {
            Logger.Info($"Command Line Args: {string.Join(" ", args)}");

#if DEBUG
            IsDebug = true;
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
                Logger.Error(e, "Error starting child process.");
                MessageBox.Show(
                    string.Format(Resources.ChildProcessErrorMessage, GetLoggerFileName()),
                    Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private static ProcessStartInfo SetupRunCommand(ProcessStartInfo startinfo, string[] args)
        {
            startinfo.CreateNoWindow = true;
            startinfo.WindowStyle = ProcessWindowStyle.Hidden;
            startinfo.UseShellExecute = true;
            startinfo.WorkingDirectory = Directory.GetCurrentDirectory();
            startinfo.Arguments = string.Join(" ", args);

            if (IsDebug)
            {
                startinfo.Arguments += debugArg;
            }

            return startinfo;
        }


        /// <summary>
        /// Validates that the minimum supported version of Outlook is installed and returns the bitness of the installed version.
        /// </summary>
        /// <returns></returns>
        private static string ValidateOutlookInstallation()
        {
            var outlookFolder = string.Empty;
            double versionSubKey = 0;

            // first make sure they have Office/Outlook 2000 (9.0) or higher installed by looking for 
            // the version subkeys in HKLM.
            using (var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office"))
            {
                if (key != null)
                {
                    string[] subkeys = key.GetSubKeyNames();

                    foreach (string subkey in subkeys)
                    {
                        Logger.Info($"Found {subkey} key");

                        if (double.TryParse(subkey, NumberStyles.Float, new NumberFormatInfo(), out var version))
                        {
                            if (version >= 11 || version > versionSubKey)
                            {
                                versionSubKey = version;
                            }
                        }
                    }
                }
            }

            if (versionSubKey <= 0)
            {
                Logger.Info("Could not find Office key.");

                MessageBox.Show(Resources.OutlookKeyNotFoundError, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return string.Empty;
            }

            if (versionSubKey < 14)
            {
                Logger.Debug("Outlook is not available or installed.");
                MessageBox.Show(
                    Resources.Office2010Requirement + Environment.NewLine +
                    Resources.InstallOutlookMsg, Resources.MissingRequirementsCapation, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return string.Empty;
            }

            Logger.Info($"Office version {versionSubKey} detected");

            // now check for the existence of the actual Outlook.exe.
            using (var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\OUTLOOK.EXE"))
            {
                if (key != null) outlookFolder = (string)key.GetValue("Path");
                if (outlookFolder != null)
                {
                    var fullPath = Path.Combine(outlookFolder, "Outlook.exe");

                    if (!File.Exists(fullPath))
                    {
                        Logger.Error($"Outlook executable not found at {fullPath}");
                        MessageBox.Show(Resources.OutlookExeNotFoundError, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return string.Empty;
                    }
                }
            }

            Logger.Info($"Outlook path reported as {outlookFolder} and Outlook.exe found in that path");

            if (string.IsNullOrEmpty(outlookFolder))
            {
                Logger.Error("Unable to find Outlook exe location in registry");
                MessageBox.Show(Resources.OutlookLocationKeyNotFoundError, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            // now check for bitness
            var outlookKey = Registry.LocalMachine.OpenSubKey($"SOFTWARE\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");
            string bitness = null;

            if (outlookKey != null)
            {
                bitness = (string)outlookKey.GetValue("Bitness");
            }
            else
            {
                Logger.Info($"Unable to find key SOFTWARE\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");
                Logger.Info($"Trying SOFTWARE\\Wow6432Node\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");

                outlookKey = Registry.LocalMachine.OpenSubKey($"SOFTWARE\\Wow6432Node\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");

                if (outlookKey != null)
                {
                    bitness = (string)outlookKey.GetValue("Bitness");
                }
            }

            if (outlookKey == null)
            {
                Logger.Info($"Unable to find key SOFTWARE\\Wow6432Node\\Microsoft\\Office\\{versionSubKey}.0\\Outlook");
            }
            else if (!string.IsNullOrWhiteSpace(bitness))
            {
                Logger.Info($"Outlook Bitness is: {bitness}");
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
