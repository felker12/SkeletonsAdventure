using System.Drawing.Drawing2D;
using RpgLibrary.ItemClasses;

namespace RpgEditor;

internal sealed class AssetThumbnails(Func<string> root) : IDisposable
{
    private readonly Dictionary<string, Bitmap?> textures = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string>? sources;

    public void Draw(DrawItemEventArgs e, ItemData item)
    {
        const int size = 32;
        e.DrawBackground();
        var source = item.SourceRectangle;
        var slot = new Rectangle(e.Bounds.Left + 5, e.Bounds.Top + (e.Bounds.Height - size) / 2, size, size);
        if (GetTexture(item.TexturePath) is { } image && source.Width > 0 && source.Height > 0 &&
            source.X >= 0 && source.Y >= 0 && source.Width <= image.Width - source.X && source.Height <= image.Height - source.Y)
        {
            float scale = Math.Min(size / (float)source.Width, size / (float)source.Height);
            float w = source.Width * scale;
            float h = source.Height * scale;
            using var background = new SolidBrush(Color.FromArgb(48, 50, 54));
            e.Graphics.FillRectangle(background, slot);
            var state = e.Graphics.Save();
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.DrawImage(image, new RectangleF(slot.Left + (size - w) / 2, slot.Top + (size - h) / 2, w, h),
                new Rectangle(source.X, source.Y, source.Width, source.Height), GraphicsUnit.Pixel);
            e.Graphics.Restore(state);
        }
        var label = Rectangle.FromLTRB(slot.Right + 8, e.Bounds.Top, e.Bounds.Right, e.Bounds.Bottom);
        TextRenderer.DrawText(e.Graphics, item.Name, e.Font, label, e.ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        e.DrawFocusRectangle();
    }

    private Bitmap? GetTexture(string asset)
    {
        asset = (asset ?? string.Empty).Replace('\\', '/');
        if (textures.TryGetValue(asset, out var cached))
            return cached;

        Bitmap? bitmap = null;
        try
        {
            sources ??= ContentReferenceValidator.TextureSources(root());
            if (sources.TryGetValue(asset, out string? path))
            {
                using var image = Image.FromFile(path);
                bitmap = new Bitmap(image);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or OutOfMemoryException)
        {
            System.Diagnostics.Trace.WriteLine($"Could not preview '{asset}'. {ex.Message}");
        }
        textures[asset] = bitmap;
        return bitmap;
    }

    public void Dispose()
    {
        foreach (var image in textures.Values)
            image?.Dispose();
        textures.Clear();
    }
}
