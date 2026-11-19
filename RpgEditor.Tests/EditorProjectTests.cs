using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;

namespace RpgEditor.Tests;

public sealed class EditorProjectTests
{
    [Fact]
    public void RenamingAssetsPropagatesQuestRewardsPrerequisitesSlayTargetsAndGuaranteedDrops()
    {
        using var directory = new TestDirectory();
        var project = EditorProject.Create(directory.Path, new RPG("References", "Fixture"));
        project.Change(null, new ItemData { Name = "Old Coin", Quantity = 1, Stackable = true });
        project.Change(null, new QuestData { Name = "Entry Quest" });
        var enemy = new EnemyData { Type = "Game.Goblin", GuaranteedItems = [new("Old Coin", 4)] };
        project.Change(null, enemy);
        var hunt = new QuestData { Name = "Hunt", RequiredQuestNameData = ["Entry Quest"],
            BaseTasksData = [new SlayTaskData { MonsterToSlay = enemy.Type, RequiredAmount = 3 }],
            RewardData = new() { Coins = 99, Items = [new ItemData { Name = "Old Coin", Quantity = 2 }] } };
        project.Change(null, hunt);
        project.Save();

        EditorAsset Find(string name) => project.Assets.Single(entry => entry.Name == name);
        var item = Find("Old Coin");
        Assert.Collection(project.FindUsages(item),
            use => Assert.Contains("Guaranteed drop", use.Location),
            use => Assert.Contains("Reward item", use.Location));
        var updatedItem = ((ItemData)item.Data).Clone();
        updatedItem.Name = "New Coin";
        project.Change(item, updatedItem, propagateRename: true);

        var prerequisite = Find("Entry Quest");
        Assert.Contains(project.FindUsages(prerequisite), use => use.Location.Contains("Prerequisite"));
        project.Change(prerequisite, new QuestData { Name = "Introduction" }, propagateRename: true);
        var goblin = Find(enemy.Type);
        Assert.Contains(project.FindUsages(goblin), use => use.Location.Contains("Slay task 1"));
        project.Change(goblin, new EnemyData((EnemyData)goblin.Data) { Type = "Game.NewGoblin" }, propagateRename: true);

        Assert.Equal("Old Coin", Assert.Single(enemy.GuaranteedItems).Name);
        Assert.Equal("Old Coin", Assert.Single(hunt.RewardData.Items).Name);
        Assert.Contains(EditorProject.Open(project.Root).Assets, asset => asset.Name == "Old Coin");
        project.Save();

        var reopened = EditorProject.Open(project.Root);
        var savedEnemy = Assert.IsType<EnemyData>(reopened.Assets.Single(asset => asset.Name == "Game.NewGoblin").Data);
        var drop = Assert.Single(savedEnemy.GuaranteedItems);
        Assert.Equal(("New Coin", 4), (drop.Name, drop.Quantity));
        var savedHunt = Assert.IsType<QuestData>(reopened.Assets.Single(asset => asset.Name == "Hunt").Data);
        var reward = Assert.Single(savedHunt.RewardData.Items);
        Assert.Equal(("New Coin", 2), (reward.Name, reward.Quantity));
        Assert.Equal(99, savedHunt.RewardData.Coins);
        Assert.Equal("Introduction", Assert.Single(savedHunt.RequiredQuestNameData));
        Assert.Equal("Game.NewGoblin", Assert.IsType<SlayTaskData>(Assert.Single(savedHunt.BaseTasksData)).MonsterToSlay);
    }

    [Fact]
    public void DeletingAnAssetCanRetargetReferencesAndRejectsCyclicQuestReplacements()
    {
        using var directory = new TestDirectory();
        var project = EditorProject.Create(directory.Path, new RPG("References", "Fixture"));
        project.Change(null, new QuestData { Name = "First" });
        project.Change(null, new QuestData { Name = "Middle", RequiredQuestNameData = ["First"] });
        project.Change(null, new QuestData { Name = "Last", RequiredQuestNameData = ["Middle"] });
        project.Change(null, new QuestData { Name = "Alternate" });
        EditorAsset Find(string name) => project.Assets.Single(entry => entry.Name == name);

        Assert.False(project.CanReplaceReferences(Find("First"), Find("Last")));
        Assert.Throws<InvalidDataException>(() => project.Remove(Find("First"), Find("Last")));
        Assert.Equal("First", Assert.Single(Assert.IsType<QuestData>(Find("Middle").Data).RequiredQuestNameData));

        project.Remove(Find("First"), Find("Alternate"));
        Assert.DoesNotContain(project.Assets, asset => asset.Name == "First");
        Assert.Equal("Alternate", Assert.Single(Assert.IsType<QuestData>(Find("Middle").Data).RequiredQuestNameData));
    }

