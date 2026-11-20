using Microsoft.Xna.Framework;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.ItemClasses.ItemManagement;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    private static Rectangle LootRectangle(Vector2 point) => new((int)point.X, (int)point.Y, GameItem.Width, GameItem.Height);

    [Fact]
    public void ManualPickupChecksCurrentReachExceptInTestGames()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var screen = new GameScreen(game, assets);
        var loot = World.CurrentLevel.EntityManager.DroppedLootManager;
        var drop = GameManager.GetItemByName("Sword");
        drop.Position = screen.Player.Center - new Vector2(GameItem.Width, GameItem.Height) / 2 + new Vector2(49, 0);
        loot.RestoreDrops([drop]);

        Assert.False(screen.TryPickUpGroundItem(drop));
        Assert.Same(drop, Assert.Single(loot.Items));

        screen.Player.Position += Vector2.UnitX;
        Assert.True(screen.TryPickUpGroundItem(drop));
        Assert.Empty(loot.Items);

        if (SkeletonsAdventure.Game1.IfDev)
        {
            var testScreen = new GameScreen(game, assets, testGame: true);
            loot = World.CurrentLevel.EntityManager.DroppedLootManager;
            loot.RestoreDrops([drop]);
            Assert.True(testScreen.TryPickUpGroundItem(drop));
            Assert.False(testScreen.TryPickUpGroundItem(drop));
        }
    }

    [Theory]
    [InlineData(-100, -100)]
    [InlineData(127, 127)]
    [InlineData(10000, 16)]
    public void LootSpreadStaysInsideTheMapAndOnThePlayersSideOfWalls(float x, float y)
    {
        var placement = new GroundLootPlacement(128, 128, 16, 16, (column, _) => column == 4,
            () => (new Vector2(16, 16), 32, 32), () => 0);
        var positions = placement.Place(new(x, y), 8, []);
        Assert.Equal(8, positions.Count);
        foreach (var position in positions)
        {
            Assert.True(placement.IsSafe(position));
            Assert.InRange(position.X, 0, 32);
            Assert.InRange(position.Y, 0, 96);
            Assert.False(LootRectangle(position).Intersects(new Rectangle(64, 0, 16, 128)));
        }
    }

    [Fact]
    public void NearbyDisconnectedPocketsAndNarrowPassagesDoNotReceiveLoot()
    {
        var placement = new GroundLootPlacement(160, 160, 16, 16, (x, y) => y == 4 && x != 4,
            () => (new Vector2(16, 16), 32, 32), () => 0);
        var positions = placement.Place(new(64, 112), 5, []);
        Assert.All(positions, position => Assert.InRange(position.Y, 0, 32));
        Assert.False(placement.IsSafe(new(64, 112)));
    }

    [Fact]
    public void OpenGroundProducesACompactSpreadWithoutOverlappingExistingLoot()
    {
        var placement = new GroundLootPlacement(256, 256, 16, 16, (_, _) => false,
            () => (new Vector2(64, 64), 32, 32), () => 0);
        var origin = new Vector2(96, 96);
        var positions = placement.Place(origin, 8, [origin]);
        var rectangles = new List<Rectangle> { LootRectangle(origin) };
        foreach (var position in positions)
        {
            Assert.InRange(Vector2.Distance(position, origin), 0, 96);
            Assert.DoesNotContain(rectangles, rectangle => rectangle.Intersects(LootRectangle(position)));
            rectangles.Add(LootRectangle(position));
        }
    }

    [Fact]
    public void CrowdedGroundKeepsEveryDropAtTheSingleSafePosition()
    {
        var placement = new GroundLootPlacement(96, 96, 16, 16,
            (x, y) => x < 2 || x > 3 || y < 2 || y > 3,
            () => (new Vector2(32, 32), 32, 32), () => 0);
        var positions = placement.Place(new(80, 80), 20, []);
        Assert.Equal(20, positions.Count);
        Assert.All(positions, position => Assert.Equal(new Vector2(32, 32), position));
    }

    [Fact]
    public void DoorChangesAndTeleportingInvalidateReachabilityCaches()
    {
        bool closed = false;
        Vector2 player = new(16, 16);
        var placement = new GroundLootPlacement(128, 128, 16, 16, (x, _) => closed && x == 4,
            () => (player, 32, 32), () => closed ? 1 : 0);
        var origin = new Vector2(96, 16);
        Assert.True(placement.Place(origin, 1, [])[0].X >= 80);
        closed = true;
        Assert.True(placement.Place(origin, 1, [])[0].X <= 32);
        player = new(96, 16);
        Assert.True(placement.Place(origin, 1, [])[0].X >= 80);
    }

    [Fact]
    public void TallPlayerFootprintsAreRespectedWhenChoosingGround()
    {
        var placement = new GroundLootPlacement(160, 160, 16, 16, (_, y) => y == 5,
            () => (new Vector2(16, 16), 32, 54), () => 0);
        var positions = placement.Place(new(64, 120), 5, []);
        Assert.All(positions, position =>
        {
            Assert.True(placement.IsSafe(position));
            Assert.True(position.Y + GameItem.Height <= 80);
        });
    }

    [Fact]
    public void DropManagerClonesSpreadsAndRepairsSavedInvalidPositions()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var manager = new DroppedLootManager
        {
            Placement = new GroundLootPlacement(128, 128, 16, 16, (x, _) => x == 4,
                () => (new Vector2(16, 16), 32, 32), () => 0)
        };
        var coins = GameManager.GetItemByName("Coins");
        coins.SetQuantity(7);
        var position = coins.Position;
        manager.Add(new List<GameItem> { coins, coins, coins }, new(120, 120));
        Assert.Equal(3, manager.Items.Count);
        Assert.All(manager.Items, item =>
        {
            Assert.NotSame(coins, item);
            Assert.Equal(7, item.Quantity);
            Assert.Equal(LootRectangle(item.Position), item.ItemRectangle);
            Assert.True(manager.Placement.IsSafe(item.Position));
        });
        Assert.Equal(position, coins.Position);
        var invalid = coins.Clone();
        invalid.Position = new(-200, -200);
        manager.RestoreDrops([.. manager.Items, invalid]);
        manager.RepairPlacement();
        Assert.Equal(4, manager.Items.Count);
        Assert.True(manager.Placement.IsSafe(invalid.Position));
        Assert.Equal(7, invalid.Quantity);
    }

    [Fact]
    public void EnemyDeathsAndLevelEntryUseTheLevelsCollisionAwarePlacement()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var level = World.CurrentLevel;
        var manager = level.EntityManager.DroppedLootManager;
        Assert.NotNull(manager.Placement);
        var enemy = GameManager.GetEnemyByName("Skeleton");
        enemy.DropTableName = "";
        enemy.GuaranteedDrops.Clear();
        var coins = GameManager.GetItemByName("Coins");
        coins.SetQuantity(7);
        enemy.GuaranteedDrops.Add(coins);
        enemy.Position = new(-10000, -10000);
        level.EntityManager.Add(enemy);
        enemy.EntityDied(new());
        var drop = Assert.Single(manager.Items);
        Assert.Equal(7, drop.Quantity);
        Assert.True(manager.Placement.IsSafe(drop.Position));
        var oldDrop = coins.Clone();
        oldDrop.Position = new(level.Width + 100, level.Height + 100);
        manager.RestoreDrops([drop, oldDrop]);
        World.SetCurrentLevel(level, World.Player.Position);
        Assert.Equal(2, manager.Items.Count);
        Assert.True(manager.Placement.IsSafe(oldDrop.Position));
    }

    [Fact]
    public void UnplaceableEnemyLootSurvivesSaveAndLevelEntryAndRecoversWhenGroundOpens()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var level = World.CurrentLevel;
        var manager = level.EntityManager.DroppedLootManager;
        bool blocked = true;
        manager.Placement = new GroundLootPlacement(level.Width, level.Height, 16, 16,
            (_, _) => blocked, () => (new Vector2(32, 32), 32, 32), () => blocked ? 1 : 0);
        var enemy = GameManager.GetEnemyByName("Skeleton");
        enemy.DropTableName = "";
        enemy.GuaranteedDrops.Clear();
        var coins = GameManager.GetItemByName("Coins");
        coins.SetQuantity(7);
        enemy.GuaranteedDrops.Add(coins);
        level.EntityManager.Add(enemy);

        enemy.EntityDied(new());
        Assert.True(enemy.IsDead);
        Assert.Empty(manager.Items);
        var saved = level.ToData();
        Assert.Equal(7, Assert.Single(saved.DroppedItemDatas).Quantity);

        SaveManager.LoadLevelDataFromLevelData(level, saved);
        World.SetCurrentLevel(level);
        manager.Update();
        Assert.Empty(manager.Items);
        Assert.Equal(7, Assert.Single(manager.GetDroppedItemData()).Quantity);

        blocked = false;
        manager.Update();
        manager.Update();
        var drop = Assert.Single(manager.Items);
        Assert.Equal(7, drop.Quantity);
        Assert.True(manager.Placement.IsSafe(drop.Position));
        Assert.True(manager.TryPickUp(drop, World.Player));
        Assert.Empty(manager.GetDroppedItemData());
    }
}
