using Microsoft.Xna.Framework;
using SkeletonsAdventure;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    [Theory]
    [InlineData("FireBall")]
    [InlineData("IceBullet")]
    [InlineData("WaterBall")]
    [InlineData("FireWave")]
    [InlineData("BlueFireWave")]
    public void AuthoredProjectileSpeedsRemainConsistentAcrossFrameRates(string name)
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        float pixelsPerSecond = assets.AttackTemplates[name].Speed * Game1.BaseSpeedMultiplier;
        foreach (int fps in new[] { 30, 60, 144 })
        {
            var source = new Entity(assets.DefaultEntityAssets) { CanMove = false };
            var shot = (ShootingAttack)assets.AttackTemplates[name].Clone();
            shot.Source = source;
            shot.AttackDelay = 0;
            shot.SetUpAttack(new(), Color.White, new(100, 100));
            shot.Position = new(100, 100);
            shot.AttackVisible = true;
            shot.MoveInDirectionOfPosition(shot.Center + new Vector2(100, 0));
            source.AttackManager.AddAttack(shot, new());
            for (int frame = 0; frame < fps; frame++)
            {
                shot.Update(new GameTime(TimeSpan.FromSeconds((frame + 1.0) / fps), TimeSpan.FromSeconds(1.0 / fps)));
                CollisionDetection.CheckEntityBoundaryCollisions([source], null, 32, 32, 4096, 4096,
                    new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / fps)));
            }
            Assert.InRange(shot.Position.X - 100, pixelsPerSecond - 0.03f, pixelsPerSecond + 0.03f);
            Assert.InRange(shot.Position.Y, 99.999f, 100.001f);
        }
    }

    [Fact]
    public void CardinalAndAimedShotsTravelAtTheSameConfiguredSpeed()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var source = new Entity(assets.DefaultEntityAssets) { Position = new(100, 100), CurrentAnimation = AnimationKey.Right };
        var shot = assets.AttackTemplates["FireBall"].Clone();
        shot.AttackDelay = 0;
        shot.Source = source;
        source.PerformAttack(new GameTime(TimeSpan.FromSeconds(1), TimeSpan.Zero), shot);
        var fired = Assert.Single(source.AttackManager.Attacks);
        fired.AttackVisible = true;
        source.CanMove = false;
        var start = fired.Position;
        fired.Update(new GameTime(TimeSpan.FromSeconds(1.1), TimeSpan.FromSeconds(0.1)));
        CollisionDetection.CheckEntityBoundaryCollisions([source], null, 32, 32, 4096, 4096,
            new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));
        float expected = shot.Speed * Game1.BaseSpeedMultiplier * 0.1f;
        Assert.InRange(fired.Position.X - start.X, expected - 0.001f, expected + 0.001f);
    }

    [Fact]
    public void MultiShotPreservesTheConfiguredProjectileSpeed()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var volley = Assert.IsAssignableFrom<MultiShotAttack>(assets.AttackTemplates["IceBullets"]);
        float expected = assets.AttackTemplates["IceBullet"].Speed;
        Assert.Equal(expected, volley.Shot.Speed);
        Assert.Equal(expected, volley.Clone().Shot.Speed);
    }
}
