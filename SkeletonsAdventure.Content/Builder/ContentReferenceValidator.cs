using System.Xml;
using System.Xml.Linq;
using SkeletonsAdventure.Content;

internal static class ContentReferenceValidator
{
    public static void Validate(string sourceDirectory)
    {
        string root = Path.GetFullPath(sourceDirectory);
        ValidateItems(root);

        HashSet<string> checkedTilesets = new(StringComparer.Ordinal);
        foreach (string name in MapCatalog.LevelNames)
        {
            string path = Path.Combine(root, MapCatalog.AssetName(name) + ".tmx");
            var map = ReadRoot(path, "map");
            foreach (var tileset in map.Elements("tileset"))
            {
                if (tileset.Attribute("source") is { } source)
                    ValidateTileset(ResolveReference(path, source.Value), checkedTilesets);
                else
                    ValidateImages(path, tileset);
            }
            foreach (var layer in map.Descendants("objectgroup").Where(e => (string?)e.Attribute("name") == "EnterExitLayer"))
            {
                foreach (var obj in layer.Elements("object"))
                {
                    string? objectName = (string?)obj.Attribute("name");
                    if (objectName is not ("Entrance" or "Enter" or "Exit"))
                        continue;

                    var destination = obj.Element("properties")?.Elements("property")
                        .FirstOrDefault(e => (string?)e.Attribute("name") == "ToLocation");
                    if (destination is null && objectName != "Exit")
                        continue;

                    string? target = (string?)destination?.Attribute("value") ?? destination?.Value;
                    if (target is null || !MapCatalog.LevelNames.Contains(target, StringComparer.Ordinal))
                        throw Error(path, $"'{objectName}' object {obj.Attribute("id")?.Value} has an invalid ToLocation '{target}'. Use a level from MapCatalog.");
                }
            }
        }
        ValidateTileset(Path.Combine(root, GameContentBuilder.LeverTileset), checkedTilesets);
    }

    private static void ValidateItems(string root)
    {
        var textureAssets = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => Path.GetExtension(path) is ".png" or ".PNG")
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Except(GameContentBuilder.ExcludedTextures, StringComparer.Ordinal)
            .Select(path => Path.ChangeExtension(path, null))
            .ToHashSet(StringComparer.Ordinal);
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string path in Directory.EnumerateFiles(Path.Combine(root, "Items"), "*.xml", SearchOption.AllDirectories))
        {
            XElement? item = ReadRoot(path, "XnaContent").Element("Asset");
            string? name = item?.Element("Name")?.Value;
            if (string.IsNullOrWhiteSpace(name))
                throw Error(path, "Item asset must declare a Name.");
            if (!names.TryAdd(name, path))
                throw Error(path, $"Duplicate item name '{name}', also defined in '{names[name]}'.");

            string? texture = item?.Element("TexturePath")?.Value.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(texture) || !textureAssets.Contains(texture))
                throw Error(path, $"Item '{name}' references texture '{texture}', which is not included in the content build.");
        }
    }

    private static void ValidateTileset(string path, HashSet<string> checkedTilesets)
    {
        if (checkedTilesets.Add(path))
            ValidateImages(path, ReadRoot(path, "tileset"));
    }

    private static void ValidateImages(string path, XElement tileset)
    {
        foreach (var image in tileset.Descendants("image"))
            ResolveReference(path, (string?)image.Attribute("source"));
    }

    private static string ResolveReference(string documentPath, string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw Error(documentPath, "A Tiled reference is missing its source path.");
        string path = Path.GetFullPath(source, Path.GetDirectoryName(documentPath)!);
        if (!File.Exists(path))
            throw Error(documentPath, $"Tiled source '{source}' could not be found.");
        return path;
    }

    private static XElement ReadRoot(string path, string expected)
    {
        if (!File.Exists(path))
            throw Error(path, "Content source file could not be found.");
        try
        {
            var root = XDocument.Load(path).Root;
            if (root?.Name == expected)
                return root;
            throw Error(path, $"Expected a <{expected}> root element.");
        }
        catch (XmlException ex)
        {
            throw Error(path, ex.Message);
        }
    }

    private static InvalidDataException Error(string path, string message) => new($"{path} : error : {message}");
}
