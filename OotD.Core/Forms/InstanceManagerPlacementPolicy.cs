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
    /// <summary>
    ///     Picks a location for a new instance window: the first working area (current screen first) with a
    ///     non-overlapping slot, preferring a spot flush against an existing window's edge, falling back to the
    ///     least-overlapping spot on the current screen.
    /// </summary>
    internal static Point SelectNewInstanceLocation(Rectangle currentWorkingArea,
        IEnumerable<Rectangle> allWorkingAreas, Size windowSize, IReadOnlyCollection<Rectangle> occupiedBounds)
    {
        foreach (var area in OrderWorkingAreas(currentWorkingArea, allWorkingAreas))
        {
            var snapped = FindSnappedLocation(area, windowSize, occupiedBounds);
            if (snapped.HasValue)
            {
                return snapped.Value;
            }

            var candidate = FindNonOverlappingLocation(area, windowSize, occupiedBounds);
            var candidateBounds = new Rectangle(candidate, windowSize);
            if (occupiedBounds.Any(existing => existing.IntersectsWith(candidateBounds)))
            {
                continue;
            }

            return candidate;
        }

        return FindNonOverlappingLocation(currentWorkingArea, windowSize, occupiedBounds);
    }

    /// <summary>
    ///     Finds a spot within <paramref name="workingArea" /> that sits flush against an edge of an existing window
    ///     without overlapping any of them, choosing the one closest to the center of the existing windows so the
    ///     group stays compact. Returns null when no such spot exists.
    /// </summary>
    internal static Point? FindSnappedLocation(Rectangle workingArea, Size windowSize,
        IReadOnlyCollection<Rectangle> occupiedBounds)
    {
        if (occupiedBounds.Count == 0)
        {
            return null;
        }

        var group = occupiedBounds.Aggregate(Rectangle.Union);
        var groupCenter = new Point(group.Left + group.Width / 2, group.Top + group.Height / 2);

        return occupiedBounds
            .SelectMany(anchor => GetSnapCandidates(anchor, windowSize))
            .Select((location, order) => (location, order))
            .Where(candidate =>
            {
                var bounds = new Rectangle(candidate.location, windowSize);
                return workingArea.Contains(bounds) && !occupiedBounds.Any(existing => existing.IntersectsWith(bounds));
            })
            .OrderBy(candidate => DistanceSquared(
                new Point(candidate.location.X + windowSize.Width / 2, candidate.location.Y + windowSize.Height / 2),
                groupCenter))
            .ThenBy(candidate => candidate.order)
            .Select(candidate => (Point?)candidate.location)
            .FirstOrDefault();
    }

    /// <summary>
    ///     Locations flush against each side of <paramref name="anchor" />, aligned to either end of that side.
    ///     Order is the tie-breaker: right, below, left, then above.
    /// </summary>
    internal static IEnumerable<Point> GetSnapCandidates(Rectangle anchor, Size windowSize)
    {
        yield return new Point(anchor.Right, anchor.Top);
        yield return new Point(anchor.Right, anchor.Bottom - windowSize.Height);
        yield return new Point(anchor.Left, anchor.Bottom);
        yield return new Point(anchor.Right - windowSize.Width, anchor.Bottom);
        yield return new Point(anchor.Left - windowSize.Width, anchor.Top);
        yield return new Point(anchor.Left - windowSize.Width, anchor.Bottom - windowSize.Height);
        yield return new Point(anchor.Left, anchor.Top - windowSize.Height);
        yield return new Point(anchor.Right - windowSize.Width, anchor.Top - windowSize.Height);
    }

    internal static Point FindNonOverlappingLocation(Rectangle workingArea, Size windowSize,
        IReadOnlyCollection<Rectangle> occupiedBounds)
    {
        var maxX = Math.Max(workingArea.Left, workingArea.Right - windowSize.Width);
        var maxY = Math.Max(workingArea.Top, workingArea.Bottom - windowSize.Height);

        const int PlacementStep = 30;

        var bestLocation = new Point(workingArea.Left, workingArea.Top);
        var smallestOverlapArea = int.MaxValue;

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

    private static long DistanceSquared(Point a, Point b)
    {
        long dx = a.X - b.X;
        long dy = a.Y - b.Y;
        return dx * dx + dy * dy;
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
