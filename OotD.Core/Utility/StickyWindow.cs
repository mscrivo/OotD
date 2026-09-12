// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OotD.Utility;

/// <summary>
///     A windows that Sticks to other windows of the same type when moved or resized.
///     You get a nice way of organizing multiple top-level windows.
///     Quite similar with WinAmp 2.x style of sticking the windows
/// </summary>
public sealed class StickyWindow : NativeWindow
{
    /// <summary>
    ///     Global List of registered StickyWindows
    /// </summary>
    private static readonly ArrayList _globalStickyWindows = [];

    #region StickyWindow Constructor

    /// <summary>
    ///     Make the form Sticky
    /// </summary>
    /// <param name="form">Form to be made sticky</param>
    public StickyWindow(Form form)
    {
        _resizingForm = false;
        _movingForm = false;

        _originalForm = form;

        _formRect = Rectangle.Empty;

        _offsetPoint = Point.Empty;
        _mousePoint = Point.Empty;

        StickOnMove = true;
        StickOnResize = true;
        StickToScreen = true;
        StickToOther = true;

        _defaultMessageProcessor = DefaultMsgProcessor;
        _moveMessageProcessor = MoveMsgProcessor;
        _resizeMessageProcessor = ResizeMsgProcessor;
        _messageProcessor = _defaultMessageProcessor;

        AssignHandle(_originalForm.Handle);
    }

    #endregion

    // public properties

    public event EventHandler? ResizeEnded;
    public event EventHandler? MoveEnded;

    #region OnHandleChange

    protected override void OnHandleChange()
    {
        if ((int)Handle != 0)
        {
            _globalStickyWindows.Add(_originalForm);
        }
        else
        {
            _globalStickyWindows.Remove(_originalForm);
        }
    }

    #endregion

    #region WndProc

    protected override void WndProc(ref Message m)
    {
        if (!_messageProcessor(ref m))
        {
            base.WndProc(ref m);
        }
    }

    #endregion

    #region DefaultMsgProcessor

    /// <summary>
    ///     Processes messages during normal operations (while the form is not resized or moved)
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    private bool DefaultMsgProcessor(ref Message m)
    {
        switch (m.Msg)
        {
            case UnsafeNativeMethods.WM.WM_NCLBUTTONDOWN:
                {
                    _originalForm.Activate();
                    _mousePoint.X = (short)UnsafeNativeMethods.Bit.LoWord((int)m.LParam);
                    _mousePoint.Y = (short)UnsafeNativeMethods.Bit.HiWord((int)m.LParam);

                    if (OnNCLButtonDown((int)m.WParam, _mousePoint))
                    {
                        //m.Result = new IntPtr ( (resizingForm || movingForm) ? 1 : 0 );
                        m.Result = _resizingForm || _movingForm ? 1 : 0;
                        return true;
                    }

                    break;
                }
        }

        return false;
    }

    #endregion

    #region OnNCLButtonDown

    /// <summary>
    ///     Checks where the click was in the NC area and starts move or resize operation
    /// </summary>
    /// <param name="iHitTest"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    private bool OnNCLButtonDown(int iHitTest, Point point)
    {
        _offsetPoint = point;

        if (iHitTest == UnsafeNativeMethods.HT.HTCAPTION)
        {
            if (!StickOnMove)
            {
                return false;
            }

            var pointInApp = _originalForm.PointToClient(Cursor.Position);
            _offsetPoint.Offset(pointInApp.X, pointInApp.Y);
            StartMove();
            return true;
        }

        var direction = GetResizeDirection(iHitTest);
        return direction != 0 && StartResize(direction);
    }

