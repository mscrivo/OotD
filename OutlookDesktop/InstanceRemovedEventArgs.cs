using System;
using System.Collections.Generic;
using System.Text;

namespace OutlookDesktop
{
    public delegate void InstanceRemovedEventHandler(object sender, InstanceRemovedEventArgs e);

    public class InstanceRemovedEventArgs : EventArgs
    {
        public String InstanceName;

        public InstanceRemovedEventArgs(String instanceName)
        {
            this.InstanceName = instanceName;
        }        
    }
}
