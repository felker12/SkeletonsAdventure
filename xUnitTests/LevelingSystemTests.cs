using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.LibraryClasses;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    [Fact]
    public void PlayerTabStagesAdjustsAndAppliesOnlyAvailablePoints()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var pause = new ExitScreen(game, assets);
        var tabs = pause.ExitScreenMenu.TabBar;
        var menu = Assert.Single(tabs.TabMenus.Values.OfType<PlayerInfoMenu>());
        var player = World.Player;
        player.GainXp(GameManager.XPTable.GetLevelXPAtLevel(1));
        tabs.SetActiveTab(menu);

        int attack = player.Attack;
        int defence = player.Defence;
        Assert.All(menu.ControlManager.OfType<MenuButton>().Where(button => button.Text == "-1"),
            button => Assert.False(button.Enabled));
        menu.AdjustPoints(0, 5);
        menu.AdjustPoints(1, 1);
        Assert.Equal(5, player.AttributePoints);
        Assert.Equal(attack, player.Attack);
        Assert.All(menu.ControlManager.OfType<MenuButton>().Where(button => button.Text == "+1"),
            button => Assert.False(button.Enabled));

        menu.AdjustPoints(0, -1);
        menu.AdjustPoints(1, 1);
        using var batch = new SpriteBatch(game.GraphicsDevice);
        pause.Draw(batch);

        menu.CancelPendingPoints();
        Assert.Equal(5, player.AttributePoints);
        Assert.Equal(attack, player.Attack);
        Assert.Contains(menu.ControlManager.OfType<MenuButton>(), button => button.Text == "Apply points" && !button.Enabled);

        menu.AdjustPoints(0, 5);
        menu.AdjustPoints(1, 1);
        menu.AdjustPoints(0, -1);
        menu.AdjustPoints(1, 1);
        menu.ApplyPendingPoints();
        Assert.Equal(attack + 4, player.Attack);
        Assert.Equal(defence + 1, player.Defence);
        Assert.Equal(0, player.AttributePoints);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(79, 0)]
    [InlineData(80, 1)]
    [InlineData(81, 1)]
    [InlineData(179, 1)]
    [InlineData(180, 2)]
    [InlineData(204019, 99)]
    [InlineData(204020, 100)]
    [InlineData(int.MaxValue, 100)]
    public void XpThresholdsIncludeTheirExactBoundary(int xp, int expectedLevel)
    {
        using var directory = new SaveDirectory();
        var table = new XPTable(directory.Path);
        Assert.Equal(expectedLevel, table.GetPlayerLevelAtXP(xp));
        var progress = table.GetProgress(xp);
        Assert.True(progress.Required > 0);
        Assert.InRange(progress.Current, 0, progress.Required);
        Assert.Equal(expectedLevel == table.MaximumLevel, progress.AtMaximumLevel);
    }

    [Theory]
    [InlineData("")]
    [InlineData("bad data")]
    [InlineData("0,0\n2,80")]
    [InlineData("0,0\n1,80\n1,180")]
    [InlineData("0,0\n1,80\n2,80")]
    [InlineData("0,0\n1,-80")]
    [InlineData("0,1")]
    [InlineData("0,0\n1,2147483648")]
    public void MalformedXpTablesAreRejectedWithoutOverwritingThem(string content)
    {
        using var directory = new SaveDirectory();
        string path = Path.Combine(directory.Path, "PlayerLevels.txt");
        File.WriteAllText(path, content);
        Assert.Throws<InvalidDataException>(() => new XPTable(directory.Path));
        Assert.Equal(content, File.ReadAllText(path));
    }

    [Fact]
    public void ProgressAndMaximumLevelFollowTheConfiguredTable()
    {
        using var directory = new SaveDirectory();
        File.WriteAllText(Path.Combine(directory.Path, "PlayerLevels.txt"), "0,0\n1,10\n2,30");
        var table = new XPTable(directory.Path);
        Assert.Equal(2, table.MaximumLevel);
        Assert.Equal(30, table.MaximumXP);
        Assert.Equal((5, 20, false), table.GetProgress(15));
        Assert.Equal((1, 1, true), table.GetProgress(30));
        Assert.Equal((1, 1, true), table.GetProgress(int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => table.GetLevelXPAtLevel(3));
    }

    [Fact]
    public void MultiLevelGainsAwardEachLevelOnceAndRefillFinalStats()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        World.MessagesToAdd.Clear();
        var player = World.Player;
        player.Health = 1;
        player.Mana = 1;
        player.GainXp(GameManager.XPTable.GetLevelXPAtLevel(3));
        Assert.Equal(3, player.Level);
        Assert.Equal(15, player.AttributePoints);
        Assert.Equal(player.BaseAttack + 6, player.AttackExcludingEquipment);
        Assert.Equal(player.MaxHealth, player.Health);
        Assert.Equal(player.MaxMana, player.Mana);
        for (int level = 1; level <= 3; level++)
            Assert.Single(World.MessagesToAdd, message => message == $"You leveled up! You are now level {level}!");
        int messages = World.MessagesToAdd.Count;
        player.GainXp(0);
        player.GainXp(1);
        Assert.Equal(15, player.AttributePoints);
        Assert.Equal(messages, World.MessagesToAdd.Count);
    }

    [Fact]
    public void HugeXpAwardsSaturateAtTheCapAndTheHudCanDrawThere()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        var screen = new GameScreen(game, assets);
        var player = World.Player;
        player.XPModifier = float.MaxValue;
        player.GainXp(int.MaxValue);
        Assert.Equal(GameManager.XPTable.MaximumLevel, player.Level);
        Assert.Equal(GameManager.XPTable.MaximumXP, player.TotalXP);
        Assert.Equal(GameManager.XPTable.MaximumLevel * 5, player.AttributePoints);
        int points = player.AttributePoints;
        int messages = World.MessagesToAdd.Count;
        player.GainXp(int.MaxValue);
        Assert.Equal(points, player.AttributePoints);
        Assert.Equal(messages, World.MessagesToAdd.Count);
        screen.UpdatePlayerStatusBars();
        using var batch = new SpriteBatch(game.GraphicsDevice);
        screen.Draw(batch);
    }

    [Fact]
    public void InvalidAwardsAndAttributeSpendingLeaveProgressUnchanged()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var player = World.Player;
        Assert.Throws<ArgumentOutOfRangeException>(() => player.GainXp(-1));
        foreach (float modifier in new[] { -1, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            Assert.Throws<ArgumentOutOfRangeException>(() => player.XPModifier = modifier);
        Assert.Equal(0, player.TotalXP);
        Assert.Equal(1f, player.XPModifier);
        player.GainXp(80);
        Assert.Equal(5, player.AttributePoints);
        int attack = player.Attack;
        Assert.Throws<ArgumentOutOfRangeException>(() => player.ApplyAttributePoints(-1, 0, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => player.ApplyAttributePoints(5, 1, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => player.ApplyAttributePoints(int.MaxValue, int.MaxValue, 0, 0));
        Assert.Equal(attack, player.Attack);
        Assert.Equal(5, player.AttributePoints);
        player.ApplyAttributePoints(5, 0, 0, 0);
        Assert.Equal(0, player.AttributePoints);
        Assert.Equal(attack + 5, player.Attack);
    }

    [Fact]
    public void LoadingAnOldThresholdSaveRepairsTheMissedLevelOnlyOnce()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var saved = World.ToData();
        saved.PlayerData.totalXP = 80;
        saved.PlayerData.EntityLevel = 0;
        saved.PlayerData.attributePoints = 0;
        saved.PlayerData.CurrentHealth = 17;
        saved.PlayerData.mana = 7;
        _ = new GameScreen(game, assets, saved, assets.TexturesLibrary.SkeletonTexture);
        Assert.Equal(1, World.Player.Level);
        Assert.Equal(5, World.Player.AttributePoints);
        Assert.Equal(17, World.Player.Health);
        Assert.Equal(7, World.Player.Mana);
        saved = World.ToData();
        _ = new GameScreen(game, assets, saved, assets.TexturesLibrary.SkeletonTexture);
        Assert.Equal(1, World.Player.Level);
        Assert.Equal(5, World.Player.AttributePoints);
        Assert.Equal(80, World.Player.TotalXP);
    }

    [Fact]
    public void LevelingUnlocksEvolutionAtItsExactThresholdAndPersistsAtTheCap()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var player = World.Player;
        int evolutionXp = GameManager.XPTable.GetLevelXPAtLevel(25);
        player.GainXp(evolutionXp - 1);
        Assert.False(player.CanEvolve);
        player.GainXp(1);
        Assert.Equal(25, player.Level);
        Assert.True(player.CanEvolve);
        player.GainXp(int.MaxValue);
        var saved = World.ToData();
        int points = player.AttributePoints;
        _ = new GameScreen(game, assets, saved, assets.TexturesLibrary.SkeletonTexture);
        Assert.Equal(100, World.Player.Level);
        Assert.Equal(points, World.Player.AttributePoints);
        World.Player.GainXp(1);
        Assert.Equal(points, World.Player.AttributePoints);
    }
}
