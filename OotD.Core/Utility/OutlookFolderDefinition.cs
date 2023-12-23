// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace OotD.Utility;

/// <summary>
/// Structure to hold the currently selected custom folder details
/// </summary>
internal struct OutlookFolderDefinition
{
    public string? OutlookFolderStoreId { get; set; }
    public string? OutlookFolderName { get; set; }
    public string? OutlookFolderEntryId { get; set; }
}
