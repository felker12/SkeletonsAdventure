using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Quests
{
    internal class LevelRequirements
    {
        public int Level { get; set; } = 0;
        public int Defence { get; set; } = 0;
        public int Attack { get; set; } = 0;

        public LevelRequirements()
        {
        }

        public LevelRequirements(LevelRequirements requirements)
        {
            Level = requirements.Level;
            Defence = requirements.Defence;
            Attack = requirements.Attack;
        }

        public LevelRequirements(RequirementData data)
        {
            Level = data.Level;
            Defence = data.Defence;
            Attack = data.Attack;
        }

        public LevelRequirements Clone()
        {
            return new LevelRequirements(this);
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
