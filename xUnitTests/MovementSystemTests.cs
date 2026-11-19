using Microsoft.Xna.Framework;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    [Theory]
    [InlineData(8, 20, 100, 0, 56, 20)]
    [InlineData(100, 20, -100, 0, 80, 20)]
    [InlineData(20, 8, 0, 100, 20, 56)]
    [InlineData(20, 100, 0, -100, 20, 80)]
    public void FastMovementStopsAtTheFirstWall(float x, float y, float dx, float dy, float expectedX, float expectedY)
    {
        bool Blocked(int column, int row) => dx != 0 ? column == 4 : row == 4;
        var result = CollisionDetection.Move(new(x, y), new(dx, dy), 8, 8, 128, 128, 16, 16, Blocked);
        Assert.Equal(new Vector2(expectedX, expectedY), result);
    }

    [Theory]
    [InlineData(-1000, 0, 0, 40)]
    [InlineData(1000, 0, 120, 40)]
    [InlineData(0, -1000, 40, 0)]
    [InlineData(0, 1000, 40, 120)]
    public void MapBoundariesClampMovementWithoutCollisionLayers(float dx, float dy, float x, float y)
    {
        var result = CollisionDetection.Move(new(40, 40), new(dx, dy), 8, 8, 128, 128, 16, 16, (_, _) => false);
        Assert.Equal(new Vector2(x, y), result);
    }

    [Fact]
    public void DiagonalMovementSlidesAlongWallsAndDoesNotCutSolidCorners()
    {
        var result = CollisionDetection.Move(new(40, 16), new(80, 80), 8, 8, 128, 128, 16, 16, (x, _) => x == 4);
        Assert.Equal(new Vector2(56, 96), result);
        result = CollisionDetection.Move(new(48, 48), new(32, 32), 16, 16, 128, 128, 16, 16, (x, y) => x == 4 && y == 4);
        Assert.False(new Rectangle((int)result.X, (int)result.Y, 16, 16).Intersects(new Rectangle(64, 64, 16, 16)));
    }

    [Fact]
    public void FractionalPositionsAndExactTileSizedPassagesRemainUsable()
    {
        var result = CollisionDetection.Move(new(55.5f, 10), new(0.75f, 0), 8, 8, 128, 128, 16, 16, (x, _) => x == 4);
        Assert.Equal(56, result.X);
        result = CollisionDetection.Move(new(64, 10), new(0, 80), 16, 16, 128, 128, 16, 16, (x, _) => x == 3 || x == 5);
        Assert.Equal(new Vector2(64, 90), result);
        result = CollisionDetection.Move(new(10, 20), new(0, 200), 8, 8, 128, 256, 16, 32, (_, y) => y == 3);
        Assert.Equal(88, result.Y);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(144)]
    public void WallSlidingHasTheSameResultAcrossFrameRates(int frames)
    {
        var position = new Vector2(10, 10);
        for (int frame = 0; frame < frames; frame++)
            position = CollisionDetection.Move(position, new Vector2(100, 100) / frames, 8, 8,
                256, 256, 16, 16, (x, _) => x == 4);
        Assert.InRange(position.X, 55.999f, 56.001f);
        Assert.InRange(position.Y, 109.999f, 110.001f);
    }

    [Fact]
    public void ProjectileCollisionStopsAtTheWallInsteadOfSlidingAroundIt()
    {
        var result = CollisionDetection.Move(new(40, 16), new(80, 80), 8, 8, 128, 128, 16, 16,
            (x, _) => x == 4, stopOnCollision: true);
        Assert.Equal(56, result.X);
        Assert.True(result.Y < 96);
    }

    [Fact]
    public void MovementUsesElapsedTimeAndAppliesProjectileSpeedOnce()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var entity = new Entity(assets.DefaultEntityAssets)
        {
            Position = new(40, 40), Width = 8, Height = 8, Motion = Vector2.One, Speed = 2
        };
        var shot = (ShootingAttack)assets.AttackTemplates["FireBall"].Clone();
        shot.Source = entity;
        shot.AttackDelay = 0;
        shot.SetUpAttack(new(), Color.White, new(80, 80));
        shot.Position = new(80, 80);
        shot.Speed = 2;
        shot.AttackVisible = true;
        shot.MoveInDirectionOfPosition(shot.Center + new Vector2(100, 0));
        entity.AttackManager.AddAttack(shot, new());
        shot.Update(new GameTime(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1)));
        CollisionDetection.CheckEntityBoundaryCollisions([entity], null, 16, 16, 512, 512,
            new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));
        Assert.InRange(Vector2.Distance(entity.Position, new(40, 40)), 9.999f, 10.001f);
        Assert.Equal(new Vector2(90, 80), shot.Position);
        Assert.Equal(shot.Rectangle, shot.DamageHitBox);
        shot.MoveInDirectionOfPosition(shot.Center);
        Assert.Equal(Vector2.Zero, shot.Motion);
    }

    [Fact]
    public void ConditionalCollisionLayersOpenAndCloseWithoutReloadingTheLevel()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var level = World.Levels.Values.First(l => l.ConditionalLayer is not null);
        var layer = level.ConditionalLayer;
        (int X, int Y)? wall = null;
        for (int y = 0; y < level.Height / level.TileHeight && wall is null; y++)
            for (int x = 1; x < level.Width / level.TileWidth; x++)
                if (layer.TryGetTile((ushort)x, (ushort)y, out var tile) && tile.HasValue && !tile.Value.IsBlank)
                {
                    wall = (x, y);
                    break;
                }
        Assert.NotNull(wall);
        var start = new Vector2(wall.Value.X * level.TileWidth - 8, wall.Value.Y * level.TileHeight);
        var entity = new Entity(assets.DefaultEntityAssets)
        {
            Position = start, Width = 8, Height = 8, Motion = Vector2.UnitX, Speed = 1
        };
        var time = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(level.TileWidth / 50.0));
        bool wasVisible = layer.IsVisible;
        try
        {
            layer.IsVisible = true;
            CollisionDetection.CheckEntityBoundaryCollisions([entity], [layer], level.TileWidth, level.TileHeight, level.Width, level.Height, time);
            Assert.Equal(start, entity.Position);
            layer.IsVisible = false;
            CollisionDetection.CheckEntityBoundaryCollisions([entity], [layer], level.TileWidth, level.TileHeight, level.Width, level.Height, time);
            Assert.True(entity.Position.X > start.X);
        }
        finally { layer.IsVisible = wasVisible; }
    }

    [Fact]
    public void PatrolDistanceAndTurnsDependOnTimeRatherThanFrameCount()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        Vector2 Simulate(int framesPerSecond)
        {
            var enemy = new Enemy(assets.DefaultEntityAssets);
            Vector2 distance = Vector2.Zero;
            for (int i = 0; i < framesPerSecond * 5; i++)
            {
                var time = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / framesPerSecond));
                enemy.IdleBehavior(time);
                distance += enemy.Motion * enemy.Speed * (float)time.ElapsedGameTime.TotalSeconds * 50;
            }
            return distance;
        }
        var thirty = Simulate(30);
        var oneFortyFour = Simulate(144);
        Assert.InRange(Vector2.Distance(thirty, oneFortyFour), 0, 0.01f);
        Assert.InRange(thirty.X, 249.99f, 250.01f);
        Assert.InRange(thirty.Y, 124.99f, 125.01f);
    }
}
