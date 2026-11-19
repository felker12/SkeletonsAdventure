using System.Drawing.Drawing2D;

namespace RpgEditor;

internal readonly record struct SpriteSheetPosition(float Zoom, float CenterX, float CenterY, bool Fit);

internal sealed class SpriteSheetView : Control
{
    private Bitmap? image;
    private PointF offset;
    private float zoom = 1;
    private bool fitting = true;
    private Point? selectionStart;
    private Point? panStart;
    private PointF panOffset;

    public SpriteSheetView()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.FromArgb(37, 39, 43);
        TabStop = true;
        AccessibleName = "Sprite sheet";
        Cursor = Cursors.Cross;
    }

    public Rectangle Selection { get; private set; }
    public SpriteGrid? Grid { get; private set; }
    public event Action<Rectangle>? SpriteSelected;
    public event Action? ViewChanged;
    public float Zoom => zoom;

    public void SetImage(Bitmap? bitmap)
    {
        image = bitmap;
        Fit();
    }

    public void SetSelection(Rectangle rectangle)
    {
        Selection = rectangle;
        Invalidate();
    }

    public void SetGrid(SpriteGrid? grid)
    {
        Grid = grid;
        Invalidate();
    }

    private float FitZoom => image is null ? 1 :
        Math.Clamp(Math.Min((ClientSize.Width - 12f) / image.Width, (ClientSize.Height - 12f) / image.Height), 0.01f, 1);

    public void Fit()
    {
        fitting = true;
        zoom = FitZoom;
        UpdateView();
    }

    public void Center(Rectangle sprite)
    {
        if (image is null || sprite.IsEmpty)
        {
            Fit();
            return;
        }
        float targetZoom = Math.Min(4, Math.Min(ClientSize.Width / (sprite.Width * 3f), ClientSize.Height / (sprite.Height * 3f)));
        CenterOn(new PointF(sprite.Left + sprite.Width / 2f, sprite.Top + sprite.Height / 2f), Math.Max(FitZoom, targetZoom));
    }

    public void ActualSize() => CenterOn(ScreenToImage(new PointF(ClientSize.Width / 2f, ClientSize.Height / 2f)), 1);

    public SpriteSheetPosition GetPosition()
    {
        var center = ScreenToImage(new PointF(ClientSize.Width / 2f, ClientSize.Height / 2f));
        return new SpriteSheetPosition(zoom, center.X, center.Y, fitting);
    }

    public void RestorePosition(SpriteSheetPosition position)
    {
        if (image is null || position.Fit || !float.IsFinite(position.Zoom) ||
            !float.IsFinite(position.CenterX) || !float.IsFinite(position.CenterY))
        {
            Fit();
            return;
        }
        var center = new PointF(Math.Clamp(position.CenterX, 0, image.Width), Math.Clamp(position.CenterY, 0, image.Height));
        CenterOn(center, Math.Clamp(position.Zoom, FitZoom, 16));
    }

    private void CenterOn(PointF point, float scale)
    {
        fitting = false;
        zoom = scale;
        offset = new PointF(ClientSize.Width / 2f - point.X * zoom, ClientSize.Height / 2f - point.Y * zoom);
        UpdateView();
    }

    private PointF ScreenToImage(PointF point) => new((point.X - offset.X) / zoom, (point.Y - offset.Y) / zoom);
    private Point ToPixel(Point point)
    {
        var position = ScreenToImage(point);
        return new Point((int)Math.Floor(position.X), (int)Math.Floor(position.Y));
    }

    private void UpdateView()
    {
        if (image is not null)
        {
            offset.X = Constrain(offset.X, ClientSize.Width, image.Width * zoom);
            offset.Y = Constrain(offset.Y, ClientSize.Height, image.Height * zoom);
        }
        Invalidate();
        ViewChanged?.Invoke();
    }

    private static float Constrain(float position, int viewport, float extent) =>
        extent < viewport ? (viewport - extent) / 2 : Math.Clamp(position, viewport - extent, 0);

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (fitting)
            Fit();
        else
            UpdateView();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (image is null)
            return;

        var target = ScreenToImage(e.Location);
        zoom = Math.Clamp(zoom * MathF.Pow(1.25f, e.Delta / 120f), FitZoom, 16);
        fitting = false;
        offset = new PointF(e.X - target.X * zoom, e.Y - target.Y * zoom);
        UpdateView();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (image is null)
            return;

        Focus();
        if (e.Button == MouseButtons.Middle)
        {
            panStart = e.Location;
            panOffset = offset;
            Cursor = Cursors.SizeAll;
            Capture = true;
            return;
        }
        var pixel = ToPixel(e.Location);
        if (e.Button != MouseButtons.Left || !new Rectangle(Point.Empty, image.Size).Contains(pixel) ||
            Grid is { } grid && !grid.ContainsCellPixel(pixel, image.Size))
            return;

        selectionStart = pixel;
        Capture = true;
        SelectAt(e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (panStart is { } start)
        {
            fitting = false;
            offset = new PointF(panOffset.X + e.X - start.X, panOffset.Y + e.Y - start.Y);
            UpdateView();
        }
        else
            SelectAt(e.Location);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left && selectionStart.HasValue)
        {
            SelectAt(e.Location);
            Capture = false;
        }
        if (e.Button == MouseButtons.Middle && panStart.HasValue)
            Capture = false;
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (!Capture)
        {
            selectionStart = null;
            panStart = null;
            Cursor = Cursors.Cross;
        }
    }

    private void SelectAt(Point location)
    {
        if (image is null || selectionStart is not { } start)
            return;
        var end = ToPixel(location);
        var rectangle = Grid is { } grid ? grid.Select(start, end, image.Size) : SpriteGrid.SelectPixels(start, end, image.Size);
        if (!rectangle.IsEmpty && rectangle != Selection)
            SpriteSelected?.Invoke(rectangle);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (image is null)
            return;

        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.DrawImage(image, new RectangleF(offset.X, offset.Y, image.Width * zoom, image.Height * zoom),
            new RectangleF(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
        if (Grid is { } grid)
            DrawGrid(e.Graphics, grid, image.Size);

        if (!Selection.IsEmpty)
        {
            using var pen = new Pen(Color.Gold, 2);
            float left = offset.X + Selection.Left * zoom;
            float top = offset.Y + Selection.Top * zoom;
            e.Graphics.DrawRectangle(pen, left, top, Selection.Width * zoom, Selection.Height * zoom);
        }
    }

    private void DrawGrid(Graphics graphics, SpriteGrid grid, Size size)
    {
        int columns = grid.Columns(size);
        int rows = grid.Rows(size);
        if (columns == 0 || rows == 0 || Math.Min(grid.StepX, grid.StepY) * zoom < 5)
            return;

        using var pen = new Pen(Color.FromArgb(115, Color.White));
        float left = offset.X + grid.Margin * zoom;
        float top = offset.Y + grid.Margin * zoom;
        float right = left + ((columns - 1) * grid.StepX + grid.Width) * zoom;
        float bottom = top + ((rows - 1) * grid.StepY + grid.Height) * zoom;
        DrawLines(left, grid.StepX * zoom, grid.Width * zoom, columns, ClientSize.Width,
            x => graphics.DrawLine(pen, x, top, x, bottom));
        DrawLines(top, grid.StepY * zoom, grid.Height * zoom, rows, ClientSize.Height,
            y => graphics.DrawLine(pen, left, y, right, y));
    }

    private static void DrawLines(float origin, float step, float extent, int count, int viewport, Action<float> draw)
    {
        int first = Math.Max(0, (int)Math.Floor(-origin / step));
        int last = Math.Min(count - 1, (int)Math.Floor((viewport - origin) / step));
        for (int i = first; i <= last; i++)
        {
            float position = origin + i * step;
            draw(position);
            if (extent < step || i == count - 1)
                draw(position + extent);
        }
    }
}
