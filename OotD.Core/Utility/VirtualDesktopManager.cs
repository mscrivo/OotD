// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NLog;

namespace OotD.Utility;

/// <summary>
///     Manages virtual desktop operations for windows.
/// </summary>
internal static class VirtualDesktopManager
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static IVirtualDesktopManager? _virtualDesktopManager;

    static VirtualDesktopManager()
    {
        try
        {
            var vdmType = Type.GetTypeFromCLSID(new Guid("AA509086-5CA9-4C25-8F95-589D3C07B48A"));
            _virtualDesktopManager = (IVirtualDesktopManager?)Activator.CreateInstance(vdmType!);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize Virtual Desktop Manager");
        }
    }

    /// <summary>
    ///     Checks if virtual desktop features are available
    /// </summary>
    public static bool IsVirtualDesktopSupported => _virtualDesktopManager != null;

    /// <summary>
    ///     Gets the GUID of the virtual desktop that contains the specified window
    /// </summary>
    public static Guid? GetWindowDesktopId(IntPtr hwnd)
    {
        try
        {
            if (_virtualDesktopManager == null)
                return null;

            _virtualDesktopManager.GetWindowDesktopId(hwnd, out var desktopId);
            return desktopId;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get window desktop ID");
            return null;
        }
    }

    /// <summary>
    ///     Moves a window to the specified virtual desktop
    ///     Note: This operation makes the window exclusive to the target desktop.
    ///     The window will be removed from all other virtual desktops and will only appear on the specified desktop.
    /// </summary>
    /// <param name="hwnd">Handle to the window to move</param>
    /// <param name="desktopId">GUID of the target virtual desktop</param>
    /// <returns>True if the window was successfully moved, false otherwise</returns>
    public static bool MoveWindowToDesktop(IntPtr hwnd, Guid desktopId)
    {
        try
        {
            if (_virtualDesktopManager == null)
                return false;

            // Move the window to the specified desktop
            // This automatically removes it from all other desktops
            var result = _virtualDesktopManager.MoveWindowToDesktop(hwnd, desktopId);

            if (result == 0) // S_OK = success
            {
                _logger.Info($"Moved window {hwnd} to desktop {desktopId}");
                return true;
            }
            else
            {
                _logger.Warn($"Failed to move window {hwnd} to desktop {desktopId}, HRESULT: {result:X}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to move window {hwnd} to desktop {desktopId}");
            return false;
        }
    }

    /// <summary>
    ///     Gets a list of available virtual desktops (basic enumeration)
    /// </summary>
    public static List<VirtualDesktopInfo> GetVirtualDesktops()
    {
        var desktops = new List<VirtualDesktopInfo>();

        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops", writable: false);
        if (key != null)
        {
            if (key.GetValue("VirtualDesktopIDs") is byte[] ids)
            {
                const int GuidSize = 16;
                var span = ids.AsSpan();
                while (span.Length >= GuidSize)
                {
                    var guid = new Guid(span.Slice(0, GuidSize));
                    string? name = null;
                    using (var keyName = key.OpenSubKey($@"Desktops\{guid:B}", writable: false))
                    {
                        name = keyName?.GetValue("Name") as string;
                    }

                    // note: you may want to use a resource string to localize the value
                    name ??= "Desktop " + (desktops.Count + 1);
                    desktops.Add(new VirtualDesktopInfo() { Id = guid, Name = name });

                    span = span.Slice(GuidSize);
                }
            }
        }

        return desktops;
    }

    /// <summary>
    ///     Gets the current virtual desktop ID by checking the desktop of the shell window
    /// </summary>
    public static Guid? GetCurrentDesktopId()
    {
        try
        {
            // Try multiple approaches to get the current desktop

            // Approach 1: Get the desktop from the shell window (explorer.exe)
            var shellWindow = GetShellWindow();
            if (shellWindow != IntPtr.Zero)
            {
                var shellDesktopId = GetWindowDesktopId(shellWindow);
                if (shellDesktopId.HasValue && shellDesktopId.Value != Guid.Empty)
                {
                    _logger.Debug($"Current desktop from shell window: {shellDesktopId}");
                    return shellDesktopId;
                }
            }

            // Approach 2: Get the desktop from any visible foreground window
            var foregroundWindow = GetForegroundWindow();
            if (foregroundWindow != IntPtr.Zero)
            {
                var fgDesktopId = GetWindowDesktopId(foregroundWindow);
                if (fgDesktopId.HasValue && fgDesktopId.Value != Guid.Empty)
                {
                    _logger.Debug($"Current desktop from foreground window: {fgDesktopId}");
                    return fgDesktopId;
                }
            }

            // Approach 3: Enumerate visible windows and find one on the current desktop
            Guid? currentDesktop = null;
            EnumWindows((hwnd, lParam) =>
            {
                if (IsWindowVisible(hwnd))
                {
                    var desktopId = GetWindowDesktopId(hwnd);
                    if (desktopId.HasValue && desktopId.Value != Guid.Empty)
                    {
                        // Check if this window is on the current desktop
                        if (_virtualDesktopManager?.IsWindowOnCurrentVirtualDesktop(hwnd) == 0) // 0 means S_OK = on current desktop
                        {
                            currentDesktop = desktopId;
                            return false; // Stop enumeration
                        }
                    }
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);

            if (currentDesktop.HasValue)
            {
                _logger.Debug($"Current desktop from visible window: {currentDesktop}");
                return currentDesktop;
            }

            _logger.Warn("Could not determine current desktop ID");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get current desktop ID");
        }

        return null;
    }

    /// <summary>
    ///     Checks if a window is on the specified virtual desktop
    /// </summary>
    public static bool IsWindowOnDesktop(IntPtr hwnd, Guid desktopId)
    {
        try
        {
            if (_virtualDesktopManager == null)
                return false;

            var result = _virtualDesktopManager.IsWindowOnCurrentVirtualDesktop(hwnd);
            var windowDesktopId = GetWindowDesktopId(hwnd);

            return windowDesktopId.HasValue && windowDesktopId.Value == desktopId;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to check if window is on desktop");
            return false;
        }
    }

    #region Native Interop

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("A5CD92FF-29BE-454C-8D04-D82879FB3F1B")]
    private interface IVirtualDesktopManager
    {
        [PreserveSig]
        int IsWindowOnCurrentVirtualDesktop(IntPtr topLevelWindow);

        [PreserveSig]
        int GetWindowDesktopId(IntPtr topLevelWindow, out Guid desktopId);

        [PreserveSig]
        int MoveWindowToDesktop(IntPtr topLevelWindow, [MarshalAs(UnmanagedType.LPStruct)] Guid desktopId);
    }

    private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hwnd);

    #endregion
}

/// <summary>
///     Information about a virtual desktop
/// </summary>
public class VirtualDesktopInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
