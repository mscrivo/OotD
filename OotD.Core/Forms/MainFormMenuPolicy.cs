namespace OotD.Forms;

/// <summary>
///     Pure hide/show and enable/disable menu toggle state extracted from the <see cref="MainForm" /> view for testing.
/// </summary>
internal static class MainFormMenuPolicy
{
    internal static VisibilityToggleState GetNextVisibilityState(bool currentlyVisible, string showText,
        string hideText)
    {
        return currentlyVisible
            ? new VisibilityToggleState(false, showText)
            : new VisibilityToggleState(true, hideText);
    }

    internal readonly record struct VisibilityToggleState(bool Visible, string MenuText);

    internal static EditingToggleState GetNextEditingState(bool currentlyEnabled)
    {
        return currentlyEnabled
            ? new EditingToggleState(false, true, true)
            : new EditingToggleState(true, false, false);
    }

    internal readonly record struct EditingToggleState(bool Enabled, bool MenuChecked, bool DisableEditingPreference);
}
