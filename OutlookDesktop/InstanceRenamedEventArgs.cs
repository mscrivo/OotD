using System;

namespace OutlookDesktop
{
    public class InstanceRenamedEventArgs : EventArgs
    {
        public InstanceRenamedEventArgs(String oldInstanceName, String newInstanceName)
        {
            OldInstanceName = oldInstanceName;
            NewInstanceName = newInstanceName;
        }

        public String OldInstanceName { get; private set; }

        public String NewInstanceName { get; private set; }
    }
}