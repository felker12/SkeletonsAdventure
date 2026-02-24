using SkeletonsAdventure.Attacks;
using RpgLibrary.AttackData;
using System.Linq;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Entities.PlayerClasses
{
    public class LearnedAttackManager(Player player)
    {
        public Player Player { get; init; } = player;
        public Dictionary<string, BasicAttack> LearnedAttacks { get; private set; } = [];

        public void LearnAttack(string attackName, BasicAttack attack)
        {
            if (CheckRequirements(attack) is false)
                return;

            //add the skill to the learned skills if the requirements are met
            if (LearnedAttacks.ContainsKey(attackName) is false)
                LearnedAttacks[attackName] = attack;
        }

        public bool CheckRequirements(BasicAttack attack)
        {
            //currently check both instead of using the commented out line to get the debug info provided by both methods
            bool levelsMet = CheckAttackLevelRequirements(attack);
            bool skillsMet = CheckAttackSkillRequirements(attack);

            return levelsMet && skillsMet;

            //return CheckAttackLevelRequirements(attack) && CheckAttackSkillRequirements(attack);
        }

        private bool CheckAttackLevelRequirements(BasicAttack attack)
        {
            if(attack.LevelRequirements is null) // No level requirements
                return true;

            // Check level requirements
            if(attack.LevelRequirements.CheckRequirements(Player))
                return true;
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
                    Debug.WriteLine($"Cannot learn attack {attack.Name}. Missing required skill: {skill}");

                return false;
            }

            return true;
        }

        public bool Contains(string attackName)
        {
            if(attackName is null)
                return false;

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

        public LearnedAttackManagerData ToData()
        {
            return new()
            {
                LearnedAttackNames = [.. LearnedAttacks.Keys],
            };
        }

        public void UpdateWithData(LearnedAttackManagerData data)
        {
            LearnedAttacks = [];

            // Resolve attacks from the GameManager attack templates without
            // using GetAttackByName to avoid creating an extra clone. Use the
            // EntityAttackClone dictionary which provides constructed attack
            // instances suitable for assigning to the player's learned attacks.
            foreach (string name in data.LearnedAttackNames)
            {
                if (string.IsNullOrEmpty(name))
                    continue;

                if (GameManager.EntityAttackClone.TryGetValue(name, out BasicAttack attack))
                {
                    LearnedAttacks[name] = attack;
                }
                else
                {
                    // If not found, skip or optionally log for debugging
                    Debug.WriteLine($"Learned attack not found in GameManager: {name}");
                }
            }
        }
    }
}
