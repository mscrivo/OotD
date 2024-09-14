using System;
using System.ComponentModel;
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

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Point Location
    {
        set => TrackBar.Location = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Minimum
    {
        set => TrackBar.Minimum = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Maximum
    {
        set => TrackBar.Maximum = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
