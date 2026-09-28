using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OotD.Forms;

/// <summary>
///     Pure placement logic for newly added instances, extracted from <see cref="InstanceManager" /> for testing.
/// </summary>
internal static class InstanceManagerPlacementPolicy
{
    private const int CascadeOffset = 30;

    /// <summary>
    ///     Picks a location for a new instance window: the first working area (current screen first) with a
    ///     non-overlapping slot, preferring a cascade from the most recently placed window, falling back to the
    ///     least-overlapping spot on the current screen.
    /// </summary>
    internal static Point SelectNewInstanceLocation(Rectangle currentWorkingArea,
        IEnumerable<Rectangle> allWorkingAreas, Size windowSize, IReadOnlyCollection<Rectangle> occupiedBounds)
    {
        var preferredStart = GetCascadedStartPoint(currentWorkingArea, windowSize, occupiedBounds);

        foreach (var area in OrderWorkingAreas(currentWorkingArea, allWorkingAreas))
        {
            var preferredForArea = area.Contains(preferredStart ?? Point.Empty)
                ? preferredStart
                : null;

            var candidate = FindNonOverlappingLocation(area, windowSize, occupiedBounds, preferredForArea);
            var candidateBounds = new Rectangle(candidate, windowSize);
            if (occupiedBounds.Any(existing => existing.IntersectsWith(candidateBounds)))
            {
                continue;
            }

            return candidate;
        }

        return FindNonOverlappingLocation(currentWorkingArea, windowSize, occupiedBounds, preferredStart);
    }

    internal static Point FindNonOverlappingLocation(Rectangle workingArea, Size windowSize,
        IReadOnlyCollection<Rectangle> occupiedBounds, Point? preferredStart = null)
    {
        var maxX = Math.Max(workingArea.Left, workingArea.Right - windowSize.Width);
        var maxY = Math.Max(workingArea.Top, workingArea.Bottom - windowSize.Height);

        const int PlacementStep = 30;

        var bestLocation = new Point(workingArea.Left, workingArea.Top);
        var smallestOverlapArea = int.MaxValue;

        if (preferredStart.HasValue)
        {
            var preferred = new Point(
                Math.Min(Math.Max(preferredStart.Value.X, workingArea.Left), maxX),
                Math.Min(Math.Max(preferredStart.Value.Y, workingArea.Top), maxY));

            if (IsNonOverlappingCandidate(new Rectangle(preferred, windowSize), occupiedBounds, out var overlapArea))
            {
                return preferred;
            }

            smallestOverlapArea = overlapArea;
            bestLocation = preferred;
        }

        for (var y = workingArea.Top; y <= maxY; y += PlacementStep)
        {
            for (var x = workingArea.Left; x <= maxX; x += PlacementStep)
            {
                var candidate = new Rectangle(x, y, windowSize.Width, windowSize.Height);

                if (IsNonOverlappingCandidate(candidate, occupiedBounds, out var overlapArea))
                {
                    return candidate.Location;
                }

                if (overlapArea >= smallestOverlapArea)
                {
                    continue;
                }

                smallestOverlapArea = overlapArea;
                bestLocation = candidate.Location;
            }
        }

        return bestLocation;
    }

    internal static IReadOnlyList<Rectangle> OrderWorkingAreas(Rectangle currentWorkingArea,
        IEnumerable<Rectangle> allWorkingAreas)
    {
        return [.. allWorkingAreas.OrderByDescending(area => area == currentWorkingArea)];
    }

    internal static Point? GetCascadedStartPoint(Rectangle workingArea, Size windowSize,
        IReadOnlyCollection<Rectangle> occupiedBounds)
    {
        if (occupiedBounds.Count == 0)
        {
            return null;
        }

        var anchor = occupiedBounds
            .OrderByDescending(rect => rect.Top)
            .ThenByDescending(rect => rect.Left)
            .First();

        var maxX = Math.Max(workingArea.Left, workingArea.Right - windowSize.Width);
        var maxY = Math.Max(workingArea.Top, workingArea.Bottom - windowSize.Height);

        var x = Math.Min(Math.Max(anchor.Left + CascadeOffset, workingArea.Left), maxX);
        var y = Math.Min(Math.Max(anchor.Top + CascadeOffset, workingArea.Top), maxY);

        return new Point(x, y);
    }

    private static bool IsNonOverlappingCandidate(Rectangle candidate, IReadOnlyCollection<Rectangle> occupiedBounds,
        out int overlapArea)
    {
        overlapArea = 0;
        var intersectsExisting = false;

        foreach (var existing in occupiedBounds)
        {
            if (!candidate.IntersectsWith(existing))
            {
                continue;
            }

            intersectsExisting = true;
            var intersection = Rectangle.Intersect(candidate, existing);
            overlapArea += intersection.Width * intersection.Height;
        }

        return !intersectsExisting;
    }
}
