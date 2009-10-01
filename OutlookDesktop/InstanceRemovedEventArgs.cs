using System;
using System.Collections.Generic;
using System.Text;

namespace OutlookDesktop
{
    public class InstanceRemovedEventArgs : EventArgs
    {
        public String InstanceName { get; set; }

        public InstanceRemovedEventArgs(String instanceName)
        {
            InstanceName = instanceName;
        }
    }
}
