// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Forms;

namespace OotD.Preferences;

/// <summary>
///     Single source of truth for the root registry key (under HKEY_CURRENT_USER) that holds all
///     OotD preferences. The global settings live directly under this key and each window
///     instance's settings live in a subkey named after the instance.
///     <para>
///         The path is overridable so tests can redirect preference storage to a throwaway key
///         instead of the real user's settings. Production code never sets it.
///     </para>
/// </summary>
internal static class PreferencesRegistry
{
    /// <summary>
    ///     The HKCU-relative path to the OotD preferences root, e.g.
    ///     <c>Software\SMR Computer Services\Outlook on the Desktop</c>.
    /// </summary>
    internal static string RootPath { get; set; } =
        $@"Software\{Application.CompanyName}\{Application.ProductName}";
}
