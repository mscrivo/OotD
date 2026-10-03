using System;
using System.Xml.Linq;
using OotD.Enums;

namespace OotD.Forms;

/// <summary>
///     Pure calendar navigation date/offset math extracted from the <see cref="MainForm" /> view for testing.
/// </summary>
internal static class MainFormCalendarNavigation
{
    internal static DateTime GetCalendarNavigationTargetDate(DateTime selectedDate, CurrentCalendarView mode,
        int offset)
    {
        return mode == CurrentCalendarView.Month
            ? selectedDate.AddMonths(offset)
            : selectedDate.AddDays(offset);
    }

    internal static int GetNextPreviousOffsetBasedOnCalendarViewMode(CurrentCalendarView mode)
    {
        return mode switch
        {
            CurrentCalendarView.Day or CurrentCalendarView.Month => 1,
            CurrentCalendarView.Week or CurrentCalendarView.WorkWeek => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    internal static CurrentCalendarView GetCalendarViewModeFromViewXml(string viewXml)
    {
        var mode = CurrentCalendarView.Day;

        var xElement = XDocument.Parse(viewXml).Element("view");
        var element = xElement?.Element("mode");
        if (element != null)
        {
            mode = (CurrentCalendarView)Convert.ToInt32(element.Value);
        }

        return mode;
    }

    internal static bool ShouldReactivateViewControl(int instanceCount, Guid buttonId, Guid lastButtonGuidClicked)
    {
        return instanceCount != 1 && buttonId != lastButtonGuidClicked;
    }
}
