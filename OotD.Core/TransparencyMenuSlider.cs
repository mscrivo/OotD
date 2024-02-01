using System;
using System.Drawing;
using System.Windows.Forms;
using MACTrackBarLib;

public class TrackBarMenuItem : ToolStripControlHost
{
#pragma warning disable CS3003
    public MACTrackBar TrackBar { get; }
#pragma warning restore CS3003

    public event EventHandler? ValueChanged;

    public ToolTip ToolTip { get; } = new ToolTip(); // Add ToolTip property

    public TrackBarMenuItem() : base(new MACTrackBar())
    {
        this.TrackBar = (MACTrackBar)this.Control;
        this.TrackBar.Scroll += TrackBar_Scroll;
    }

    public Point Location
    {
        get { return this.TrackBar.Location; }
        set { this.TrackBar.Location = value; }
    }

    public int Minimum
    {
        get { return this.TrackBar.Minimum; }
        set { this.TrackBar.Minimum = value; }
    }

    public int Maximum
    {
        get { return this.TrackBar.Maximum; }
        set { this.TrackBar.Maximum = value; }
    }

    public int Value
    {
        get { return this.TrackBar.Value; }
        set { this.TrackBar.Value = value; }
    }

    public TickStyle TickStyle
    {
        get { return this.TrackBar.TickStyle; }
        set { this.TrackBar.TickStyle = value; }
    }

    // Add more properties as needed...

    private void TrackBar_Scroll(object? sender, EventArgs e)
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