    internal static ResizeDir GetResizeDirection(int hitTest)
    {
        return hitTest switch
        {
            UnsafeNativeMethods.HT.HTTOPLEFT => ResizeDir.Top | ResizeDir.Left,
            UnsafeNativeMethods.HT.HTTOP => ResizeDir.Top,
            UnsafeNativeMethods.HT.HTTOPRIGHT => ResizeDir.Top | ResizeDir.Right,
            UnsafeNativeMethods.HT.HTRIGHT => ResizeDir.Right,
            UnsafeNativeMethods.HT.HTBOTTOMRIGHT => ResizeDir.Bottom | ResizeDir.Right,
            UnsafeNativeMethods.HT.HTBOTTOM => ResizeDir.Bottom,
            UnsafeNativeMethods.HT.HTBOTTOMLEFT => ResizeDir.Bottom | ResizeDir.Left,
            UnsafeNativeMethods.HT.HTLEFT => ResizeDir.Left,
            _ => 0
        };
    }

    #endregion

    #region Utilities

    /// <summary>
    ///     Clamps <paramref name="iP1" /> into the closed range [<paramref name="iM1" />, <paramref name="iM2" />].
    /// </summary>
    internal static int NormalizeInside(int iP1, int iM1, int iM2)
    {
        if (iP1 <= iM1)
        {
            return iM1;
        }

        return iP1 >= iM2 ? iM2 : iP1;
    }

    #endregion

    #region Cancel

    private void Cancel()
    {
        _originalForm.Capture = false;
        _movingForm = false;
        _resizingForm = false;
        _messageProcessor = _defaultMessageProcessor;
    }

    #endregion

    private void OnResizeEnded()
    {
        ResizeEnded?.Invoke(this, EventArgs.Empty);
    }

    private void OnMoveEnded()
    {
        MoveEnded?.Invoke(this, EventArgs.Empty);
    }

    #region ResizeDir

    [Flags]
    internal enum ResizeDir
    {
        Top = 2,
        Bottom = 4,
        Left = 8,
        Right = 16
    }

    #endregion

    #region Message Processor

    // Internal Message Processor
    private delegate bool ProcessMessage(ref Message m);

    private ProcessMessage _messageProcessor;

    // Messages processors based on type
    private readonly ProcessMessage _defaultMessageProcessor;
    private readonly ProcessMessage _moveMessageProcessor;
    private readonly ProcessMessage _resizeMessageProcessor;

    #endregion

    #region Internal properties

    // Move stuff
    private bool _movingForm;
    private Point _offsetPoint; // primary offset

    // Resize stuff
    private bool _resizingForm;
    private ResizeDir _resizeDirection;
    private Point _mousePoint; // mouse position

    // General Stuff
    private readonly Form _originalForm; // the form
    private Rectangle _formRect; // form bounds
    private Rectangle _formOriginalRect; // bounds before last operation started

    #endregion

    #region Public operations and properties

    /// <summary>
    ///     Distance in pixels between two forms or a form and the screen where the sticking should start
    ///     Default value = 20
    /// </summary>
    public static int StickGap { get; set; } = 10;

    /// <summary>
    ///     Allow the form to stick while resizing
    ///     Default value = true
    /// </summary>
    public bool StickOnResize { get; set; }

    /// <summary>
    ///     Allow the form to stick while moving
    ///     Default value = true
    /// </summary>
    public bool StickOnMove { get; set; }

    /// <summary>
    ///     Allow sticking to Screen Margins
    ///     Default value = true
    /// </summary>
    public bool StickToScreen { get; set; }

    /// <summary>
    ///     Allow sticking to other StickWindows
    ///     Default value = true
    /// </summary>
    public bool StickToOther { get; set; }

    /// <summary>
    ///     Register a new form as an external reference form.
    ///     All Sticky windows will try to stick to the external references
    ///     Use this to register your MainFrame so the child windows try to stick to it, when your MainFrame is NOT a sticky
    ///     window
    /// </summary>
    /// <param name="frmExternal">External window that will be used as reference</param>
    public static void RegisterExternalReferenceForm(Form frmExternal)
    {
        _globalStickyWindows.Add(frmExternal);
    }

