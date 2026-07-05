// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace OotD;

/// <summary>
///     Thin x64 host process. All application logic lives in OotD.Core; this exe exists only to
///     provide a 64-bit process so that Outlook COM interop matches a 64-bit Outlook installation.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Startup.Run(args);
    }
}
