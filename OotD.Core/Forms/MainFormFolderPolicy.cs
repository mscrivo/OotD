// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using OotD.Enums;

namespace OotD.Forms;

/// <summary>
///     Pure folder name/path matching extracted from the <see cref="MainForm" /> view for testing.
/// </summary>
internal static class MainFormFolderPolicy
{
    /// <summary>
    ///     Returns the <see cref="FolderViewType" /> whose default folder name matches
    ///     <paramref name="folderName" /> (checked in <see cref="DefaultFolderViewTypes" /> order), or null
    ///     when it matches none -- i.e. the selected folder is a custom folder.
    /// </summary>
    internal static FolderViewType? MatchFolderViewTypeByName(
        string? folderName, IReadOnlyDictionary<FolderViewType, string?> knownFolderNames)
    {
        foreach (var type in DefaultFolderViewTypes)
        {
            if (knownFolderNames.TryGetValue(type, out var name) &&
                string.Equals(folderName, name, StringComparison.Ordinal))
            {
                return type;
            }
        }

        return null;
    }

    /// <summary>The default folder view types, in the precedence used when matching a folder by name.</summary>
    internal static readonly FolderViewType[] DefaultFolderViewTypes =
    [
        FolderViewType.Calendar,
        FolderViewType.Contacts,
        FolderViewType.Inbox,
        FolderViewType.Notes,
        FolderViewType.Tasks,
        FolderViewType.Todo
    ];

    /// <summary>
    /// </summary>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    internal static string GetFolderNameFromFullPath(string? fullPath)
    {
        if (fullPath != null)
        {
            return fullPath.Substring(fullPath.LastIndexOf('\\') + 1,
                fullPath.Length - fullPath.LastIndexOf('\\') - 1);
        }

        return string.Empty;
    }

    internal static string GetFolderPath(string folderPath)
    {
        return folderPath.Replace("\\\\Personal Folders\\", "");
    }
}
