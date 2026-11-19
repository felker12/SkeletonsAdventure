using System.Xml;
using System.Xml.Linq;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;
using SkeletonsAdventure.Content;

namespace RpgEditor;

internal sealed record AssetUsage(string File, string Location, bool CanPropagate);

internal static class AssetReferences
{
    private enum Kind { Item, Quest, Enemy }

    public static IReadOnlyList<AssetUsage> Find(EditorProject project, EditorAsset target)
    {
        Kind kind = GetKind(target);
        var uses = project.Assets.OrderBy(asset => asset.RelativePath, StringComparer.OrdinalIgnoreCase)
            .SelectMany(asset => GetLinks(asset.Data, kind)
                .Where(link => link.Name == target.Name)
                .Select(link => new AssetUsage(asset.RelativePath, $"{asset.Name} / {link.Location}", true)))
            .ToList();
        if (!project.IsGameContent)
            return uses;

        FindTiledUses(project.Root, target, kind, uses);
        if (kind == Kind.Item)
        {
            if (DataAssetCatalog.BasicDrops.Any(entry => entry.Name == target.Name))
                uses.Add(new("Game code", "Basic drop table", false));
            if (target.Name == DataAssetCatalog.Coins)
                uses.Add(new("Game code", "Player currency and starting inventory", false));
            if (target.Name == DataAssetCatalog.Bones)
                uses.Add(new("Game code", "Default skeleton guaranteed drop", false));
        }
        if (kind == Kind.Enemy && target.Name == DataAssetCatalog.EliteSkeletonType)
            uses.Add(new("Game code", "Level lever kill requirement", false));
        if (kind == Kind.Enemy && target.Name == DataAssetCatalog.SkeletonType)
            uses.Add(new("Game code", "Slay target in built-in quests", false));
        if (kind == Kind.Quest && target.Name == BuiltInQuests.TestQuest)
            uses.Add(new("Game code", "Built-in Test2 prerequisite (the built-in quest remains available)", false));
        return uses;
    }

    public static EditorAsset Rewrite(EditorAsset source, EditorAsset target, string replacementName)
    {
        Kind kind = GetKind(target);
        if (!GetLinks(source.Data, kind).Any(link => link.Name == target.Name))
            return source;

        string oldName = target.Name;
        if (source.Data is EnemyData enemy)
        {
            return source with
            {
                Data = new EnemyData(enemy)
                {
                    GuaranteedItems = enemy.GuaranteedItems.Select(item =>
                        new ItemBaseData(item.Name == oldName ? replacementName : item.Name, item.Quantity)).ToList()
                }
            };
        }

        var quest = ((QuestData)source.Data).Clone();
        switch (kind)
        {
            case Kind.Item:
                foreach (var item in quest.RewardData.Items.Where(item => item.Name == oldName))
                    item.Name = replacementName;
                break;
            case Kind.Quest:
                quest.RequiredQuestNameData = quest.RequiredQuestNameData.Select(name => name == oldName ? replacementName : name)
                    .ToList();
                break;
            case Kind.Enemy:
                foreach (var task in quest.BaseTasksData.OfType<SlayTaskData>().Where(task => task.MonsterToSlay == oldName))
                    task.MonsterToSlay = replacementName;
                break;
        }
        return source with { Data = quest };
    }

    public static bool CanReplace(EditorAsset target, EditorAsset candidate) =>
        !string.Equals(target.RelativePath, candidate.RelativePath, StringComparison.OrdinalIgnoreCase) &&
        GetKind(target) == GetKind(candidate) &&
        (target.Data is not ItemData { Stackable: true } || candidate.Data is ItemData { Stackable: true }) &&
        (target.Data is not EnemyData || candidate.Data is EnemyData);

    private static Kind GetKind(EditorAsset asset) => asset.Data switch
    {
        ItemData => Kind.Item,
        QuestData => Kind.Quest,
        _ => Kind.Enemy
    };

    private static IEnumerable<(string Name, string Location)> GetLinks(object data, Kind target)
    {
        if (data is EnemyData enemy && target == Kind.Item)
            return enemy.GuaranteedItems.Select(item => (item.Name, "Guaranteed drop"));
        if (data is not QuestData quest)
            return [];

        return target switch
        {
            Kind.Item => quest.RewardData.Items.Select(item => (item.Name, "Reward item")),
            Kind.Quest => quest.RequiredQuestNameData.Select(name => (name, "Prerequisite")),
            _ => quest.BaseTasksData.Select((task, index) => (task, index))
                .Where(entry => entry.task is SlayTaskData)
                .Select(entry => (((SlayTaskData)entry.task).MonsterToSlay, $"Slay task {entry.index + 1}"))
        };
    }

    private static void FindTiledUses(string root, EditorAsset target, Kind kind, List<AssetUsage> uses)
    {
        if (kind == Kind.Item)
            return;
        string enemyName = target.Name[(target.Name.LastIndexOf('.') + 1)..];
        foreach (string level in MapCatalog.LevelNames)
        {
            string relative = MapCatalog.AssetName(level) + ".tmx";
            try
            {
                var map = XDocument.Load(Path.Combine(root, relative));
                foreach (var obj in map.Descendants("object"))
                {
                    string? name = (string?)obj.Attribute("name");
                    string? layer = (string?)obj.Parent?.Attribute("name");
                    bool referencesAsset = kind == Kind.Enemy
                        ? layer == "SpawnerLayer" && name == enemyName
                        : obj.Element("properties")?.Elements("property").Any(property =>
                            (string?)property.Attribute("name") == "Quests" &&
                            ((string?)property.Attribute("value") ?? property.Value)
                                .Split(',', StringSplitOptions.TrimEntries).Contains(target.Name, StringComparer.Ordinal)) == true;
                    if (referencesAsset)
                    {
                        string property = kind == Kind.Enemy ? "Spawner name" : "Quests property";
                        uses.Add(new(relative, $"{layer} / Object {obj.Attribute("id")?.Value} '{name}' / {property}", false));
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or XmlException)
            {
                uses.Add(new(relative, $"Could not inspect map. {ex.Message}", false));
            }
        }
    }
}
