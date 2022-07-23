// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CommandLine;
using Microsoft.Office.Interop.Outlook;
using NLog;
using OotD.Forms;
using OotD.Preferences;
using OotD.Properties;
using Application = Microsoft.Office.Interop.Outlook.Application;
using Exception = System.Exception;
using Timer = System.Timers.Timer;

namespace OotD;

internal static class Startup
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static Guid LastNextButtonClicked;
    public static Guid LastPreviousButtonClicked;

    private static Application? _outlookApp;
    public static NameSpace? OutlookNameSpace;
    private static MAPIFolder? _outlookFolder;
    private static Explorer? _outlookExplorer;
    private static Timer? _checkIfOutlookIsRunningTimer;

    public static bool UpdateDetected;

    /// <summary>
    /// The main entry point for the application.
    /// We only want one instance of the application to be running.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed(ProcessCommandLineArgs);

        _logger.Debug("Checking to see if there is an instance running.");

        using (new Mutex(true, AppDomain.CurrentDomain.FriendlyName, out var createdNew))
        {
            if (createdNew)
            {
                try
                {
                    _outlookApp = new Application();
                    OutlookNameSpace = _outlookApp.GetNamespace("MAPI");

                    // Before we do anything else, wait for the RPC server to be available, as the program will crash if it's not.
                    // This is especially likely when OotD is set to start with windows.
                    if (!IsRPCServerAvailable(OutlookNameSpace))
                    {
                        return;
                    }

                    _outlookFolder = OutlookNameSpace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);

                    // WORKAROUND: Beginning with Outlook 2007 SP2, Microsoft decided to kill all outlook instances 
                    // when opening and closing an item from the view control, even though the view control was still running.
                    // The only way I've found to work around it and keep the view control from crashing after opening an item,
                    // is to get this global instance of the active explorer and keep it going until the user closes the app.
                    _outlookExplorer = _outlookFolder.GetExplorer();

                    _checkIfOutlookIsRunningTimer = new Timer { Interval = 3000 };
                    _checkIfOutlookIsRunningTimer.Elapsed += (_, _) =>
                    {
                        try
                        {
                            // try to access _outlookExplorer and if it throws that means
                            // Outlook is dead.
                            _ = _outlookExplorer.CurrentView;
                        }
                        catch
                        {
                            _checkIfOutlookIsRunningTimer.Stop();

                            MessageBox.Show(Resources.OutlookNotRunning, Resources.ErrorCaption,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                            Environment.Exit(-1);
                        }
                    };
                    _checkIfOutlookIsRunningTimer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Resources.ErrorInitializingApp + ' ' + ex, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

                _logger.Info("Starting the instance manager and loading instances.");
                var instanceManager = new InstanceManager();

                try
                {
                    instanceManager.LoadInstances();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not load instances");
                    return;
                }

                System.Windows.Forms.Application.Run(instanceManager);
            }
            else
            {
                // let the user know the program is already running.
                _logger.Warn("Instance is already running, exiting.");
                MessageBox.Show(Resources.ProgramIsAlreadyRunning, Resources.ProgramIsAlreadyRunningCaption,
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
    }

    private static void ProcessCommandLineArgs(Options opts)
    {
        if (opts.StartDebugger)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }

        if (opts.CreateStartupEntry)
        {
            TaskScheduling.CreateOotDStartupTask(_logger);
            Environment.Exit(0);
        }

        // ReSharper disable once InvertIf
        if (opts.RemoveStartupEntry)
        {
            TaskScheduling.RemoveOotDStartupTask(_logger);
            Environment.Exit(0);
        }
    }

    /// <summary>
    /// This method will test that the RPC server is available by calling GetDefaultFolder on the outlook namespace object.
    /// It will try this for up to 1 minute before giving up and showing the user an error message.
    /// </summary>
    /// <param name="outlookNameSpace"></param>
    /// <returns></returns>
    private static bool IsRPCServerAvailable(_NameSpace outlookNameSpace)
    {
        var retryCount = 0;
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

    public static void DisposeOutlookObjects()
    {
        try
        {
            _checkIfOutlookIsRunningTimer?.Dispose();
            _outlookExplorer?.Close();

            _outlookExplorer = null;
            _outlookFolder = null;
            OutlookNameSpace = null;
            _outlookApp = null;
        }
        catch
        {
            // ignore any exceptions cleaning up as they might result
            // from a crash in Outlook itself.
        }
    }
}