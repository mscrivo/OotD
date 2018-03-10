using System;

namespace OotD.Events
{
    public class InstanceRemovedEventArgs : EventArgs
    {
        public InstanceRemovedEventArgs(string instanceName)
        {
            InstanceName = instanceName;
        }

        public string InstanceName { get; }
    }
}