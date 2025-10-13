
namespace SkeletonsAdventure.Engines
{
    internal class KillCounter
    {
        public Dictionary<string, int> EnemyKills { get; private set; } = [];

        public KillCounter() { }

        public void RecordKill(string enemyName)
        {
            if (EnemyKills.TryGetValue(enemyName, out int value))
            {
                EnemyKills[enemyName] = ++value;
            }
            else
            {
                EnemyKills[enemyName] = 1;
            }
        }

        public int GetKillCount(string enemyName)
        {
            if (EnemyKills.TryGetValue(enemyName, out int value))
            {
                return value;
            }
            return 0;
        }

        public override string ToString()
        {
            string result = "Kill Counts:\n";

            foreach (var (enemy, count) in EnemyKills)
            {
                result += $"{enemy}: {count}\n";
            }

            return result;
        }
    }
}
