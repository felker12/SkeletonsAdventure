using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tilemaps;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameEvents;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    [Fact]
    public void PauseItemsTabEquipsGearUpdatesStatsAndInspectsConsumables()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        game.GameScreen = new GameScreen(game, assets);
        game.ExitScreen = new ExitScreen(game, assets);
        var tabs = game.ExitScreen.ExitScreenMenu.TabBar;
        Assert.Equal(new[] { "Save", "Player", "Items", "Quests", "Evolution", "Skills", "Action Bar", "Settings" },
            tabs.TabMenus.Values.Select(menu => menu.Title));

        var player = World.Player;
        Assert.True(player.Backpack.Add(GameManager.GetItemByName("Sword")));
        Assert.True(player.Backpack.Add(GameManager.GetItemByName("Bones")));
        var sword = Assert.IsType<Weapon>(player.Backpack.Items[0]);
        var bones = player.Backpack.Items[1];
        int attackWithoutSword = player.Attack;
        var items = Assert.Single(tabs.TabMenus.Values.OfType<ItemsMenu>());
        tabs.SetActiveTab(items);
        items.SelectItem(sword);
        Assert.True(items.ToggleSelectedEquipment());
        Assert.True(sword.Equipped);
        Assert.Equal(attackWithoutSword + sword.AttackValue, player.Attack);
        Assert.Contains(items.ControlManager.OfType<Label>(), label => label.Text.StartsWith($"Attack  {player.Attack}\n"));
        Assert.True(items.ToggleSelectedEquipment());
        Assert.False(sword.Equipped);
        Assert.Equal(attackWithoutSword, player.Attack);

        sword.LevelRequirements.Level = player.Level + 1;
        items.SelectItem(sword);
        Assert.Contains(items.ControlManager.OfType<MenuButton>(), button => button.Text == "Equip" && !button.Enabled);
        Assert.False(items.ToggleSelectedEquipment());
        Assert.Contains(items.ControlManager.OfType<Label>(), label => label.Text.Contains("Requirements not met"));

        items.SelectItem(bones);
        Assert.DoesNotContain(items.ControlManager.OfType<MenuButton>(), button => button.Visible && button.Text == "Equip");
        Assert.False(items.ToggleSelectedEquipment());
        Assert.Equal(1, bones.Quantity);
        using var batch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(game.GraphicsDevice);
        game.ExitScreen.Draw(batch);
    }

    [Fact]
    public void InventoryPopupFitsInViewportAtLowerRightCorner()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var screen = new GameScreen(game, assets);
        var popup = screen.GameItemPopUpBox;
        var viewport = game.GraphicsDevice.Viewport.Bounds;
        var anchor = new Point(viewport.Right - 1, viewport.Bottom - 1);
        popup.Position = anchor.ToVector2();

        var equip = popup.Buttons[0];
        var drop = popup.Buttons[^1];
        equip.Visible = drop.Visible = true;
        equip.SetText("Equip Round Shield");
        drop.SetText("Drop Round Shield");

        popup.Update(new GameTime(), false, Matrix.Identity, viewport);

        Assert.Equal(viewport.Right, popup.Rectangle.Right);
        Assert.Equal(viewport.Bottom, popup.Rectangle.Bottom);
        Assert.True(popup.Rectangle.Contains(anchor));
        Assert.True(viewport.Contains(equip.Rectangle));
        Assert.True(viewport.Contains(drop.Rectangle));
    }

    [Fact]
    public void CompiledMapsKeepTheirOwnOffsetsForSharedTilesets()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);

        var level = World.Levels["Level0"];
        var map = level.TiledMap;
        var layer = map.Layers.GetLayer<TilemapTileLayer>("InteractableObjectLayer");
        const int markerColumn = 23;
        const int markerRow = 24;
        var marker = Assert.IsType<TilemapTile>(layer.GetTile(markerColumn, markerRow));
        int localId = marker.GetLocalId(map.Tilesets, out var atlas);
        Assert.Equal("ProjectUtumno_full", atlas.Name);
        Assert.Equal(new Rectangle(1312, 896, 32, 32), atlas.GetTileRegion(localId));
        var questNode = Assert.Single(level.InteractableObjectManager.Objects);
        var markerBounds = new Rectangle(markerColumn * level.TileWidth,
            (markerRow + 1) * level.TileHeight - atlas.TileHeight, atlas.TileWidth, atlas.TileHeight);
        Assert.Equal(markerBounds, questNode.Rectangle);
        Assert.Equal(1093, World.Levels["Level1"].TiledMap.Tilesets.Single(set => set.Name == atlas.Name).FirstGlobalId);
        Assert.Equal(1, World.Levels["TestLevel"].TiledMap.Tilesets.Single(set => set.Name == atlas.Name).FirstGlobalId);
    }

    [Fact]
    public void FreeCameraPansAndZoomsOnlyInDevAndResetsWhenFollowingAgain()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var screen = new GameScreen(game, assets);
        var camera = screen.Camera;
        var playerPosition = new Vector2(2000, 2000);
        var viewport = game.GraphicsDevice.Viewport;
        var screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;
        camera.Update(playerPosition);
        Assert.Equal(screenCenter, Vector2.Transform(playerPosition, camera.Transformation));

        var followedView = camera.Transformation;
        camera.ToggleDetached(playerPosition);
        Assert.Equal(SkeletonsAdventure.Game1.IfDev, camera.IsDetached);
        if (!camera.IsDetached)
        {
            camera.MoveDetached(Vector2.UnitX, 1, TimeSpan.FromSeconds(1));
            Assert.Equal(followedView, camera.Transformation);
            return;
        }

        camera.Update(playerPosition + new Vector2(100, 100));
        Assert.Equal(followedView, camera.Transformation);
        camera.MoveDetached(Vector2.Zero, 1, TimeSpan.Zero);
        Assert.Equal(1.25f, camera.Transformation.M11);
        Assert.InRange(Vector2.Distance(screenCenter, Vector2.Transform(playerPosition, camera.Transformation)), 0f, 1f);
        camera.MoveDetached(Vector2.UnitX, 0, TimeSpan.FromSeconds(1));
        Assert.InRange(Vector2.Transform(playerPosition, camera.Transformation).X, screenCenter.X - 601, screenCenter.X - 599);

        camera.MoveDetached(Vector2.Zero, -100, TimeSpan.Zero);
        Assert.Equal(0.125f, camera.Transformation.M11);
        camera.MoveDetached(Vector2.Zero, 100, TimeSpan.Zero);
        Assert.Equal(4f, camera.Transformation.M11);
        using var batch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(game.GraphicsDevice);
        screen.Draw(batch);

        camera.ToggleDetached(playerPosition);
        Assert.False(camera.IsDetached);
        Assert.Equal(followedView, camera.Transformation);
        camera.ToggleDetached(playerPosition);
        World.SetCurrentLevel(World.Levels["Level1"]);
        Assert.False(camera.IsDetached);
        Assert.Equal(1f, camera.Transformation.M11);
    }

    [Fact]
    public void NewGameMenuFitsAvailableOptionsAndKeepsTestLoadoutSeparate()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var menu = new MenuScreen(game, assets);
        bool isDev = SkeletonsAdventure.Game1.IfDev;
        Assert.Equal(new Rectangle(80, 272, 372, isDev ? 260 : 202), menu.ButtonPanelBounds);

        if (isDev)
        {
            using var directory = new SaveDirectory();
            menu.StartNewGame(testGame: true);
            var testPlayer = World.Player;
            Assert.True(game.GameScreen.IsTestGame);
            Assert.Equal(300, testPlayer.BaseAttack);
            Assert.Equal(3000, testPlayer.Health);
            Assert.Equal(1000, testPlayer.Mana);
            Assert.Equal(3, testPlayer.Backpack.Items.Count(item => item.Name == "Sword"));
            Assert.Equal(nameof(IceBullet), testPlayer.KeybindingsManager.Keybindings[Keys.D3].Name);
            var pauseMenu = game.ExitScreen.ExitScreenMenu;
            Assert.Contains(pauseMenu.TabBar.ActiveMenu.ControlManager, control => control.Text == "Save Game" && control.Enabled);
            testPlayer.Health = 2345;
            Assert.True(SaveManager.SaveGame(game, pauseMenu, directory.Path));
            menu.StartNewGame();
            SaveManager.LoadSavedGame(game, assets, directory.Path);
            Assert.True(game.GameScreen.IsTestGame);
            Assert.Equal(2345, World.Player.Health);
            Assert.Equal(3, World.Player.Backpack.Items.Count(item => item.Name == "Sword"));
            Assert.Equal(nameof(IceBullet), World.Player.KeybindingsManager.Keybindings[Keys.D3].Name);
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => menu.StartNewGame(testGame: true));
        }

        menu.StartNewGame();
        var player = World.Player;
        Assert.False(game.GameScreen.IsTestGame);
        Assert.Equal(12, player.BaseAttack);
        Assert.Equal(80, player.Health);
        Assert.Equal(20, player.Mana);
        Assert.Collection(player.Backpack.Items,
            item =>
            {
                Assert.Equal("Sword", item.Name);
                Assert.True(Assert.IsType<Weapon>(item).Equipped);
            },
            item =>
            {
                Assert.Equal("Bones", item.Name);
                Assert.Equal(3, item.Quantity);
            });
        Assert.Equal(nameof(FireBall), Assert.Single(player.LearnedAttackManager.LearnedAttacks).Key);
        Assert.Equal(nameof(FireBall), player.KeybindingsManager.Keybindings[Keys.D1].Name);
    }

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
