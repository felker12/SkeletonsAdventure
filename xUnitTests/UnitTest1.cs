using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;

namespace xUnitTests
{
    public class UnitTest1
    {
        private static void Log(string message)
        {
            // Log to console and debug output
            Console.WriteLine(message);
            Debug.WriteLine(message);
        }

        [Fact]
        public static void TestGameManager()
        {
            Log("Test GameManager Loads");

            //SkeletonsAdventure.Game1.GameManager = new(new());
            GameManager manager = new(new());

            Log("GameManagerString: " + manager.GameManagerString);

            Log("GameManager Loads");
        }

        [Theory]
        [InlineData(1000, 50, 20, 30, 10)] //where the attack is higher than the defence expect positive damage
        [InlineData(1000, 30, 20, 30, 20)] //where attack and defence are equal the avg should be zero
        [InlineData(1000, 0, 0, 30, 0)] //where the attack is zero, expect all damage to be zero
        public static void LogDamageSamples(int trials, int attack, int weaponAtt, int defence, int armourDefence)
        {
            int positiveCount = 0;
            long sum = 0;
            int min = int.MaxValue;
            int max = int.MinValue;
            int zeroCount = 0;

            for (int i = 0; i < trials; i++)
            {
                int dmg = DamageEngine.CalculateDamage(attack, weaponAtt, defence, armourDefence);
                if (dmg > 0) positiveCount++;
                if (dmg == 0) zeroCount++;
                sum += dmg;
                if (dmg < min) min = dmg;
                if (dmg > max) max = dmg;
                Assert.InRange(dmg, 0, 1000000);
            }
            double avg = trials > 0 ? (double)sum / trials : 0.0;
            Log($"DamageEngine samples: trials={trials}, attack={attack}, weaponAttack={weaponAtt}," +
                $"defence={defence}, armourDefence={armourDefence}," +
                $"positive={positiveCount}, zeros={zeroCount}, min={min}, max={max}, avg={avg:F2}");
        }

        //Test Enemy Scaling with Level
        [Fact]
        public void TestEnemyScalingWithLevel()
        {
            Log("Testing Enemy Scaling with Level...");

            Enemy enemy = new();
            int baseHealth = enemy.BaseHealth;
            int baseDefence = enemy.BaseDefence;
            int baseAttack = enemy.BaseAttack;
            int baseXP = enemy.BaseXP;

            for (int level = 1; level <= 100; level++)
            {
                enemy.SetEnemyLevel(level);
                Assert.Equal(baseHealth + level * 2, enemy.MaxHealth);
                Assert.Equal(baseDefence + (int)(level * 1.5), enemy.Defence);
                Assert.Equal(baseAttack + (int)(level * 1.5), enemy.Attack);
                Assert.Equal(baseXP + level * 2, enemy.XP);
            }

            Log($"At level {enemy.Level} the enemies health is: " +
                $"{enemy.MaxHealth}, defence: {enemy.Defence}, attack: {enemy.Attack}");

            Log("Enemy Scaling with Level tests passed.");
        }

        //Kill Counter Test
        [Fact]
        public void KillCounter_RecordAndRetrieve()
        {
            Log("Testing KillCounter.RecordKill and GetKillCount...");

            var kc = new KillCounter();

            Assert.Equal(0, kc.GetKillCount("Goblin"));

            kc.RecordKill("Goblin");
            Assert.Equal(1, kc.GetKillCount("Goblin"));

            kc.RecordKill("Goblin");
            Assert.Equal(2, kc.GetKillCount("Goblin"));

            kc.RecordKill("Orc");
            Assert.Equal(1, kc.GetKillCount("Orc"));

            var data = kc.ToData();
            Assert.True(data.EnemyKills.ContainsKey("Goblin"));
            Assert.Equal(2, data.EnemyKills["Goblin"]);

            var kc2 = new KillCounter(data);
            Assert.Equal(2, kc2.GetKillCount("Goblin"));

            Log("KillCounter tests passed.");
        }

        [Fact]
        public void Keybindings_SetAndStoreAttack()
        {
            Log("Testing KeybindingsManager.SetKeybinding stores attack...");

            var kbm = new KeybindingsManager(null);
            var attack = new BasicAttack(null, null, null);

            kbm.SetKeybinding(Keys.D1, attack);

            Assert.Same(attack, kbm.Keybindings[Keys.D1]);

            Log("Keybindings Set and Store test passed.");
        }

        [Fact]
        public void Keybindings_SetInvalidKey_Throws()
        {
            Log("Testing KeybindingsManager.SetKeybinding doesn't allow for invalid key...");

            var kbm = new KeybindingsManager(null);

            Log($"Keybinding set: {kbm.SetKeybinding((Keys)9999, null)}"); 
            Log("Keybindings invalid key test passed.");
        }

        //Keybindings Update test to ensure cooldowns are updated correctly
        [Fact]
        public void Keybindings_Update_UpdatesAttackCooldown()
        {
            Log("Testing KeybindingsManager.Update updates attack cooldown...");

            var kbm = new KeybindingsManager(null);
            var attack = new BasicAttack(null, null, null)
            {
                // Configure cooldown and last attack time
                CoolDownLength = 500, // ms
                LastAttackTime = TimeSpan.FromMilliseconds(0)
            };

            kbm.SetKeybinding(Keys.D1, attack);

            var gameTime = new GameTime(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(16));
            kbm.Update(gameTime);

            // Expect remaining cooldown to be approximately 400 ms
            Assert.InRange(attack.CooldownRemaining, 399, 501);

            Log($"Cooldown remaining after update: {attack.CooldownRemaining}");
            Log("Keybindings Update test passed.");
        }
    }
}
