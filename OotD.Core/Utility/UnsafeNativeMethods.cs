// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable IdentifierTypo
#pragma warning disable IDE1006 // Naming Styles

namespace OotD.Utility;

internal static class UnsafeNativeMethods
{
    private static readonly nint HWND_BOTTOM = new(1);
    private static readonly nint HWND_TOPMOST = new(-1);
    private static readonly nint HWND_NOTTOPMOST = new(-2);

    private const int SWP_NOACTIVATE = 0x10;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOSIZE = 0x0001;
    internal const int SWP_NOZORDER = 0x0004;
    internal const int SWP_NOOWNERZORDER = 0x0200;
    internal const int SWP_NOSENDCHANGING = 0x0400;

    private const int ZPOS_FLAGS = SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_NOSENDCHANGING;

    private const int DWMWA_EXCLUDED_FROM_PEEK = 12;

    internal const int WM_PARENTNOTIFY = 0x0210;
    internal const int WM_NCACTIVATE = 0x86;
    internal const int WM_RBUTTONDOWN = 0x0204;
    internal const int WM_LBUTTONDOWN = 0x0201;
    internal const int WM_WINDOWPOSCHANGING = 70;
    internal const int WM_WINDOWPOSCHANGED = 71;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWPOS
    {
        internal nint hwnd;
        internal nint hwndInsertAfter;
        internal int x;
        internal int y;
        internal int cx;
        internal int cy;
        internal int flags;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

    [DllImport("dwmapi.dll", PreserveSig = false)]
    private static extern void DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern nint FindWindowEx(nint parentHandle, nint childAfter, string className, string windowTitle);

    [DllImport("user32.dll")]
    internal static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    internal static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        nint hWnd,
        nint hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);

    [DllImport("user32.dll")]
    internal static extern short GetAsyncKeyState(Keys vKey);

    /// <summary>
    /// This will send the specified window to the bottom of the z-order, so that it's effectively behind every other window.
    /// This only works for Vista or higher and when Aero is disabled, so the code checks for that condition.
    /// </summary>
    /// <param name="windowToSendBack">the form to work with</param>
    /// <param name="caller"></param>
    internal static void SendWindowToBack(Form windowToSendBack, [CallerMemberName] string? caller = null)
    {
        Debug.WriteLine($"Sending to bottom from {caller}");
        SetWindowPos(windowToSendBack.Handle, HWND_BOTTOM, 0, 0, 0, 0, ZPOS_FLAGS);
    }

    /// <summary>
    /// This will send the specified window to not the top most of the z-order. This effectively undoes
    /// setting it to top-most.
    /// </summary>
    /// <param name="window"></param>
    /// <param name="caller"></param>
    internal static void SendWindowToNotTopMost(Form window, [CallerMemberName] string? caller = null)
    {
        Debug.WriteLine($"Sending to not top most from {caller}");
        SetWindowPos(window.Handle, HWND_NOTTOPMOST, 0, 0, 0, 0, ZPOS_FLAGS);
    }

