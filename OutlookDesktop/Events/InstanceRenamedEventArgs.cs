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

        public string OldInstanceName { get; private set; }

        public string NewInstanceName { get; private set; }
    }
}