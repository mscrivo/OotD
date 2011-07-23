using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OutlookDesktop.Properties;
using BitFactory.Logging;

namespace OutlookDesktop
{
    internal static class UnsafeNativeMethods
    {
        private const int HWND_BOTTOM = 0x1;
        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_NOMOVE = 0x2;
        private const int SWP_NOSIZE = 0x1;
        private const int DWMWA_EXCLUDED_FROM_PEEK = 12;

        public const int WM_NCLBUTTONDOWN = 0x00A1;
        public const int HTBORDER = 18;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;
        public const int HTCAPTION = 2;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;

        private enum DwmNCRenderingPolicy
        {
            UseWindowStyle,
            Disabled,
            Enabled,
            Last
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
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
            IntPtr hWnd, // window handle
            int hWndInsertAfter, // placement-order handle
            int X, // horizontal position
            int Y, // vertical position
            int cx, // width
            int cy, // height
            uint uFlags); // window positioning flags

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
                ConfigLogger.Instance.LogError(String.Format("Error pinning window to desktop, OS: {0}.",Environment.OSVersion.Version));
                MessageBox.Show(form, Resources.ErrorInitializingApp + Environment.NewLine + ex.Message, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                             SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            }
        }

        /// <summary>
        /// Does not hide the calendar when the user hovers their mouse over the "Show Desktop" button 
        /// in Windows 7.
        /// </summary>
        /// <param name="window"></param>
        public static void RemoveWindowFromAeroPeek(Form window)
        {
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1 && DwmIsCompositionEnabled())
            {
                var renderPolicy = (int)DwmNCRenderingPolicy.Enabled;

                DwmSetWindowAttribute(window.Handle, DWMWA_EXCLUDED_FROM_PEEK, ref renderPolicy, sizeof(int));
            }
        }
    }
}