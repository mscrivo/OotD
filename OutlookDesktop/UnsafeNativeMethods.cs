using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BitFactory.Logging;
using OutlookDesktop.Properties;

namespace OutlookDesktop
{
    internal static class UnsafeNativeMethods
    {
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOZORDER = 0x0004;

        private const int DWMWA_EXCLUDED_FROM_PEEK = 12;

        public const int WM_NCLBUTTONDOWN = 0x00A1;
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
        };

        public const int WM_WINDOWPOSCHANGING =70;

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

        public static void PinWindowToDesktop(Form form)
        {
            // for XP and 2000, the following hack pins the window to the desktop quite nicely
            // (ie. pressing show desktop still shows, the calendar), but it can only be called 
            // once during init for it to work.
            try
            {
                form.SendToBack();
                IntPtr pWnd = FindWindow("Progman", null);
                SetParent(form.Handle, pWnd);
            }
            catch (Exception ex)
            {
                ConfigLogger.Instance.LogError(String.Format("Error pinning window to desktop, OS: {0}.", Environment.OSVersion.Version));
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
            if (Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled())
            {
                SetWindowPos(windowToSendBack.Handle, HWND_BOTTOM, 0, 0, 0, 0,
                             SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE );
            }
        }

        /// <summary>
        /// Does not hide the calendar when the user hovers their mouse over the "Show Desktop" button 
        /// in Windows 7.
        /// </summary>
        /// <param name="window"></param>
        public static void RemoveWindowFromAeroPeek(Form window)
        {
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1 &&
                DwmIsCompositionEnabled())
            {
                var renderPolicy = (int)DwmNCRenderingPolicy.Enabled;

                DwmSetWindowAttribute(window.Handle, DWMWA_EXCLUDED_FROM_PEEK, ref renderPolicy, sizeof(int));
            }
        }

        private enum DwmNCRenderingPolicy
        {
            UseWindowStyle,
            Disabled,
            Enabled,
            Last
        }  
    }
}