    [Fact]
    public void TiledReferencesReportObjectIdsAndStayManualWhenAnAssetIsRenamed()
    {
        using var directory = new TestDirectory();
        string root = directory.Path;
        Directory.CreateDirectory(Path.Combine(root, "Items"));
        Directory.CreateDirectory(Path.Combine(root, "EntityData"));
        Directory.CreateDirectory(Path.Combine(root, "Quests"));
        Directory.CreateDirectory(Path.Combine(root, "TiledFiles"));
        XnaSerializer.Serialize(Path.Combine(root, "EntityData/Spider.xml"), new EnemyData { Type = "Game.Spider" });
        XnaSerializer.Serialize(Path.Combine(root, "Quests/Hunt.xml"), new QuestData { Name = "Hunt" });
        XnaSerializer.Serialize(Path.Combine(root, "Quests/Another Quest.xml"), new QuestData { Name = "Another Quest" });
        XnaSerializer.Serialize(Path.Combine(root, "Quests/Hunt Again.xml"), new QuestData { Name = "Hunt Again" });
        XnaSerializer.Serialize(Path.Combine(root, "Items/Sword.xml"), new WeaponData { Name = "Sword", TexturePath = "Sword" });
        File.WriteAllBytes(Path.Combine(root, "Sword.png"), []);
        File.WriteAllText(Path.Combine(root, "TiledFiles/doors_lever_chest_animation.tsx"), "<tileset />");
        foreach (string level in new[] { "Level0", "Level0_Test", "Level1", "TestLevel" })
            File.WriteAllText(Path.Combine(root, $"TiledFiles/{level}.tmx"), "<map />");
        string mapPath = Path.Combine(root, "TiledFiles/Level0.tmx");
        const string Map = """
            <map>
              <objectgroup name="InteractableObjectLayerObjects">
                <object id="31" name="Quest giver"><properties><property name="Quests" value="Hunt, Another Quest"/></properties></object>
                <object id="33" name="Different quest"><properties><property name="Quests" value="Hunt Again"/></properties></object>
              </objectgroup>
              <objectgroup name="SpawnerLayer"><object id="42" name="Spider"/></objectgroup>
            </map>
            """;
        File.WriteAllText(mapPath, Map);

        var project = EditorProject.Open(root);
        var quest = project.Assets.Single(asset => asset.Name == "Hunt");
        var questUse = Assert.Single(project.FindUsages(quest));
        Assert.False(questUse.CanPropagate);
        Assert.Equal("TiledFiles/Level0.tmx", questUse.File);
        Assert.Contains("Object 31", questUse.Location);
        var spider = project.Assets.Single(asset => asset.Name == "Game.Spider");
        Assert.Contains("Object 42", Assert.Single(project.FindUsages(spider)).Location);
        Assert.Contains(project.FindUsages(project.Assets.Single(asset => asset.Name == "Sword")),
            use => !use.CanPropagate && use.Location == "Basic drop table");
        project.Change(quest, new QuestData { Name = "New Hunt" }, propagateRename: true);
        var questError = Assert.Throws<InvalidDataException>(() => project.Save());
        Assert.Contains("object 31", questError.Message);
        Assert.Equal(Map, File.ReadAllText(mapPath));
        File.WriteAllText(mapPath, Map.Replace("Hunt, Another Quest", "New Hunt, Another Quest", StringComparison.Ordinal));

        project.Change(spider, new EnemyData { Type = "Game.NewSpider" }, propagateRename: true);
        var enemyError = Assert.Throws<InvalidDataException>(() => project.Save());
        Assert.Contains("Spawner object 42", enemyError.Message);
        File.WriteAllText(mapPath, File.ReadAllText(mapPath).Replace("name=\"Spider\"", "name=\"NewSpider\"", StringComparison.Ordinal));
        project.Save();
        Assert.False(project.IsDirty);
    }

