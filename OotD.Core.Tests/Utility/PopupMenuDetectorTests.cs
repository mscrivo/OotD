using OotD.Utility;

namespace OotD.Core.Tests.Utility;

public class PopupMenuDetectorTests
{
    [Fact]
    public void IsMenuOpen_WithOnlyFormsShowing_ReturnsFalse()
    {
        using var form = CreateForm(FormBorderStyle.None);
        using var sizable = CreateForm(FormBorderStyle.Sizable);
        form.Show();
        sizable.Show();

        PopupMenuDetector.IsMenuOpen().Should().BeFalse();
    }

    [Fact]
    public void IsMenuOpen_TracksContextMenuVisibility()
    {
        using var form = CreateForm(FormBorderStyle.None);
        form.Show();
        using var menu = new ContextMenuStrip();
        menu.Items.Add("Open");

        menu.Show(form, Point.Empty);
        var whileOpen = PopupMenuDetector.IsMenuOpen();
        menu.Close();
        var afterClose = PopupMenuDetector.IsMenuOpen();

        whileOpen.Should().BeTrue();
        afterClose.Should().BeFalse();
    }

    [Fact]
    public void IsMenuOpen_IgnoresTooltips()
    {
        using var form = CreateForm(FormBorderStyle.None);
        form.Show();
        using var toolTip = new ToolTip();

        toolTip.Show("tip", form, 0, 0);
        try
        {
            PopupMenuDetector.IsMenuOpen().Should().BeFalse();
        }
        finally
        {
            toolTip.Hide(form);
        }
    }

    private static Form CreateForm(FormBorderStyle borderStyle)
    {
        return new Form
        {
            FormBorderStyle = borderStyle,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Bounds = new Rectangle(-32000, -32000, 50, 50)
        };
    }
}
