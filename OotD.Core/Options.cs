// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace OotD;

// ReSharper disable once ClassNeverInstantiated.Global
#pragma warning disable IDE0079 // Remove unnecessary suppression
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
public class Options
{
    [Option('d', "debug", Default = false, HelpText = "Start Debugger.")]
    public bool StartDebugger { get; set; }

    [Option('s', "createStartupEntry", Default = false, HelpText = "Create scheduled task to start OotD at logon.")]
    public bool CreateStartupEntry { get; set; }

    [Option('r', "removeStartupEntry", Default = false, HelpText = "Remove scheduled task to start OotD at logon.")]
    public bool RemoveStartupEntry { get; set; }
}
