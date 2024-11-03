// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace OotD.Forms;

/// <summary>
///     Class used to store the result of an InputBox.Show message.
/// </summary>
public class InputBoxResult
{
    public bool Ok { get; set; }

    public string Text { get; set; } = string.Empty;
}
