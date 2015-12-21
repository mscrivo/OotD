using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;
using NLog;
using OutlookDesktop.Forms;
using OutlookDesktop.Properties;
using Application = Microsoft.Office.Interop.Outlook.Application;
using Exception = System.Exception;

namespace OutlookDesktop
{
    internal static class Startup
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Guid LastNextButtonClicked;
        public static Guid LastPreviousButtonClicked;

        public static Application OutlookApp;
        public static NameSpace OutlookNameSpace;
        public static MAPIFolder OutlookFolder;
        public static Explorer OutlookExplorer;

        public static bool UpdateDetected;

        /// <summary>
        /// The main entry point for the application.
        /// We only only one instance of the application to be running.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Logger.Debug("Checking to see if there is an instance running.");
            if (IsAlreadyRunning())
            {
                // let the user know the program is already running.
                Logger.Warn("Instance is already running, exiting.");
                MessageBox.Show(Resources.ProgramIsAlreadyRunning, Resources.ProgramIsAlreadyRunningCaption,
                                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                if (!IsOutlook2003OrHigherInstalled())
                {
                    Logger.Debug("Outlook is not avaliable or installed.");
                    MessageBox.Show(
                        Resources.Office2000Requirement + Environment.NewLine +
                        Resources.InstallOutlookMsg, Resources.MissingRequirementsCapation, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    OutlookApp = new Application();
                    OutlookNameSpace = OutlookApp.GetNamespace("MAPI");

                    // Before we do anything else, wait for the RPC server to be available, as the program will crash if it's not.
                    // This is especially likely when OotD is set to start with windows.
                    if (!IsRPCServerAvailable(OutlookNameSpace)) return;

                    OutlookFolder = OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);

                    // WORKAROUND: Beginning wih Outlook 2007 SP2, Microsoft decided to kill all outlook instances 
                    // when opening and closing an item from the view control, even though the view control was still running.
                    // The only way I've found to work around it and keep the view control from crashing after opening an item,
                    // is to get this global instance of the active explorer and keep it going until the user closes the app.
                    OutlookExplorer = OutlookFolder.GetExplorer();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Resources.ErrorInitializingApp + ' ' + ex, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                System.Windows.Forms.Application.EnableVisualStyles();

                Logger.Info("Starting the instance manager and loading instances.");
                var instanceManager = new InstanceManager();

                try
                {
                    instanceManager.LoadInstances();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Could not load instances");
                    return;
                }

                System.Windows.Forms.Application.Run(instanceManager);
            }
        }

        /// <summary>
        /// This method will test that the RPC server is available by calling GetDefaultFolder on the outlook namespace object.
        /// It will try this for up to 1 minute before giving up and showing the user an error message.
        /// </summary>
        /// <param name="outlookNameSpace"></param>
        /// <returns></returns>
        private static bool IsRPCServerAvailable(NameSpace outlookNameSpace)
        {
            int retryCount = 0;
            while (retryCount < 120)
            {
                try
                {
                    outlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
                    return true;
                }
                catch (COMException loE)
                {
                    if ((uint)loE.ErrorCode == 0x80010001)
                    {
                        retryCount++;
                        // RPC_E_CALL_REJECTED - sleep half a second then try again
                        Thread.Sleep(500);
                    }
                }
            }

            MessageBox.Show(Resources.ErrorInitializingApp + ' ' + Resources.Windows_RPC_Server_is_not_available, Resources.ErrorCaption,
                MessageBoxButtons.OK, MessageBoxIcon.Error);

            return false;
        }

        /// <summary>
        /// check if given exe already running or not
        /// </summary>
        /// <returns>returns true if already running</returns>
        private static bool IsAlreadyRunning()
        {
            string programPath = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(programPath))
            {
                var fileInfo = new FileInfo(programPath);
                string sExeName = fileInfo.Name;
                bool createdNew = false;

                if (!string.IsNullOrEmpty(sExeName))
                {
                    // ReSharper disable ObjectCreationAsStatement
                    new Mutex(true, $"Local\\{sExeName}", out createdNew);
                    // ReSharper restore ObjectCreationAsStatement
                }

                return !createdNew;
            }

            return false;
        }

        /// <summary>
        /// Returns true if Outlook 2000 (or higher) is installed.
        /// </summary>
        /// <returns>New version of Office need to be explicitly supported in this function.</returns>
        private static bool IsOutlook2003OrHigherInstalled()
        {
            bool hasOffice2003OrHigher = false;
            string outlookPath = string.Empty;

            // first make sure they have Office/Outlook 2000 (9.0) or higher installed by looking for 
            // the version subkeys in HKLM.
            using (var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Office"))
            {
                Logger.Debug("Successfully read reg key: HKLM\\Software\\Microsoft\\Office");
                if (key != null)
                {
                    string[] subkeys = key.GetSubKeyNames();

                    foreach (string subkey in subkeys)
                    {
                        Logger.Debug($"Analyzing subkey '{subkey}'");
                        double versionSubKey;
                        if (double.TryParse(subkey, NumberStyles.Float, new NumberFormatInfo(), out versionSubKey))
                        {
                            Logger.Debug($"Office Version: {versionSubKey} Detected");
                            if (versionSubKey >= 11)
                            {
                                Logger.Debug($"Office Version: {versionSubKey} Detected");
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
                Logger.Debug("Office 2003 or higher is installed, now checking for Outlook exe");

                using (var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\OUTLOOK.EXE"))
                {
                    if (key != null) outlookPath = (string)key.GetValue("Path");
                    Logger.Debug($"Office path reported as: {outlookPath}");
                    if (outlookPath != null)
                    {
                        Logger.Debug($"Checking for Outlook exe in: {outlookPath}");
                        if (File.Exists(Path.Combine(outlookPath, "Outlook.exe")))
                        {
                            Logger.Debug("Outlook exe found.  We're all good.");
                            return true;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(outlookPath))
            {
                Logger.Error($"Outlook path was reported as: {Path.Combine(outlookPath, "Outlook.exe")}, but this file could not be found.");
            }

            return false;
        }
    }
}