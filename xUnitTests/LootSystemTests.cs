using RpgLibrary.DataClasses;
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses.ItemManagement;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    private sealed class LootRolls(params long[] rolls) : Random
    {
        private readonly Queue<long> _rolls = new(rolls);
        public int Calls { get; private set; }
        public override long NextInt64(long maxValue)
        {
            Calls++;
            Assert.NotEmpty(_rolls);
            long value = _rolls.Dequeue();
            Assert.InRange(value, 0, maxValue - 1);
            return value;
        }
    }

    private sealed class ControlledLootEnemy(EntityAssets assets, DropTable table) : Enemy(assets)
    {
        protected override DropTable GetDropTable() => table;
    }

    [Fact]
    public void EnemyRollsRespectEveryBoundaryIncludingNoDropWithoutRetrying()
    {
        using var game = new SkeletonsAdventure.Game1();
        _ = new GameManager(game);
        var random = new LootRolls(Enumerable.Range(0, 100).Select(value => (long)value).ToArray());
        var table = new DropTable([new("Coins", 20), new("Robes", 5), new("Bones", 20)], 55, random);
        var results = Enumerable.Range(0, 100).Select(_ => table.GetDrop()?.Name).ToArray();
        Assert.All(results.Take(20), item => Assert.Equal("Coins", item));
        Assert.All(results.Skip(20).Take(5), item => Assert.Equal("Robes", item));
        Assert.All(results.Skip(25).Take(20), item => Assert.Equal("Bones", item));
        Assert.All(results.Skip(45), item => Assert.Null(item));
        Assert.Equal(100, random.Calls);
    }

    [Fact]
    public void GuaranteedChestDrawsUseRemainingWeightsAndNeverRepeatAnItem()
    {
        using var game = new SkeletonsAdventure.Game1();
        _ = new GameManager(game);
        var random = new LootRolls(1, 0, 0);
        var table = new DropTable([new("Coins", 1), new("Robes", 1), new("Bones", 1)], 97, random);
        var items = table.GetUniqueDropsList(10);
        Assert.Equal(new[] { "Robes", "Coins", "Bones" }, items.Select(item => item.Name));
        Assert.Equal(3, items.Select(item => item.Name).Distinct().Count());
        Assert.Equal(3, random.Calls);
        Assert.Equal(3, table.Count);
    }

    [Fact]
    public void ChestQuantityRangesIncludeBothLimitsAndZeroWeightItemsAreExcluded()
    {
        using var game = new SkeletonsAdventure.Game1();
        _ = new GameManager(game);
        var table = new DropTable([new("Coins", 1), new("Robes", 1), new("Bones", 0)], 99,
            new LootRolls(0, 0, 1, 0, 0));
        Assert.Single(table.GetRandomAmountOfUniqueDrops(1, 2));
        var two = table.GetRandomAmountOfUniqueDrops(1, 2);
        Assert.Equal(2, two.Count);
        Assert.DoesNotContain(two, item => item.Name == "Bones");
    }

    [Fact]
    public void ItemQuantitiesIncludeBothLimitsAndDropsAreIndependentInstances()
    {
        using var game = new SkeletonsAdventure.Game1();
        _ = new GameManager(game);
        var table = new DropTable([new("Coins", 100, 2, 4)], 0, new LootRolls(0, 0, 99, 2));
        var first = table.GetDrop();
        var second = table.GetDrop();
        Assert.Equal(2, first.Quantity);
        Assert.Equal(4, second.Quantity);
        Assert.NotSame(first, second);
        first.SetQuantity(99);
        Assert.Equal(4, second.Quantity);
        Assert.Equal(1, GameManager.GetItemByName("Coins").Quantity);
    }

    [Fact]
    public void GuaranteedEnemyDropsAreIndependentOfEmptyOrSuccessfulRandomRolls()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var table = new DropTable([new("Coins", 1)], 99, new LootRolls(99, 99, 0, 0));
        var enemy = new ControlledLootEnemy(assets.DefaultEntityAssets, table)
            { DropTableName = "Controlled", NumberOfItemsToDrop = 2 };
        var guaranteed = GameManager.GetItemByName("Coins");
        guaranteed.SetQuantity(3);
        enemy.GuaranteedDrops.Add(guaranteed);
        Assert.Equal(3, Assert.Single(enemy.GetDrops()).Quantity);
        Assert.Equal(new[] { 1, 1, 3 }, enemy.GetDrops().Select(item => item.Quantity));
        Assert.Equal(3, Assert.Single(enemy.GuaranteedDrops.Items).Quantity);
    }

    [Fact]
    public void LegacyTablesInferNoDropAndNewWeightedTablesRoundTrip()
    {
        using var directory = new SaveDirectory();
        using var game = new SkeletonsAdventure.Game1();
        _ = new GameManager(game);
        var legacy = new DropTableData { DropTableList = [new("Coins", 20, 1, 1), new("Bones", 20, 1, 1)] };
        string path = Path.Combine(directory.Path, "Loot.xml");
        XnaSerializer.Serialize(path, legacy);
        var xml = System.Xml.Linq.XDocument.Load(path);
        foreach (var element in xml.Descendants("NoDropWeight").ToList())
            element.Remove();
        xml.Save(path);
        var restored = new DropTable(XnaSerializer.Deserialize<DropTableData>(path));
        Assert.Equal(60, restored.NoDropWeight);
        var weighted = new DropTable(new List<DropTableItem> { new("Coins", 200), new("Bones", 100) }, 50);
        XnaSerializer.Serialize(path, weighted.ToData());
        restored = new DropTable(XnaSerializer.Deserialize<DropTableData>(path));
        Assert.Equal(50, restored.NoDropWeight);
        Assert.Equal(200, restored.DropTableDictionary["Coins"].DropChance);
    }

    [Fact]
    public void InvalidEntriesAndGuaranteedTablesWithoutEligibleItemsFailClearly()
    {
        using var game = new SkeletonsAdventure.Game1();
        _ = new GameManager(game);
        Assert.Throws<InvalidDataException>(() => new DropTable(new List<DropTableItem> { new("missing", 1) }));
        Assert.Throws<InvalidDataException>(() => new DropTable(new List<DropTableItem> { new("Coins", -1) }));
        Assert.Throws<InvalidDataException>(() => new DropTable(new List<DropTableItem> { new("Coins", 101) }));
        Assert.Throws<InvalidDataException>(() => new DropTable(new List<DropTableItem> { new("Coins", 1, 0, 1) }));
        Assert.Throws<InvalidDataException>(() => new DropTable(new List<DropTableItem> { new("Coins", 1, 2, 1) }));
        Assert.Throws<InvalidDataException>(() => new DropTable(new List<DropTableItem> { new("Coins", 1), new("Coins", 1) }));
        Assert.Throws<InvalidDataException>(() => new DropTable(new List<DropTableItem> { new("Sword", 1, 2, 2) }));
        Assert.Throws<InvalidDataException>(() => new DropTable(new List<DropTableItem>(), -1));
        var empty = new DropTable();
        Assert.Null(empty.GetDrop());
        Assert.Empty(empty.GetUniqueDropsList(0));
        Assert.Throws<InvalidOperationException>(() => empty.GetUniqueDropsList(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => empty.GetDropsList(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => empty.GetRandomAmountOfUniqueDrops(3, 2));
    }

    [Fact]
    public void TableDefinitionsDoNotShareMutableInputsOrExposeMutableEntries()
    {
        using var game = new SkeletonsAdventure.Game1();
        _ = new GameManager(game);
        var entries = new List<DropTableItem> { new("Coins", 20) };
        var table = new DropTable(entries);
        var clone = table.Clone();
        entries.Clear();
        Assert.Single(table.DropTableDictionary);
        Assert.NotSame(table.DropTableDictionary["Coins"], clone.DropTableDictionary["Coins"]);
        var exported = table.ToData();
        exported.DropTableList[0].DropChance = 0;
        Assert.Equal(20, table.DropTableDictionary["Coins"].DropChance);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, DropTableItem>)table.DropTableDictionary).Clear());
        var armor = new ArmorData { Name = "No requirements", LevelRequirementData = null! };
        Assert.NotNull(armor.Clone().LevelRequirementData);
        Assert.NotNull(GameManager.GetItemByName("Robes"));
        Assert.NotNull(GameManager.GetItemByName("Shoes"));
    }

    [Fact]
    public void ChestGenerationPickupAndRestorationNeverRerollOrChangeTheTable()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var level = World.Levels.Values.First(l => l.ChestManager.Chests.Count > 0);
        var chest = level.ChestManager.Chests[0];
        var random = new LootRolls(0, 0);
        chest.DropTable = new DropTable([new("Coins", 1), new("Bones", 1)], 98, random);
        chest.LootAmountRange = new(2, 2);
        chest.EnsureContents();
        var items = chest.Items;
        chest.EnsureContents();
        Assert.Same(items, chest.Items);
        Assert.Equal(2, chest.Items.Count);
        Assert.Equal(2, random.Calls);
        var first = chest.Items[0];
        Assert.True(chest.TryTakeItem(first));
        Assert.False(chest.TryTakeItem(first));
        Assert.Equal(2, chest.DropTable.Count);
        var saved = World.ToData();
        _ = new GameScreen(game, assets, saved, assets.TexturesLibrary.SkeletonTexture);
        var restored = World.Levels[level.Name].ChestManager.Chests.First(c => c.Position == chest.Position);
        restored.EnsureContents();
        Assert.Equal("Bones", Assert.Single(restored.Items).Name);
        Assert.True(restored.TryTakeItem(restored.Items[0]));
        restored.EnsureContents();
        Assert.Empty(restored.Items);
        Assert.True(restored.ChestEmptied);
    }
}
