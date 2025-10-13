
namespace RpgLibrary.DataClasses
{
    public class KillCounterData
    {
        public Dictionary<string, int> EnemyKills { get; set; } = new();

        public KillCounterData() { }

        public KillCounterData(Dictionary<string, int> enemyKills)
        {
            EnemyKills = enemyKills;
        }

        public KillCounterData(KillCounterData killCounter)
        {
            EnemyKills = new(killCounter.EnemyKills);
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
