using System.Xml;
using System.Xml.Linq;
using SkeletonsAdventure.Content;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;

internal static class ContentReferenceValidator
{
    internal static IReadOnlyList<string> ExcludedTextures { get; } =
    [
        "AttackSprites/FireBallSpriteSheet_OG.png",
        "Player/SkeletonSpriteSheet_LeftHalf.png",
        "Player/SkeletonSpriteSheet_Stylized.png",
        // TSX assets build their own textures and would collide with these output paths.
        "TiledFiles/CrossedOffBlock.png",
        "TiledFiles/doors_lever_chest_animation.png",
        "TiledFiles/fire_animation.png",
        "TiledFiles/Objects.png",
        "TiledFiles/ProjectUtumno_full.png",
        "TiledFiles/walls_floor.png"
    ];
    internal const string LeverTileset = "TiledFiles/doors_lever_chest_animation.tsx";

    internal static void WriteAssetList(string sourceDirectory, string destination)
    {
        string root = Path.GetFullPath(sourceDirectory);
        var paths = Files(root, "Items").Concat(Files(root, "EntityData")).Concat(Files(root, "Quests"))
            .Select(path => Path.ChangeExtension(Path.GetRelativePath(root, path), null).Replace('\\', '/'))
            .Order(StringComparer.Ordinal);
        string text = string.Join('\n', paths) + "\n";
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
        if (!File.Exists(destination) || File.ReadAllText(destination) != text)
            File.WriteAllText(destination, text);
    }

    public static void Validate(string sourceDirectory)
    {
        string root = Path.GetFullPath(sourceDirectory);
        var items = ValidateItems(root);
        var enemies = ValidateEnemies(root, items);
        var quests = ValidateQuests(root, items, enemies);

        HashSet<string> checkedTilesets = new(StringComparer.Ordinal);
        foreach (string name in MapCatalog.LevelNames)
        {
            string path = Path.Combine(root, MapCatalog.AssetName(name) + ".tmx");
            var map = ReadRoot(path, "map");
            foreach (var property in map.Descendants("property").Where(e => (string?)e.Attribute("name") == "Quests"))
            {
                string names = (string?)property.Attribute("value") ?? property.Value;
                foreach (string questName in names.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    if (!quests.Contains(questName))
                        throw Error(path, $"Unknown quest '{questName}' in a Tiled Quests property.");
            }
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
        ValidateTileset(Path.Combine(root, LeverTileset), checkedTilesets);
    }

    internal static Dictionary<string, string> TextureSources(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => Path.GetExtension(path) is ".png" or ".PNG")
            .Select(path => (File: path, Relative: Path.GetRelativePath(root, path).Replace('\\', '/')))
            .Where(texture => !ExcludedTextures.Contains(texture.Relative, StringComparer.Ordinal))
            .ToDictionary(texture => Path.ChangeExtension(texture.Relative, null), texture => texture.File, StringComparer.Ordinal);

    private static Dictionary<string, bool> ValidateItems(string root)
    {
        var textureAssets = TextureSources(root);
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        var items = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (string path in Directory.EnumerateFiles(Path.Combine(root, "Items"), "*.xml", SearchOption.AllDirectories))
        {
            XElement? item = ReadRoot(path, "XnaContent").Element("Asset");
            string? name = item?.Element("Name")?.Value;
            if (string.IsNullOrWhiteSpace(name))
                throw Error(path, "Item asset must declare a Name.");
            if (!names.TryAdd(name, path))
                throw Error(path, $"Duplicate item name '{name}', also defined in '{names[name]}'.");

            string? texture = item?.Element("TexturePath")?.Value.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(texture) || !textureAssets.ContainsKey(texture))
                throw Error(path, $"Item '{name}' references texture '{texture}', which is not included in the content build.");
            items.Add(name, string.Equals(item?.Element("Stackable")?.Value, "true", StringComparison.OrdinalIgnoreCase));
        }
        return items;
    }

    private static HashSet<string> ValidateEnemies(string root, Dictionary<string, bool> items)
    {
        var enemies = new HashSet<string>(StringComparer.Ordinal);
        foreach (string path in Files(root, "EntityData"))
        {
            var enemy = ReadAsset<EnemyData>(path);
            if (string.IsNullOrWhiteSpace(enemy.Type) || !enemies.Add(enemy.Type))
                throw Error(path, $"Missing or duplicate enemy type '{enemy.Type}'.");
            if (enemy.DropTableName.Length > 0 && enemy.DropTableName != DataAssetCatalog.BasicDropTable)
                throw Error(path, $"Unknown enemy drop table '{enemy.DropTableName}'.");
            foreach (var item in enemy.GuaranteedItems)
                ValidateItemReference(path, item.Name, item.Quantity, items);
        }
        return enemies;
    }

    private static HashSet<string> ValidateQuests(string root, Dictionary<string, bool> items, HashSet<string> enemies)
    {
        var authored = new Dictionary<string, (QuestData Quest, string Path)>(StringComparer.Ordinal);
        foreach (string path in Files(root, "Quests"))
        {
            var quest = ReadAsset<QuestData>(path);
            if (string.IsNullOrWhiteSpace(quest.Name) || quest.Name.Contains(',') ||
                !authored.TryAdd(quest.Name, (quest, path)))
                throw Error(path, $"Invalid or duplicate quest name '{quest.Name}'.");
        }
        var names = BuiltInQuests.Names.Concat(authored.Keys).ToHashSet(StringComparer.Ordinal);
        foreach (var (quest, path) in authored.Values)
        {
            foreach (string required in quest.RequiredQuestNameData)
                if (required == quest.Name || !names.Contains(required))
                    throw Error(path, $"Invalid quest prerequisite '{required}'.");
            foreach (var reward in quest.RewardData.Items)
                ValidateItemReference(path, reward.Name, reward.Quantity, items);
            foreach (var task in quest.BaseTasksData.OfType<SlayTaskData>())
                if (!enemies.Contains(task.MonsterToSlay))
                    throw Error(path, $"Unknown slay target '{task.MonsterToSlay}'.");
        }
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        foreach (string questName in authored.Keys)
            CheckPrerequisiteCycle(questName);
        return names;

        void CheckPrerequisiteCycle(string name)
        {
            if (visited.Contains(name))
                return;
            // The only built-in prerequisite is Test2 -> Test Quest.
            if (!authored.TryGetValue(name, out var asset))
            {
                if (name == BuiltInQuests.Test2)
                    CheckPrerequisiteCycle(BuiltInQuests.TestQuest);
                return;
            }
            if (!visiting.Add(name))
                throw Error(asset.Path, $"Quest '{name}' is in a prerequisite cycle.");
            foreach (string prerequisite in asset.Quest.RequiredQuestNameData)
                CheckPrerequisiteCycle(prerequisite);
            visiting.Remove(name);
            visited.Add(name);
        }
    }

    private static void ValidateItemReference(string path, string name, int quantity, Dictionary<string, bool> items)
    {
        if (!items.TryGetValue(name, out bool stackable))
            throw Error(path, $"Unknown item '{name}'.");
        if (quantity < 1 || (!stackable && quantity != 1))
            throw Error(path, $"Invalid quantity {quantity} for '{name}'.");
    }

    private static IEnumerable<string> Files(string root, string directory)
    {
        string path = Path.Combine(root, directory);
        return Directory.Exists(path) ? Directory.EnumerateFiles(path, "*.xml", SearchOption.AllDirectories) : [];
    }

    private static T ReadAsset<T>(string path)
    {
        try { return XnaSerializer.Deserialize<T>(path); }
        catch (Exception ex) { throw Error(path, ex.Message); }
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
