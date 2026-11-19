using Microsoft.Xna.Framework;
using MonoGame.Extended.Tilemaps;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameEvents;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    [Fact]
    public void PausingFreezesVolleyCooldownAndTileAnimationUntilPlayResumes()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        using var input = new InputHandler(game);
        game.GameScreen = new GameScreen(game, assets);
        game.ExitScreen = new ExitScreen(game, assets);
        game.StateManager.ChangeState(game.GameScreen);
        game.StateManager.Update(new GameTime(TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100)));

        var player = World.Player;
        player.Mana = 100;
        var volley = Volley(player);
        var aim = player.Center + new Vector2(200, 0);
        player.PerformAimedAttack(World.TotalTimeInWorld, volley, aim);
        player.PerformAimedAttack(World.TotalTimeInWorld, volley, aim);
        var fired = Assert.IsType<MultiShotAttack>(Assert.Single(player.AttackManager.Attacks));

        var frames = new TileAnimation([new(0, 100), new(1, 100), new(2, 1500)]);
        var animation = new TiledAnimation("Lever", assets.LeverAnimation.Texture, frames);
        var tileEvent = new AnimatedTileEvent(animation, player.Position);
        var layer = World.CurrentLevel.TiledMap.Layers.OfType<TilemapTileLayer>().First();
        bool layerVisible = layer.IsVisible;
        World.AddGameEventToCurrentLevel(tileEvent);
        World.AddGameEventToCurrentLevel(new TimeDelayedGameEvent(new ToggleLayerVisibility(layer), tileEvent.Duration));
        game.StateManager.Update(new GameTime(TimeSpan.FromSeconds(30.1), TimeSpan.FromMilliseconds(100)));
        Assert.Equal(1, frames.CurrentFrameIndex);
        Assert.Equal(0, fired.ShotsFired);

        game.StateManager.ChangeState(game.ExitScreen);
        game.StateManager.Update(new GameTime(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(1)));
        Assert.Equal(TimeSpan.FromMilliseconds(200), World.TotalTimeInWorld.TotalGameTime);
        Assert.Equal(100f, tileEvent.ElapsedTime);
        Assert.Equal(1, frames.CurrentFrameIndex);
        Assert.Equal(0, fired.ShotsFired);
        Assert.True(player.AttackManager.AttackingIsOnCoolDown(World.TotalTimeInWorld));

        game.StateManager.ChangeState(game.GameScreen);
        game.StateManager.Update(new GameTime(TimeSpan.FromSeconds(60.1), TimeSpan.FromMilliseconds(100)));
        Assert.Equal(TimeSpan.FromMilliseconds(300), World.TotalTimeInWorld.TotalGameTime);
        Assert.Equal(1, fired.ShotsFired);
        Assert.Equal(2, frames.CurrentFrameIndex);
        Assert.True(player.AttackManager.AttackingIsOnCoolDown(World.TotalTimeInWorld));

        game.StateManager.Update(new GameTime(TimeSpan.FromSeconds(61.2), TimeSpan.FromMilliseconds(1100)));
        Assert.Equal(1300f, tileEvent.ElapsedTime);
        Assert.False(tileEvent.IsComplete);
        Assert.False(player.AttackManager.AttackingIsOnCoolDown(World.TotalTimeInWorld));
        Assert.Equal(layerVisible, layer.IsVisible);

        game.StateManager.Update(new GameTime(TimeSpan.FromSeconds(61.6), TimeSpan.FromMilliseconds(400)));
        Assert.True(tileEvent.IsComplete);
        Assert.Equal(!layerVisible, layer.IsVisible);
    }

    [Fact]
    public void StartingAndLoadingGamesKeepMapLayerStateSeparate()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var (name, original) = World.Levels.First(entry => entry.Value.ConditionalLayer is not null);
        bool visibleByDefault = original.ConditionalLayer.IsVisible;
        original.ConditionalLayer.IsVisible = !visibleByDefault;
        var save = World.ToData();

        _ = new GameScreen(game, assets);
        var fresh = World.Levels[name];
        Assert.NotSame(original.TiledMap, fresh.TiledMap);
        Assert.NotSame(original.ConditionalLayer, fresh.ConditionalLayer);
        Assert.Same(original.TiledMap.Tilesets.First().Texture, fresh.TiledMap.Tilesets.First().Texture);
        Assert.Equal(visibleByDefault, fresh.ConditionalLayer.IsVisible);

        _ = new GameScreen(game, assets, save, World.Player.Texture);
        var restored = World.Levels[name];
        Assert.NotSame(fresh.TiledMap, restored.TiledMap);
        Assert.Equal(!visibleByDefault, restored.ConditionalLayer.IsVisible);
        Assert.Equal(visibleByDefault, fresh.ConditionalLayer.IsVisible);
    }

    [Fact]
    public void ChangingLevelsClearsAttacksAndAimButKeepsCooldown()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var player = World.Player;
        var time = new GameTime();
        var volley = Volley(player);
        var aim = player.Center + new Vector2(200, 0);
        player.PerformAimedAttack(time, volley, aim);
        player.PerformBasicAttack(time, player.BasicAttack);
        Assert.Single(player.AttackManager.Attacks);
        player.Motion = Vector2.UnitX;
        player.CanMove = player.CanAttack = false;

        var next = World.Levels.Values.First(level => level != World.CurrentLevel);
        World.SetCurrentLevel(next);

        Assert.Same(next, World.CurrentLevel);
        Assert.Empty(player.AttackManager.Attacks);
        Assert.Equal(Vector2.Zero, player.Motion);
        Assert.True(player.CanMove);
        Assert.True(player.CanAttack);
        Assert.True(player.AttackManager.AttackingIsOnCoolDown(time));

        time.TotalGameTime = TimeSpan.FromSeconds(2);
        player.PerformAimedAttack(time, volley, player.Center + new Vector2(200, 0));
        Assert.Empty(player.AttackManager.Attacks);
    }
}
