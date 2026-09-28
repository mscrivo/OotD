using System.Reflection;
using OotD.Utility;
using static OotD.Utility.UnsafeNativeMethods;

namespace OotD.Core.Tests.Utility;

/// <summary>
///     Drives StickyWindow through real window messages (sent to the subclassed form) to cover the move and
///     resize message loops end to end.
/// </summary>
public class StickyWindowMessageTests : IDisposable
{
    private readonly Form _form;
    private readonly StickyWindow _stickyWindow;
    private readonly Rectangle _originalBounds = new(200, 200, 300, 200);
    private int _moveEnded;
    private int _resizeEnded;

    public StickyWindowMessageTests()
    {
        _form = new Form { StartPosition = FormStartPosition.Manual, Bounds = _originalBounds };
        _stickyWindow = new StickyWindow(_form) { StickToScreen = false, StickToOther = false };
        _stickyWindow.MoveEnded += (_, _) => _moveEnded++;
        _stickyWindow.ResizeEnded += (_, _) => _resizeEnded++;
    }

    public void Dispose()
    {
        _form.Capture = false;
        _stickyWindow.ReleaseHandle();
        _form.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void CaptionDrag_MovesFormAndRaisesMoveEnded()
    {
        Send(WM.WM_NCLBUTTONDOWN, HT.HTCAPTION, 0, 0);
        var mouse = new Point(40, 30);
        var expected = StickyWindow.ComputeMoveBounds(_originalBounds, _form.PointToScreen(mouse), GetOffsetPoint(),
            Screen.FromPoint(_form.PointToScreen(mouse)).WorkingArea, [], StickyWindow.StickGap, false, false);

        Send(WM.WM_MOUSEMOVE, 0, mouse.X, mouse.Y);

        _form.Bounds.Should().Be(expected);
        _moveEnded.Should().Be(0);

        Send(WM.WM_LBUTTONUP, 0, mouse.X, mouse.Y);

        _moveEnded.Should().Be(1);
        _form.Capture.Should().BeFalse();
    }

    [Fact]
    public void CaptionDrag_EscapeRestoresOriginalPosition()
    {
        Send(WM.WM_NCLBUTTONDOWN, HT.HTCAPTION, 0, 0);
        Send(WM.WM_MOUSEMOVE, 0, 120, 90);
        Send(WM.WM_KEYDOWN, 0x41, 0, 0); // any other key keeps the drag going
        _form.Capture.Should().BeTrue();

        Send(WM.WM_KEYDOWN, VK.VK_ESCAPE, 0, 0);
        Send(WM.WM_LBUTTONUP, 0, 0, 0);

        _form.Bounds.Should().Be(_originalBounds);
        _moveEnded.Should().Be(0);
    }

    [Fact]
    public void CaptionDrag_WhenCaptureIsLost_CancelsWithoutMoving()
    {
        Send(WM.WM_NCLBUTTONDOWN, HT.HTCAPTION, 0, 0);
        _form.Capture = false;

        Send(WM.WM_MOUSEMOVE, 0, 120, 90);
        Send(WM.WM_LBUTTONUP, 0, 0, 0);

        _form.Bounds.Should().Be(_originalBounds);
        _moveEnded.Should().Be(0);
    }

    [Fact]
    public void CaptionDrag_WithSnappingEnabled_ConsidersOtherStickyWindows()
    {
        using var other = new Form { StartPosition = FormStartPosition.Manual, Bounds = new Rectangle(0, 0, 50, 50) };
        StickyWindow.RegisterExternalReferenceForm(other);
        try
        {
            _stickyWindow.StickToScreen = true;
            _stickyWindow.StickToOther = true;

            Send(WM.WM_NCLBUTTONDOWN, HT.HTCAPTION, 0, 0);
            Send(WM.WM_MOUSEMOVE, 0, 10, 10);
            Send(WM.WM_LBUTTONUP, 0, 10, 10);

            _moveEnded.Should().Be(1);
        }
        finally
        {
            StickyWindow.UnregisterExternalReferenceForm(other);
        }
    }

    [Fact]
    public void RightEdgeDrag_ResizesFormAndRaisesResizeEnded()
    {
        Send(WM.WM_NCLBUTTONDOWN, HT.HTRIGHT, 0, 0);
        var mouse = new Point(380, 50);
        var expectedRight = _form.PointToScreen(mouse).X;

        Send(WM.WM_MOUSEMOVE, 0, mouse.X, mouse.Y);

        _form.Left.Should().Be(_originalBounds.Left);
        _form.Right.Should().Be(expectedRight);
        _form.Height.Should().Be(_originalBounds.Height);

        Send(WM.WM_LBUTTONUP, 0, mouse.X, mouse.Y);

        _resizeEnded.Should().Be(1);
        _form.Capture.Should().BeFalse();
    }

    [Fact]
    public void EdgeDrag_EscapeRestoresOriginalSize()
    {
        Send(WM.WM_NCLBUTTONDOWN, HT.HTBOTTOM, 0, 0);
        Send(WM.WM_MOUSEMOVE, 0, 100, 350);
        _form.Height.Should().NotBe(_originalBounds.Height);

        Send(WM.WM_KEYDOWN, VK.VK_ESCAPE, 0, 0);
        Send(WM.WM_LBUTTONUP, 0, 0, 0);

        _form.Bounds.Should().Be(_originalBounds);
        _resizeEnded.Should().Be(0);
    }

    [Fact]
    public void EdgeDrag_WhenCaptureIsLost_CancelsWithoutResizing()
    {
        Send(WM.WM_NCLBUTTONDOWN, HT.HTLEFT, 0, 0);
        _form.Capture = false;

        Send(WM.WM_MOUSEMOVE, 0, 100, 100);
        Send(WM.WM_LBUTTONUP, 0, 0, 0);

        _form.Bounds.Should().Be(_originalBounds);
        _resizeEnded.Should().Be(0);
    }

    [Fact]
    public void NonClientClickOutsideCaptionOrEdges_IsLeftToDefaultProcessing()
    {
        Send(WM.WM_NCLBUTTONDOWN, 1 /* HTCLIENT */, 0, 0);
        Send(WM.WM_MOUSEMOVE, 0, 120, 90);

        _form.Bounds.Should().Be(_originalBounds);
        _form.Capture.Should().BeFalse();
    }

    private nint Send(int message, int wParam, int x, int y)
    {
        return SendMessage(_form.Handle, (uint)message, wParam, (y << 16) | (x & 0xFFFF));
    }

    private Point GetOffsetPoint()
    {
        return (Point)typeof(StickyWindow)
            .GetField("_offsetPoint", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(_stickyWindow)!;
    }
}