    /// <summary>
    ///     Unregister a form from the external references.
    ///     <see cref="RegisterExternalReferenceForm" />
    /// </summary>
    /// <param name="frmExternal">External window that will was used as reference</param>
    public static void UnregisterExternalReferenceForm(Form frmExternal)
    {
        _globalStickyWindows.Remove(frmExternal);
    }

    #endregion

    #region ResizeOperations

    private bool StartResize(ResizeDir resDir)
    {
        if (!StickOnResize)
        {
            return false; // leave default processing !
        }

        _resizeDirection = resDir;
        _formRect = _originalForm.Bounds;
        _formOriginalRect = _originalForm.Bounds; // save the old bounds

        if (!_originalForm.Capture) // start capturing messages
        {
            _originalForm.Capture = true;
        }

        _messageProcessor = _resizeMessageProcessor;

        return true; // catch the message
    }

    private bool ResizeMsgProcessor(ref Message m)
    {
        if (!_originalForm.Capture)
        {
            Cancel();
            return false;
        }

        switch (m.Msg)
        {
            case UnsafeNativeMethods.WM.WM_LBUTTONUP:
                {
                    // ok, resize finished !!!
                    EndResize();
                    break;
                }
            case UnsafeNativeMethods.WM.WM_MOUSEMOVE:
                {
                    _mousePoint.X = (short)UnsafeNativeMethods.Bit.LoWord((int)m.LParam);
                    _mousePoint.Y = (short)UnsafeNativeMethods.Bit.HiWord((int)m.LParam);
                    Resize(_mousePoint);
                    break;
                }
            case UnsafeNativeMethods.WM.WM_KEYDOWN:
                {
                    if ((int)m.WParam == UnsafeNativeMethods.VK.VK_ESCAPE)
                    {
                        _originalForm.Bounds = _formOriginalRect; // set back old size
                        Cancel();
                    }

                    break;
                }
        }

        return false;
    }

    private void EndResize()
    {
        Cancel();
        OnResizeEnded();
    }

    #endregion

    #region Resize Computing

    private void Resize(Point p)
    {
        p = _originalForm.PointToScreen(p);
        var activeScr = Screen.FromPoint(p);
        var originalBounds = _originalForm.Bounds;
        _formRect = StretchResizeBounds(originalBounds, p, _resizeDirection);

        var offsets = ComputeResizeOffsets(_formRect, activeScr.WorkingArea, GetOtherWindowBounds(),
            _resizeDirection, StickGap, StickToScreen, StickToOther);
        _formRect = ApplyResizeOffsets(originalBounds, _formRect, offsets, _resizeDirection,
            StickGap, _originalForm.MinimumSize, _originalForm.MaximumSize,
            SystemInformation.MinWindowTrackSize, SystemInformation.MaxWindowTrackSize);
        _originalForm.Bounds = _formRect;
    }

    internal static Rectangle StretchResizeBounds(Rectangle bounds, Point mousePoint, ResizeDir direction)
    {
        if ((direction & ResizeDir.Left) != 0)
        {
            bounds.Width = bounds.Right - mousePoint.X;
            bounds.X = mousePoint.X;
        }

        if ((direction & ResizeDir.Right) != 0)
        {
            bounds.Width = mousePoint.X - bounds.Left;
        }

        if ((direction & ResizeDir.Top) != 0)
        {
            bounds.Height = bounds.Bottom - mousePoint.Y;
            bounds.Y = mousePoint.Y;
        }

        if ((direction & ResizeDir.Bottom) != 0)
        {
            bounds.Height = mousePoint.Y - bounds.Top;
        }

        return bounds;
    }

