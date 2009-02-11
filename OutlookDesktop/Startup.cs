using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using OutlookDesktop.Properties;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;

namespace OutlookDesktop
{
    class Startup
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// We only only one instance of the application to be running.
        /// </summary>
        [STAThread]
        static void Main()
        {
            log.Debug("Checking to see if there is a instance running.");
            if (IsAlreadyRunning())
            {
                // let the user know the program is already running.
                log.Warn("Instance is already running, exiting.");
                MessageBox.Show(Resources.ProgramIsAlreadyRunning, Resources.ProgramIsAlreadyRunningCaption, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {

                if (!IsOutlook2000OrHigherInstalled())
                {
                    log.Error("Outlook is not avaliable or installed.");
                    MessageBox.Show("This program requires Microsoft Outlook 2000 or higher." + Environment.NewLine + "Please install Microsoft Office and try again.", "Missing Requirements", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Application.EnableVisualStyles();

                log.Debug("Starting the instance manager and loading instances.");
                InstanceManager instanceManager = new InstanceManager();
                instanceManager.LoadInstances();

                Application.Run(instanceManager);
            }
        }

        /// <summary>
        /// GetCurrentInstanceWindowHandle
        /// </summary>
        /// <returns></returns>
        private static IntPtr GetCurrentInstanceWindowHandle()
        {
            IntPtr hWnd = IntPtr.Zero;
            Process process = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(process.ProcessName);
            foreach (Process _process in processes)
            {
                // Get the first instance that is not this instance, has the
                // same process name and was started from the same file name
                // and location. Also check that the process has a valid
                // window handle in this session to filter out other user's
                // processes.
                if (_process.Id != process.Id &&
                    _process.MainModule.FileName == process.MainModule.FileName &&
                    _process.MainWindowHandle != IntPtr.Zero)
                {
                    hWnd = _process.MainWindowHandle;
                    break;
                }
            }
            return hWnd;
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

            mutex = new Mutex(true, "Local\\" + sExeName, out createdNew);

            return !createdNew;
        }

        static Mutex mutex;
        const int SW_RESTORE = 9;

        /// <summary>
        /// Returns the 32-bit Program Files directory depending on which Windows arch we're running on.
        /// </summary>
        /// <returns></returns>
        private static string ProgramFilesx86()
        {
            String programFilesX86Dir = string.Empty;

            if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                programFilesX86Dir = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            programFilesX86Dir = Environment.GetEnvironmentVariable("ProgramFiles");
            log.Debug("Program Files Directory: " + programFilesX86Dir);
            return programFilesX86Dir;
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
                log.Info("Successfully read reg key: HKLM\\Software\\Microsoft\\Office");
                string[] subkeys = key.GetSubKeyNames();

                foreach (string subkey in subkeys)
                {
                    log.Info("Analyzing subkey '" + subkey + "'");
                    double versionSubKey;
                    try
                    {
                        versionSubKey = double.Parse(subkey);
                        log.Info("Office Version: " + versionSubKey.ToString());
                        if (versionSubKey > 9)
                        {
                            hasOffice2000OrHigher = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {                        
                       log.Error("Error parsing registry: " + ex.ToString());
                    }
                }
            }

            // now check for the existence of the actual Outlook.exe.
            if (hasOffice2000OrHigher)
            {
                log.Info("Office 2000 or higher is installed, now checking for Outlook exe");

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\OUTLOOK.EXE"))
                {
                    outlookPath = (string)key.GetValue("Path");
                    log.Info("Office path reported as: " + outlookPath);
                    if (outlookPath != null)
                    {
                        log.Info("Checking for Outlook exe in: " + outlookPath);
                        if (File.Exists(Path.Combine(outlookPath, "Outlook.exe")))
                        {
                            log.Info("Outlook exe found.");
                            return true;
                        }
                    }
                }
            }

            log.Error("Outlook path was reported as: " + Path.Combine(outlookPath, "Outlook.exe") + " but this file could not be found.");
            return false;
        }
    }
}
