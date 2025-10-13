using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using RpgLibrary.QuestClasses;

namespace SkeletonsAdventure.Quests
{
    internal class KillRequirements
    {
        public Dictionary<Enemy, int> RequiredEnemyKills { get; private set; } = [];

        public KillRequirements() { }


        internal bool CheckRequirements(Player player)
        {
            return CheckRequirements(player.KillCounter);
        }

        internal bool CheckRequirements(KillCounter killCounter)
        {
            foreach (var (enemy, requiredAmount) in RequiredEnemyKills)
            {
                int playerKills = killCounter.GetKillCount(enemy.Name);

                if (playerKills < requiredAmount)
                    return false;
            }

            //if all requirements are met return true
            return true;
        }

        internal void AddKillRequirement(Enemy enemy, int requiredAmount = 1)
        {
            RequiredEnemyKills.Add(enemy, requiredAmount);
        }

        public override string ToString()
        {
            string result = "Kill Requirements:\n";
            foreach (var (enemy, count) in RequiredEnemyKills)
            {
                result += $"{enemy.Name}: {count}\n";
            }
            return result;
        }

        public KillRequirementData ToData()
        {
            Dictionary<string, int> data = [];

            foreach (var (enemy, count) in RequiredEnemyKills)
            {
                data.Add(enemy.Name, count);
            }

            return new KillRequirementData(data);
        }
    }
}
