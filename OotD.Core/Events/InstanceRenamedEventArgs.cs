// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace OotD.Events;

public class InstanceRenamedEventArgs(string oldInstanceName, string newInstanceName) : EventArgs
{
    public string OldInstanceName { get; } = oldInstanceName;

    public string NewInstanceName { get; } = newInstanceName;
}
