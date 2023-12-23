// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
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

internal static partial class UnsafeNativeMethods
{
    private static readonly nint HWND_BOTTOM = new(1);
    private static readonly nint HWND_TOPMOST = new(-1);

    private const int SWP_NOACTIVATE = 0x10;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOSIZE = 0x0001;
    internal const int SWP_NOZORDER = 0x0004;
    internal const int SWP_NOOWNERZORDER = 0x0200;
    internal const int SWP_NOSENDCHANGING = 0x0400;

    private const int ZPOS_FLAGS = SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSENDCHANGING;

    private const int DWMWA_EXCLUDED_FROM_PEEK = 12;

    internal const int WM_PARENTNOTIFY = 0x0210;
    internal const int WM_NCACTIVATE = 0x86;
    internal const int WM_RBUTTONDOWN = 0x0204;
    internal const int WM_WINDOWPOSCHANGING = 70;

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

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);

    [LibraryImport("user32.dll", EntryPoint = "ReleaseCapture")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ReleaseCapture();

    [LibraryImport("user32.dll", EntryPoint = "SendMessageA")]
    internal static partial nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial void SetWindowPos(IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);

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
    /// This will send the specified window to the top of the z-order, so that it's effectively on top of every other window.
    /// </summary>
    /// <param name="windowToSendToTop">the form to work with</param>
    /// <param name="caller"></param>
    internal static void SendWindowToTop(Form windowToSendToTop, [CallerMemberName] string? caller = null)
    {
        Debug.WriteLine($"Sending to top most from {caller}");
        SetWindowPos(windowToSendToTop.Handle, HWND_TOPMOST, 0, 0, 0, 0, ZPOS_FLAGS);
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int GetWindowLong(IntPtr hWnd, int nIndex);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetWindow(IntPtr hWnd, uint wCmd);

    /// <summary>
    /// Does not hide the calendar when the user hovers their mouse over the "Show Desktop" button 
    /// in Windows 7.
    /// </summary>
    /// <param name="window"></param>
    internal static void RemoveWindowFromAeroPeek(IWin32Window window)
    {
        var renderPolicy = (int)DwmNCRenderingPolicy.Enabled;
        Marshal.ThrowExceptionForHR(
        DwmSetWindowAttribute(window.Handle, DWMWA_EXCLUDED_FROM_PEEK, ref renderPolicy, sizeof(int)));
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
}
#pragma warning restore IDE1006 // Naming Styles
