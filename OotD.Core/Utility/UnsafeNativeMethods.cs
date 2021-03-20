// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles

namespace OotD.Utility
{
    internal static class UnsafeNativeMethods
    {
        private static readonly IntPtr HWND_BOTTOM = new(1);

        private static readonly IntPtr HWND_TOP = new(0);

        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_SHOWWINDOW = 0x00040;
        public const int SWP_NOOWNERZORDER = 0x0200;
        public const int SWP_NOSENDCHANGING = 0x0400;

        private const int ZPOS_FLAGS = SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_NOSENDCHANGING;

        private const int DWMWA_EXCLUDED_FROM_PEEK = 12;

        public const int WM_NCLBUTTONDOWN = 0x00A1;
        public const int WM_PARENTNOTIFY = 0x0210;
        public const int WM_NCACTIVATE = 0x86;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_WINDOWPOSCHANGING = 70;
        public const int WM_WINDOWPOSCHANGED = 0x0047;

        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;
        public const int HTCAPTION = 2;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern short GetAsyncKeyState(int vKey);

        // This helper static method is required because the 32-bit version of user32.dll does not contain this API
        // (on any versions of Windows), so linking the method will fail at run-time. The bridge dispatches the request
        // to the correct function (GetWindowLong in 32-bit mode and GetWindowLongPtr in 64-bit mode)
        public static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        public static bool PinToDesktop(Form form)
        {
            form.SendToBack();
            var hWndTmp = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", string.Empty);
            if (hWndTmp != IntPtr.Zero)
            {
                hWndTmp = FindWindowEx(hWndTmp, IntPtr.Zero, "SHELLDLL_DefView", string.Empty);
                if (hWndTmp != IntPtr.Zero)
                {
                    SetWindowLongPtr(new HandleRef(form, form.Handle), -8, (IntPtr)hWndTmp);
                    return true;
                }
            }

            RemoveWindowFromAeroPeek(form);
            return false;
        }

        /// <summary>
        /// This will send the specified window to the bottom of the z-order, so that it's effectively behind every other window.
        /// This only works for Vista or higher and when Aero is disabled, so the code checks for that condition.
        /// </summary>
        /// <param name="windowToSendBack">the form to work with</param>
        public static void SendWindowToBack(Form windowToSendBack)
        {
            SetWindowPos(windowToSendBack.Handle, HWND_BOTTOM, 0, 0, 0, 0, ZPOS_FLAGS);
        }

        /// <summary>
        /// This will send the specified window to the top of the z-order, so that it's effectively on top of every other window.
        /// </summary>
        /// <param name="windowToSendToTop">the form to work with</param>
        public static void SendWindowToTop(Form windowToSendToTop)
        {
            SetWindowPos(windowToSendToTop.Handle, HWND_TOP, 0, 0, 0, 0, ZPOS_FLAGS);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLongPtr32(hWnd, nIndex);
        }

        private const uint GW_HWNDPREV = 3;

        public const int WS_EX_TOPMOST = 0x00000008;
        static readonly int GWL_EXSTYLE = -20;

        /// <summary>
        /// This method will find the top most window and put ours just after it. This is
        /// useful when the user initiates show desktop and we want OotD to display there.
        /// </summary>
        /// <param name="form"></param>
        public static void SetBottomMost(Form form)
        {
            var winPos = form.Handle;

            while ((winPos = GetWindow(winPos, GW_HWNDPREV)) != IntPtr.Zero)
            {
                if ((GetWindowLongPtr(winPos, GWL_EXSTYLE).ToInt32() & WS_EX_TOPMOST) == WS_EX_TOPMOST)
                {
                    if (SetWindowPos(form.Handle, winPos, 0, 0, 0, 0, ZPOS_FLAGS))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Does not hide the calendar when the user hovers their mouse over the "Show Desktop" button 
        /// in Windows 7.
        /// </summary>
        /// <param name="window"></param>
        public static void RemoveWindowFromAeroPeek(Form window)
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
        public class VK
        {
            public const int VK_SHIFT = 0x10;
            public const int VK_CONTROL = 0x11;
            public const int VK_MENU = 0x12;
            public const int VK_ESCAPE = 0x1B;

            public static bool IsKeyPressed(int KeyCode)
            {
                return (GetAsyncKeyState(KeyCode) & 0x0800) == 0;
            }
        }

        /// <summary>
        ///     WM is just a placeholder class for WM (WindowMessage) definitions
        /// </summary>
        public class WM
        {
            public const int WM_MOUSEMOVE = 0x0200;
            public const int WM_NCMOUSEMOVE = 0x00A0;
            public const int WM_NCLBUTTONDOWN = 0x00A1;
            public const int WM_NCLBUTTONUP = 0x00A2;
            public const int WM_NCLBUTTONDBLCLK = 0x00A3;
            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP = 0x0202;
            public const int WM_KEYDOWN = 0x0100;
        }

        /// <summary>
        ///     HT is just a placeholder for HT (HitTest) definitions
        /// </summary>
        public class HT
        {
            public const int HTERROR = -2;
            public const int HTTRANSPARENT = -1;
            public const int HTNOWHERE = 0;
            public const int HTCLIENT = 1;
            public const int HTCAPTION = 2;
            public const int HTSYSMENU = 3;
            public const int HTGROWBOX = 4;
            public const int HTSIZE = HTGROWBOX;
            public const int HTMENU = 5;
            public const int HTHSCROLL = 6;
            public const int HTVSCROLL = 7;
            public const int HTMINBUTTON = 8;
            public const int HTMAXBUTTON = 9;
            public const int HTLEFT = 10;
            public const int HTRIGHT = 11;
            public const int HTTOP = 12;
            public const int HTTOPLEFT = 13;
            public const int HTTOPRIGHT = 14;
            public const int HTBOTTOM = 15;
            public const int HTBOTTOMLEFT = 16;
            public const int HTBOTTOMRIGHT = 17;
            public const int HTBORDER = 18;
            public const int HTREDUCE = HTMINBUTTON;
            public const int HTZOOM = HTMAXBUTTON;
            public const int HTSIZEFIRST = HTLEFT;
            public const int HTSIZELAST = HTBOTTOMRIGHT;
            public const int HTOBJECT = 19;
            public const int HTCLOSE = 20;
            public const int HTHELP = 21;
        }

        public class Bit
        {
            public static int HiWord(int iValue)
            {
                return (iValue >> 16) & 0xFFFF;
            }

            public static int LoWord(int iValue)
            {
                return iValue & 0xFFFF;
            }
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles
