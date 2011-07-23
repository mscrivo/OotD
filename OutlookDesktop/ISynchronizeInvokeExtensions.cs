using System;
using System.ComponentModel;

namespace OutlookDesktop
{
    /// <summary>
    /// Extension method to allow accessing properties and fields in a worker thread without throwing:
    /// "Cross-thread operation not valid: Control 'MainForm' accessed from a thread other than the thread it was created on."
    /// </summary>
    public static class ISynchronizeInvokeExtensions
    {
        public static void InvokeEx<T>(this T @this, Action<T> action) where T : ISynchronizeInvoke
        {
            if (@this.InvokeRequired)
            {
                @this.Invoke(action, new object[] { @this });
            }
            else
            {
                action(@this);
            }
        }
    }
}
