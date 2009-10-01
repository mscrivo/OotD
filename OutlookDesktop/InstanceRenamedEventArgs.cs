using System;
using System.Collections.Generic;
using System.Text;

namespace OutlookDesktop
{
    public class InstanceRenamedEventArgs : EventArgs
    {
        public String OldInstanceName { get; set; }

        public String NewInstanceName { get; set; }

        public InstanceRenamedEventArgs(String oldInstanceName, String newInstanceName)
        {
            OldInstanceName = oldInstanceName;
            NewInstanceName = newInstanceName;
        }
    }
}
