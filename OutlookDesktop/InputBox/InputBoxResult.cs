using System;
using System.Collections.Generic;
using System.Text;

namespace OutlookDesktop
{
    /// <summary>
    /// Class used to store the result of an InputBox.Show message.
    /// </summary>
    public class InputBoxResult
    {
        public bool Ok
        {
            get { return _Ok; }
            set { _Ok = value; }
        }
        private bool _Ok;

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        private string _text;
    }
}
