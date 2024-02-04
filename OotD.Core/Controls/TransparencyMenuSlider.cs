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

    public TrackBarMenuItem() : base(new MACTrackBar())
    {
        TrackBar = (MACTrackBar)Control;
        TrackBar.Scroll += TrackBar_Scroll;
    }

    public Point Location
    {
        set => TrackBar.Location = value;
    }

    public int Minimum
    {
        set => TrackBar.Minimum = value;
    }

    public int Maximum
    {
        set => TrackBar.Maximum = value;
    }

    public int Value
    {
        get { return TrackBar.Value; }
        set { TrackBar.Value = value; }
    }

    // Add more properties as needed...

    private void TrackBar_Scroll(object? sender, EventArgs e)
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
