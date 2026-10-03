using System;
using System.Runtime.InteropServices;

namespace OotD.Utility;

/// <summary>
///     Pins windows to the desktop so that they always render behind other application windows.
///     A hidden helper window is created and pinned to the very bottom of the z-order; pinned
///     windows are inserted directly behind it. This keeps the window at the bottom of the stack
///     so it never floats above other apps.
/// </summary>
internal static class DesktopPinning
{
    private const string AnchorClassName = "OotDDesktopAnchor";
    private const string HelperWindowTitle = "OotD Positioning Helper";

    private const uint CS_HREDRAW = 0x2;
    private const uint CS_VREDRAW = 0x1;
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOACTIVATE = 0x10;
    private const int SWP_NOOWNERZORDER = 0x0200;
    private const int SWP_NOSENDCHANGING = 0x0400;
    private const uint ZPOS_FLAGS = (uint)(SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSENDCHANGING);

    private static IntPtr _helperWindow;
    private static GCHandle _wndProcHandle;

    /// <summary>
    ///     Creates the hidden helper window used to anchor pinned windows. Must be called once on
    ///     a thread with a message loop before any pinning occurs. Safe to call multiple times.
    /// </summary>
    internal static void Initialize()
    {
        if (_helperWindow != IntPtr.Zero)
        {
            return;
        }

        try
        {
            UnsafeNativeMethods.WndProc wndProc = AnchorWndProc;
            _wndProcHandle = GCHandle.Alloc(wndProc);

            var wc = new UnsafeNativeMethods.WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf<UnsafeNativeMethods.WNDCLASSEX>(),
                style = CS_HREDRAW | CS_VREDRAW,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc),
                hInstance = UnsafeNativeMethods.GetModuleHandleA(null),
                lpszClassName = AnchorClassName
            };

            // Register by reference so the marshaller keeps the struct (and its class-name string)
            // pinned and valid for the duration of the call. Copying to unmanaged memory first causes
            // RegisterClassEx to fail with ERROR_INVALID_PARAMETER.
            var atom = UnsafeNativeMethods.RegisterClassEx(ref wc);

            _helperWindow = UnsafeNativeMethods.CreateWindowEx(
                UnsafeNativeMethods.WS_EX_TOOLWINDOW,
                AnchorClassName,
                HelperWindowTitle,
                UnsafeNativeMethods.WS_POPUP | UnsafeNativeMethods.WS_DISABLED,
                UnsafeNativeMethods.CW_USEDEFAULT,
                UnsafeNativeMethods.CW_USEDEFAULT,
                UnsafeNativeMethods.CW_USEDEFAULT,
                UnsafeNativeMethods.CW_USEDEFAULT,
                IntPtr.Zero,
                IntPtr.Zero,
                wc.hInstance,
                IntPtr.Zero);

            if (_helperWindow != IntPtr.Zero)
            {
                // Start at the very bottom of the normal z-order band.
                SetHelperPosition(hwndInsertAfter: new IntPtr(1));
            }
        }
        catch
        {
            _helperWindow = IntPtr.Zero;
        }
    }

    /// <summary>
    ///     The window that pinned windows should be inserted directly behind. Returns the helper
    ///     window when it has been created, otherwise IntPtr.Zero (callers fall back to
    ///     HWND_BOTTOM).
    /// </summary>
    internal static IntPtr GetPinnedAnchorWindow()
    {
        return _helperWindow;
    }

    private static void SetHelperPosition(IntPtr hwndInsertAfter)
    {
        if (_helperWindow != IntPtr.Zero)
        {
            UnsafeNativeMethods.SetWindowPos(_helperWindow, hwndInsertAfter, 0, 0, 0, 0, ZPOS_FLAGS);
        }
    }

    private static nint AnchorWndProc(IntPtr hWnd, uint uMsg, nint wParam, nint lParam)
    {
        // Keep the helper from ever changing z-order on its own.
        if (uMsg == 0x46 /* WM_WINDOWPOSCHANGING */)
        {
            var wp = Marshal.PtrToStructure<UnsafeNativeMethods.WINDOWPOS>(lParam);
            wp.flags |= UnsafeNativeMethods.SWP_NOZORDER;
            Marshal.StructureToPtr(wp, lParam, true);
            return 0;
        }

        return UnsafeNativeMethods.DefWindowProc(hWnd, uMsg, wParam, lParam);
    }
}