    internal static Rectangle ApplyResizeOffsets(Rectangle originalBounds, Rectangle bounds, Rectangle offsets,
        ResizeDir direction, int stickGap, Size minimumSize, Size maximumSize, Size minimumTrackSize,
        Size maximumTrackSize)
    {
        bounds.Width += ClearSnapSentinel(offsets.X, stickGap) + ClearSnapSentinel(offsets.Width, stickGap);
        bounds.Height += ClearSnapSentinel(offsets.Y, stickGap) + ClearSnapSentinel(offsets.Height, stickGap);

        if ((direction & ResizeDir.Left) != 0)
        {
            bounds.Width = ConstrainResizeDimension(bounds.Width, minimumSize.Width, maximumSize.Width,
                minimumTrackSize.Width, maximumTrackSize.Width);
            bounds.X = originalBounds.Right - bounds.Width;
        }

        if ((direction & ResizeDir.Top) != 0)
        {
            bounds.Height = ConstrainResizeDimension(bounds.Height, minimumSize.Height, maximumSize.Height,
                minimumTrackSize.Height, maximumTrackSize.Height);
            bounds.Y = originalBounds.Bottom - bounds.Height;
        }

        return bounds;
    }

    internal static int ClearSnapSentinel(int offset, int stickGap) => offset == stickGap + 1 ? 0 : offset;

    internal static int ConstrainResizeDimension(int value, int minimum, int maximum, int minimumTrack,
        int maximumTrack)
    {
        if (maximum != 0)
        {
            value = Math.Min(value, maximum);
        }

        return Math.Max(Math.Max(Math.Min(value, maximumTrack), minimum), minimumTrack);
    }

    internal static Rectangle ComputeResizeOffsets(Rectangle bounds, Rectangle workingArea,
        IEnumerable<Rectangle> otherWindows, ResizeDir direction, int stickGap, bool stickToScreen, bool stickToOther)
    {
        var offsets = new Rectangle(stickGap + 1, stickGap + 1, 0, 0);
        if (stickToScreen)
        {
            offsets = ComputeResizeSnap(bounds, workingArea, offsets, direction, stickGap, false);
        }

        if (stickToOther)
        {
            foreach (var otherBounds in otherWindows)
            {
                offsets = ComputeResizeSnap(bounds, otherBounds, offsets, direction, stickGap, true);
            }
        }

        return offsets;
    }

    /// <summary>
    ///     Pure computation of the resize snap offsets for a single reference rectangle.
    ///     Given the current form bounds and the rectangle to snap against, returns the updated
    ///     offset rectangle (X/Width adjust the left/right edges, Y/Height adjust the top/bottom edges).
    ///     The edges considered are limited to those in <paramref name="resizeDirection" />.
    /// </summary>
    /// <param name="formRect">Current (already stretched) form bounds.</param>
    /// <param name="toRect">Rectangle to try to snap to.</param>
    /// <param name="formOffsetRect">Accumulated offsets so far (seeded with StickGap + 1 in the edges to snap).</param>
    /// <param name="resizeDirection">Which edges are being resized.</param>
    /// <param name="stickGap">Snap distance in pixels.</param>
    /// <param name="bInsideStick">Allow snapping on the inside (eg: form to another form).</param>
    internal static Rectangle ComputeResizeSnap(Rectangle formRect, Rectangle toRect, Rectangle formOffsetRect,
        ResizeDir resizeDirection, int stickGap, bool bInsideStick)
    {
        var offset = formOffsetRect;

        if (formRect.Right >= toRect.Left - stickGap && formRect.Left <= toRect.Right + stickGap)
        {
            if ((resizeDirection & ResizeDir.Top) == ResizeDir.Top)
            {
                if (Math.Abs(formRect.Top - toRect.Bottom) <= Math.Abs(offset.Top) && bInsideStick)
                {
                    offset.Y = formRect.Top - toRect.Bottom; // snap top to bottom
                }
                else if (Math.Abs(formRect.Top - toRect.Top) <= Math.Abs(offset.Top))
                {
                    offset.Y = formRect.Top - toRect.Top; // snap top to top
                }
            }

            if ((resizeDirection & ResizeDir.Bottom) == ResizeDir.Bottom)
            {
                if (Math.Abs(formRect.Bottom - toRect.Top) <= Math.Abs(offset.Bottom) && bInsideStick)
                {
                    offset.Height = toRect.Top - formRect.Bottom; // snap Bottom to top
                }
                else if (Math.Abs(formRect.Bottom - toRect.Bottom) <= Math.Abs(offset.Bottom))
                {
                    offset.Height = toRect.Bottom - formRect.Bottom; // snap bottom to bottom
                }
            }
        }

        if (formRect.Bottom < toRect.Top - stickGap || formRect.Top > toRect.Bottom + stickGap)
        {
            return offset;
        }

        if ((resizeDirection & ResizeDir.Right) == ResizeDir.Right)
        {
            if (Math.Abs(formRect.Right - toRect.Left) <= Math.Abs(offset.Right) && bInsideStick)
            {
                offset.Width = toRect.Left - formRect.Right; // Stick right to left
            }
            else if (Math.Abs(formRect.Right - toRect.Right) <= Math.Abs(offset.Right))
            {
                offset.Width = toRect.Right - formRect.Right; // Stick right to right
            }
        }

        if ((resizeDirection & ResizeDir.Left) == ResizeDir.Left)
        {
            if (Math.Abs(formRect.Left - toRect.Right) <= Math.Abs(offset.Left) && bInsideStick)
            {
                offset.X = formRect.Left - toRect.Right; // Stick left to right
            }
            else if (Math.Abs(formRect.Left - toRect.Left) <= Math.Abs(offset.Left))
            {
                offset.X = formRect.Left - toRect.Left; // Stick left to left
            }
        }

        return offset;
    }

