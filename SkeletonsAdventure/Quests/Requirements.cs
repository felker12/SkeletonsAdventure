using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Quests
{
    public class Requirements
    {
        public int Level { get; set; } = 0;
        public int Defence { get; set; } = 0;
        public int Attack { get; set; } = 0;

        public Requirements()
        {
        }

        public Requirements(Requirements requirements)
        {
            Level = requirements.Level;
            Defence = requirements.Defence;
            Attack = requirements.Attack;
        }

        public Requirements(RequirementData data)
        {
            Level = data.Level;
            Defence = data.Defence;
            Attack = data.Attack;
        }

        public Requirements Clone()
        {
            return new Requirements(this);
        }

        internal bool CheckRequirements(Player player)
        {
            return CheckRequirements(player.Level, player.Defence, player.Attack);
        }

        public bool CheckRequirements(int level, int defence, int attack)
        {
            return level >= Level && defence >= Defence && attack >= Attack;
        }

        public int GetTotalRequirements()
        {
            return Level + Defence + Attack;
        }

        public RequirementData GetRequirementData()
        {
            return new RequirementData
            {
                Level = Level,
                Defence = Defence,
                Attack = Attack
            };
        }

        public override string ToString()
        {
            return $"Level: {Level}, " +
                   $"Defence: {Defence}, " +
                   $"Attack: {Attack}";
        }
    }
}
