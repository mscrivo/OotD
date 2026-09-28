using System;

namespace OotD.Events;

public class InstanceRemovedEventArgs(string instanceName) : EventArgs
{
    public string InstanceName { get; } = instanceName;
}