    #endregion

    #region Move Operations

    private void StartMove()
    {
        _formRect = _originalForm.Bounds;
        _formOriginalRect = _originalForm.Bounds; // save original position

        if (!_originalForm.Capture) // start capturing messages
        {
            _originalForm.Capture = true;
        }

        _messageProcessor = _moveMessageProcessor;
    }

    private bool MoveMsgProcessor(ref Message m)
    {
        // internal message loop
        if (!_originalForm.Capture)
        {
            Cancel();
            return false;
        }

        switch (m.Msg)
        {
            case UnsafeNativeMethods.WM.WM_LBUTTONUP:
                {
                    // ok, move finished !!!
                    EndMove();
                    break;
                }
            case UnsafeNativeMethods.WM.WM_MOUSEMOVE:
                {
                    _mousePoint.X = (short)UnsafeNativeMethods.Bit.LoWord((int)m.LParam);
                    _mousePoint.Y = (short)UnsafeNativeMethods.Bit.HiWord((int)m.LParam);
                    Move(_mousePoint);
                    break;
                }
            case UnsafeNativeMethods.WM.WM_KEYDOWN:
                {
                    if ((int)m.WParam == UnsafeNativeMethods.VK.VK_ESCAPE)
                    {
                        _originalForm.Bounds = _formOriginalRect; // set back old size
                        Cancel();
                    }

                    break;
                }
        }

        return false;
    }

    private void EndMove()
    {
        Cancel();
        OnMoveEnded();
    }

    #endregion

    #region Move Computing

    private void Move(Point p)
    {
        p = _originalForm.PointToScreen(p);
        _formRect = ComputeMoveBounds(_formRect, p, _offsetPoint, Screen.FromPoint(p).WorkingArea,
            GetOtherWindowBounds(), StickGap, StickToScreen, StickToOther);
        _originalForm.Bounds = _formRect;
    }

    private IEnumerable<Rectangle> GetOtherWindowBounds() => _globalStickyWindows.OfType<Form>()
        .Where(form => form != _originalForm).Select(form => form.Bounds);

