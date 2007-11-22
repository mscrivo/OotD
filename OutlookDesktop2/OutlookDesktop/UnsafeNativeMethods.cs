using System;
using System.Runtime.InteropServices;

namespace OutlookDesktop
{
    class UnsafeNativeMethods
    {

        private UnsafeNativeMethods()
        {
        }

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
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
    }
}
