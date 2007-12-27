using System;
using System.Collections.Generic;
using System.Text;

namespace OutlookDesktop
{
    public class InstanceRenamedEventArgs : EventArgs
    {
        public String OldInstanceName
        {
            get { return _oldInstanceName; }
            set { _oldInstanceName = value; }
        }
        private String _oldInstanceName;

        public String NewInstanceName
        {
            get { return _newInstanceName; }
            set { _newInstanceName = value; }
        }
        private String _newInstanceName;

        public InstanceRenamedEventArgs(String oldInstanceName, String newInstanceName)
        {
            _oldInstanceName = oldInstanceName;
            _newInstanceName = newInstanceName;
        }
    }
}
