// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

    internal static (CurrentCalendarView type, int offset) GetNextPreviousOffsetBasedOnCalendarViewMode(
        CurrentCalendarView mode)
    {
        var offset = mode switch
        {
            CurrentCalendarView.Day => (CurrentCalendarView.Day, 1),
            CurrentCalendarView.Week => (CurrentCalendarView.Week, 7),
            CurrentCalendarView.WorkWeek => (CurrentCalendarView.WorkWeek, 7),
            CurrentCalendarView.Month => (CurrentCalendarView.Month, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        return offset;
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