    /// <summary>
    /// This will send the specified window to the top of the z-order, so that it's effectively on top of every other window.
    /// </summary>
    /// <param name="windowToSendToTop">the form to work with</param>
    /// <param name="caller"></param>
    internal static void SendWindowToTop(Form windowToSendToTop, [CallerMemberName] string? caller = null)
    {
        Debug.WriteLine($"Sending to top most from {caller}");
        SetWindowPos(windowToSendToTop.Handle, HWND_TOPMOST, 0, 0, 0, 0, ZPOS_FLAGS);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    private const int GW_HWNDPREV = 3;
    private const int GWL_STYLE = -16;
    private const int WS_EX_TOPMOST = 0x0008;
    private const int WS_VISIBLE = 0x10000000;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    /// <summary>
    /// Helper method to make calling GetClassName easier
    /// </summary>
    /// <param name="hwnd"></param>
    /// <returns></returns>
    internal static string GetWindowClass(IntPtr hwnd)
    {
        var _sb = new StringBuilder(32);
        _ = GetClassName(hwnd, _sb, _sb.Capacity);
        return _sb.ToString();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    internal static IntPtr ShellWindow;

    internal static IntPtr GetDefaultShellWindow()
    {
        var shellWindow = GetShellWindow();
        if (shellWindow != IntPtr.Zero)
        {
            if (shellWindow == ShellWindow)
            {
                return shellWindow;
            }

            if (!GetWindowClass(shellWindow).Equals("Progman"))
            {
                ShellWindow = IntPtr.Zero;
            }
        }

        ShellWindow = shellWindow;
        return shellWindow;
    }

    internal static bool BelongToSameProcess(IntPtr hwndA, IntPtr hwndB)
    {
        _ = GetWindowThreadProcessId(hwndA, out var procAId);
        _ = GetWindowThreadProcessId(hwndB, out var procBId);

        return procAId == procBId;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint wCmd);

    public static bool IsWindowInFrontOfOtherWindows(IntPtr hWnd)
    {
        var prevWindow = GetWindow(hWnd, GW_HWNDPREV);

        Debug.WriteLine($"Checking if window is above others, prev wnd is: {GetWindowClass(prevWindow)}");

        while (prevWindow != IntPtr.Zero)
        {
            if (IsWindowVisible(prevWindow) && !BelongToSameProcess(hWnd, prevWindow) && Window.IsOverlapped(prevWindow))
            {
                Debug.WriteLine($"{GetWindowClass(prevWindow)} is visible, does not belong to our process and is overlapping ...");

                var style = GetWindowLong(prevWindow, GWL_STYLE);
                if ((style & WS_VISIBLE) == WS_VISIBLE && (style & WS_EX_TOPMOST) != WS_EX_TOPMOST)
                {
                    Debug.WriteLine($"{GetWindowClass(prevWindow)} is also not top most, so our window must be in front of it");
                    return true;
                }
            }

            prevWindow = GetWindow(prevWindow, GW_HWNDPREV);
        }

        Debug.WriteLine("no other visible windows are behind the current window, it must be in front of all other windows");
        return false;
    }

    /// <summary>
    /// Does not hide the calendar when the user hovers their mouse over the "Show Desktop" button 
    /// in Windows 7.
    /// </summary>
    /// <param name="window"></param>
    internal static void RemoveWindowFromAeroPeek(IWin32Window window)
    {
        var renderPolicy = (int)DwmNCRenderingPolicy.Enabled;

        DwmSetWindowAttribute(window.Handle, DWMWA_EXCLUDED_FROM_PEEK, ref renderPolicy, sizeof(int));
    }

    private enum DwmNCRenderingPolicy
    {
        UseWindowStyle,
        Disabled,
        Enabled,
        Last
    }

    /// <summary>
    ///     VK is just a placeholder for VK (VirtualKey) general definitions
    /// </summary>
    internal class VK
    {
        internal const int VK_ESCAPE = 0x1B;
    }

    /// <summary>
    ///     WM is just a placeholder class for WM (WindowMessage) definitions
    /// </summary>
    internal class WM
    {
        internal const int WM_MOUSEMOVE = 0x0200;
        internal const int WM_NCLBUTTONDOWN = 0x00A1;
        internal const int WM_LBUTTONUP = 0x0202;
        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP = 0x0101;
    }

    /// <summary>
    ///     HT is just a placeholder for HT (HitTest) definitions
    /// </summary>
    internal class HT
    {
        internal const int HTCAPTION = 2;
        internal const int HTLEFT = 10;
        internal const int HTRIGHT = 11;
        internal const int HTTOP = 12;
        internal const int HTTOPLEFT = 13;
        internal const int HTTOPRIGHT = 14;
        internal const int HTBOTTOM = 15;
        internal const int HTBOTTOMLEFT = 16;
        internal const int HTBOTTOMRIGHT = 17;
    }

    internal class Bit
    {
        internal static int HiWord(int iValue)
        {
            return (iValue >> 16) & 0xFFFF;
        }

        internal static int LoWord(int iValue)
        {
            return iValue & 0xFFFF;
        }
    }

    internal static class HookManager
    {
        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const int WINEVENT_SKIPOWNPROCESS = 2;

        internal const int EVENT_SYSTEM_FOREGROUND = 3;

        internal static IntPtr SubscribeToWindowEvents(WinEventProc lpfnWinEventProc)
        {
            if (windowEventHook != IntPtr.Zero)
            {
                return windowEventHook;
            }

            windowEventHook = SetWinEventHook(
                EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero,
                lpfnWinEventProc,
                0,
                0,
                WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

            if (windowEventHook == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return windowEventHook;
        }

        private static IntPtr windowEventHook;

        internal delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc,
            WinEventProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, KbHook lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        internal const int HC_ACTION = 0;
        internal const int WH_KEYBOARD_LL = 13;

        internal delegate IntPtr KbHook(int nCode, IntPtr wParam, [In] IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        internal readonly struct KbHookParam
        {
            internal readonly int VkCode;
            internal readonly int ScanCode;
            internal readonly int Flags;
            internal readonly int Time;
            internal readonly IntPtr Extra;
        }
    }

    public static class Window
    {
        public static bool IsOverlapped(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                throw new InvalidOperationException("Window does not yet exist");
            if (!IsWindowVisible(hWnd))
                return false;

            var visited = new HashSet<IntPtr> { hWnd };

            GetWindowRect(hWnd, out var thisRect);

            while ((hWnd = GetWindow(hWnd, GW_HWNDPREV)) != IntPtr.Zero && !visited.Contains(hWnd))
            {
                visited.Add(hWnd);
                if (IsWindowVisible(hWnd) && GetWindowRect(hWnd, out var testRect) && IntersectRect(out _, ref thisRect, ref testRect))
                    return true;
            }

            return false;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, [Out] out RECT lpRect);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IntersectRect([Out] out RECT lprcDst, [In] ref RECT lprcSrc1, [In] ref RECT lprcSrc2);

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct RECT
        {
            private readonly int left;
            private readonly int top;
            private readonly int right;
            private readonly int bottom;
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles
