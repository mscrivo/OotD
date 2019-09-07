using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NLog;
using OotD.Properties;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace OotD.Utility
{
    internal static class UnsafeNativeMethods
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);

        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_SHOWWINDOW = 0x00040;
        public const int SWP_NOOWNERZORDER = 0x0200;
        public const int SWP_NOSENDCHANGING = 0x0400;

        private const int DWMWA_EXCLUDED_FROM_PEEK = 12;

        public const int WM_NCLBUTTONDOWN = 0x00A1;
        public const int WM_PARENTNOTIFY = 0x0210;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_MOUSELEAVE = 0x02A3;
        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_ACTIVATE = 0x6;
        public const int WM_ACTIVATEAPP = 0x1C;
        public const int WM_NCACTIVATE = 0x86;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_WINDOWPOSCHANGING = 70;
        public const int WM_NCHITTEST = 0x84;


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
        public static extern bool DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr GetDesktopWindow();

        public static void PinWindowToDesktop(Form form)
        {
            // for XP and 2000, the following hack pins the window to the desktop quite nicely
            // (ie. pressing show desktop still shows, the calendar), but it can only be called 
            // once during init for it to work.
            try
            {
                form.SendToBack();
                IntPtr pWnd = FindWindow("Progman", null!);
                SetParent(form.Handle, pWnd);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error pinning window to desktop, OS: {Environment.OSVersion.Version}.");
                MessageBox.Show(form, Resources.ErrorInitializingApp + Environment.NewLine + ex.Message,
                                Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// This will send the specified window to the bottom of the z-order, so that it's effectively behind every other window.
        /// This only works for Vista or higher and when Aero is disabled, so the code checks for that condition.
        /// </summary>
        /// <param name="windowToSendBack">the form to work with</param>
        public static void SendWindowToBack(Form windowToSendBack)
        {
            SetWindowPos(windowToSendBack.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
        }

        /// <summary>
        /// This will send the specified window to the top of the z-order, so that it's effectively on top of every other window.
        /// </summary>
        /// <param name="windowToSendToTop">the form to work with</param>
        public static void SendWindowToTop(Form windowToSendToTop)
        {
            SetWindowPos(windowToSendToTop.Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
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

        public static bool Is64Bit()
        {
            return Marshal.SizeOf(typeof(IntPtr)) == 8;
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