namespace RpgEditor;

internal readonly record struct SpriteGrid(int Width, int Height, int Margin = 0, int Spacing = 0)
{
    internal const int MaxInput = 32768;
    public int StepX => Width + Spacing;
    public int StepY => Height + Spacing;
    public int Columns(Size sheet) => Count(sheet.Width, Width);
    public int Rows(Size sheet) => Count(sheet.Height, Height);

    private int Count(int extent, int cell) => Math.Max(0, (extent - 2 * Margin + Spacing) / (cell + Spacing));

    public bool ContainsCellPixel(Point pixel, Size sheet)
    {
        int x = pixel.X - Margin;
        int y = pixel.Y - Margin;
        return x >= 0 && y >= 0 && x / StepX < Columns(sheet) && y / StepY < Rows(sheet) &&
            x % StepX < Width && y % StepY < Height;
    }

    public Rectangle Select(Point start, Point end, Size sheet)
    {
        if (!ContainsCellPixel(start, sheet))
            return Rectangle.Empty;

        int startX = (start.X - Margin) / StepX;
        int startY = (start.Y - Margin) / StepY;
        int endX = Math.Clamp((end.X - Margin) / StepX, 0, Columns(sheet) - 1);
        int endY = Math.Clamp((end.Y - Margin) / StepY, 0, Rows(sheet) - 1);
        return Rectangle.FromLTRB(
            Margin + Math.Min(startX, endX) * StepX,
            Margin + Math.Min(startY, endY) * StepY,
            Margin + Math.Max(startX, endX) * StepX + Width,
            Margin + Math.Max(startY, endY) * StepY + Height);
    }

    public static Rectangle SelectPixels(Point start, Point end, Size sheet)
    {
        end.X = Math.Clamp(end.X, 0, sheet.Width - 1);
        end.Y = Math.Clamp(end.Y, 0, sheet.Height - 1);
        return Rectangle.FromLTRB(
            Math.Min(start.X, end.X), Math.Min(start.Y, end.Y),
            Math.Max(start.X, end.X) + 1, Math.Max(start.Y, end.Y) + 1);
    }
}
