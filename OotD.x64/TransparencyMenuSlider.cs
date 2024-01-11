using System;
using System.Drawing;
using System.Windows.Forms;

public class TrackBarMenuItem : ToolStripControlHost
{
    public TrackBar TrackBar { get; }

    public event EventHandler? ValueChanged;

    public TrackBarMenuItem() : base(new TrackBar())
    {
        this.TrackBar = (TrackBar)this.Control;
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

    // Add more properties as needed...

    private void TrackBar_Scroll(object? sender, EventArgs e)
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
