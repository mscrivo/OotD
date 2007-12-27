using System;
using System.Collections.Generic;
using System.Text;

namespace OutlookDesktop
{
    /// <summary>
    /// EventArgs used to Validate an InputBox
    /// </summary>
    public class InputBoxValidatingEventArgs : EventArgs
    {
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        private string _text;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        private string _message;

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
        private bool _cancel;
    }
}
