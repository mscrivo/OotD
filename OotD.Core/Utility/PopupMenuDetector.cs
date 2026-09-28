using System;
using System.Text;
using System.Windows.Forms;

namespace OotD.Utility;

/// <summary>
///     Detects whether a context menu is currently open on a thread. The Outlook view control shows its
///     context menus as popup windows on the UI thread (Win32 menus or Office command bar popups) that
///     OotD has no handle to, so this looks for any visible popup window that isn't one of OotD's forms.
/// </summary>
internal static class PopupMenuDetector
{
    private const int GWL_STYLE = -16;
    private const int WS_POPUP = unchecked((int)0x80000000);

    // Popups that can be visible without a menu being open.
    private const string TooltipClassName = "tooltips_class32";
    private static readonly string[] ImeClassNames = ["IME", "MSCTFIME UI"];

    internal static bool IsMenuOpen()
    {
        return IsMenuOpen(UnsafeNativeMethods.GetCurrentThreadId());
    }

    internal static bool IsMenuOpen(uint threadId)
    {
        var menuOpen = false;
        UnsafeNativeMethods.EnumThreadWindows(threadId, (hwnd, _) =>
        {
            menuOpen = IsMenuWindow(hwnd);
            return !menuOpen;
        }, IntPtr.Zero);

        return menuOpen;
    }

    private static bool IsMenuWindow(IntPtr hwnd)
    {
        if (!UnsafeNativeMethods.IsWindowVisible(hwnd) ||
            (UnsafeNativeMethods.GetWindowLong(hwnd, GWL_STYLE) & WS_POPUP) == 0 ||
            Control.FromHandle(hwnd) is Form)
        {
            return false;
        }

        var classNameBuilder = new StringBuilder(128);
        UnsafeNativeMethods.GetClassName(hwnd, classNameBuilder, classNameBuilder.Capacity);
        var className = classNameBuilder.ToString();

        // WinForms superclasses common controls, e.g. "WindowsForms10.tooltips_class32.app.0...".
        return !className.Contains(TooltipClassName, StringComparison.Ordinal) &&
               Array.IndexOf(ImeClassNames, className) < 0;
    }
}
