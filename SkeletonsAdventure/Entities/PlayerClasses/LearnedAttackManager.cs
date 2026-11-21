using SkeletonsAdventure.Attacks;
using RpgLibrary.AttackData;
using System.Linq;

namespace SkeletonsAdventure.Entities.PlayerClasses
{
    public class LearnedAttackManager(Player player, IReadOnlyDictionary<string, BasicAttack> attackTemplates)
    {
        public Player Player { get; init; } = player;
        private readonly IReadOnlyDictionary<string, BasicAttack> _attackTemplates =
            attackTemplates ?? throw new ArgumentNullException(nameof(attackTemplates));
        public Dictionary<string, BasicAttack> LearnedAttacks { get; private set; } = [];

        public void LearnAttack(string attackName, BasicAttack attack)
        {
            if (CheckRequirements(attack) is false)
                return;

            //add the skill to the learned skills if the requirements are met
            if (LearnedAttacks.ContainsKey(attackName) is false)
                LearnedAttacks[attackName] = attack;
        }

        public bool CheckRequirements(BasicAttack attack) =>
            (attack.LevelRequirements?.CheckRequirements(Player) ?? true) &&
            (attack.SkillRequirementsNames?.All(Contains) ?? true);

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
            LearnedAttacks.Clear();

            foreach (string name in data.LearnedAttackNames)
            {
                if (string.IsNullOrEmpty(name))
                    continue;

                if (_attackTemplates.TryGetValue(name, out BasicAttack attack))
                {
                    LearnedAttacks[name] = attack.Clone();
                    LearnedAttacks[name].Source = Player;
                }
                else
                {
                    throw new System.IO.InvalidDataException($"Learned attack template not found: {name}");
                }
            }
        }
    }
}
