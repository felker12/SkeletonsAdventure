using Microsoft.Xna.Framework;
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
    [InlineData("FireBall", 612.5f)]
    [InlineData("IceBullet", 703.125f)]
    [InlineData("WaterBall", 512f)]
    [InlineData("FireWave", 200f)]
    [InlineData("BlueFireWave", 200f)]
    public void AuthoredProjectileSpeedsPreserveTheirPreviousTravelAcrossFrameRates(string name, float pixelsPerSecond)
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        foreach (int fps in new[] { 30, 60, 144 })
        {
            var source = new Entity(assets.DefaultEntityAssets) { CanMove = false };
            var shot = (ShootingAttack)assets.AttackTemplates[name].Clone();
            shot.Source = source;
            shot.Position = new(100, 100);
            shot.AttackVisible = true;
            shot.MoveInDirectionOfPosition(shot.Center + new Vector2(100, 0));
            source.AttackManager.AddAttack(shot, new());
            for (int frame = 0; frame < fps; frame++)
                CollisionDetection.CheckEntityBoundaryCollisions([source], null, 32, 32, 4096, 4096,
                    new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / fps)));
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
        shot.Source = source;
        source.PerformAttack(new GameTime(TimeSpan.FromSeconds(1), TimeSpan.Zero), shot);
        var fired = Assert.Single(source.AttackManager.Attacks);
        fired.AttackVisible = true;
        source.CanMove = false;
        var start = fired.Position;
        CollisionDetection.CheckEntityBoundaryCollisions([source], null, 32, 32, 4096, 4096,
            new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));
        Assert.InRange(fired.Position.X - start.X, 61.249f, 61.251f);
    }

    [Fact]
    public void MultiShotUsesTheRetunedProjectileSpeed()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var volley = Assert.IsAssignableFrom<MultiShotAttack>(assets.AttackTemplates["IceBullets"]);
        Assert.Equal(14.0625f, volley.Speed);
        Assert.Equal(14.0625f, volley.Shot.Speed);
        Assert.Equal(14.0625f, volley.Clone().Shot.Speed);
    }
}
