namespace RpgLibrary.QuestClasses
{
    public class LevelRequirementData
    {
        public int Level { get; set; } = 0;
        public int Defence { get; set; } = 0;
        public int Attack { get; set; } = 0;

        public LevelRequirementData() { }

        public LevelRequirementData(LevelRequirementData data)
        {
            Level = data.Level;
            Defence = data.Defence;
            Attack = data.Attack;
        }

        public LevelRequirementData Clone()
        {
            return new LevelRequirementData(this);
        }

        public override string ToString()
        {
            return $"Level: {Level}, " +
                $"Defence: {Defence}, " +
                $"Attack: {Attack}";
        }
    }
}
