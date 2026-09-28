using System;

namespace OotD.Events;

public class InstanceRenamedEventArgs(string oldInstanceName, string newInstanceName) : EventArgs
{
    public string OldInstanceName { get; } = oldInstanceName;

    public string NewInstanceName { get; } = newInstanceName;
}
