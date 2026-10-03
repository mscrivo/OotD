using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using NLog;
using OotD.Forms;
using OotD.Preferences;
using OotD.Properties;
using OotD.Utility;
using Application = Microsoft.Office.Interop.Outlook.Application;
using Exception = System.Exception;
using Timer = System.Timers.Timer;

namespace OotD;

// Application composition root: Outlook COM bootstrap, single-instance mutex and WinForms message
// loop. No unit-testable logic, so excluded from coverage.
[ExcludeFromCodeCoverage]
public static class Startup
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    internal static Guid LastNextButtonClicked;
    internal static Guid LastPreviousButtonClicked;

    private static Application? _outlookApp;
    internal static NameSpace? OutlookNameSpace;
    private static MAPIFolder? _outlookFolder;
    private static Explorer? _outlookExplorer;
    private static IntPtr _outlookExplorerHwnd;
    private static readonly Timer _checkIfOutlookIsRunningTimer = new() { Interval = 3000 };

    internal static bool UpdateDetected;
    private static InstanceManager? _instanceManager;

    /// <summary>
    ///     The main entry point for the application, invoked by the thin platform-specific launchers
    ///     (OotD.x64 / OotD.x86). We only want one instance of the application to be running.
    /// </summary>
    public static void Run(string[] args)
    {
        ProcessCommandLineArgs(args);

        _logger.Debug("Checking to see if there is an instance running.");

        using (var singleInstanceMutex = new Mutex(false, AppDomain.CurrentDomain.FriendlyName))
        {
            bool acquired;
            try
            {
                // Wait a few seconds instead of failing immediately so that a restart
                // (new process launched while the old one is still shutting down and
                // releasing the mutex) doesn't get rejected as "already running".
                acquired = singleInstanceMutex.WaitOne(TimeSpan.FromSeconds(5));
            }
            catch (AbandonedMutexException)
            {
                // The previous instance died without releasing the mutex; we own it now.
                acquired = true;
            }

            if (acquired)
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

                    // Add-ins (e.g. CalDAV Synchronizer) can Activate() this normally hidden explorer from
                    // their NewExplorer handler. If the user then closes it and it was Outlook's last window,
                    // Outlook quits, so hide it again whenever it shows up.
                    // Grab its HWND now: later IOleWindow calls from event handlers or the timer fail with
                    // RPC_E_CANTCALLOUT_ININPUTSYNCCALL, but the HWND is stable for the explorer's lifetime.
                    TryCacheExplorerWindow();

                    var explorerEvents = (ExplorerEvents_10_Event)_outlookExplorer;
                    // Activate is dispatched input-synchronously; defer until activation has finished.
                    explorerEvents.Activate += () =>
                    {
                        if (_instanceManager is { IsHandleCreated: true })
                        {
                            _instanceManager.BeginInvoke(HideExplorerWindow);
                        }
                    };
                    explorerEvents.Close += () =>
                    {
                        _logger.Info("OotD's Outlook explorer was closed externally; releasing it.");
                        _outlookExplorer = null;
                        _outlookExplorerHwnd = IntPtr.Zero;
                    };
                    // NewExplorer handlers may already have shown it before we subscribed.
                    HideExplorerWindow();

                    _checkIfOutlookIsRunningTimer.Elapsed += (_, _) =>
                    {
                        // Catch anything that showed the explorer without raising Activate.
                        if (_outlookExplorerHwnd != IntPtr.Zero)
                        {
                            HideExplorerWindow();
                        }
                        else if (_instanceManager is { IsHandleCreated: true })
                        {
                            _instanceManager.BeginInvoke(HideExplorerWindow);
                        }

                        // capture the field so a concurrent DisposeOutlookObjects (which nulls it
                        // during normal shutdown) isn't mistaken for a dead Outlook.
                        var outlookNameSpace = OutlookNameSpace;
                        if (outlookNameSpace == null)
                        {
                            return;
                        }

                        try
                        {
                            // try to access the namespace and if it throws that means Outlook is dead.
                            // Don't probe the explorer: it can be closed while Outlook keeps running.
                            _ = outlookNameSpace.CurrentProfileName;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Outlook liveness check failed; assuming Outlook has exited.");
                            _checkIfOutlookIsRunningTimer.Stop();

                            // System.Timers.Timer fires on a thread-pool thread; marshal the
                            // message box and shutdown onto the UI thread when we have one.
                            static void NotifyAndExit()
                            {
                                // Switching on "New Outlook" closes classic Outlook, so point that out
                                // rather than leaving the user to think Outlook crashed.
                                MessageBox.Show(
                                    OutlookStartupDiagnostics.WithNewOutlookHint(Resources.OutlookNotRunning,
                                        NewOutlook.IsEnabled()),
                                    Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                                Environment.Exit(-1);
                            }

                            if (_instanceManager is { IsHandleCreated: true })
                            {
                                _instanceManager.InvokeEx(_ => NotifyAndExit());
                            }
                            else
                            {
                                NotifyAndExit();
                            }
                        }
                    };
                    _checkIfOutlookIsRunningTimer.Start();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error starting Outlook.");
                    MessageBox.Show(OutlookStartupDiagnostics.GetMessage(ex, NewOutlook.IsEnabled()),
                        Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ApplyUiCultureOverrideFromEnvironment();

                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                // Follow the Windows light/dark app theme (#187).
                System.Windows.Forms.Application.SetColorMode(SystemColorMode.System);

                _logger.Info("Starting the instance manager and loading instances.");
                _instanceManager = new InstanceManager();

                try
                {
                    using var proc = Process.GetCurrentProcess();
                    using var curModule = proc.MainModule;

                    _instanceManager.LoadInstances();
                    System.Windows.Forms.Application.Run(_instanceManager);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not load instances");
                }
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

    private static void TryCacheExplorerWindow()
    {
        if (_outlookExplorerHwnd != IntPtr.Zero || _outlookExplorer is not UnsafeNativeMethods.IOleWindow oleWindow)
        {
            return;
        }

        try
        {
            oleWindow.GetWindow(out var hwnd);
            _outlookExplorerHwnd = hwnd;
            _logger.Debug($"OotD's Outlook explorer window: 0x{hwnd:X}");
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Could not get OotD's Outlook explorer window.");
        }
    }

    private static void HideExplorerWindow()
    {
        TryCacheExplorerWindow();

        var hwnd = _outlookExplorerHwnd;
        if (hwnd != IntPtr.Zero && UnsafeNativeMethods.IsWindowVisible(hwnd))
        {
            _logger.Info("OotD's Outlook explorer became visible (likely activated by an add-in); hiding it.");
            UnsafeNativeMethods.ShowWindow(hwnd, UnsafeNativeMethods.SW_HIDE);
        }
    }

    private static void ApplyUiCultureOverrideFromEnvironment()
    {
        const string uiCultureEnvVar = "OOTD_UI_CULTURE";

        var cultureName = Environment.GetEnvironmentVariable(uiCultureEnvVar);
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return;
        }

        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentUICulture = culture;

            _logger.Info($"Applied UI culture override from {uiCultureEnvVar}: {culture.Name}");
        }
        catch (CultureNotFoundException ex)
        {
            _logger.Warn(ex, $"Invalid culture value '{cultureName}' in {uiCultureEnvVar}; ignoring override.");
        }
    }

    private static void ProcessCommandLineArgs(string[] args)
    {
        bool Has(string shortName, string longName) => args.Contains(shortName) || args.Contains(longName);

        if (Has("-d", "--debug"))
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }

        if (Has("-s", "--createStartupEntry"))
        {
            TaskScheduling.EnsureOotDStartupTask(_logger);
            Environment.Exit(0);
        }

        if (Has("-r", "--removeStartupEntry"))
        {
            TaskScheduling.RemoveOotDStartupTask(_logger);
            Environment.Exit(0);
        }
    }

    /// <summary>
    ///     This method will test that the RPC server is available by calling GetDefaultFolder on the outlook namespace object.
    ///     It will try this for up to 1 minute before giving up and showing the user an error message.
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
                // Count and pace every failure, not just RPC_E_CALL_REJECTED (0x80010001),
                // so an unexpected repeating COM error can't spin this loop forever.
                retryCount++;
                _logger.Debug($"RPC server not available yet (0x{(uint)loE.ErrorCode:X8}), attempt {retryCount}.");
                Thread.Sleep(500);
            }
        }

        MessageBox.Show(
            OutlookStartupDiagnostics.WithNewOutlookHint(
                Resources.ErrorInitializingApp + ' ' + Resources.Windows_RPC_Server_is_not_available,
                NewOutlook.IsEnabled()),
            Resources.ErrorCaption,
            MessageBoxButtons.OK, MessageBoxIcon.Error);

        return false;
    }

    internal static void DisposeOutlookObjects()
    {
        try
        {
            _checkIfOutlookIsRunningTimer.Dispose();
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
