using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
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

    /// <summary>
    ///     OotD's built-in calendar ViewXML only sets the view mode, so the Outlook View Control falls back to its
    ///     default 30 minute time scale. Copies the time scale (<c>displaytimeunits</c>) from the user's Outlook
    ///     calendar view into <paramref name="viewXml" /> so OotD matches Outlook. Returns <paramref name="viewXml" />
    ///     unchanged when either XML is not a calendar view or Outlook's view has no usable time scale.
    /// </summary>
    internal static string ApplyOutlookTimeScale(string viewXml, string? outlookViewXml)
    {
        if (string.IsNullOrEmpty(viewXml) || string.IsNullOrEmpty(outlookViewXml))
        {
            return viewXml;
        }

        try
        {
            var outlookView = GetCalendarViewElement(XDocument.Parse(outlookViewXml));
            var timeUnits = outlookView?.Element(TimeScaleElementName)?.Value.Trim();
            if (!int.TryParse(timeUnits, NumberStyles.None, CultureInfo.InvariantCulture, out var minutes) ||
                minutes <= 0)
            {
                return viewXml;
            }

            var document = XDocument.Parse(viewXml);
            var view = GetCalendarViewElement(document);
            if (view == null)
            {
                return viewXml;
            }

            view.SetElementValue(TimeScaleElementName, minutes.ToString(CultureInfo.InvariantCulture));

            return document.Declaration != null ? document.Declaration + Environment.NewLine + document : document.ToString();
        }
        catch (XmlException)
        {
            return viewXml;
        }
    }

    private const string TimeScaleElementName = "displaytimeunits";

    private static XElement? GetCalendarViewElement(XDocument document)
    {
        var view = document.Root;
        return view is { Name.LocalName: "view" } && (string?)view.Attribute("type") == "calendar" ? view : null;
    }

    internal readonly record struct SavedViewSettings(string? OutlookFolderView, string? OutlookFolderName,
        string ViewXml);
}