    [Fact]
    public void RecentProjectsPersistInOrderAndReopeningDoesNotDuplicateThem()
    {
        using var directory = new TestDirectory();
        string historyPath = Path.Combine(directory.Path, "preferences", "RecentProjects.json");
        var history = new RecentProjects(historyPath);
        Assert.Empty(history.Entries);

        var projects = Enumerable.Range(0, 11)
            .Select(i => EditorProject.Create(Path.Combine(directory.Path, i.ToString()), new RPG($"Project {i}", "Fixture")))
            .ToArray();
        foreach (var project in projects)
            history.Remember(project);

        var reopenedHistory = new RecentProjects(historyPath);
        Assert.Equal(projects.Skip(1).Reverse().Select(project => project.Root), reopenedHistory.Entries.Select(entry => entry.Path));

        var reopenedProject = EditorProject.Open(Path.GetDirectoryName(projects[4].Root)!);
        reopenedHistory.Remember(reopenedProject);
        var saved = new RecentProjects(historyPath).Entries;
        Assert.Equal(new RecentProject(projects[4].Root, projects[4].Title), saved[0]);
        Assert.Equal(10, saved.Count);
        Assert.Single(saved, entry => entry.Path == projects[4].Root);
    }

    [Fact]
    public void UnreadableRecentHistoryCanBeReplacedWhenAProjectOpens()
    {
        using var directory = new TestDirectory();
        string historyPath = Path.Combine(directory.Path, "RecentProjects.json");
        File.WriteAllText(historyPath, "{incomplete");
        var history = new RecentProjects(historyPath);
        Assert.Empty(history.Entries);

        var project = EditorProject.Create(directory.Path, new RPG("Recovered history", "Fixture"));
        history.Remember(project);
        Assert.Equal(new RecentProject(project.Root, project.Title), Assert.Single(new RecentProjects(historyPath).Entries));
    }

    [Fact]
    public void StagedEditsRenamesAndDeletesPersistInTheSameFiles()
    {
        using var directory = new TestDirectory();
        var project = EditorProject.Create(directory.Path, new RPG("Sample", "Editor regression fixture"));
        var armor = new ArmorData { Name = "Old, Robe", Quantity = 1, LevelRequirementData = new() { Level = 8 } };
        var quest = new QuestData { Name = "Gather" };
        var enemy = new EnemyData { Type = "Test.Skeleton", BaseHealth = 10, DropTableName = "BasicDropTable",
            GuaranteedItems = [new("Bones", 2)] };
        var another = new EnemyData { Type = "Test.Spider", BaseHealth = 20 };
        project.Change(null, armor);
        project.Change(null, quest);
        project.Change(null, enemy);
        project.Change(null, another);
        Assert.True(project.IsDirty);
        Assert.Empty(EditorProject.Open(directory.Path).Assets);
        project.Save();
        Assert.False(project.IsDirty);

        var opened = EditorProject.Open(project.Root);
        var armorEntry = opened.Assets.Single(asset => asset.Data is ArmorData);
        var edited = ((ArmorData)armorEntry.Data).Clone();
        edited.Name = "Renamed Robe";
        opened.Change(armorEntry, edited);
        var questEntry = opened.Assets.Single(asset => asset.Data is QuestData);
        opened.Change(questEntry, new QuestData { Name = "Gather", Description = "Persist this edit" });
        var spider = opened.Assets.Single(asset => asset.Name == another.Type);
        opened.Remove(spider);
        Assert.True(File.Exists(System.IO.Path.Combine(opened.Root, spider.RelativePath)));
        opened.Save();

        var result = EditorProject.Open(project.Root);
        var loadedArmor = result.Assets.Single(asset => asset.Data is ArmorData);
        Assert.Equal(armorEntry.RelativePath, loadedArmor.RelativePath);
        Assert.Equal("Renamed Robe", loadedArmor.Name);
        Assert.Equal(8, Assert.IsType<ArmorData>(loadedArmor.Data).LevelRequirementData.Level);
        var loadedEnemy = Assert.Single(result.Assets.Select(asset => asset.Data).OfType<EnemyData>());
        Assert.Equal(enemy.Type, loadedEnemy.Type);
        Assert.Equal(2, Assert.Single(loadedEnemy.GuaranteedItems).Quantity);
        Assert.Equal("Persist this edit", Assert.Single(result.Assets.Select(asset => asset.Data).OfType<QuestData>()).Description);
        Assert.False(File.Exists(System.IO.Path.Combine(opened.Root, spider.RelativePath)));
    }

