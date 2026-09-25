// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OotD.Forms;

/// <summary>
///     Pure window geometry: screen bounds, resize direction, cursor and opacity extracted from the <see cref="MainForm" /> view for testing.
/// </summary>
internal static class MainFormWindowPolicy
{
    internal static bool IsWindowOnAnyScreen(Rectangle windowBounds, IEnumerable<Rectangle> screenAreas)
    {
        return screenAreas.Any(area => area.IntersectsWith(windowBounds));
    }

    /// <summary>
    ///     Returns the resize direction for a point in form client coordinates. Edges are grabbable within
    ///     <paramref name="edgeWidth" /> of the form's outer edge; corners also extend <paramref name="cornerLength" />
    ///     along each adjoining edge so they're easier to hit.
    /// </summary>
    internal static ResizeDirection GetResizeDirection(Point location, Size formSize, bool lockPosition,
        int edgeWidth = DefaultResizeEdgeWidth, int cornerLength = DefaultResizeEdgeWidth)
    {
        if (lockPosition)
        {
            return ResizeDirection.None;
        }

        var nearLeft = location.X < edgeWidth;
        var nearRight = location.X >= formSize.Width - edgeWidth;
        var nearTop = location.Y < edgeWidth;
        var nearBottom = location.Y >= formSize.Height - edgeWidth;

        var alongLeft = location.X < cornerLength;
        var alongRight = location.X >= formSize.Width - cornerLength;
        var alongTop = location.Y < cornerLength;
        var alongBottom = location.Y >= formSize.Height - cornerLength;

        if ((nearTop && alongLeft) || (nearLeft && alongTop))
        {
            return ResizeDirection.TopLeft;
        }

        if ((nearBottom && alongLeft) || (nearLeft && alongBottom))
        {
            return ResizeDirection.BottomLeft;
        }

        if ((nearBottom && alongRight) || (nearRight && alongBottom))
        {
            return ResizeDirection.BottomRight;
        }

        if ((nearTop && alongRight) || (nearRight && alongTop))
        {
            return ResizeDirection.TopRight;
        }

        if (nearLeft)
        {
            return ResizeDirection.Left;
        }

        if (nearRight)
        {
            return ResizeDirection.Right;
        }

        if (nearTop)
        {
            return ResizeDirection.Top;
        }

        return nearBottom ? ResizeDirection.Bottom : ResizeDirection.None;
    }

    private const int DefaultResizeEdgeWidth = 4;

    internal static Cursor GetCursorForResizeDirection(ResizeDirection resizeDirection)
    {
        return resizeDirection switch
        {
            ResizeDirection.Left => Cursors.SizeWE,
            ResizeDirection.Right => Cursors.SizeWE,
            ResizeDirection.Top => Cursors.SizeNS,
            ResizeDirection.Bottom => Cursors.SizeNS,
            ResizeDirection.BottomLeft => Cursors.SizeNESW,
            ResizeDirection.TopRight => Cursors.SizeNESW,
            ResizeDirection.BottomRight => Cursors.SizeNWSE,
            ResizeDirection.TopLeft => Cursors.SizeNWSE,
            _ => Cursors.Default
        };
    }

    internal enum ResizeDirection
    {
        None = 0,
        Left = 1,
        TopLeft = 2,
        Top = 3,
        TopRight = 4,
        Right = 5,
        BottomRight = 6,
        Bottom = 7,
        BottomLeft = 8
    }

    internal static double NormalizeOpacityPercentage(decimal percentage)
    {
        var opacityVal = (double)(percentage / 100);
        return Math.Abs(opacityVal - 1) < double.Epsilon ? 0.99 : opacityVal;
    }
}
