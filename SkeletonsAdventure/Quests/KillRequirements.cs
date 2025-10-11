using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Quests
{
    internal class KillRequirements
    {
        public Dictionary<Enemy, int> RequiredEnemyKills { get; set; } = [];

        public KillRequirements() { }


        public bool CheckRequirements(Player player)
        {
            return CheckRequirements(player.KillCounter);
        }

        public bool CheckRequirements(KillCounter killCounter)
        {
            foreach (var (enemy, requiredKills) in RequiredEnemyKills)
            {
                //TODO test this
                if (killCounter.GetKillCount(enemy.GetType().Name) >= requiredKills)
                {
                    return true;
                }
            }

            return false;
        }

        //dont think this will work as intended
        public bool CheckRequirements(Enemy enemy, int kills)
        {
            if (RequiredEnemyKills.TryGetValue(enemy, out int requiredKills))
            {
                return kills >= requiredKills;
            }
            return false;
        }
    }
}
