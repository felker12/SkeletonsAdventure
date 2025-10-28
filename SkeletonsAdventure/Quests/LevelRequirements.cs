using RpgLibrary.QuestClasses;
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

        public LevelRequirements(LevelRequirementData data)
        {
            if (data is null) return;

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
            return CheckRequirements(player.Level, player.DefenceExcludingEquipment, player.AttackExcludingEquipment);
        }

        public bool CheckRequirements(int level, int defence, int attack)
        {
            return level >= Level && defence >= Defence && attack >= Attack;
        }

        public int GetTotalRequirements()
        {
            return Level + Defence + Attack;
        }

        public LevelRequirementData ToData()
        {
            return new()
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

        public string MissingRequirementsText(Player player)
        {
            string result = "Requirements not met: ";
            bool previousRequirementMet;

            if (player.Level < Level)
            {
                result += $"Level: {player.Level}/{Level}";
                previousRequirementMet = false;

            }
            else
                previousRequirementMet = true;

            if (player.Defence < Defence)
            {
                if (previousRequirementMet is false)
                    result += ", ";

                result += $"Defence: {player.DefenceExcludingEquipment}/{Defence}";
                previousRequirementMet = false;
            }
            else
                previousRequirementMet = true;

            if (player.Attack < Attack)
            {
                if (previousRequirementMet is false)
                    result += ", ";

                result += $"Attack: {player.AttackExcludingEquipment}/{Attack}";
            }

            return result;
        }
    }
}
