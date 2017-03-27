using System;

namespace OutlookDesktop.Events
{
    public class InstanceRenamedEventArgs : EventArgs
    {
        public InstanceRenamedEventArgs(string oldInstanceName, string newInstanceName)
        {
            OldInstanceName = oldInstanceName;
            NewInstanceName = newInstanceName;
        }

        public string OldInstanceName { get; }

        public string NewInstanceName { get; }
    }
}