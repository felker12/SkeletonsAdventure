using Microsoft.Xna.Framework;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.EntityHelperClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    [Fact]
    public void EnemySightRespectsWallsCornersAndDynamicDoors()
    {
        bool closed = true;
        var map = new EnemyNavigation(160, 160, 16, 16, (x, _) => closed && x == 4);
        Assert.False(map.HasLineOfSight(new(20, 20), new(100, 20)));
        Assert.True(map.HasLineOfSight(new(20, 20), new(30, 20)));
        closed = false;
        Assert.True(map.HasLineOfSight(new(20, 20), new(100, 20)));
        var corner = new EnemyNavigation(160, 160, 16, 16, (x, y) => x == 1 && y == 0);
        Assert.False(corner.HasLineOfSight(new(8, 8), new(40, 40)));
        Assert.False(map.HasLineOfSight(new(-1, 8), new(40, 40)));
        Assert.True(map.HasLineOfSight(new(8, 56), new(32, 32)));
    }

    [Theory]
    [InlineData(90, 50, AnimationKey.Right)]
    [InlineData(10, 50, AnimationKey.Left)]
    [InlineData(50, 10, AnimationKey.Up)]
    [InlineData(50, 90, AnimationKey.Down)]
    public void EnemiesFaceTheirTargetBeforeAttackingAndStopInReach(int x, int y, AnimationKey expected)
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        World.Player.Position = new(x, y);
        World.Player.Width = World.Player.Height = 32;
        var enemy = new Enemy(assets.DefaultEntityAssets) { Position = new(50, 50), Width = 32, Height = 32 };
        EnemyAI.CheckIfEnemyDetectPlayer(new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.016)), [enemy], World.Player);
        Assert.Equal(expected, enemy.CurrentAnimation);
        Assert.Equal(Vector2.Zero, enemy.Motion);
        Assert.Equal(EnemyBehavior.Pursuing, enemy.Behavior);
        Assert.Single(enemy.AttackManager.Attacks);
    }

    [Fact]
    public void WallsPreventDetectionAndLostTargetsAreInvestigatedThenForgotten()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        World.Player.Position = new(128, 64);
        World.Player.Width = World.Player.Height = 16;
        var enemy = new Enemy(assets.DefaultEntityAssets) { Position = new(32, 64), Width = 16, Height = 16 };
        bool closed = true;
        var map = new EnemyNavigation(512, 512, 32, 32, (x, _) => closed && x == 3);
        void Think(double seconds) => EnemyAI.CheckIfEnemyDetectPlayer(
            new GameTime(TimeSpan.FromSeconds(seconds), TimeSpan.FromSeconds(0.016)), [enemy], World.Player, map);
        Think(1);
        Assert.Equal(EnemyBehavior.Patrolling, enemy.Behavior);
        Assert.Empty(enemy.AttackManager.Attacks);
        closed = false;
        Think(2);
        Assert.Equal(EnemyBehavior.Pursuing, enemy.Behavior);
        Assert.True(enemy.Motion.X > 0);
        closed = true;
        Think(3);
        Assert.Equal(EnemyBehavior.Investigating, enemy.Behavior);
        Assert.Empty(enemy.AttackManager.Attacks);
        Think(9);
        Assert.Equal(EnemyBehavior.Patrolling, enemy.Behavior);
        Assert.Null(enemy.LastKnownPlayerPosition);
    }

    [Fact]
    public void NavigationRoutesAroundWallsInsteadOfPushingIntoThem()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        bool Blocked(int x, int y) => x == 4 && y < 6;
        var map = new EnemyNavigation(192, 192, 16, 16, Blocked);
        var enemy = new Enemy(assets.DefaultEntityAssets) { Position = new(32, 32), Width = 16, Height = 16, Speed = 2 };
        var target = new Vector2(112, 32);
        for (int frame = 0; frame < 300; frame++)
        {
            var time = new GameTime(TimeSpan.FromSeconds(frame / 60.0), TimeSpan.FromSeconds(1.0 / 60));
            var motion = map.DirectionTo(enemy, target, time);
            enemy.Position = CollisionDetection.Move(enemy.Position, motion * (100f / 60), 16, 16, 192, 192, 16, 16, Blocked);
            Assert.False(enemy.Rectangle.Intersects(new Rectangle(64, 0, 16, 96)));
        }
        Assert.InRange(Vector2.Distance(enemy.Position, target), 0, 0.6f);
    }

    [Fact]
    public void NavigationDoesNotSendLargeEnemiesThroughNarrowGaps()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        bool Blocked(int x, int y) => y == 4 && x != 4;
        var map = new EnemyNavigation(192, 192, 16, 16, Blocked);
        var enemy = new Enemy(assets.DefaultEntityAssets) { Position = new(32, 16), Width = 32, Height = 32, Speed = 2 };
        for (int frame = 0; frame < 120; frame++)
        {
            var time = new GameTime(TimeSpan.FromSeconds(frame / 60.0), TimeSpan.FromSeconds(1.0 / 60));
            var motion = map.DirectionTo(enemy, new(64, 112), time);
            enemy.Position = CollisionDetection.Move(enemy.Position, motion * (100f / 60), 32, 32, 192, 192, 16, 16, Blocked);
            Assert.True(enemy.Position.Y <= 32.001f, $"Enemy crossed the narrow gap at {enemy.Position}.");
        }
    }

    [Fact]
    public void FreshHitsRestartInvestigationAndMovementLocksAreRespected()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        World.Player.Position = new(1000, 1000);
        World.Player.Attack = 2;
        var enemy = new Enemy(assets.DefaultEntityAssets)
            { Health = 100, Defence = 0, CheckedLastAtackArea = true, LastTimeAttacked = TimeSpan.FromSeconds(1), CanMove = false };
        var attack = World.Player.BasicAttack.Clone();
        attack.Source = World.Player;
        attack.DamageModifier = 1;
        var time = new GameTime(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.016));
        enemy.GetHitByAttack(attack, time);
        Assert.False(enemy.CheckedLastAtackArea);
        EnemyAI.CheckIfEnemyDetectPlayer(time, [enemy], World.Player);
        Assert.Equal(EnemyBehavior.Investigating, enemy.Behavior);
        Assert.Equal(Vector2.Zero, enemy.Motion);
        enemy.Respawn(new(10, 10));
        Assert.Equal(EnemyBehavior.Patrolling, enemy.Behavior);
        Assert.Null(enemy.LastKnownPlayerPosition);
        Assert.False(enemy.CheckedLastAtackArea);
    }

    [Fact]
    public void EliteScalingAndRespawningDoNotAccumulateStatsOrLootRolls()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var enemy = GameManager.GetEnemyByName("EliteSkeleton");
        enemy.SetEnemyLevel(10);
        var expected = (enemy.MaxHealth, enemy.Attack, enemy.Defence, enemy.XP, enemy.RandomLootRollCount);
        for (int i = 0; i < 5; i++)
        {
            enemy.SetEnemyLevel(10);
            enemy.Respawn(new(10, 10));
            Assert.Equal(expected, (enemy.MaxHealth, enemy.Attack, enemy.Defence, enemy.XP, enemy.RandomLootRollCount));
        }
        Assert.Equal(3, enemy.RandomLootRollCount);
        Assert.Throws<ArgumentOutOfRangeException>(() => enemy.SetEnemyLevel(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => enemy.SetEnemyLevel(new RpgLibrary.DataClasses.MinMaxPair(5, 4)));
    }

    [Fact]
    public void EnemyClonesHaveIndependentGuaranteedLoot()
    {
        using var game = new SkeletonsAdventure.Game1();
        _ = new GameManager(game);
        foreach (var enemy in GameManager.EnemiesClone.Values)
        {
            int originalCount = enemy.GuaranteedDrops.Items.Count;
            var clone = enemy.Clone();
            Assert.NotSame(enemy.GuaranteedDrops, clone.GuaranteedDrops);
            if (clone.GuaranteedDrops.Items.Count > 0)
            {
                var original = enemy.GuaranteedDrops.Items[0];
                var copied = clone.GuaranteedDrops.Items[0];
                Assert.NotSame(original, copied);
                int quantity = original.Quantity;
                copied.SetQuantity(quantity + 5);
                Assert.Equal(quantity, original.Quantity);
            }
            clone.GuaranteedDrops.Clear();
            Assert.Equal(originalCount, enemy.GuaranteedDrops.Items.Count);
        }
    }

    [Fact]
    public void CloningAWoundedElitePreservesScaledStatsWithoutSharingAttacks()
    {
        using var game = new SkeletonsAdventure.Game1();
        _ = new GameManager(game);
        var enemy = GameManager.GetEnemyByName("EliteSkeleton");
        enemy.SetEnemyLevel(30);
        enemy.Health = 7;
        enemy.Speed = 4;
        var clone = enemy.Clone();
        Assert.Equal(enemy.Level, clone.Level);
        Assert.Equal(enemy.MaxHealth, clone.MaxHealth);
        Assert.Equal(enemy.Attack, clone.Attack);
        Assert.Equal(enemy.Defence, clone.Defence);
        Assert.Equal(enemy.XP, clone.XP);
        Assert.Equal(enemy.RandomLootRollCount, clone.RandomLootRollCount);
        Assert.Equal(7, clone.Health);
        Assert.Equal(4, clone.Speed);
        Assert.NotSame(enemy.BasicAttack, clone.BasicAttack);
        Assert.Same(clone, clone.BasicAttack.Source);
    }

    [Fact]
    public void ClosingADoorInvalidatesTheNextCachedWaypointImmediately()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        Point? closedDoor = null;
        bool Blocked(int x, int y) => (x == 4 && y < 6) || closedDoor == new Point(x, y);
        var map = new EnemyNavigation(192, 192, 16, 16, Blocked);
        var enemy = new Enemy(assets.DefaultEntityAssets) { Position = new(32, 32), Width = 16, Height = 16, Speed = 2 };
        var target = new Vector2(112, 32);
        var first = map.DirectionTo(enemy, target, new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016)));
        Assert.NotEqual(Vector2.Zero, first);
        var waypoint = enemy.Position + first * 16;
        closedDoor = new((int)(waypoint.X / 16), (int)(waypoint.Y / 16));
        var second = map.DirectionTo(enemy, target, new GameTime(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.016)));
        Assert.NotEqual(first, second);
    }
}
