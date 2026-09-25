using System;
using System.Drawing;
using System.Windows.Forms;

namespace OotD.Controls;

/// <summary>
///     Borderless image-only button for the MainForm header bar. It paints itself so it blends into the
///     header in both light and dark mode; WinForms' dark-mode flat button renderer ignores the inherited
///     BackColor, draws its own border and insets the content area, which clips the small toolbar icons.
/// </summary>
public class HeaderButton : Button
{
    private static readonly Color HoverOverlay = Color.FromArgb(60, Color.White);
    private static readonly Color PressedOverlay = Color.FromArgb(50, Color.Black);

    private bool _mouseOver;
    private bool _mouseDown;

    public HeaderButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        TabStop = false;
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var g = pevent.Graphics;
        g.Clear(BackColor);

        if (Enabled && (_mouseOver || _mouseDown))
        {
            using var overlay = new SolidBrush(_mouseDown ? PressedOverlay : HoverOverlay);
            g.FillRectangle(overlay, ClientRectangle);
        }

        if (Image is not { } image)
        {
            return;
        }

        var x = (Width - image.Width) / 2;
        var y = (Height - image.Height) / 2;
        if (Enabled)
        {
            g.DrawImage(image, x, y, image.Width, image.Height);
        }
        else
        {
            ControlPaint.DrawImageDisabled(g, image, x, y, BackColor);
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _mouseOver = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _mouseOver = false;
        _mouseDown = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        if (mevent.Button == MouseButtons.Left)
        {
            _mouseDown = true;
            Invalidate();
        }

        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _mouseDown = false;
        Invalidate();
        base.OnMouseUp(mevent);
    }
}
