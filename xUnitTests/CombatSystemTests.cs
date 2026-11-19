using Microsoft.Xna.Framework;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.EntityHelperClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.ItemClasses.ItemManagement;
using SkeletonsAdventure.Quests;
using SkeletonsAdventure.States;

namespace xUnitTests;

public partial class UnitTest1
{
    private sealed class LootCountingEnemy(EntityAssets assets) : Enemy(assets)
    {
        public int DropRolls { get; private set; }
        public int Updates { get; private set; }
        public override List<GameItem> GetDrops()
        {
            DropRolls++;
            return [GameManager.GetItemByName("Coins")];
        }
        public override void Update(GameTime time)
        {
            Updates++;
            base.Update(time);
        }
    }

    private static BasicAttack LethalAttack(Entity attacker)
    {
        attacker.Attack = 1000;
        var attack = attacker.BasicAttack.Clone();
        attack.Source = attacker;
        attack.DamageModifier = 1;
        return attack;
    }

    [Fact]
    public void MultipleLethalHitsAwardOneKillOneQuestStepAndOneLootRoll()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var enemy = new LootCountingEnemy(assets.DefaultEntityAssets) { Health = 1, Defence = 0, XP = 5 };
        var entities = new EntityManager();
        entities.Add(enemy);
        var task = new SlayTask { EntityToSlay = enemy, RequiredAmount = 5 };
        World.Player.ActiveQuests.Add(new Quest { Active = true, Tasks = [task] });
        int originalXp = World.Player.TotalXP;
        var time = new GameTime(TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(16));
        enemy.GetHitByAttack(LethalAttack(World.Player), time);
        enemy.GetHitByAttack(LethalAttack(World.Player), time);
        DamageEngine.GetHitByAttack(enemy, LethalAttack(World.Player), time);
        enemy.EntityDiedByAttack(LethalAttack(World.Player));
        entities.Update(time);
        Assert.True(enemy.IsDead);
        Assert.Equal(0, enemy.Health);
        Assert.Equal(originalXp + 5, World.Player.TotalXP);
        Assert.Equal(1, World.Player.KillCounter.GetKillCount(nameof(LootCountingEnemy)));
        Assert.Equal(1, task.CompletedAmount);
        Assert.Equal(1, enemy.DropRolls);
        Assert.Single(entities.DroppedLootManager.Items);
        Assert.Equal(0, enemy.Updates);
        Assert.Equal(TimeSpan.FromSeconds(20), enemy.LastDeathTime);
    }

    [Fact]
    public void EnvironmentalDeathRollsOnceAndRespawnAllowsAnotherDeath()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var enemy = new LootCountingEnemy(assets.DefaultEntityAssets)
        {
            Health = 0, MaxHealth = 10, RespawnTime = 3, RespawnPosition = new Vector2(34, 56)
        };
        var entities = new EntityManager();
        entities.Add(enemy);
        var time = new GameTime(TimeSpan.FromSeconds(10), TimeSpan.Zero);
        entities.Update(time);
        entities.Update(time);
        Assert.Equal(1, enemy.DropRolls);
        Assert.False(enemy.CanAttack);
        Assert.False(enemy.CanMove);
        entities.Update(new GameTime(TimeSpan.FromSeconds(13), TimeSpan.Zero));
        Assert.False(enemy.IsDead);
        Assert.Equal(10, enemy.Health);
        Assert.Equal(enemy.RespawnPosition, enemy.Position);
        Assert.True(enemy.CanAttack);
        Assert.True(enemy.CanMove);
        enemy.EntityDied(new GameTime(TimeSpan.FromSeconds(14), TimeSpan.Zero));
        enemy.EntityDied(new GameTime(TimeSpan.FromSeconds(15), TimeSpan.Zero));
        Assert.Equal(TimeSpan.FromSeconds(14), enemy.LastDeathTime);
        Assert.Equal(2, enemy.DropRolls);
    }

    [Fact]
    public void InvincibilityAndRepeatedContactCannotApplyDamageTwice()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var enemy = new Enemy(assets.DefaultEntityAssets) { Health = 100, Defence = 0, IsInvincible = true };
        var attack = LethalAttack(World.Player);
        enemy.GetHitByAttack(attack, new());
        Assert.Equal(100, enemy.Health);
        Assert.Empty(enemy.AttacksHitBy);
        enemy.IsInvincible = false;
        World.Player.Attack = 2;
        enemy.GetHitByAttack(attack, new());
        int health = enemy.Health;
        enemy.GetHitByAttack(attack, new());
        Assert.InRange(health, 98, 99);
        Assert.Equal(health, enemy.Health);
    }

    [Fact]
    public void DeadPlayersCannotCastAndEnemiesStopPursuingThem()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var player = World.Player;
        player.IsDead = true;
        player.CanAttack = true;
        player.Mana = 100;
        var time = new GameTime(TimeSpan.FromMinutes(1), TimeSpan.Zero);
        player.PerformAttack(time, player.BasicAttack);
        var projectile = Assert.IsAssignableFrom<ShootingAttack>(assets.AttackTemplates["FireBall"].Clone());
        player.PerformAimedAttack(time, projectile, Vector2.Zero);
        player.PerformAimedAttack(time, projectile, Vector2.Zero);
        Assert.Empty(player.AttackManager.Attacks);
        Assert.Equal(100, player.Mana);
        var enemy = new Enemy(assets.DefaultEntityAssets) { Motion = Vector2.One };
        EnemyAI.CheckIfEnemyDetectPlayer(time, [enemy], player);
        Assert.Equal(Vector2.Zero, enemy.Motion);
    }

    [Fact]
    public void LootPickupConsumesEachDropOnceAndLeavesDropsWhenBackpackIsFull()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var entities = new EntityManager();
        var coins = GameManager.GetItemByName("Coins");
        coins.SetQuantity(7);
        entities.DroppedLootManager.Add(coins, World.Player.Position - new Vector2(20, 20));
        entities.PickUpLoot();
        entities.PickUpLoot();
        Assert.Equal(7, Assert.Single(World.Player.Backpack.Items).Quantity);
        Assert.Empty(entities.DroppedLootManager.Items);
        World.Player.Backpack.Items.Clear();
        var equipment = GameManager.ItemsClone.Values.First(i => !i.Stackable);
        for (int i = 0; i < World.Player.Backpack.Capacity; i++)
            World.Player.Backpack.Items.Add(equipment.Clone());
        entities.DroppedLootManager.Add(equipment, World.Player.Position - new Vector2(20, 20));
        entities.PickUpLoot();
        Assert.Single(entities.DroppedLootManager.Items);
        Assert.Equal(World.Player.Backpack.Capacity, World.Player.Backpack.Count);
    }

    [Fact]
    public void GuaranteedOnlyDropsWorkAndGroundLootDoesNotMoveItsTemplate()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var enemy = new Enemy(assets.DefaultEntityAssets) { Health = 0, DropTableName = "" };
        var coins = GameManager.GetItemByName("Coins");
        coins.Position = new Vector2(9, 11);
        enemy.GuaranteedDrops.Add(coins);
        var entities = new EntityManager();
        entities.Add(enemy);
        entities.Update(new());
        Assert.Single(entities.DroppedLootManager.Items);
        Assert.Equal(new Vector2(9, 11), enemy.GuaranteedDrops.Items[0].Position);
        var table = new DropTable(new List<DropTableItem> { new("Coins", 100, 1, 1) });
        Assert.Empty(table.GetDrops(0));
        Assert.Equal(3, table.GetDrops(3).Length);
        Assert.Empty(new DropTable().GetDropsList(3));
    }

    [Fact]
    public void RemovingEntitiesUnhooksDeathLootAndRespawnClearsHitHistory()
    {
        using var game = new SkeletonsAdventure.Game1();
        var assets = new GameManager(game);
        _ = new GameScreen(game, assets);
        var enemy = new LootCountingEnemy(assets.DefaultEntityAssets);
        var entities = new EntityManager();
        entities.Add(enemy);
        entities.Remove(enemy);
        enemy.EntityDied(new());
        Assert.Equal(0, enemy.DropRolls);
        enemy.AttacksHitBy.Add(World.Player.BasicAttack);
        enemy.Respawn();
        Assert.Empty(enemy.AttacksHitBy);
        entities.Add(enemy);
        entities.Clear();
        enemy.EntityDied(new());
        Assert.Equal(0, enemy.DropRolls);
    }
}
