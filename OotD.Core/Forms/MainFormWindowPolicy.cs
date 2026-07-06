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

    internal static ResizeDirection GetResizeDirection(Point location, Size formSize, bool lockPosition)
    {
        if (lockPosition)
        {
            return ResizeDirection.None;
        }

        if (location is { X: < ResizeBorderWidth, Y: < ResizeBorderWidth })
        {
            return ResizeDirection.TopLeft;
        }

        if (location.X < ResizeBorderWidth && location.Y > formSize.Height - ResizeBorderWidth)
        {
            return ResizeDirection.BottomLeft;
        }

        if (location.X > formSize.Width - ResizeBorderWidth && location.Y > formSize.Height - ResizeBorderWidth)
        {
            return ResizeDirection.BottomRight;
        }

        if (location.X > formSize.Width - ResizeBorderWidth && location.Y < ResizeBorderWidth)
        {
            return ResizeDirection.TopRight;
        }

        if (location.X < ResizeBorderWidth)
        {
            return ResizeDirection.Left;
        }

        if (location.X > formSize.Width - ResizeBorderWidth)
        {
            return ResizeDirection.Right;
        }

        if (location.Y < ResizeBorderWidth)
        {
            return ResizeDirection.Top;
        }

        if (location.Y > formSize.Height - ResizeBorderWidth)
        {
            return ResizeDirection.Bottom;
        }

        return ResizeDirection.None;
    }

    private const int ResizeBorderWidth = 4;

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
