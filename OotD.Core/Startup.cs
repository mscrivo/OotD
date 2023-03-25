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
using static OotD.Utility.UnsafeNativeMethods;
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
    private static readonly Timer _checkIfOutlookIsRunningTimer = new() { Interval = 3000 };
    private static readonly Timer _keyDeBounceTimer = new() { Interval = 200, AutoReset = false };

    public static bool UpdateDetected;
    public static bool IsShowingDesktop;
    public static InstanceManager? InstanceManager;
    private static IntPtr _showDesktopKeyPressHookHandle = IntPtr.Zero;
    private static IntPtr _windowEventsHookHandle = IntPtr.Zero;

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
                InstanceManager = new InstanceManager();

                try
                {
                    using var proc = Process.GetCurrentProcess();
                    using var curModule = proc.MainModule;
                    var moduleHandle = GetModuleHandle(curModule?.ModuleName ?? throw new InvalidOperationException());
                    _showDesktopKeyPressHookHandle = HookManager.SetWindowsHookEx(HookManager.WH_KEYBOARD_LL, KeyboardEventCallback, moduleHandle, 0);

                    _windowEventsHookHandle = HookManager.SubscribeToWindowEvents(WindowEventCallback);

                    _keyDeBounceTimer.Elapsed += (_, _) =>
                    {
                        _logger.Debug("Disabling KeyDeBounderTimer");
                        _keyDeBounceTimer.Enabled = false;
                    };

                    InstanceManager.LoadInstances();
                    System.Windows.Forms.Application.Run(InstanceManager);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not load instances");
                }
                finally
                {
                    HookManager.UnhookWindowsHookEx(_showDesktopKeyPressHookHandle);
                    HookManager.UnhookWindowsHookEx(_windowEventsHookHandle);
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

    /// <summary>
    /// Event callback for EVENT_SYSTEM_FOREGROUND.
    /// When this fires, we are no longer in show desktop mode and want to restore the
    /// OotD windows to the bottom of the z-order.
    /// </summary>
    /// <param name="eventHook"></param>
    /// <param name="eventType"></param>
    /// <param name="hwnd"></param>
    /// <param name="idObject"></param>
    /// <param name="idChild"></param>
    /// <param name="eventThread"></param>
    /// <param name="eventTime"></param>
    private static void WindowEventCallback(IntPtr eventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint eventThread, uint eventTime)
    {
        _logger.Debug("In WindowEventCallback...");
        if (eventType != HookManager.EVENT_SYSTEM_FOREGROUND)
        {
            return;
        }

        if (!GetWindowClass(hwnd).Equals("WorkerW") || !BelongToSameProcess(GetDefaultShellWindow(), hwnd))
        {
            return;
        }

        _logger.Debug("WorkerW found...");

        const int MaxRetries = 5;
        var counter = 0;
        while (counter < MaxRetries && FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null!) == IntPtr.Zero)
        {
            Thread.Sleep(2);
            counter++;
        }

        _logger.Debug("ShellDefView found, sending all to back...");

        IsShowingDesktop = false;
        InstanceManager!.SendAllToBack();
    }

    /// <summary>
    /// Returns true if Win+D keyboard combo was pressed
    /// </summary>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    private static bool IsWin_D(IntPtr wParam, IntPtr lParam)
    {
        if (wParam != WM.WM_KEYDOWN && wParam != WM.WM_KEYUP)
        {
            return false;
        }

        var keyInfo = (HookManager.KbHookParam)Marshal.PtrToStructure(lParam, typeof(HookManager.KbHookParam))!;
        if (keyInfo.VkCode != (int)Keys.D) return false;

        return GetAsyncKeyState(Keys.LWin) < 0 || GetAsyncKeyState(Keys.RWin) < 0;
    }

    /// <summary>
    /// Hook into low-level keyboard events to detect when Win+D is pressed so
    /// we can bring the OotD windows to the top so they remain visible.
    /// </summary>
    /// <param name="nCode"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    private static IntPtr KeyboardEventCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        switch (nCode)
        {
            case HookManager.HC_ACTION when IsWin_D(wParam, lParam):
                if (_keyDeBounceTimer.Enabled) break;
                _keyDeBounceTimer.Start();
                _logger.Debug("Win+D pressed");
                _logger.Debug("sending to top");
                InstanceManager!.SendAllToTop();
                IsShowingDesktop = true;
                break;
        }

        return HookManager.CallNextHookEx(_showDesktopKeyPressHookHandle, nCode, wParam, lParam);
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
