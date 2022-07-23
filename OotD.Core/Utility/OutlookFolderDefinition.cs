// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace OotD.Utility;

/// <summary>
/// Structure to hold the currently selected custom folder details
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal struct OutlookFolderDefinition
{
    public string? OutlookFolderName;
    public string? OutlookFolderStoreId;
    public string? OutlookFolderEntryId;
}