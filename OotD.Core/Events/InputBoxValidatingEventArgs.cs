using System;

namespace OotD.Events;

/// <summary>
///     EventArgs used to Validate an InputBox
/// </summary>
public class InputBoxValidatingEventArgs : EventArgs
{
    public string? Text { get; init; }

    public string? Message { get; set; }

    public bool Cancel { get; set; }
}
