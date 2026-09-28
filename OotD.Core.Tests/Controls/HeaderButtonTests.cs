using OotD.Controls;

namespace OotD.Core.Tests.Controls;

public class HeaderButtonTests
{
    private static readonly Color Background = Color.FromArgb(64, 64, 64);

    [Fact]
    public void Constructor_UsesBorderlessFlatStyleAndSkipsTabStop()
    {
        using var button = new TestHeaderButton();

        button.FlatStyle.Should().Be(FlatStyle.Flat);
        button.FlatAppearance.BorderSize.Should().Be(0);
        button.TabStop.Should().BeFalse();
    }

    [Fact]
    public void Paint_WhenIdle_FillsWithBackColor()
    {
        using var button = CreateButton();

        button.RenderCorner().Should().Be(Background.ToArgb());
    }

    [Fact]
    public void Paint_MouseStatesTintTheBackground()
    {
        using var button = CreateButton();

        button.RaiseMouseEnter();
        var hover = button.RenderCorner();

        button.RaiseMouseDown(MouseButtons.Left);
        var pressed = button.RenderCorner();

        button.RaiseMouseUp();
        var released = button.RenderCorner();

        button.RaiseMouseLeave();
        var idle = button.RenderCorner();

        Brightness(hover).Should().BeGreaterThan(Brightness(Background.ToArgb()));
        Brightness(pressed).Should().BeLessThan(Brightness(Background.ToArgb()));
        released.Should().Be(hover);
        idle.Should().Be(Background.ToArgb());
    }

    [Fact]
    public void Paint_RightButtonDoesNotShowPressedState()
    {
        using var button = CreateButton();
        button.RaiseMouseEnter();
        var hover = button.RenderCorner();

        button.RaiseMouseDown(MouseButtons.Right);

        button.RenderCorner().Should().Be(hover);
    }

    [Fact]
    public void Paint_LeavingWhilePressedClearsPressedState()
    {
        using var button = CreateButton();
        button.RaiseMouseEnter();
        button.RaiseMouseDown(MouseButtons.Left);

        button.RaiseMouseLeave();

        button.RenderCorner().Should().Be(Background.ToArgb());
    }

    [Fact]
    public void Paint_WhenDisabled_IgnoresHover()
    {
        using var button = CreateButton();
        button.Enabled = false;
        button.RaiseMouseEnter();

        button.RenderCorner().Should().Be(Background.ToArgb());
    }

    [Fact]
    public void Paint_DrawsImageCenteredAndGraysItOutWhenDisabled()
    {
        using var image = new Bitmap(8, 8);
        using (var g = Graphics.FromImage(image))
        {
            g.Clear(Color.Red);
        }

        using var button = CreateButton();
        button.Image = image;

        using (var enabled = button.Render())
        {
            enabled.GetPixel(12, 12).ToArgb().Should().Be(Color.Red.ToArgb());
            enabled.GetPixel(2, 2).ToArgb().Should().Be(Background.ToArgb());
        }

        button.Enabled = false;
        using var disabled = button.Render();
        disabled.GetPixel(12, 12).ToArgb().Should().NotBe(Color.Red.ToArgb());
    }

    private static TestHeaderButton CreateButton()
    {
        return new TestHeaderButton { Size = new Size(24, 24), BackColor = Background };
    }

    private static int Brightness(int argb)
    {
        var color = Color.FromArgb(argb);
        return color.R + color.G + color.B;
    }

    private sealed class TestHeaderButton : HeaderButton
    {
        public Bitmap Render()
        {
            var bitmap = new Bitmap(Width, Height);
            using var g = Graphics.FromImage(bitmap);
            OnPaint(new PaintEventArgs(g, ClientRectangle));
            return bitmap;
        }

        public int RenderCorner()
        {
            using var bitmap = Render();
            return bitmap.GetPixel(1, 1).ToArgb();
        }

        public void RaiseMouseEnter() => OnMouseEnter(EventArgs.Empty);

        public void RaiseMouseLeave() => OnMouseLeave(EventArgs.Empty);

        public void RaiseMouseDown(MouseButtons button) => OnMouseDown(new MouseEventArgs(button, 1, 5, 5, 0));

        public void RaiseMouseUp() => OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, 5, 5, 0));
    }
}
