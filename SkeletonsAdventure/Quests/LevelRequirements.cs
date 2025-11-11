using RpgLibrary.DataClasses;
using SkeletonsAdventure.Entities.PlayerClasses;

namespace SkeletonsAdventure.Quests
{
    internal class LevelRequirements
    {
        public int Level { get; set; } = 0;
        public int Defence { get; set; } = 0;
        public int Attack { get; set; } = 0;
        public int Mana { get; set; } = 0;

        public LevelRequirements()
        {
        }

        public LevelRequirements(LevelRequirements requirements)
        {
            Level = requirements.Level;
            Defence = requirements.Defence;
            Attack = requirements.Attack;
            Mana = requirements.Mana;
        }

        public LevelRequirements(LevelRequirementData data)
        {
            if (data is null) 
                return;

            Level = data.Level;
            Defence = data.Defence;
            Attack = data.Attack;
            Mana = data.Mana;
        }

        public LevelRequirements Clone()
        {
            return new LevelRequirements(this);
        }

        public bool CheckRequirements(Player player)
        {
            return CheckRequirements(player.Level, player.DefenceExcludingEquipment, 
                player.AttackExcludingEquipment, player.ManaExcludingEquipment);
        }

        public bool CheckRequirements(int level, int defence, int attack, int mana)
        {
            return level >= Level && defence >= Defence && attack >= Attack && mana >= Mana;
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
                $"Attack: {Attack}, " +
                $"Mana: {Mana}";
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
            else
                previousRequirementMet = true;

            if(player.Mana < Mana)
            {
                if (previousRequirementMet is false)
                    result += ", ";

                result += $"Mana: {player.ManaExcludingEquipment}/{Mana}";
            }

            return result;
        }
    }
}
