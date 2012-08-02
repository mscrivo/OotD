using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using BitFactory.Logging;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;
using OutlookDesktop.Forms;
using OutlookDesktop.Properties;
using Application = Microsoft.Office.Interop.Outlook.Application;
using Exception = System.Exception;

namespace OutlookDesktop
{
    internal static class Startup
    {
        public static Application OutlookApp;
        public static NameSpace OutlookNameSpace;
        public static MAPIFolder OutlookFolder;
        public static Explorer OutlookExplorer;

        /// <summary>
        /// The main entry point for the application.
        /// We only only one instance of the application to be running.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            ConfigLogger.Instance.LogDebug("Checking to see if there is an instance running.");
            if (IsAlreadyRunning())
            {
                // let the user know the program is already running.
                ConfigLogger.Instance.LogWarning("Instance is already running, exiting.");
                MessageBox.Show(Resources.ProgramIsAlreadyRunning, Resources.ProgramIsAlreadyRunningCaption,
                                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                if (!IsOutlook2003OrHigherInstalled())
                {
                    ConfigLogger.Instance.LogDebug("Outlook is not avaliable or installed.");
                    MessageBox.Show(
                        Resources.Office2000Requirement + Environment.NewLine +
                        Resources.InstallOutlookMsg, Resources.MissingRequirementsCapation, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    OutlookApp = new Application();
                    OutlookNameSpace = OutlookApp.GetNamespace("MAPI");
                    OutlookFolder = OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);

                    // WORKAROUND: Beginning wih Outlook 2007 SP2, Microsoft decided to kill all outlook instances 
                    // when opening and closing an item from the view control, even though the view control was still running.
                    // The only way I've found to work around it and keep the view control from crashing after opening an item,
                    // is to get this global instance of the active explorer and keep it going until the user closes the app.
                    OutlookExplorer = OutlookFolder.GetExplorer();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Resources.ErrorInitializingApp + @" " + ex, Resources.ErrorCaption,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                System.Windows.Forms.Application.EnableVisualStyles();

                ConfigLogger.Instance.LogInfo("Starting the instance manager and loading instances.");
                var instanceManager = new InstanceManager();
                instanceManager.LoadInstances();

                System.Windows.Forms.Application.Run(instanceManager);
            }
        }

        /// <summary>
        /// check if given exe alread running or not
        /// </summary>
        /// <returns>returns true if already running</returns>
        private static bool IsAlreadyRunning()
        {
            string strLoc = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(strLoc))
            {
                FileSystemInfo fileInfo = new FileInfo(strLoc);
                string sExeName = fileInfo.Name;
                bool createdNew = false;

                if (!string.IsNullOrEmpty(sExeName))
                    new Mutex(true, string.Format("Local\\{0}", sExeName), out createdNew);

                return !createdNew;
            }

            return false;
        }

        /// <summary>
        /// Returns true if Outlook 2000 (or higher) is installed.
        /// </summary>
        /// <returns>New version of Office need to be explicily supported in this function.</returns>
        private static bool IsOutlook2003OrHigherInstalled()
        {
            bool hasOffice2003OrHigher = false;
            string outlookPath = string.Empty;

            // first make sure they have Office/Outlook 2000 (9.0) or higher installed by looking for 
            // the version subkeys in HKLM.
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office"))
            {
                ConfigLogger.Instance.LogDebug("Successfully read reg key: HKLM\\Software\\Microsoft\\Office");
                if (key != null)
                {
                    string[] subkeys = key.GetSubKeyNames();

                    foreach (string subkey in subkeys)
                    {
                        ConfigLogger.Instance.LogDebug(String.Format("Analyzing subkey '{0}'", subkey));
                        double versionSubKey;
                        CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                        if (double.TryParse(subkey, NumberStyles.Float, culture, out versionSubKey))
                        {
                            ConfigLogger.Instance.LogDebug(string.Format("Office Version: {0}", versionSubKey));
                            if (versionSubKey >= 11)
                            {
                                hasOffice2003OrHigher = true;
                                break;
                            }
                        }
                    }
                }
            }

            // now check for the existence of the actual Outlook.exe.
            if (hasOffice2003OrHigher)
            {
                ConfigLogger.Instance.LogDebug("Office 2003 or higher is installed, now checking for Outlook exe");

                using (
                    RegistryKey key =
                        Registry.LocalMachine.OpenSubKey(
                            "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\OUTLOOK.EXE"))
                {
                    if (key != null) outlookPath = (string) key.GetValue("Path");
                    ConfigLogger.Instance.LogDebug(string.Format("Office path reported as: {0}", outlookPath));
                    if (outlookPath != null)
                    {
                        ConfigLogger.Instance.LogDebug(string.Format("Checking for Outlook exe in: {0}", outlookPath));
                        if (File.Exists(Path.Combine(outlookPath, "Outlook.exe")))
                        {
                            ConfigLogger.Instance.LogDebug("Outlook exe found.");
                            return true;
                        }
                    }
                }
            }

            if (outlookPath != null)
                ConfigLogger.Instance.LogError(
                    string.Format("Outlook path was reported as: {0}, but this file could not be found.",
                                  Path.Combine(outlookPath, "Outlook.exe")));

            return false;
        }
    }
}