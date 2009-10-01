using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OutlookDesktop.Properties;

namespace OutlookDesktop
{
    class UnsafeNativeMethods
    {

        private UnsafeNativeMethods()
        {
        }

        /// <summary>
        /// Consts to deal with window location.
        /// </summary>
        private const int SWP_DRAWFRAME = 0x20;
        private const int SWP_NOMOVE = 0x2;
        private const int SWP_NOSIZE = 0x1;
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_NOACTIVATE = 0x10;
        private const int HWND_BOTTOM = 0x1;
        private const int HWND_TOP = 0x0;
        private const int HWND_TOPMOST = -0x1;
        private const int GWL_EXSTYLE = (-20);
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const int WS_EX_APPWINDOW = 0x40000;

        /// <summary>
        /// Standard logging block.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(
           IntPtr hWnd,            // window handle
           int hWndInsertAfter,    // placement-order handle
           int X,                  // horizontal position
           int Y,                  // vertical position
           int cx,                 // width
           int cy,                 // height
           uint uFlags);           // window positioning flags


        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static bool PinWindowToDesktop(Form form)
        {
            // for XP and 2000, the following hack pins the window to the desktop quite nicely
            // (ie. pressing show desktop still shows, the calendar), but it can only be called 
            // once during init for it to work.
            try
            {
                form.SendToBack();
                IntPtr pWnd = FindWindow("Progman", null);
                SetParent(form.Handle, pWnd);
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(String.Format("Error pinning window to desktop, OS: {0}.", Environment.OSVersion.Version), Resources.ErrorInitializingApp);
                MessageBox.Show(form, Resources.ErrorInitializingApp + " " + ex, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;                
            }
        }

        public static bool SendWindowToBack(Form windowToSendBack)
        {
            // Make window "Always on Bottom" i.e. pinned to desktop, so that
            // other windows don't get trapped behind it.
            try
            {
                //windowToSendBack.SendToBack();
                //IntPtr pWnd = UnsafeNativeMethods.FindWindow("Progman", null);
                //UnsafeNativeMethods.SetParent(this.Handle, pWnd);

                //IntPtr pWnd = UnsafeNativeMethods.FindWindow("Progman", null);
                //if (pWnd == null) log.Error("Unable to find Progman");

                //pWnd = UnsafeNativeMethods.FindWindowEx(pWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                //if (pWnd == null) log.Error("Unable to find SHELLDLL_DefView");

                //pWnd = UnsafeNativeMethods.FindWindowEx(pWnd, IntPtr.Zero, "SysListView32", null);
                //if (pWnd == null) log.Error("Unable to find SysListView32");
                //IntPtr tWnd = this.Handle;
                //UnsafeNativeMethods.SetParent(tWnd, pWnd);

                if (Environment.OSVersion.Version.Major >= 6)
                {
                    // Vista or Above
                    // TODO: Find a better way, this sucks!
                    SetWindowPos(windowToSendBack.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                }

                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(String.Format("Error pinning window to desktop, OS: {0}.", Environment.OSVersion.Version), Resources.ErrorInitializingApp);
                MessageBox.Show(windowToSendBack, Resources.ErrorInitializingApp + " " + ex, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }

    }
}
