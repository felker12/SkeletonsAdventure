using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    internal class PlayerLearnedAttackManager(Player player)
    {
        public Player Player { get; init; } = player;
        public Dictionary<string, BasicAttack> LearnedAttacks { get; private set; } = [];

        public void LearnAttack(string attackName, BasicAttack attack)
        {
            if (CheckAttackLevelRequirements(attack) is false)
                return;

            if (CheckAttackSkillRequirements(attack) is false)
                return;

            if (LearnedAttacks.ContainsKey(attackName) is false)
                LearnedAttacks[attackName] = attack;
        }

        private bool CheckAttackLevelRequirements(BasicAttack attack)
        {
            if(attack.LevelRequirements is null) // No level requirements
                return true;

            // Check level requirements
            if(attack.LevelRequirements.CheckRequirements(Player))
            {
                return true;
            }
            else
            {
                Debug.WriteLine(attack.LevelRequirements.MissingRequirementsText(Player));
                return false;
            }
        }

        private bool CheckAttackSkillRequirements(BasicAttack attack)
        {
            List<string> missingSkills = [];

            // Check skill requirements
            if (attack.SkillRequirementsNames is not null)
            { 
                foreach (string attackName in attack.SkillRequirementsNames)
                {
                    if(Contains(attackName) is false)
                        missingSkills.Add(attackName);
                }
            }

            if (missingSkills.Count > 0)
            {
                foreach (string skill in missingSkills)
                {
                    Debug.WriteLine($"Cannot learn attack {attack.Name}. Missing required skill: {skill}");
                }

                return false;
            }

            return true;
        }

        public bool Contains(string attackName)
        {
            return LearnedAttacks.ContainsKey(attackName);
        }

        public void Clear() 
        { 
            LearnedAttacks.Clear(); 
        }

        public override string ToString()
        {
            return $"Learned Attacks: {string.Join(", ", LearnedAttacks.Keys)}";
        }
    }
}
