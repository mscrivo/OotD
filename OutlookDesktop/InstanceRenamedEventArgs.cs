using System;
using System.Collections.Generic;
using System.Text;

namespace OutlookDesktop
{
    public delegate void InstanceRenamedEventHandler(object sender, InstanceRenamedEventArgs e);

    public class InstanceRenamedEventArgs
    {
        public String OldInstanceName;
        public String NewInstanceName;

        public InstanceRenamedEventArgs(String oldInstanceName, String newInstanceName)
        {
            this.OldInstanceName = oldInstanceName;
            this.NewInstanceName = newInstanceName;
        }
    }
}
