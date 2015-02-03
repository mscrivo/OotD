using System;

namespace OutlookDesktop.Events
{
    public class InstanceRemovedEventArgs : EventArgs
    {
        public InstanceRemovedEventArgs(String instanceName)
        {
            InstanceName = instanceName;
        }

        public String InstanceName { get; private set; }
    }
}