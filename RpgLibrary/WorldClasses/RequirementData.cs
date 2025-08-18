
namespace RpgLibrary.WorldClasses
{
    public class RequirementData
    {
        public int Level { get; set; } = 0;
        public int Defence { get; set; } = 0;
        public int Attack { get; set; } = 0;

        public RequirementData() { }

        public RequirementData(RequirementData data)
        {
            Level = data.Level;
            Defence = data.Defence;
            Attack = data.Attack;
        }

        public RequirementData clone()
        {
            return new RequirementData(this);
        }

        public override string ToString()
        {
            return $"Level: {Level}, " +
                $"Defence: {Defence}, " +
                $"Attack: {Attack}";
        }
    }
}
