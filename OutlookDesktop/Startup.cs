using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Microsoft.Win32;
using OutlookDesktop.Properties;

namespace OutlookDesktop
{
    internal static class Startup
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Mutex _mutex;

        /// <summary>
        /// The main entry point for the application.
        /// We only only one instance of the application to be running.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Log.Debug("Checking to see if there is a instance running.");
            if (IsAlreadyRunning())
            {
                // let the user know the program is already running.
                Log.Warn("Instance is already running, exiting.");
                MessageBox.Show(Resources.ProgramIsAlreadyRunning, Resources.ProgramIsAlreadyRunningCaption,
                                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                if (!IsOutlook2000OrHigherInstalled())
                {
                    Log.Error("Outlook is not avaliable or installed.");
                    MessageBox.Show(
                        "This program requires Microsoft Outlook 2000 or higher." + Environment.NewLine +
                        "Please install Microsoft Office and try again.", "Missing Requirements", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Application.EnableVisualStyles();

                Log.Debug("Starting the instance manager and loading instances.");
                var instanceManager = new InstanceManager();
                instanceManager.LoadInstances();

                Application.Run(instanceManager);
            }
        }

        /// <summary>
        /// check if given exe alread running or not
        /// </summary>
        /// <returns>returns true if already running</returns>
        private static bool IsAlreadyRunning()
        {
            string strLoc = Assembly.GetExecutingAssembly().Location;
            FileSystemInfo fileInfo = new FileInfo(strLoc);
            string sExeName = fileInfo.Name;
            bool createdNew;

            _mutex = new Mutex(true, "Local\\" + sExeName, out createdNew);

            return !createdNew;
        }

        /// <summary>
        /// Returns true if Outlook 2000 (or higher) is installed.
        /// </summary>
        /// <returns>New version of Office need to be explicily supported in this function.</returns>
        private static bool IsOutlook2000OrHigherInstalled()
        {
            bool hasOffice2000OrHigher = false;
            string outlookPath = string.Empty;

            // first make sure they have Office/Outlook 2000 (9.0) or higher installed by looking for 
            // the version subkeys in HKLM.
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office"))
            {
                Log.Info("Successfully read reg key: HKLM\\Software\\Microsoft\\Office");
                if (key != null)
                {
                    string[] subkeys = key.GetSubKeyNames();

                    foreach (string subkey in subkeys)
                    {
                        Log.Info("Analyzing subkey '" + subkey + "'");
                        double versionSubKey;
                        CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                        if (double.TryParse(subkey, NumberStyles.Float, culture, out versionSubKey))
                        {
                            Log.Info("Office Version: " + versionSubKey);
                            if (versionSubKey > 9)
                            {
                                hasOffice2000OrHigher = true;
                                break;
                            }
                        }
                    }
                }
            }

            // now check for the existence of the actual Outlook.exe.
            if (hasOffice2000OrHigher)
            {
                Log.Info("Office 2000 or higher is installed, now checking for Outlook exe");

                using (
                    RegistryKey key =
                        Registry.LocalMachine.OpenSubKey(
                            "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\OUTLOOK.EXE"))
                {
                    if (key != null) outlookPath = (string) key.GetValue("Path");
                    Log.Info("Office path reported as: " + outlookPath);
                    if (outlookPath != null)
                    {
                        Log.Info("Checking for Outlook exe in: " + outlookPath);
                        if (File.Exists(Path.Combine(outlookPath, "Outlook.exe")))
                        {
                            Log.Info("Outlook exe found.");
                            return true;
                        }
                    }
                }
            }

            if (outlookPath != null)
                Log.Error("Outlook path was reported as: " + Path.Combine(outlookPath, "Outlook.exe") +
                          " but this file could not be found.");

            return false;
        }
    }
}