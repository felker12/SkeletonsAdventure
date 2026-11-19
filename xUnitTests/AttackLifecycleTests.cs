using Microsoft.Xna.Framework;
using RpgLibrary.AttackData;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.EntityHelperClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.Shapes;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    private static ShootingAttack Projectile(Entity source, Vector2 start, int delay = 0, int length = 1000)
    {
        var shot = new ShootingAttack(new AttackData
        {
            AttackDelay = delay, AttackLength = length, Speed = 20, DamageModifier = 1
        }, source.Texture, source.Info.SpriteFont, source) { Width = 8, Height = 8, Motion = Vector2.UnitX };
        shot.SetUpAttack(new(), Color.White, start - new Vector2((source.Width - 8) / 2f, (source.Height - 8) / 2f));
        return shot;
    }

    private static MultiShotAttack Volley(Entity source)
    {
        var shot = new ShootingAttack(new AttackData
        {
            AttackDelay = 100, AttackLength = 600, Speed = 4, DamageModifier = 1
        }, source.Texture, source.Info.SpriteFont) { Width = 8, Height = 8 };
        return new MultiShotAttack(new MultiShotAttackData
        {
            AttackDelay = 200, ShotCount = 3, ShotInterval = TimeSpan.FromMilliseconds(100),
            AttackCoolDownLength = 1000, DamageModifier = 0.6f, ManaCost = 7
        }, source.Texture, shot);
    }

    [Fact]
    public void AttacksStartingAtZeroRespectDelayAndFinalActiveTime()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var source = new Entity(assets.DefaultEntityAssets);
        var shot = Projectile(source, new(32, 32), 100, 300);
        Assert.False(shot.AttackVisible);
        shot.Update(new GameTime(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100)));
        Assert.True(shot.AttackVisible);
        Assert.Equal(0, shot.MovementSecondsThisFrame);
        shot.Update(new GameTime(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(400)));
        Assert.Equal(0.2, shot.MovementSecondsThisFrame, 6);
        Assert.True(shot.AttackVisible);
        Assert.True(shot.AttackTimedOut());
        shot.Update(new GameTime(TimeSpan.FromMilliseconds(600), TimeSpan.FromMilliseconds(100)));
        Assert.False(shot.AttackVisible);
        Assert.Equal(0, shot.MovementSecondsThisFrame);
    }

    [Fact]
    public void AttackClonesKeepSettingsAndStartWithFreshRuntimeState()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var source = new Entity(assets.DefaultEntityAssets);
        var shot = Projectile(source, new(32, 32), 100, 600);
        shot.Update(new GameTime(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(300)));
        shot.MovementSegments.Add((new(32, 32), new(48, 32)));
        shot.HitTargets.Add(source);
        var copy = shot.Clone();
        Assert.False(copy.HasStarted);
        Assert.Equal(TimeSpan.Zero, copy.Duration);
        Assert.Equal(Vector2.Zero, copy.Position);
        Assert.Equal(Vector2.Zero, copy.Motion);
        Assert.Empty(copy.MovementSegments);
        Assert.Empty(copy.HitTargets);
        Assert.NotSame(shot.LevelRequirements, copy.LevelRequirements);
        Assert.NotSame(shot.SkillRequirementsNames, copy.SkillRequirementsNames);
        var volley = Volley(source);
        for (int i = 0; i < 10; i++)
            volley = volley.Clone();
        Assert.Equal(600, volley.Shot.AttackLength);
        Assert.Equal(100, volley.Shot.AttackDelay);
        Assert.Equal(1000, volley.AttackLength);
        Assert.Equal(0, volley.ShotsFired);
        Assert.Empty(volley.Shots);
        volley.Source = source;
        volley.SetUpAttack(new(), Color.White, new(32, 32));
        volley.Update(new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)));
        Assert.Equal(3, volley.ShotsFired);
        volley.SetUpAttack(new GameTime(TimeSpan.FromSeconds(2), TimeSpan.Zero), Color.White, new(32, 32));
        Assert.Equal(0, volley.ShotsFired);
        Assert.Empty(volley.Shots);
        Assert.Equal(600, volley.Shot.AttackLength);
    }

    [Fact]
    public void MultishotAssignsCasterOffsetsOnceChargesOnceAndPreservesScheduledBirthTimes()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var player = World.Player;
        player.Position = new(40, 40);
        player.Mana = 100;
        var template = Volley(player);
        player.PerformAimedAttack(new(), template, new(500, player.Center.Y));
        player.PerformAimedAttack(new(), template, new(500, player.Center.Y));
        Assert.Equal(93, player.Mana);
        var volley = Assert.IsType<MultiShotAttack>(Assert.Single(player.AttackManager.Attacks));
        var time = new GameTime(TimeSpan.FromMilliseconds(550), TimeSpan.FromMilliseconds(550));
        player.AttackManager.Update(time);
        var children = player.AttackManager.Attacks.Where(attack => attack != volley).ToArray();
        Assert.Equal(3, children.Length);
        Assert.Equal(new[] { 200.0, 300.0, 400.0 }, children.Select(attack => attack.StartTime.TotalMilliseconds));
        Assert.All(children, child =>
        {
            Assert.Same(player, child.Source);
            Assert.Equal(new Vector2(52, 63), child.StartPosition);
            Assert.Equal(0, child.ManaCost);
            Assert.Equal(600, child.AttackLength);
        });
        Assert.Equal(new[] { 0.25, 0.15, 0.05 }, children.Select(attack => Math.Round(attack.MovementSecondsThisFrame, 2)));
        Assert.False(volley.AttackVisible);
        Assert.True(player.AttackManager.AttackingIsOnCoolDown(time));
        Assert.False(player.AttackManager.AttackingIsOnCoolDown(new GameTime(TimeSpan.FromMilliseconds(650), TimeSpan.Zero)));
        CollisionDetection.CheckEntityBoundaryCollisions([player], null, 32, 32, 4096, 4096, time);
        for (int i = 0; i < children.Length; i++)
            Assert.InRange(children[i].Position.X, 102 - i * 20 - 0.01f, 102 - i * 20 + 0.01f);
        Assert.Equal(93, player.Mana);
    }

    [Fact]
    public void FinalMovementHitsCrossedTargetsBeforeExpiryCleanupAndOnlyOnce()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var source = new Entity(assets.DefaultEntityAssets) { Attack = 2, Position = new(32, 64) };
        var target = new Entity(assets.DefaultEntityAssets) { Health = 100, Defence = 0, Position = new(200, 64), Width = 8, Height = 8 };
        var manager = new EntityManager();
        manager.Add(source);
        manager.Add(target);
        var shot = Projectile(source, new(32, 64));
        source.AttackManager.AddAttack(shot, new());
        var time = new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        manager.Update(time, time);
        Assert.Equal(100, target.Health);
        Assert.Single(source.AttackManager.Attacks);
        CollisionDetection.CheckEntityBoundaryCollisions(manager.Entities, null, 16, 16, 2048, 2048, time);
        source.AttackManager.CheckAttackHit(manager.Entities, time);
        int health = target.Health;
        Assert.InRange(health, 98, 99);
        target.AttacksHitBy.Clear();
        manager.ResolveAttackHits(time);
        Assert.Equal(health, target.Health);
        Assert.Empty(source.AttackManager.Attacks);
        Assert.InRange(shot.Position.X, 1031.99f, 1032.01f);
    }

    [Fact]
    public void WallClippedProjectileHitsBeforeTheWallButNeverBehindIt()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var level = World.Levels.Values.First(level => level.ConditionalLayer is not null);
        var layer = level.ConditionalLayer;
        (int X, int Y)? wall = null;
        for (int y = 0; y < level.Height / level.TileHeight && wall is null; y++)
            for (int x = 4; x < level.Width / level.TileWidth - 1; x++)
            {
                bool Solid(int column) => layer.GetTile(column, y).HasValue;
                if (Solid(x) && !Solid(x - 1) && !Solid(x - 2) && !Solid(x - 3))
                { wall = (x, y); break; }
            }
        Assert.NotNull(wall);
        float wallX = wall.Value.X * level.TileWidth, wallY = wall.Value.Y * level.TileHeight;
        var source = World.Player;
        source.CanMove = false;
        source.Attack = 2;
        var before = new Enemy(assets.DefaultEntityAssets) { Health = 100, Defence = 0, Width = 8, Height = 8, Position = new(wallX - 16, wallY) };
        var behind = new Enemy(assets.DefaultEntityAssets) { Health = 100, Defence = 0, Width = 8, Height = 8, Position = new(wallX + level.TileWidth + 4, wallY) };
        var shot = Projectile(source, new(wallX - 3 * level.TileWidth, wallY));
        source.AttackManager.AddAttack(shot, new());
        var time = new GameTime(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        source.AttackManager.Update(time);
        source.CanMove = false;
        bool visible = layer.IsVisible;
        try
        {
            layer.IsVisible = true;
            CollisionDetection.CheckEntityBoundaryCollisions([source], [layer], level.TileWidth, level.TileHeight, level.Width, level.Height, time);
            source.AttackManager.CheckAttackHit([before, behind], time);
            Assert.InRange(before.Health, 98, 99);
            Assert.Equal(100, behind.Health);
            Assert.True(shot.Position.X + shot.Width <= wallX);
            Assert.Equal(Vector2.Zero, shot.Motion);
        }
        finally { layer.IsVisible = visible; }
    }

    [Fact]
    public void LargeShapedAttacksUseTheirGeometryBeyondOneHundredPixels()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var player = World.Player;
        player.Attack = 2;
        var shape = (TriangleAttack)assets.AttackTemplates["TriangleAttack"].Clone();
        shape.Source = player;
        shape.AttackDelay = 0;
        shape.SetUpAttack(new(), Color.White, new(100, 100));
        shape.Triangles.Add(new Triangle(new(100, 100), new(300, 100), new(300, 200), Color.Red));
        shape.DamageModifier = 1;
        player.AttackManager.AddAttack(shape, new());
        var inside = new Enemy(assets.DefaultEntityAssets) { Position = new(250, 120), Width = 8, Height = 8, Health = 100, Defence = 0 };
        var outside = new Enemy(assets.DefaultEntityAssets) { Position = new(105, 180), Width = 8, Height = 8, Health = 100, Defence = 0 };
        player.AttackManager.CheckAttackHit([inside, outside], new());
        Assert.InRange(inside.Health, 98, 99);
        Assert.Equal(100, outside.Health);
        Assert.False(ShootingAttack.SweptHit(new(0, 0), new(100, 100), 8, 8, new(5, 80, 8, 8)));
    }

    [Theory]
    [InlineData(30)]
    [InlineData(144)]
    public void DelayedFastShotsHitSmallTargetsAndExpireConsistentlyAcrossFrameRates(int fps)
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var source = new Entity(assets.DefaultEntityAssets) { Attack = 2 };
        var target = new Entity(assets.DefaultEntityAssets) { Health = 100, Defence = 0, Position = new(200, 64), Width = 8, Height = 8 };
        var manager = new EntityManager();
        manager.Add(source);
        manager.Add(target);
        var shot = Projectile(source, new(32, 64), 100, 600);
        source.AttackManager.AddAttack(shot, new());
        for (int frame = 1; frame <= fps; frame++)
        {
            var time = new GameTime(TimeSpan.FromSeconds((double)frame / fps), TimeSpan.FromSeconds(1.0 / fps));
            manager.Update(time, time);
            CollisionDetection.CheckEntityBoundaryCollisions(manager.Entities, null, 16, 16, 2048, 2048, time);
            manager.ResolveAttackHits(time);
        }
        Assert.InRange(target.Health, 98, 99);
        Assert.Empty(source.AttackManager.Attacks);
        Assert.InRange(shot.Position.X, 531.99f, 532.01f);
    }

    [Fact]
    public void GameplayFramesCanUpdateAndDrawAnActualVolley()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        using var input = new InputHandler(game);
        var screen = new GameScreen(game, assets);
        game.GameScreen = screen;
        var player = World.Player;
        player.Mana = 100;
        var volley = (ShootingAttack)assets.AttackTemplates["IceBullets"].Clone();
        var target = player.Center + new Vector2(200, 0);
        player.PerformAimedAttack(new(), volley, target);
        player.PerformAimedAttack(new(), volley, target);
        var fired = Assert.IsAssignableFrom<MultiShotAttack>(Assert.Single(player.AttackManager.Attacks));
        Assert.Equal(99, player.Mana);
        for (int frame = 1; frame <= 12; frame++)
            screen.Update(new GameTime(TimeSpan.FromSeconds(frame * 0.1), TimeSpan.FromSeconds(0.1)));
        using var batch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(game.GraphicsDevice);
        screen.Draw(batch);
        Assert.True(fired.ShotsFired > 0);
        Assert.All(player.AttackManager.Attacks, attack => Assert.Same(player, attack.Source));
    }
}
