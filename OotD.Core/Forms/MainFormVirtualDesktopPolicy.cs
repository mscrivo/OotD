// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace OotD.Forms;

/// <summary>
///     Pure virtual-desktop assignment extracted from the <see cref="MainForm" /> view for testing.
/// </summary>
internal static class MainFormVirtualDesktopPolicy
{
    internal static Guid? GetAssignedVirtualDesktopId(string? virtualDesktopId)
    {
        return !string.IsNullOrEmpty(virtualDesktopId) &&
               Guid.TryParse(virtualDesktopId, out var desktopId) &&
               desktopId != Guid.Empty
            ? desktopId
            : null;
    }

    internal static bool ShouldHideFromAltTab(string? virtualDesktopId)
    {
        return !GetAssignedVirtualDesktopId(virtualDesktopId).HasValue;
    }
}
