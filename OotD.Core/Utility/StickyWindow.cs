// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

namespace OotD.Utility;

/// <summary>
///     A windows that Sticks to other windows of the same type when moved or resized.
///     You get a nice way of organizing multiple top-level windows.
///     Quite similar with WinAmp 2.x style of sticking the windows
/// </summary>
#pragma warning disable IDE0079 // Remove unnecessary suppression
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
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
        _formOffsetRect = Rectangle.Empty;

        _formOffsetPoint = Point.Empty;
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

        switch (iHitTest)
        {
            case UnsafeNativeMethods.HT.HTCAPTION:
                {
                    // request for move
                    if (!StickOnMove)
                    {
                        return false; // leave default processing
                    }

                    var pointInApp = _originalForm.PointToClient(Cursor.Position);
                    _offsetPoint.Offset(pointInApp.X, pointInApp.Y);
                    StartMove();
                    return true;
                }

            // requests for resize
            case UnsafeNativeMethods.HT.HTTOPLEFT:
                return StartResize(ResizeDir.Top | ResizeDir.Left);
            case UnsafeNativeMethods.HT.HTTOP:
                return StartResize(ResizeDir.Top);
            case UnsafeNativeMethods.HT.HTTOPRIGHT:
                return StartResize(ResizeDir.Top | ResizeDir.Right);
            case UnsafeNativeMethods.HT.HTRIGHT:
                return StartResize(ResizeDir.Right);
            case UnsafeNativeMethods.HT.HTBOTTOMRIGHT:
                return StartResize(ResizeDir.Bottom | ResizeDir.Right);
            case UnsafeNativeMethods.HT.HTBOTTOM:
                return StartResize(ResizeDir.Bottom);
            case UnsafeNativeMethods.HT.HTBOTTOMLEFT:
                return StartResize(ResizeDir.Bottom | ResizeDir.Left);
            case UnsafeNativeMethods.HT.HTLEFT:
                return StartResize(ResizeDir.Left);
        }

        return false;
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
    private Point _formOffsetPoint; // calculated offset rect to be added !! (min distances in all directions!!)
    private Point _offsetPoint; // primary offset

    // Resize stuff
    private bool _resizingForm;
    private ResizeDir _resizeDirection;
    private Rectangle _formOffsetRect; // calculated rect to fix the size
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
    // ReSharper disable once UnusedMember.Global
    public static void RegisterExternalReferenceForm(Form frmExternal)
    {
        _globalStickyWindows.Add(frmExternal);
    }

    /// <summary>
    ///     Unregister a form from the external references.
    ///     <see cref="RegisterExternalReferenceForm" />
    /// </summary>
    /// <param name="frmExternal">External window that will was used as reference</param>
    // ReSharper disable once UnusedMember.Global
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
        _formRect = _originalForm.Bounds;

        var iRight = _formRect.Right;
        var iBottom = _formRect.Bottom;

        // no normalize required
        // first stretch the window to the new position
        if ((_resizeDirection & ResizeDir.Left) == ResizeDir.Left)
        {
            _formRect.Width = _formRect.X - p.X + _formRect.Width;
            _formRect.X = iRight - _formRect.Width;
        }

        if ((_resizeDirection & ResizeDir.Right) == ResizeDir.Right)
        {
            _formRect.Width = p.X - _formRect.Left;
        }

        if ((_resizeDirection & ResizeDir.Top) == ResizeDir.Top)
        {
            _formRect.Height = _formRect.Height - p.Y + _formRect.Top;
            _formRect.Y = iBottom - _formRect.Height;
        }

        if ((_resizeDirection & ResizeDir.Bottom) == ResizeDir.Bottom)
        {
            _formRect.Height = p.Y - _formRect.Top;
        }

        // this is the real new position
        // now, try to snap it to different objects (first to screen)

        // CARE !!! We use "Width" and "Height" as Bottom & Right!! (C++ style)
        //formOffsetRect = new Rectangle ( stickGap + 1, stickGap + 1, 0, 0 );
        _formOffsetRect.X = StickGap + 1;
        _formOffsetRect.Y = StickGap + 1;
        _formOffsetRect.Height = 0;
        _formOffsetRect.Width = 0;

        if (StickToScreen)
        {
            Resize_Stick(activeScr.WorkingArea, false);
        }

        if (StickToOther)
        {
            // now try to stick to other forms
            foreach (var sw in _globalStickyWindows)
            {
                var form = sw as Form;
                if (form == _originalForm)
                {
                    continue;
                }

                if (form != null)
                {
                    Resize_Stick(form.Bounds, true);
                }
            }
        }

        // Fix (clear) the values that were not updated to stick
        if (_formOffsetRect.X == StickGap + 1)
        {
            _formOffsetRect.X = 0;
        }

        if (_formOffsetRect.Width == StickGap + 1)
        {
            _formOffsetRect.Width = 0;
        }

        if (_formOffsetRect.Y == StickGap + 1)
        {
            _formOffsetRect.Y = 0;
        }

        if (_formOffsetRect.Height == StickGap + 1)
        {
            _formOffsetRect.Height = 0;
        }

        // compute the new form size
        if ((_resizeDirection & ResizeDir.Left) == ResizeDir.Left)
        {
            // left resize requires special handling of X & Width according to MinSize and MinWindowTrackSize
            var iNewWidth = _formRect.Width + _formOffsetRect.Width + _formOffsetRect.X;

            if (_originalForm.MaximumSize.Width != 0)
            {
                iNewWidth = Math.Min(iNewWidth, _originalForm.MaximumSize.Width);
            }

            iNewWidth = Math.Min(iNewWidth, SystemInformation.MaxWindowTrackSize.Width);
            iNewWidth = Math.Max(iNewWidth, _originalForm.MinimumSize.Width);
            iNewWidth = Math.Max(iNewWidth, SystemInformation.MinWindowTrackSize.Width);

            _formRect.X = iRight - iNewWidth;
            _formRect.Width = iNewWidth;
        }
        else
        {
            // other resizes
            _formRect.Width += _formOffsetRect.Width + _formOffsetRect.X;
        }

        if ((_resizeDirection & ResizeDir.Top) == ResizeDir.Top)
        {
            var iNewHeight = _formRect.Height + _formOffsetRect.Height + _formOffsetRect.Y;

            if (_originalForm.MaximumSize.Height != 0)
            {
                iNewHeight = Math.Min(iNewHeight, _originalForm.MaximumSize.Height);
            }

            iNewHeight = Math.Min(iNewHeight, SystemInformation.MaxWindowTrackSize.Height);
            iNewHeight = Math.Max(iNewHeight, _originalForm.MinimumSize.Height);
            iNewHeight = Math.Max(iNewHeight, SystemInformation.MinWindowTrackSize.Height);

            _formRect.Y = iBottom - iNewHeight;
            _formRect.Height = iNewHeight;
        }
        else
        {
            // all other resizing are fine 
            _formRect.Height += _formOffsetRect.Height + _formOffsetRect.Y;
        }

        // Done !!
        _originalForm.Bounds = _formRect;
    }

    private void Resize_Stick(Rectangle toRect, bool bInsideStick)
    {
        _formOffsetRect = ComputeResizeSnap(_formRect, toRect, _formOffsetRect, _resizeDirection, StickGap, bInsideStick);
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

        // ReSharper disable once InvertIf
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
        var activeScr = Screen.FromPoint(p); // get the screen from the point !!

        if (!activeScr.WorkingArea.Contains(p))
        {
            p.X = NormalizeInside(p.X, activeScr.WorkingArea.Left, activeScr.WorkingArea.Right);
            p.Y = NormalizeInside(p.Y, activeScr.WorkingArea.Top, activeScr.WorkingArea.Bottom);
        }

        p.Offset(-_offsetPoint.X, -_offsetPoint.Y);

        // p is the exact location of the frame - so we can play with it
        // to detect the new position according to different bounds
        _formRect.Location = p; // this is the new position of the form

        _formOffsetPoint.X = StickGap + 1; // (more than) maximum gaps
        _formOffsetPoint.Y = StickGap + 1;

        if (StickToScreen)
        {
            Move_Stick(activeScr.WorkingArea, false);
        }

        // Now try to snap to other windows
        if (StickToOther)
        {
            foreach (var sw in _globalStickyWindows)
            {
                var form = sw as Form;
                if (form == _originalForm)
                {
                    continue;
                }

                if (form != null)
                {
                    Move_Stick(form.Bounds, true);
                }
            }
        }

        if (_formOffsetPoint.X == StickGap + 1)
        {
            _formOffsetPoint.X = 0;
        }

        if (_formOffsetPoint.Y == StickGap + 1)
        {
            _formOffsetPoint.Y = 0;
        }

        _formRect.Offset(_formOffsetPoint);

        _originalForm.Bounds = _formRect;
    }

    /// <summary>
    /// </summary>
    /// <param name="toRect">Rect to try to snap to</param>
    /// <param name="bInsideStick">Allow snapping on the inside (eg: window to screen)</param>
    private void Move_Stick(Rectangle toRect, bool bInsideStick)
    {
        _formOffsetPoint = ComputeMoveSnap(_formRect, toRect, _formOffsetPoint, StickGap, bInsideStick);
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
