using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;

namespace RpgEditor.Tests;

public sealed class EditorProjectTests
{
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
