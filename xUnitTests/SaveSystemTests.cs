using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.GameObjectClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    private sealed class SaveDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SkeletonSaveTests", Guid.NewGuid().ToString("N"));
        public SaveDirectory() => Directory.CreateDirectory(Path);
        public void Dispose() => Directory.Delete(Path, recursive: true);
    }

    private static SaveGameData Snapshot(int health = 10) => new()
    {
        World = new WorldData
        {
            CurrentLevel = "Level0",
            Levels = new() { ["Level0"] = new() },
            PlayerData = new PlayerData { CurrentHealth = health }
        }
    };

    [Fact]
    public void LegacyItemWeightIsDiscardedAndOmittedWhenResaving()
    {
        using var directory = new SaveDirectory();
        var snapshot = Snapshot();
        snapshot.World.PlayerData.backpack =
        [
            new WeaponData { Name = "Sword", Quantity = 1, AttackValue = 7 },
            new ConsumableData { Name = "Bones", Quantity = 3, Stackable = true }
        ];
        SaveStorage.Write(directory.Path, snapshot);
        string path = Path.Combine(directory.Path, SaveStorage.FileName);
        var document = System.Xml.Linq.XDocument.Load(path);
        Assert.Empty(document.Descendants("Weight"));
        var prices = document.Descendants("Price").ToArray();
        Assert.Equal(2, prices.Length);
        foreach (var price in prices)
            price.AddAfterSelf(new System.Xml.Linq.XElement("Weight", "4.5"));
        document.Save(path);

        var restored = SaveStorage.Read(directory.Path);
        Assert.Collection(restored.World.PlayerData.backpack,
            item => Assert.Equal(7, Assert.IsType<WeaponData>(item).AttackValue),
            item => Assert.Equal(3, Assert.IsType<ConsumableData>(item).Quantity));
        SaveStorage.Write(directory.Path, restored);
        Assert.Empty(System.Xml.Linq.XDocument.Load(path).Descendants("Weight"));
    }

    [Fact]
    public void SnapshotReplacementAndBackupRecoveryKeepWholeGenerations()
    {
        using var directory = new SaveDirectory();
        var first = Snapshot(10);
        first.Messages.Add("first");
        SaveStorage.Write(directory.Path, first);
        var second = Snapshot(20);
        second.Messages.Add("second");
        SaveStorage.Write(directory.Path, second);
        Assert.Equal(20, SaveStorage.Read(directory.Path).World.PlayerData.CurrentHealth);
        File.WriteAllText(System.IO.Path.Combine(directory.Path, SaveStorage.FileName), "interrupted/corrupt");
        var recovered = SaveStorage.Read(directory.Path);
        Assert.Equal(10, recovered.World.PlayerData.CurrentHealth);
        Assert.Equal("first", Assert.Single(recovered.Messages));
        SaveStorage.Write(directory.Path, second);
        File.WriteAllText(System.IO.Path.Combine(directory.Path, SaveStorage.FileName), "corrupt again");
        Assert.Equal(10, SaveStorage.Read(directory.Path).World.PlayerData.CurrentHealth);
        Assert.Empty(Directory.GetFiles(directory.Path, "*.tmp"));
    }

    [Fact]
    public void FailedSnapshotWriteLeavesPreviousSaveUntouched()
    {
        using var directory = new SaveDirectory();
        SaveStorage.Write(directory.Path, Snapshot(35));
        // A rejected version must never touch the existing file.
        var broken = Snapshot(1);
        broken.FormatVersion = 99;
        Assert.Throws<InvalidDataException>(() => SaveStorage.Write(directory.Path, broken));
        // A replacement failure after serialization must also preserve it and remove the temp file.
        using (var locked = new FileStream(System.IO.Path.Combine(directory.Path, SaveStorage.FileName), FileMode.Open, FileAccess.Read, FileShare.None))
            Assert.ThrowsAny<IOException>(() => SaveStorage.Write(directory.Path, Snapshot(1)));
        Assert.Equal(35, SaveStorage.Read(directory.Path).World.PlayerData.CurrentHealth);
        Assert.Empty(Directory.GetFiles(directory.Path, "*.tmp"));
    }

    [Fact]
    public void LegacySaveLoadsWithoutRewritingOriginalFiles()
    {
        using var directory = new SaveDirectory();
        var legacy = Snapshot(17);
        legacy.World.Levels["Level0"].Chests.Add(new RpgLibrary.GameObjectClasses.ChestData());
        legacy.Bindings.Keybindings[Keys.D1] = "FireBall";
        XnaSerializer.Serialize(System.IO.Path.Combine(directory.Path, "World.xml"), legacy.World);
        // Original saves predate the optional chest initialization flag.
        string worldPath = System.IO.Path.Combine(directory.Path, "World.xml");
        var xml = System.Xml.Linq.XDocument.Load(worldPath);
        foreach (var flag in xml.Descendants("ContentsGenerated").ToList())
            flag.Remove();
        xml.Save(worldPath);
        XnaSerializer.Serialize(System.IO.Path.Combine(directory.Path, "GameScreenMenuData.xml"), legacy.Menus);
        XnaSerializer.Serialize(System.IO.Path.Combine(directory.Path, "ExitScreenData.xml"), legacy.PauseMenu);
        XnaSerializer.Serialize(System.IO.Path.Combine(directory.Path, "MessageBox.xml"), legacy.Messages);
        XnaSerializer.Serialize(System.IO.Path.Combine(directory.Path, "Keybindings.xml"), legacy.Bindings);
        XnaSerializer.Serialize(System.IO.Path.Combine(directory.Path, "LearnedAttackManager.xml"), legacy.LearnedAttacks);
        var restored = SaveStorage.Read(directory.Path);
        Assert.Equal(17, restored.World.PlayerData.CurrentHealth);
        Assert.Contains("FireBall", restored.LearnedAttacks.LearnedAttackNames);
        Assert.Null(Assert.Single(restored.World.Levels["Level0"].Chests).ContentsGenerated);
        Assert.False(File.Exists(System.IO.Path.Combine(directory.Path, SaveStorage.FileName)));
    }

    [Fact]
    public void LoadingPreservesWoundedAndDeadEnemiesAndDoesNotMultiplyDrops()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var saved = World.ToData();
        var pair = saved.Levels.First(p => p.Value.EntityManagerData.EntityData.OfType<EnemyData>().Count() >= 2);
        var enemies = pair.Value.EntityManagerData.EntityData.OfType<EnemyData>().Take(2).ToArray();
        var fresh = World.Levels[pair.Key].EntityManager.Entities.OfType<Enemy>()
            .First(enemy => enemy.Position == enemies[0].Position && enemy.GetType().FullName == enemies[0].Type);
        enemies[0].BaseXP = fresh.BaseXP - 3;
        enemies[0].CurrentHealth = 1;
        enemies[0].IsDead = false;
        enemies[0].GuaranteedItems = [new ItemBaseData("Coins", 7)];
        enemies[1].CurrentHealth = 0;
        enemies[1].IsDead = true;
        enemies[1].LastDeathTime = TimeSpan.FromMinutes(3);
        saved.TotalTimeInWorld = TimeSpan.FromMinutes(4);
        for (int i = 0; i < 2; i++)
        {
            _ = new GameScreen(game, assets, saved, assets.DefaultEntityAssets.Texture);
            var loaded = World.Levels[pair.Key].EntityManager.Entities.OfType<Enemy>().ToArray();
            var wounded = loaded.First(e => e.Position == enemies[0].Position && e.GetType().FullName == enemies[0].Type);
            Assert.Equal(1, wounded.Health);
            Assert.Equal(fresh.BaseXP, wounded.BaseXP);
            Assert.Equal(fresh.XP, wounded.XP);
            Assert.Equal(7, Assert.Single(wounded.GuaranteedDrops.Items).Quantity);
            var dead = loaded.First(e => e.Position == enemies[1].Position && e.GetType().FullName == enemies[1].Type);
            Assert.True(dead.IsDead);
            Assert.Equal(0, dead.Health);
            Assert.Equal(TimeSpan.FromMinutes(3), dead.LastDeathTime);
            saved = World.ToData();
        }
    }

    [Fact]
    public void RestoringInteractablesUsesTiledIdsForMovedObjectsAndPositionsForLegacySaves()
    {
        using var directory = new SaveDirectory();
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var manager = World.Levels.Values.First(l => l.InteractableObjectManager.Count > 0).InteractableObjectManager;
        var original = manager.Objects[0];
        Vector2 originalPosition = original.Position;
        var saved = manager.ToData();
        var state = saved.InteractableObjectsData[0];
        state.Active = false;
        state.LastInteractedTime = TimeSpan.FromSeconds(12);
        Assert.True(state.TiledObjectId > 0);
        string path = Path.Combine(directory.Path, "Interactables.xml");
        XnaSerializer.Serialize(path, saved);

        original.Position += new Vector2(16, 32);
        var restored = XnaSerializer.Deserialize<InteractableObjectManagerData>(path);
        Assert.Equal(original.TiledObjectId, restored.InteractableObjectsData[0].TiledObjectId);
        manager.LoadFromData(restored);
        Assert.Same(original, manager.Objects[0]);
        Assert.Equal(originalPosition + new Vector2(16, 32), original.Position);
        Assert.False(original.Active);
        Assert.Equal(TimeSpan.FromSeconds(12), original.LastInteractedTime);

        original.Position = originalPosition;
        original.Active = true;
        restored.InteractableObjectsData[0].TiledObjectId += 1000;
        Assert.Throws<InvalidDataException>(() => manager.LoadFromData(restored));
        Assert.True(original.Active);

        var legacyXml = System.Xml.Linq.XDocument.Load(path);
        foreach (var id in legacyXml.Descendants("TiledObjectId").ToArray())
            id.Remove();
        legacyXml.Save(path);
        manager.LoadFromData(XnaSerializer.Deserialize<InteractableObjectManagerData>(path));
        Assert.False(original.Active);
    }

    [Fact]
    public void RestoredEquipmentUsesBackpackInstancesAndImmediatelyUpdatesStats()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var weapon = GameManager.ItemsClone.Values.OfType<Weapon>().First();
        var saved = World.ToData();
        // Qualify the player without mutating the shared item template through a clone.
        saved.PlayerData.EntityLevel = weapon.LevelRequirements.Level;
        saved.PlayerData.totalXP = GameManager.XPTable.GetLevelXPAtLevel(saved.PlayerData.EntityLevel);
        saved.PlayerData.BaseAttack = Math.Max(saved.PlayerData.BaseAttack, weapon.LevelRequirements.Attack);
        saved.PlayerData.BaseDefence = Math.Max(saved.PlayerData.BaseDefence, weapon.LevelRequirements.Defence);
        saved.PlayerData.baseMana = Math.Max(saved.PlayerData.baseMana, weapon.LevelRequirements.Mana);
        var equipped = weapon.ToData();
        equipped.Equipped = true;
        saved.PlayerData.backpack = [equipped, new ItemData { Name = "Coins", Quantity = 13 }, new ItemData { Name = "Coins", Quantity = 9 }];
        _ = new GameScreen(game, assets, saved, assets.TexturesLibrary.SkeletonTexture);
        var restored = Assert.IsType<Weapon>(World.Player.Backpack.Items[0]);
        Assert.True(restored.Equipped);
        Assert.Equal(World.Player.AttackExcludingEquipment + restored.AttackValue, World.Player.Attack);
        Assert.Equal(new[] { 13, 9 }, World.Player.Backpack.Items.Skip(1).Select(i => i.Quantity));
        World.Player.EquippedItems.TryUnequipItem(restored);
        Assert.Equal(0, World.Player.EquippedItems.EquippedItemsAttackBonus());
    }

    [Fact]
    public void RestoredSkillsShareInstancesAndClearEmptySlots()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        World.Player.LearnedAttackManager.UpdateWithData(new() { LearnedAttackNames = ["FireBall", "IceBullet"] });
        World.Player.SetKeybinding(new() { [Keys.D1] = "IceBullet", [Keys.D2] = "IceBullet", [Keys.D3] = "" });
        Assert.Same(World.Player.LearnedAttackManager.LearnedAttacks["IceBullet"], World.Player.KeybindingsManager.Keybindings[Keys.D1]);
        Assert.Same(World.Player.KeybindingsManager.Keybindings[Keys.D1], World.Player.KeybindingsManager.Keybindings[Keys.D2]);
        Assert.Null(World.Player.KeybindingsManager.Keybindings[Keys.D3]);
        Assert.Null(World.Player.KeybindingsManager.Keybindings[Keys.D0]);
        var count = World.Player.LearnedAttackManager.LearnedAttacks.Count;
        _ = new ExitScreen(game, assets);
        Assert.Equal(count, World.Player.LearnedAttackManager.LearnedAttacks.Count);
    }

    [Fact]
    public void FailedRestorationPreservesCurrentSession()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        game.GameScreen = new GameScreen(game, assets);
        var originalScreen = game.GameScreen;
        var originalPlayer = World.Player;
        var originalLevels = World.Levels;
        var originalTime = World.TotalTimeInWorld;
        originalTime.TotalGameTime = TimeSpan.FromMinutes(8);
        var broken = Snapshot();
        broken.World = World.ToData();
        broken.World.PlayerData.backpack = [new ItemData { Name = "removed item", Quantity = 1 }];
        Assert.Throws<InvalidDataException>(() => SaveManager.RestoreGame(game, assets, broken));
        Assert.Same(originalScreen, game.GameScreen);
        Assert.Same(originalPlayer, World.Player);
        Assert.Same(originalLevels, World.Levels);
        Assert.Same(originalTime, World.TotalTimeInWorld);
        Assert.Equal(TimeSpan.FromMinutes(8), World.TotalTimeInWorld.TotalGameTime);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LoadRecoversFromReadablePrimaryWithMissingContentAndPreservesBackup(bool missingMap)
    {
        using var directory = new SaveDirectory();
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        game.GameScreen = new GameScreen(game, assets);
        game.ExitScreen = new ExitScreen(game, assets);

        World.Player.Health = 3;
        SaveStorage.Write(directory.Path, SaveManager.CaptureGame(game, game.ExitScreen.ExitScreenMenu));
        World.Player.Health = 1;
        var primary = SaveManager.CaptureGame(game, game.ExitScreen.ExitScreenMenu);
        if (missingMap)
        {
            primary.World.Levels["RemovedMap"] = new();
            primary.World.CurrentLevel = "RemovedMap";
        }
        else
            primary.World.PlayerData.backpack = [new ItemData { Name = "Removed item", Quantity = 1 }];
        SaveStorage.Write(directory.Path, primary);
        Assert.Equal(1, SaveStorage.Read(directory.Path).World.PlayerData.CurrentHealth);

        SaveManager.LoadSavedGame(game, assets, directory.Path);
        Assert.Equal(3, World.Player.Health);
        Assert.Equal("Level0", World.CurrentLevel.Name);
        Assert.Contains("Loaded the backup save.", game.GameScreen.MessageBox.Messages);
        Assert.True(game.PreserveSaveBackup);

        World.Player.Health = 2;
        Assert.True(SaveManager.SaveGame(game, game.ExitScreen.ExitScreenMenu, directory.Path));
        Assert.False(game.PreserveSaveBackup);
        Assert.Equal(2, SaveStorage.Read(directory.Path).World.PlayerData.CurrentHealth);
        string backupPath = System.IO.Path.Combine(directory.Path, SaveStorage.FileName + ".bak");
        Assert.Equal(3, XnaSerializer.Deserialize<SaveGameData>(backupPath).World.PlayerData.CurrentHealth);
    }

    [Fact]
    public void FailedPrimaryAndBackupReportBothRestoreErrorsAndKeepTheActiveSession()
    {
        using var directory = new SaveDirectory();
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var screen = game.GameScreen = new GameScreen(game, assets);
        game.ExitScreen = new ExitScreen(game, assets);
        var player = World.Player;
        var levels = World.Levels;
        var backup = SaveManager.CaptureGame(game, game.ExitScreen.ExitScreenMenu);
        backup.World.PlayerData.backpack = [new ItemData { Name = "Removed item", Quantity = 1 }];
        SaveStorage.Write(directory.Path, backup);
        var primary = SaveManager.CaptureGame(game, game.ExitScreen.ExitScreenMenu);
        primary.World.Levels["RemovedMap"] = new();
        primary.World.CurrentLevel = "RemovedMap";
        SaveStorage.Write(directory.Path, primary);

        var failure = Assert.Throws<InvalidDataException>(() => SaveManager.LoadSavedGame(game, assets, directory.Path));
        var causes = Assert.IsType<AggregateException>(failure.InnerException).InnerExceptions;
        Assert.Collection(causes,
            cause => Assert.Contains("Unknown saved level: RemovedMap", cause.Message),
            cause => Assert.Contains("Unknown saved item: Removed item", cause.Message));
        Assert.Same(screen, game.GameScreen);
        Assert.Same(player, World.Player);
        Assert.Same(levels, World.Levels);
    }

    [Fact]
    public void CompleteSnapshotRoundTripPreservesWorldAndRebuildsPauseMenu()
    {
        using var directory = new SaveDirectory();
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        game.GameScreen = new GameScreen(game, assets);
        game.ExitScreen = new ExitScreen(game, assets);
        World.Player.Health = 3;
        World.Player.Mana = 2;
        World.Player.RespawnPosition = new Vector2(73, 89);
        World.TotalTimeInWorld.TotalGameTime = TimeSpan.FromMinutes(5);
        World.AddMessage("unsent quest progress");
        var saved = SaveManager.CaptureGame(game, game.ExitScreen.ExitScreenMenu);
        SaveStorage.Write(directory.Path, saved);
        var oldScreen = game.GameScreen;
        SaveManager.RestoreGame(game, assets, SaveStorage.Read(directory.Path));
        Assert.NotSame(oldScreen, game.GameScreen);
        Assert.Equal(3, World.Player.Health);
        Assert.Equal(2, World.Player.Mana);
        Assert.Equal(new Vector2(73, 89), World.Player.RespawnPosition);
        Assert.Equal(TimeSpan.FromMinutes(5), World.TotalTimeInWorld.TotalGameTime);
        Assert.Contains("unsent quest progress", game.GameScreen.MessageBox.Messages);
        Assert.Empty(World.MessagesToAdd);
        foreach (var binding in saved.Bindings.Keybindings.Where(b => !string.IsNullOrEmpty(b.Value)))
            Assert.Same(World.Player.LearnedAttackManager.LearnedAttacks[binding.Value], World.Player.KeybindingsManager.Keybindings[binding.Key]);
    }

    [Fact]
    public void UnvisitedChestsStayUninitializedAndEmptiedChestsStayEmpty()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var level = World.Levels.Values.First(l => l.ChestManager.Chests.Count >= 2);
        var unopened = level.ChestManager.Chests[0];
        var emptied = level.ChestManager.Chests[1];
        unopened.Items = null;
        emptied.Items = [];
        emptied.ChestEmptied = true;
        var saved = World.ToData();
        _ = new GameScreen(game, assets, saved, assets.TexturesLibrary.SkeletonTexture);
        var loaded = World.Levels[level.Name].ChestManager.Chests;
        Assert.Null(loaded.First(c => c.Position == unopened.Position).Items);
        Assert.Empty(loaded.First(c => c.Position == emptied.Position).Items);
        Assert.True(loaded.First(c => c.Position == emptied.Position).ChestEmptied);
    }

    [Fact]
    public void SnapshotSettlesPendingLeverTogglesWithoutChangingLiveWorld()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var level = World.Levels.Values.First(l => l.ConditionalLayer is not null);
        var layer = level.ConditionalLayer;
        bool visible = layer.IsVisible;
        level.GameEventManager.AddEvent(new SkeletonsAdventure.GameEvents.TimeDelayedGameEvent(
            new SkeletonsAdventure.GameEvents.ToggleLayerVisibility(layer), 500));
        var saved = World.ToData();
        Assert.Equal(visible, layer.IsVisible);
        _ = new GameScreen(game, assets, saved, assets.TexturesLibrary.SkeletonTexture);
        Assert.Equal(!visible, World.Levels[level.Name].ConditionalLayer.IsVisible);
    }
}