    [Fact]
    public void FailedSaveRestoresEarlierEditsAndKeepsPendingChangesForRetry()
    {
        using var directory = new TestDirectory();
        var project = EditorProject.Create(directory.Path, new RPG("Sample", "Editor regression fixture"));
        project.Change(null, new QuestData { Name = "First", Description = "Original" });
        project.Change(null, new QuestData { Name = "Second", Description = "Original" });
        project.Save();
        var first = project.Assets.Single(asset => asset.Name == "First");
        var second = project.Assets.Single(asset => asset.Name == "Second");
        project.Change(first, new QuestData { Name = "First", Description = "Edited" });
        project.Remove(second);
        string secondPath = System.IO.Path.Combine(project.Root, second.RelativePath);

        using (var locked = File.Open(secondPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            Assert.ThrowsAny<IOException>(() => project.Save());

        Assert.True(project.IsDirty);
        var original = EditorProject.Open(project.Root);
        Assert.Equal(2, original.Assets.Count());
        Assert.All(original.Assets, entry => Assert.Equal("Original", Assert.IsType<QuestData>(entry.Data).Description));

        project.Save();
        Assert.False(project.IsDirty);
        Assert.Equal("Edited", Assert.IsType<QuestData>(Assert.Single(EditorProject.Open(project.Root).Assets).Data).Description);
    }

    [Fact]
    public void CurrentContentLayoutAcceptsAuthoringAndRollsBackBrokenReferences()
    {
        using var directory = new TestDirectory();
        string root = System.IO.Path.Combine(directory.Path, "Assets");
        string itemPath = Write("Items/Weapons/Sword.xml");
        XnaSerializer.Serialize(itemPath, new WeaponData { Name = "Sword", Quantity = 1, TexturePath = "Sword" });
        File.WriteAllBytes(Write("Sword.png"), []);
        foreach (string map in new[] { "Level0", "Level0_Test", "Level1", "TestLevel" })
            File.WriteAllText(Write($"TiledFiles/{map}.tmx"), "<map />");
        File.WriteAllText(Write("TiledFiles/doors_lever_chest_animation.tsx"), "<tileset />");
        XnaSerializer.Serialize(Write("EntityData/SpiderData.xml"), new EnemyData { Type = "Spider", BaseHealth = 5 });

        var project = EditorProject.Open(directory.Path);
        Assert.True(project.IsGameContent);
        project.Change(null, new ShieldData { Name = "Shield", Quantity = 1, TexturePath = "Sword" });
        project.Change(null, new ConsumableData { Name = "Potion", Quantity = 1, TexturePath = "Sword" });
        project.Change(null, new QuestData { Name = "Clear the Road", RequiredQuestNameData = ["SlaySkeletons"],
            BaseTasksData = [new SlayTaskData { MonsterToSlay = "Spider" }],
            RewardData = new() { Items = [new() { Name = "Potion", Quantity = 1 }] } });
        project.Save();
        var opened = EditorProject.Open(root);
        Assert.Single(opened.Assets, asset => asset.Data is ShieldData);
        Assert.Single(opened.Assets, asset => asset.Data is ConsumableData);
        var quest = Assert.IsType<QuestData>(opened.Assets.Single(asset => asset.Data is QuestData).Data);
        Assert.Equal("Potion", Assert.Single(quest.RewardData.Items).Name);
        Assert.IsType<SlayTaskData>(Assert.Single(quest.BaseTasksData));

        var sword = opened.Assets.Single(asset => asset.Data is WeaponData);
        var broken = ((WeaponData)sword.Data).Clone();
        broken.TexturePath = "BadTexture";
        opened.Change(sword, broken);
        var failure = Assert.Throws<InvalidDataException>(() => opened.Save());
        Assert.Contains("BadTexture", failure.Message);
        Assert.True(opened.IsDirty);
        Assert.Equal("Sword", XnaSerializer.Deserialize<ItemData>(itemPath).TexturePath);

        string Write(string relative)
        {
            string path = System.IO.Path.Combine(root, relative);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            return path;
        }
    }

    private sealed class TestDirectory : IDisposable
    {
        public string Path { get; } = Directory.CreateTempSubdirectory("RpgEditorTests-").FullName;
        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
