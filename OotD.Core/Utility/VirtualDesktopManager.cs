using System;
using System.Collections.Generic;
using Microsoft.Win32;
using NLog;
using static OotD.Utility.UnsafeNativeMethods;

namespace OotD.Utility;

/// <summary>
///     Manages virtual desktop operations for windows.
/// </summary>
internal static class VirtualDesktopManager
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///     The public virtual desktop COM API, created by the static constructor. Overridable so tests can
    ///     substitute a fake.
    /// </summary>
    internal static IVirtualDesktopManager? DesktopManager { get; set; }

    /// <summary>
    ///     The undocumented virtual desktop COM API used for enumeration, created by the static constructor.
    ///     Overridable so tests can substitute a fake.
    /// </summary>
    internal static IVirtualDesktopManagerInternal? DesktopManagerInternal { get; set; }

    /// <summary>
    ///     HKCU-relative path to Explorer's virtual desktop state. Overridable so tests can point it at a
    ///     throwaway key; production code never sets it.
    /// </summary>
    internal static string RegistryPath { get; set; } = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops";

    static VirtualDesktopManager()
    {
        try
        {
            // Initialize public API
            var vdmType = Type.GetTypeFromCLSID(CLSID_VirtualDesktopManager);
            if (vdmType != null)
            {
                DesktopManager = (IVirtualDesktopManager?)Activator.CreateInstance(vdmType);
            }

            // Initialize internal API for desktop enumeration
            try
            {
                var shellType = Type.GetTypeFromCLSID(CLSID_ImmersiveShell);
                if (shellType != null)
                {
                    var shell = Activator.CreateInstance(shellType);
                    if (shell is IServiceProvider10 serviceProvider)
                    {
                        var managerInternalGuid = typeof(IVirtualDesktopManagerInternal).GUID;
                        var serviceGuid = SID_VirtualDesktopManagerInternal;
                        var obj = serviceProvider.QueryService(ref serviceGuid, ref managerInternalGuid);
                        DesktopManagerInternal = obj as IVirtualDesktopManagerInternal;
                        _logger.Debug("Internal Virtual Desktop API initialized successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to initialize internal Virtual Desktop API - will use registry fallback");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize Virtual Desktop Manager");
        }
    }

    /// <summary>
    ///     Checks if virtual desktop features are available.
    /// </summary>
    public static bool IsVirtualDesktopSupported => DesktopManager != null;

    /// <summary>
    ///     Gets the GUID of the virtual desktop that contains the specified window.
    /// </summary>
    public static Guid? GetWindowDesktopId(IntPtr hwnd)
    {
        try
        {
            if (DesktopManager == null)
            {
                return null;
            }

            var hr = DesktopManager.GetWindowDesktopId(hwnd, out var desktopId);
            if (hr == 0 && desktopId != Guid.Empty)
            {
                return desktopId;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get window desktop ID");
        }

        return null;
    }

    /// <summary>
    ///     Moves a window to the specified virtual desktop.
    /// </summary>
    public static bool MoveWindowToDesktop(IntPtr hwnd, Guid desktopId)
    {
        try
        {
            if (DesktopManager == null)
            {
                return false;
            }

            var result = DesktopManager.MoveWindowToDesktop(hwnd, desktopId);

            if (result == 0)
            {
                _logger.Info($"Moved window {hwnd} to desktop {desktopId}");
                return true;
            }

            _logger.Warn($"Failed to move window {hwnd} to desktop {desktopId}, HRESULT: {result:X}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to move window {hwnd} to desktop {desktopId}");
            return false;
        }
    }

    /// <summary>
    ///     Gets a list of all available virtual desktops.
    /// </summary>
    public static List<VirtualDesktopInfo> GetVirtualDesktops()
    {
        var desktops = new List<VirtualDesktopInfo>();

        try
        {
            if (DesktopManager == null)
            {
                return desktops;
            }

            _logger.Debug("Getting virtual desktops list");

            // Method 1: Try internal API first
            if (DesktopManagerInternal != null)
            {
                try
                {
                    var desktopList = GetDesktopsFromInternalApi();
                    if (desktopList.Count > 0)
                    {
                        _logger.Info($"Found {desktopList.Count} virtual desktops via internal API");
                        return desktopList;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Internal API enumeration failed");
                }
            }

            // Method 2: Try reading from registry
            try
            {
                var registryDesktops = GetDesktopsFromRegistry();
                if (registryDesktops.Count > 0)
                {
                    _logger.Info($"Found {registryDesktops.Count} virtual desktops via registry");
                    return registryDesktops;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Registry enumeration failed");
            }

            // Method 3: Fallback to window enumeration
            var foundDesktops = new HashSet<Guid>();

            EnumWindows((hwnd, lParam) =>
            {
                try
                {
                    if (IsWindowVisible(hwnd))
                    {
                        var desktopId = GetWindowDesktopId(hwnd);
                        if (desktopId.HasValue && desktopId.Value != Guid.Empty)
                        {
                            foundDesktops.Add(desktopId.Value);
                        }
                    }
                }
                catch
                {
                    // Ignore errors for individual windows
                }

                return true;
            }, IntPtr.Zero);

            var index = 1;
            foreach (var desktopId in foundDesktops)
            {
                desktops.Add(new VirtualDesktopInfo
                {
                    Id = desktopId,
                    Name = $"Desktop {index}"
                });
                index++;
            }

            _logger.Info($"Found {desktops.Count} virtual desktops via window enumeration");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to enumerate virtual desktops");
        }

        return desktops;
    }

    /// <summary>
    ///     Gets virtual desktops from the Windows Registry.
    /// </summary>
    private static List<VirtualDesktopInfo> GetDesktopsFromRegistry()
    {
        var desktops = new List<VirtualDesktopInfo>();

        try
        {
            // Virtual desktop IDs are stored in the registry
            // Windows 10: HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);

            if (key == null)
            {
                _logger.Debug("Virtual Desktops registry key not found");
                return desktops;
            }

            // Get the list of desktop IDs (stored as a binary blob of GUIDs)
            var desktopIdsBytes = key.GetValue("VirtualDesktopIDs") as byte[];
            if (desktopIdsBytes == null || desktopIdsBytes.Length == 0)
            {
                _logger.Debug("VirtualDesktopIDs registry value not found or empty");
                return desktops;
            }

            // Each GUID is 16 bytes
            var guidSize = 16;
            var desktopCount = desktopIdsBytes.Length / guidSize;

            _logger.Debug($"Registry contains {desktopCount} virtual desktop IDs");

            // Try to get desktop names from Windows 11 registry location
            var desktopNames = GetDesktopNamesFromRegistry();

            for (var i = 0; i < desktopCount; i++)
            {
                var guidBytes = new byte[guidSize];
                Array.Copy(desktopIdsBytes, i * guidSize, guidBytes, 0, guidSize);
                var desktopId = new Guid(guidBytes);

                var name = $"Desktop {i + 1}";

                // Check if we have a custom name for this desktop
                if (desktopNames.TryGetValue(desktopId, out var customName) && !string.IsNullOrEmpty(customName))
                {
                    name = customName;
                }

                desktops.Add(new VirtualDesktopInfo
                {
                    Id = desktopId,
                    Name = name
                });

                _logger.Debug($"Found desktop {i + 1}: {desktopId} - {name}");
            }
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Failed to read virtual desktops from registry");
        }

        return desktops;
    }

    /// <summary>
    ///     Gets custom desktop names from Windows 11 registry.
    /// </summary>
    private static Dictionary<Guid, string> GetDesktopNamesFromRegistry()
    {
        var names = new Dictionary<Guid, string>();

        try
        {
            // Windows 11 stores desktop names in a different location
            using var key = Registry.CurrentUser.OpenSubKey($@"{RegistryPath}\Desktops");

            if (key == null)
            {
                return names;
            }

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                try
                {
                    if (Guid.TryParse(subKeyName.Trim('{', '}'), out var desktopId))
                    {
                        using var subKey = key.OpenSubKey(subKeyName);
                        var name = subKey?.GetValue("Name") as string;
                        if (!string.IsNullOrEmpty(name))
                        {
                            names[desktopId] = name;
                        }
                    }
                }
                catch
                {
                    // Ignore individual subkey errors
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Failed to read desktop names from registry");
        }

        return names;
    }

    private static List<VirtualDesktopInfo> GetDesktopsFromInternalApi()
    {
        var desktops = new List<VirtualDesktopInfo>();

        if (DesktopManagerInternal == null)
        {
            return desktops;
        }

        try
        {
            var desktopArray = DesktopManagerInternal.GetDesktops(IntPtr.Zero);
            if (desktopArray == null)
            {
                return desktops;
            }

            desktopArray.GetCount(out var count);
            _logger.Debug($"Internal API reports {count} desktops");

            for (var i = 0; i < count; i++)
            {
                desktopArray.GetAt(i, typeof(IVirtualDesktop).GUID, out var desktop);
                if (desktop is IVirtualDesktop vd)
                {
                    vd.GetID(out var id);
                    var name = $"Desktop {i + 1}";

                    // Try to get the actual name (Windows 11+)
                    try
                    {
                        if (vd is IVirtualDesktop2 vd2)
                        {
                            var desktopName = vd2.GetName();
                            if (!string.IsNullOrEmpty(desktopName))
                            {
                                name = desktopName;
                            }
                        }
                    }
                    catch
                    {
                        // Name not available, use default
                    }

                    desktops.Add(new VirtualDesktopInfo
                    {
                        Id = id,
                        Name = name
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Failed to enumerate desktops via internal API");
        }

        return desktops;
    }

    /// <summary>
    ///     Gets the current virtual desktop ID.
    /// </summary>
    public static Guid? GetCurrentDesktopId()
    {
        try
        {
            if (DesktopManager == null)
            {
                return null;
            }

            // Try reading from registry first (most reliable)
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                var currentDesktopBytes = key?.GetValue("CurrentVirtualDesktop") as byte[];
                if (currentDesktopBytes != null && currentDesktopBytes.Length == 16)
                {
                    var currentId = new Guid(currentDesktopBytes);
                    _logger.Debug($"Current desktop from registry: {currentId}");
                    return currentId;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to get current desktop from registry");
            }

            // Try internal API
            if (DesktopManagerInternal != null)
            {
                try
                {
                    var currentDesktop = DesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero);
                    if (currentDesktop != null)
                    {
                        currentDesktop.GetID(out var id);
                        _logger.Debug($"Current desktop from internal API: {id}");
                        return id;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to get current desktop from internal API");
                }
            }

            // Fallback: Try the shell window
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

            // Fallback to foreground window
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

            _logger.Warn("Could not determine current desktop ID");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get current desktop ID");
        }

        return null;
    }

}

/// <summary>
///     Information about a virtual desktop.
/// </summary>
public class VirtualDesktopInfo
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
