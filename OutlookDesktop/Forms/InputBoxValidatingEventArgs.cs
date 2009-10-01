using System;

namespace OutlookDesktop.Forms
{
    /// <summary>
    /// EventArgs used to Validate an InputBox
    /// </summary>
    public class InputBoxValidatingEventArgs : EventArgs
    {
        public string Text { get; set; }

        public string Message { get; set; }

        public bool Cancel { get; set; }
    }
}