// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace OotD.Forms;

/// <summary>
///     Pure toolbar button visibility extracted from the <see cref="MainForm" /> view for testing.
/// </summary>
internal static class MainFormToolbarPolicy
{
    internal static ToolbarButtonVisibility GetToolbarButtonVisibilityFor(string? defaultMessagePath)
    {
        return defaultMessagePath switch
        {
            "IPM.Appointment" => new ToolbarButtonVisibility(true, false),
            "IPM.Note" => new ToolbarButtonVisibility(false, true),
            _ => new ToolbarButtonVisibility(false, false)
        };
    }

    internal readonly record struct ToolbarButtonVisibility(bool CalendarNavigationVisible,
        bool NewEmailButtonVisible);
}
