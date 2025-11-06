namespace RpgLibrary.DataClasses
{
    public class KillRequirementData
    {
        public Dictionary<string, int> RequiredEnemyKills { get; private set; } = new();

        public KillRequirementData() { }

        public KillRequirementData(Dictionary<string, int> requiredEnemyKills)
        {
            RequiredEnemyKills = requiredEnemyKills;
        }

        public KillRequirementData(KillRequirementData killRequirements)
        {
            RequiredEnemyKills = new(killRequirements.RequiredEnemyKills);
        }

        public override string ToString()
        {
            string result = "Kill Requirements:\n";
            foreach (var (enemy, count) in RequiredEnemyKills)
            {
                result += $"{enemy}: {count}\n";
            }

            return result;
        }
    }
}
