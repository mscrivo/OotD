// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace OotD.Events
{
    /// <summary>
    /// EventArgs used to Validate an InputBox
    /// </summary>
    public class InputBoxValidatingEventArgs : EventArgs
    {
        public string? Text { get; init; }

        public string? Message { get; set; }

        public bool Cancel { get; set; }
    }
}
