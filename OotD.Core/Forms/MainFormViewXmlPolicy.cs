// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using OotD.Enums;

namespace OotD.Forms;

/// <summary>
///     Pure view-XML persistence / folder-type view policy extracted from the <see cref="MainForm" /> view for testing.
/// </summary>
internal static class MainFormViewXmlPolicy
{
    internal static bool ShouldPersistViewXmlForFolder(string? folderName, string? calendarFolderName)
    {
        return !string.IsNullOrEmpty(folderName) &&
               !string.IsNullOrEmpty(calendarFolderName) &&
               string.Equals(folderName, calendarFolderName, StringComparison.Ordinal);
    }

    internal static string GetDefaultViewXmlForFolder(string? folderName, string? calendarFolderName,
        string monthXml)
    {
        return ShouldPersistViewXmlForFolder(folderName, calendarFolderName) ? monthXml : string.Empty;
    }

    /// <summary>
    ///     Only the Calendar keeps a custom ViewXML (its month/day view); every other folder uses the
    ///     default view. A stale calendar ViewXML left applied when switching to another folder stops the
    ///     Outlook View Control from switching on the first attempt, so it must be cleared for any
    ///     non-Calendar view.
    /// </summary>
    internal static bool ShouldClearViewXmlForFolderType(FolderViewType folderViewType)
    {
        return folderViewType != FolderViewType.Calendar;
    }

    internal static SavedViewSettings GetSavedViewSettings(string? view, string? folder, string? viewXml,
        string? calendarFolderName)
    {
        return new SavedViewSettings(view, folder,
            ShouldPersistViewXmlForFolder(folder, calendarFolderName) ? viewXml ?? string.Empty : string.Empty);
    }

    internal readonly record struct SavedViewSettings(string? OutlookFolderView, string? OutlookFolderName,
        string ViewXml);
}
