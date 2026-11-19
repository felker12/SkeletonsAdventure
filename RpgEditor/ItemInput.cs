using RpgLibrary.ItemClasses;

namespace RpgEditor;

internal static class ItemInput
{
    public static string? Validate(ItemData item, EditorProject? project)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
            return "Enter an item name.";
        if (item.Price < 0 || !float.IsFinite(item.Weight) || item.Weight < 0)
            return "Price and weight must be nonnegative.";
        if (item.Quantity < 1 || (!item.Stackable && item.Quantity != 1))
            return "Quantity must be positive. Non-stackable items have a quantity of one.";
        var source = item.SourceRectangle;
        if (source.X < 0 || source.Y < 0 || source.Width < 1 || source.Height < 1)
            return "Enter a source rectangle with nonnegative coordinates and positive dimensions.";
        if (project?.IsGameContent == true)
        {
            var textures = ContentReferenceValidator.TextureSources(project.Root);
            if (!textures.TryGetValue(item.TexturePath.Replace('\\', '/'), out string? path))
                return "Choose a texture included in the game content build.";
            try
            {
                using var image = Image.FromFile(path);
                if (source.Width > image.Width - source.X || source.Height > image.Height - source.Y)
                    return "The source rectangle must fit inside the selected texture.";
            }
            catch (Exception ex) when (ex is IOException or ArgumentException or OutOfMemoryException)
            {
                return $"Could not open texture '{item.TexturePath}'.";
            }
        }
        return null;
    }
}
