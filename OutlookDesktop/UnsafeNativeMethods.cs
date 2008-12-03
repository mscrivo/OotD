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

        public static bool SendWindowToDesktop(Form windowToSendBack)
        {
            // Make window "Always on Bottom" i.e. pinned to desktop, so that
            // other windows don't get trapped behind it.  We do this, by attaching
            // the form to the "Progman" window, which is the main window in Windows.
            try
            {
                windowToSendBack.SendToBack();
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

                if (System.Environment.OSVersion.Version.Major < 6)
                {
                    // Older version of windows or DWM is not enabled

                    windowToSendBack.SendToBack();
                    IntPtr pWnd = UnsafeNativeMethods.FindWindow("Progman", null);
                    UnsafeNativeMethods.SetParent(windowToSendBack.Handle, pWnd);
                }
                else
                {
                    // Vista or Above
                    // TODO: Find a better way, this sucks!
                    UnsafeNativeMethods.SetWindowPos(windowToSendBack.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                }

                return true;

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error placing window to back", Resources.ErrorInitializingApp);
                MessageBox.Show(windowToSendBack, Resources.ErrorInitializingApp + " " + ex.ToString(), Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }

    }
}