    internal static Rectangle ComputeMoveBounds(Rectangle bounds, Point mousePoint, Point mouseOffset,
        Rectangle workingArea, IEnumerable<Rectangle> otherWindows, int stickGap, bool stickToScreen, bool stickToOther)
    {
        mousePoint.X = NormalizeInside(mousePoint.X, workingArea.Left, workingArea.Right);
        mousePoint.Y = NormalizeInside(mousePoint.Y, workingArea.Top, workingArea.Bottom);
        mousePoint.Offset(-mouseOffset.X, -mouseOffset.Y);
        bounds.Location = mousePoint;

        var offset = new Point(stickGap + 1, stickGap + 1);

        if (stickToScreen)
        {
            offset = ComputeMoveSnap(bounds, workingArea, offset, stickGap, false);
        }

        if (stickToOther)
        {
            foreach (var otherBounds in otherWindows)
            {
                offset = ComputeMoveSnap(bounds, otherBounds, offset, stickGap, true);
            }
        }

        bounds.Offset(ClearSnapSentinel(offset.X, stickGap), ClearSnapSentinel(offset.Y, stickGap));
        return bounds;
    }

    /// <summary>
    ///     Pure computation of the move snap offset for a single reference rectangle.
    ///     Given the tentative form position and a rectangle to snap against, returns the updated
    ///     offset point that should be added to the form position.
    /// </summary>
    /// <param name="formRect">Tentative form bounds at the new (unsnapped) location.</param>
    /// <param name="toRect">Rectangle to try to snap to.</param>
    /// <param name="formOffsetPoint">Accumulated offset so far (seeded with StickGap + 1 in each axis).</param>
    /// <param name="stickGap">Snap distance in pixels.</param>
    /// <param name="bInsideStick">Allow snapping on the inside (eg: window to screen).</param>
    internal static Point ComputeMoveSnap(Rectangle formRect, Rectangle toRect, Point formOffsetPoint, int stickGap,
        bool bInsideStick)
    {
        var offset = formOffsetPoint;

        // compare distance from toRect to formRect
        // and then with the found distances, compare the most closed position
        if (formRect.Bottom >= toRect.Top - stickGap && formRect.Top <= toRect.Bottom + stickGap)
        {
            if (bInsideStick)
            {
                if (Math.Abs(formRect.Left - toRect.Right) <= Math.Abs(offset.X))
                {
                    // left 2 right
                    offset.X = toRect.Right - formRect.Left;
                }

                if (Math.Abs(formRect.Left + formRect.Width - toRect.Left) <= Math.Abs(offset.X))
                {
                    // right 2 left
                    offset.X = toRect.Left - formRect.Width - formRect.Left;
                }
            }

            if (Math.Abs(formRect.Left - toRect.Left) <= Math.Abs(offset.X))
            {
                // snap left 2 left
                offset.X = toRect.Left - formRect.Left;
            }

            if (Math.Abs(formRect.Left + formRect.Width - toRect.Left - toRect.Width) <= Math.Abs(offset.X))
            {
                // snap right 2 right
                offset.X = toRect.Left + toRect.Width - formRect.Width - formRect.Left;
            }
        }

        if (formRect.Right < toRect.Left - stickGap || formRect.Left > toRect.Right + stickGap)
        {
            return offset;
        }

        if (bInsideStick)
        {
            if (Math.Abs(formRect.Top - toRect.Bottom) <= Math.Abs(offset.Y))
            {
                // Stick Top to Bottom
                offset.Y = toRect.Bottom - formRect.Top;
            }

            if (Math.Abs(formRect.Top + formRect.Height - toRect.Top) <= Math.Abs(offset.Y))
            {
                // snap Bottom to Top
                offset.Y = toRect.Top - formRect.Height - formRect.Top;
            }
        }

        // try to snap top 2 top also
        if (Math.Abs(formRect.Top - toRect.Top) <= Math.Abs(offset.Y))
        {
            // top 2 top
            offset.Y = toRect.Top - formRect.Top;
        }

        if (Math.Abs(formRect.Top + formRect.Height - toRect.Top - toRect.Height) <= Math.Abs(offset.Y))
        {
            // bottom 2 bottom
            offset.Y = toRect.Top + toRect.Height - formRect.Height - formRect.Top;
        }

        return offset;
    }

    #endregion
}
