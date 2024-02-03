using System;
using System.Drawing;
using System.Windows.Forms;
using MACTrackBarLib;

namespace OotD.Controls;

public class TrackBarMenuItem : ToolStripControlHost
{
#pragma warning disable CS3003
    public MACTrackBar TrackBar { get; }
#pragma warning restore CS3003

    public event EventHandler? ValueChanged;

    public ToolTip ToolTip { get; } = new ToolTip(); // Add ToolTip property

    public TrackBarMenuItem() : base(new MACTrackBar())
    {
        TrackBar = (MACTrackBar)Control;
        TrackBar.Scroll += TrackBar_Scroll;
    }

    public Point Location
    {
        get { return TrackBar.Location; }
        set { TrackBar.Location = value; }
    }

    public int Minimum
    {
        get { return TrackBar.Minimum; }
        set { TrackBar.Minimum = value; }
    }

    public int Maximum
    {
        get { return TrackBar.Maximum; }
        set { TrackBar.Maximum = value; }
    }

    public int Value
    {
        get { return TrackBar.Value; }
        set { TrackBar.Value = value; }
    }

    public TickStyle TickStyle
    {
        get { return TrackBar.TickStyle; }
        set { TrackBar.TickStyle = value; }
    }

    // Add more properties as needed...

    private void TrackBar_Scroll(object? sender, EventArgs e)
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
