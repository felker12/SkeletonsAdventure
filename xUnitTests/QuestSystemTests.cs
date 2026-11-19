using Microsoft.Xna.Framework;
using MonoGame.Extended.Tilemaps;
using RpgLibrary.DataClasses;
using RpgLibrary.GameObjectClasses;
using RpgLibrary.QuestClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.Quests;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    [Fact]
    public void InteractionRequiresAnActiveMatchingTaskAndAValidUse()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var player = World.Player;
        string map = World.CurrentLevel.Name;
        var template = new Quest(new QuestData
        {
            Name = "Use the shrine",
            BaseTasksData = [new LocationTaskData { Action = QuestLocationAction.Interact, MapName = map, ObjectId = 42 }]
        });
        var obj = new TilemapRectangleObject(42, player.Position, new Vector2(64, 64));
        var shrine = new QuestNode(obj, assets.FontsLibrary.Arial12) { MapName = map, Quests = [template] };
        var time = new GameTime(TimeSpan.FromSeconds(2), TimeSpan.Zero);
        shrine.Interact(time, player);
        var quest = Assert.Single(player.ActiveQuests);
        Assert.Equal(0, quest.ActiveTask.CompletedAmount);

        shrine.Interact(time, player);
        Assert.False(quest.IsCompleted); // Same interaction is still on cooldown.
        time.TotalGameTime = TimeSpan.FromSeconds(4);
        shrine.MapName = "DifferentLevel";
        shrine.Interact(time, player);
        Assert.False(quest.IsCompleted);
        shrine.MapName = map;
        shrine.LevelRequirements = new LevelRequirements { Level = player.Level + 1 };
        shrine.Interact(time, player);
        Assert.False(quest.IsCompleted);
        shrine.LevelRequirements = null;
        shrine.Interact(time, player);
        Assert.True(quest.IsCompleted);
    }

    [Fact]
    public void AreasOnlyAdvanceTheCurrentObjectiveOnEntryAndLocationTasksRoundTrip()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var player = World.Player;
        var level = World.CurrentLevel;
        var start = player.Position;
        var area = new Rectangle((int)start.X, (int)start.Y, 64, 64);
        level.QuestAreas.Add(7, area);
        level.QuestAreas.Add(8, area);
        var template = new Quest(new QuestData
        {
            Name = "Visit the area",
            BaseTasksData =
            [
                new LocationTaskData { Action = QuestLocationAction.Interact, MapName = level.Name, ObjectId = 5 },
                new LocationTaskData { Action = QuestLocationAction.ReachArea, MapName = level.Name, ObjectId = 7 },
                new LocationTaskData { Action = QuestLocationAction.ReachArea, MapName = level.Name, ObjectId = 8 }
            ]
        });
        Assert.True(player.TryStartQuest(template));
        var active = Assert.Single(player.ActiveQuests);
        level.QuestAreas.Update(player);
        Assert.Equal(0, active.Tasks[1].CompletedAmount);
        Assert.True(player.ProgressLocationTasks(QuestLocationAction.Interact, level.Name, id => id == 5));
        level.QuestAreas.Update(player);
        Assert.Equal(0, active.Tasks[1].CompletedAmount);

        player.Position = start + new Vector2(100, 0);
        level.QuestAreas.Update(player);
        player.Position = start;
        level.QuestAreas.Update(player);
        Assert.Equal(1, active.Tasks[1].CompletedAmount);
        Assert.Equal(0, active.Tasks[2].CompletedAmount);

        string path = Path.GetTempFileName();
        try
        {
            XnaSerializer.Serialize(path, active.GetQuestData());
            var restored = new Quest(XnaSerializer.Deserialize<QuestData>(path));
            var next = Assert.IsType<LocationTask>(restored.ActiveTask);
            Assert.Equal(8, next.ObjectId);
            Assert.Equal(level.Name, next.MapName);
            Assert.Equal(QuestLocationAction.ReachArea, next.Action);
            Assert.Equal(1, restored.Tasks[1].CompletedAmount);
            player.ActiveQuests[0] = restored;
            level.QuestAreas.Update(player);
            Assert.False(restored.IsCompleted);
            player.Position = start + new Vector2(100, 0);
            level.QuestAreas.Update(player);
            player.Position = start;
            level.QuestAreas.Update(player);
            Assert.True(restored.IsCompleted);
        }
        finally { File.Delete(path); }
    }

    private static Quest SimpleQuest(string name) => new()
    {
        Name = name,
        Tasks = [new BaseTask { TaskToComplete = "First", RequiredAmount = 1 }]
    };

    [Fact]
    public void QuestAcceptanceRequiresAllPrerequisitesAndPreventsDuplicateNames()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var quest = SimpleQuest("Follow-up");
        quest.RequiredQuestNames = ["First", "Second"];
        Assert.False(World.Player.TryStartQuest(quest));
        World.Player.CompletedQuests.Add(new Quest { Name = "First", IsCompleted = true });
        Assert.False(World.Player.TryStartQuest(quest));
        World.Player.CompletedQuests.Add(new Quest { Name = "Second", IsCompleted = true });
        Assert.True(World.Player.TryStartQuest(quest));
        Assert.False(World.Player.TryStartQuest(quest.Clone()));
        World.Player.AddActiveQuest(quest.Clone());
        Assert.Single(World.Player.ActiveQuests);
        Assert.False(quest.Active);
        Assert.Equal("Follow-up", World.Player.DisplayQuestName);
    }

    [Fact]
    public void QuestCompletionIsGuardedAndRewardsOnlyOnce()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var template = SimpleQuest("Reward once");
        template.Reward = new QuestReward { Coins = 12, XP = 7 };
        Assert.True(World.Player.TryStartQuest(template));
        var active = Assert.Single(World.Player.ActiveQuests);
        active.CompleteQuest();
        Assert.False(active.IsCompleted);
        active.ProgressTask();
        active.ProgressTask();
        active.CompleteQuest();
        World.Player.CheckQuestCompleted();
        World.Player.CheckQuestCompleted();
        Assert.Empty(World.Player.ActiveQuests);
        Assert.Single(World.Player.CompletedQuests);
        Assert.Equal(7, World.Player.TotalXP);
        Assert.Equal(12, Assert.Single(World.Player.Backpack.Items).Quantity);
        Assert.False(World.Player.TryStartQuest(template));
        Assert.Equal("", World.Player.DisplayQuestName);
        Assert.Equal(0, template.Tasks[0].CompletedAmount);
    }

    [Fact]
    public void UnstartedCompletedAndEmptyQuestsDoNotAdvanceOrRepeatMessages()
    {
        World.MessagesToAdd.Clear();
        var quest = SimpleQuest("Safe progression");
        quest.ProgressTask();
        quest.CompleteQuest();
        Assert.False(quest.IsCompleted);
        Assert.Equal(0, quest.Tasks[0].CompletedAmount);
        quest.StartQuest();
        quest.ProgressTask();
        int messageCount = World.MessagesToAdd.Count;
        quest.ProgressTask();
        quest.CheckTasksCompleted();
        quest.CompleteQuest();
        quest.StartQuest();
        Assert.False(quest.Active);
        Assert.True(quest.IsCompleted);
        Assert.Equal(messageCount, World.MessagesToAdd.Count);
        var empty = new Quest { Name = "No objectives" };
        empty.StartQuest();
        empty.CheckTasksCompleted();
        Assert.False(empty.Active);
        Assert.False(empty.IsCompleted);
    }

    [Fact]
    public void QuestGiverAvailabilityAndInteractionRespectRequirementsAndCooldown()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var first = SimpleQuest("First");
        var followUp = SimpleQuest("Next");
        followUp.RequiredQuestNames = [first.Name];
        var locked = SimpleQuest("High level");
        locked.Requirements.Level = 99;
        var node = new QuestNode(new QuestNodeData
        {
            Position = World.Player.Position, Width = 64, Height = 64, CooldownTime = 1000
        }, assets.FontsLibrary.Arial12) { Quests = [first, followUp, locked] };
        Assert.Equal(1, node.CheckAvailableQuestsToStart(World.Player));
        var clonedNode = node.Clone();
        clonedNode.Quests[0].Tasks[0].CompletedAmount = 1;
        Assert.Equal(3, clonedNode.Quests.Count);
        Assert.Equal(0, first.Tasks[0].CompletedAmount);
        node.Interact(new GameTime(TimeSpan.FromSeconds(2), TimeSpan.Zero), World.Player);
        Assert.Single(World.Player.ActiveQuests);
        Assert.Equal(0, node.CheckAvailableQuestsToStart(World.Player));
        World.Player.ActiveQuests[0].ProgressTask();
        World.Player.CheckQuestCompleted();
        Assert.Equal(1, node.CheckAvailableQuestsToStart(World.Player));
        node.Interact(new GameTime(TimeSpan.FromSeconds(2.5), TimeSpan.Zero), World.Player);
        Assert.Empty(World.Player.ActiveQuests);
        node.Visible = false;
        node.Interact(new GameTime(TimeSpan.FromSeconds(4), TimeSpan.Zero), World.Player);
        Assert.Empty(World.Player.ActiveQuests);
        node.Visible = true;
        node.Interact(new GameTime(TimeSpan.FromSeconds(4), TimeSpan.Zero), World.Player);
        Assert.Equal("Next", Assert.Single(World.Player.ActiveQuests).Name);
        Assert.Equal(0, first.Tasks[0].CompletedAmount);
    }

    [Fact]
    public void ZeroCoinRewardsDoNotCreateInvalidItemsAndOverflowKeepsEquipmentType()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var zero = SimpleQuest("No rewards");
        World.Player.AddActiveQuest(zero);
        zero.ProgressTask();
        World.Player.CheckQuestCompleted();
        Assert.Empty(World.Player.Backpack.Items);
        Assert.Empty(World.CurrentLevel.EntityManager.DroppedLootManager.Items);
        var weapon = GameManager.ItemsClone.Values.OfType<Weapon>().First();
        for (int i = 0; i < World.Player.Backpack.Capacity; i++)
            World.Player.Backpack.Items.Add(weapon.Clone());
        var reward = SimpleQuest("Overflow");
        reward.Reward = new QuestReward { Coins = 9, Items = [weapon] }.Clone();
        World.Player.AddActiveQuest(reward);
        reward.ProgressTask();
        World.Player.CheckQuestCompleted();
        var drops = World.CurrentLevel.EntityManager.DroppedLootManager.Items;
        Assert.Equal(2, drops.Count);
        Assert.Equal(9, drops.Single(item => item.Name == "Coins").Quantity);
        Assert.IsType<Weapon>(drops.Single(item => item.Name == weapon.Name));
        World.Player.CheckQuestCompleted();
        Assert.Equal(2, drops.Count);
    }

    [Fact]
    public void SequentialSlayObjectivesMatchTheCurrentTargetOnly()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var quest = new Quest(new QuestData
        {
            Name = "Two targets",
            BaseTasksData = [new SlayTaskData { MonsterToSlay = "Skeleton", RequiredAmount = 1 },
                new SlayTaskData { MonsterToSlay = "Goblin", RequiredAmount = 1 }]
        });
        World.Player.AddActiveQuest(quest);
        void Kill(string type)
        {
            var enemy = GameManager.EnemiesClone.Values.First(enemy => enemy.GetType().Name == type);
            enemy.Health = 1;
            enemy.Defence = 0;
            enemy.GetHitByAttack(LethalAttack(World.Player), new());
        }
        Kill("Goblin");
        Assert.Equal(0, quest.Tasks[0].CompletedAmount);
        Kill("Skeleton");
        Assert.Equal(1, quest.Tasks[0].CompletedAmount);
        Assert.Equal(0, quest.Tasks[1].CompletedAmount);
        Kill("Skeleton");
        Assert.Equal(0, quest.Tasks[1].CompletedAmount);
        Kill("Goblin");
        Assert.True(quest.IsCompleted);
        Assert.Null(quest.ActiveTask);
    }

    [Fact]
    public void QuestSaveRoundTripKeepsProgressAndDoesNotPayCompletedRewardsAgain()
    {
        using var directory = new SaveDirectory();
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        game.GameScreen = new GameScreen(game, assets);
        game.ExitScreen = new ExitScreen(game, assets);
        var first = SimpleQuest("Finished");
        first.Reward.Coins = 4;
        World.Player.AddActiveQuest(first);
        first.ProgressTask();
        World.Player.CheckQuestCompleted();
        var progress = SimpleQuest("In progress");
        progress.Tasks[0].RequiredAmount = 3;
        World.Player.AddActiveQuest(progress);
        progress.ProgressTask();
        SaveStorage.Write(directory.Path, SaveManager.CaptureGame(game, game.ExitScreen.ExitScreenMenu));
        SaveManager.RestoreGame(game, assets, SaveStorage.Read(directory.Path));
        World.Player.CheckQuestCompleted();
        Assert.Equal(4, Assert.Single(World.Player.Backpack.Items).Quantity);
        Assert.Single(World.Player.CompletedQuests);
        Assert.Equal(1, Assert.Single(World.Player.ActiveQuests).Tasks[0].CompletedAmount);
        Assert.Equal("In progress", World.Player.DisplayQuestName);
    }

    [Fact]
    public void QuestDataClonesPreserveActiveStateAndSeparateMutableProgressAndRequirements()
    {
        var data = new QuestData
        {
            Active = true,
            RequirementData = new() { Mana = 8 },
            RequiredQuestNameData = ["Previous"],
            BaseTasksData = [new SlayTaskData { MonsterToSlay = "Skeleton", RequiredAmount = 3, CompletedAmount = 1 }]
        };
        var copy = data.Clone();
        copy.BaseTasksData[0].CompletedAmount++;
        copy.RequirementData.Mana = 15;
        copy.RequiredQuestNameData.Clear();
        Assert.True(copy.Active);
        Assert.IsType<SlayTaskData>(copy.BaseTasksData[0]);
        Assert.Equal(1, data.BaseTasksData[0].CompletedAmount);
        Assert.Equal(8, data.RequirementData.Mana);
        Assert.Single(data.RequiredQuestNameData);
        Assert.Equal(8, new LevelRequirements(data.RequirementData).ToData().Mana);
    }

    [Fact]
    public void RestoringDuplicatePendingCompletionsPaysOneRewardAndKeepsTheBestProgress()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        game.GameScreen = new GameScreen(game, assets);
        game.ExitScreen = new ExitScreen(game, assets);
        var finished = SimpleQuest("Pending reward");
        finished.Reward.Coins = 6;
        World.Player.AddActiveQuest(finished);
        finished.ProgressTask();
        var saved = SaveManager.CaptureGame(game, game.ExitScreen.ExitScreenMenu);
        saved.World.PlayerData.activeQuests.Add(finished.GetQuestData());
        var progress = SimpleQuest("Progress");
        progress.StartQuest();
        progress.Tasks[0].RequiredAmount = 5;
        var earlier = progress.GetQuestData();
        progress.ProgressTask();
        saved.World.PlayerData.activeQuests.Add(earlier);
        saved.World.PlayerData.activeQuests.Add(progress.GetQuestData());
        SaveManager.RestoreGame(game, assets, saved);
        World.Player.CheckQuestCompleted();
        World.Player.CheckQuestCompleted();
        Assert.Equal(6, Assert.Single(World.Player.Backpack.Items).Quantity);
        Assert.Single(World.Player.CompletedQuests);
        Assert.Equal(1, Assert.Single(World.Player.ActiveQuests).Tasks[0].CompletedAmount);
    }
}
