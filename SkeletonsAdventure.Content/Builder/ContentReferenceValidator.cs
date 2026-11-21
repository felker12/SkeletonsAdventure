using System.Xml;
using System.Xml.Linq;
using System.Globalization;
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
    internal sealed record QuestTarget(QuestLocationAction Action, string Map, int ObjectId, string Name)
    {
        public override string ToString() => $"{Map} · {Name} (#{ObjectId})";
    }

    internal static IReadOnlyList<QuestTarget> ReadQuestTargets(string root)
    {
        var targets = new List<QuestTarget>();
        foreach (string level in MapCatalog.LevelNames)
        {
            string path = Path.Combine(root, MapCatalog.AssetName(level) + ".tmx");
            var map = ReadRoot(path, "map");
            AddTargets(MapCatalog.InteractableLayer, QuestLocationAction.Interact);
            AddTargets(MapCatalog.QuestAreaLayer, QuestLocationAction.ReachArea);

            void AddTargets(string layerName, QuestLocationAction action)
            {
                var layer = map.Elements("objectgroup").FirstOrDefault(candidate => (string?)candidate.Attribute("name") == layerName);
                if (layer is null)
                    return;
                foreach (var obj in layer.Elements("object"))
                {
                    if (!int.TryParse((string?)obj.Attribute("id"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ||
                        id <= 0 || !HasSize(obj))
                        continue;
                    if (action == QuestLocationAction.ReachArea)
                    {
                        if (obj.Elements().Any(child => child.Name != "properties") ||
                            obj.Attribute("gid") is not null || obj.Attribute("template") is not null || IsRotated(obj))
                            continue;
                    }
                    else
                    {
                        var properties = obj.Element("properties")?.Elements("property").ToArray() ?? [];
                        var type = properties.FirstOrDefault(property => (string?)property.Attribute("name") == "TypeOfObject");
                        if (type is null)
                            continue;
                        if (((string?)type.Attribute("value") ?? type.Value) == "Quest" &&
                            !properties.Any(property => (string?)property.Attribute("name") == "Quests"))
                            continue;
                    }

                    string name = (string?)obj.Attribute("name") ?? "";
                    targets.Add(new(action, level, id, string.IsNullOrWhiteSpace(name) ?
                        (action == QuestLocationAction.Interact ? "Interactable" : "Quest area") : name));
                }
            }
        }
        return targets;

        static bool HasSize(XElement obj) =>
            double.TryParse((string?)obj.Attribute("width"), NumberStyles.Float, CultureInfo.InvariantCulture, out double width) &&
            double.TryParse((string?)obj.Attribute("height"), NumberStyles.Float, CultureInfo.InvariantCulture, out double height) &&
            width is >= 1 and <= int.MaxValue && height is >= 1 and <= int.MaxValue;

        static bool IsRotated(XElement obj) => obj.Attribute("rotation") is { } rotation &&
            (!double.TryParse(rotation.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double angle) || angle != 0);
    }

    internal static void WriteAssetList(string sourceDirectory, string destination)
    {
        string root = Path.GetFullPath(sourceDirectory);
        var paths = Files(root, "Items").Concat(Files(root, "EntityData")).Concat(Files(root, "Quests"))
            .Select(path => Path.ChangeExtension(Path.GetRelativePath(root, path), null).Replace('\\', '/'))
            .Append(TextureCatalog.AssetName)
            .Order(StringComparer.Ordinal);
        string text = string.Join('\n', paths) + "\n";
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
        if (!File.Exists(destination) || File.ReadAllText(destination) != text)
            File.WriteAllText(destination, text);
    }

    public static void Validate(string sourceDirectory)
    {
        string root = Path.GetFullPath(sourceDirectory);
        var textureAssets = TextureSources(root);
        var textures = ReadTextureCatalog(root);
        string catalogPath = Path.Combine(root, TextureCatalog.AssetName + ".xml");
        if (!textures.ContainsKey(TextureCatalog.SkeletonBody))
            throw Error(catalogPath, $"Missing required texture '{TextureCatalog.SkeletonBody}'.");
        foreach (var (name, path) in textures)
        {
            if (string.IsNullOrWhiteSpace(name) || name != name.Trim())
                throw Error(catalogPath, $"Invalid texture name '{name}'.");
            if (string.IsNullOrWhiteSpace(path) || !textureAssets.ContainsKey(path))
                throw Error(catalogPath, $"Texture '{name}' references asset '{path}', which is not included in the content build.");
        }

        var items = ValidateItems(root, textureAssets);
        var enemies = ValidateEnemies(root, items, textures);
        var quests = ValidateQuests(root, items, enemies);
        var spawnerNames = enemies.Select(EnemyClassCatalog.SpawnName).ToHashSet(StringComparer.Ordinal);

        HashSet<string> checkedTilesets = new(StringComparer.Ordinal);
        foreach (string name in MapCatalog.LevelNames)
        {
            string path = Path.Combine(root, MapCatalog.AssetName(name) + ".tmx");
            var map = ReadRoot(path, "map");
            foreach (var layer in map.Descendants("objectgroup").Where(e => (string?)e.Attribute("name") == "SpawnerLayer"))
            {
                foreach (var obj in layer.Elements("object"))
                {
                    string? spawner = (string?)obj.Attribute("name");
                    if (!string.IsNullOrWhiteSpace(spawner) && !spawnerNames.Contains(spawner))
                        throw Error(path, $"Spawner object {obj.Attribute("id")?.Value} references unknown enemy '{spawner}'.");
                }
            }
            foreach (var property in map.Descendants("property").Where(e => (string?)e.Attribute("name") == "Quests"))
            {
                string names = (string?)property.Attribute("value") ?? property.Value;
                foreach (string questName in names.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    if (!quests.Contains(questName))
                    {
                        string? id = (string?)property.Ancestors("object").FirstOrDefault()?.Attribute("id");
                        string location = id is null ? "a Tiled Quests property" : $"the Quests property on object {id}";
                        throw Error(path, $"Unknown quest '{questName}' in {location}.");
                    }
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

    internal static Dictionary<string, string> ReadTextureCatalog(string root, Stream? source = null)
    {
        string path = Path.Combine(root, TextureCatalog.AssetName + ".xml");
        try
        {
            var catalog = source is null ? XnaSerializer.Deserialize<Dictionary<string, string>>(path) :
                XnaSerializer.Deserialize<Dictionary<string, string>>(source);
            return catalog ?? throw new InvalidDataException("Expected a texture catalog.");
        }
        catch (Exception ex) { throw Error(path, ex.Message); }
    }

    private static Dictionary<string, bool> ValidateItems(string root, Dictionary<string, string> textureAssets)
    {
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

    private static HashSet<string> ValidateEnemies(string root, Dictionary<string, bool> items, Dictionary<string, string> textures)
    {
        var enemies = new HashSet<string>(StringComparer.Ordinal);
        foreach (string path in Files(root, "EntityData"))
        {
            var enemy = ReadAsset<EnemyData>(path);
            if (!EnemyClassCatalog.Contains(enemy.Type))
                throw Error(path, $"Unknown enemy class '{enemy.Type}'. Choose a registered enemy class.");
            if (string.IsNullOrWhiteSpace(enemy.Type) || !enemies.Add(enemy.Type))
                throw Error(path, $"Missing or duplicate enemy type '{enemy.Type}'.");
            if (string.IsNullOrWhiteSpace(enemy.TextureName))
                throw Error(path, $"Enemy '{enemy.Type}' must declare a TextureName from the texture catalog.");
            if (!textures.ContainsKey(enemy.TextureName))
                throw Error(path, $"Enemy '{enemy.Type}' references unknown texture name '{enemy.TextureName}'.");
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
        HashSet<(QuestLocationAction Action, string Map, int ObjectId)>? targetIds = null;
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
            foreach (var task in quest.BaseTasksData.OfType<LocationTaskData>())
            {
                targetIds ??= ReadQuestTargets(root).Select(target => (target.Action, target.Map, target.ObjectId)).ToHashSet();
                if (!targetIds.Contains((task.Action, task.MapName, task.ObjectId)))
                    throw Error(path, $"Invalid {task.Action} target in '{task.MapName}', object {task.ObjectId}.");
                if (task.RequiredAmount != 1 || task.CompletedAmount is < 0 or > 1)
                    throw Error(path, $"Location task '{task.TaskToComplete}' must have a required count of one and progress of zero or one.");
            }
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
