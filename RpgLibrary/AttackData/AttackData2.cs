using Microsoft.Xna.Framework;
using RpgLibrary.DataClasses;
using RpgLibrary.QuestClasses;

namespace RpgLibrary.AttackData
{
    public class AttackData2
    {
        public int AttackLength { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public Vector2 AttackOffset { get; set; }
        public TimeSpan LastAttackTime { get; set; }
        public int AttackCoolDownLength { get; set; }
        public float Speed { get; set; }
        public float DamageModifier { get; set; }
        public int ManaCost { get; set; }
        public int AttackDelay { get; set; }
        public LevelRequirementData LevelRequirement { get; set; } = new();
        public List<string> SkillRequirementsNames { get; set; } = new();

        public AttackData2() { }

        public AttackData2(AttackData data)
        {
            AttackLength = data.AttackLength;
            StartTime = data.StartTime;
            Duration = data.Duration;
            AttackOffset = data.AttackOffset;
            LastAttackTime = data.LastAttackTime;
            AttackCoolDownLength = data.AttackCoolDownLength;
            Speed = data.Speed;
            DamageModifier = data.DamageModifier;
            ManaCost = data.ManaCost;
            AttackDelay = data.AttackDelay;
        }

        public override string ToString()
        {
            return $"Attack Length: {AttackLength}, " +
                $"Start Time: {StartTime}, " +
                $"Duration: {Duration}, " +
                $"Attack Offset: {AttackOffset}, " +
                $"Last Attack Time: {LastAttackTime}, " +
                $"Attack Cool Down Length: {AttackCoolDownLength}, " +
                $"Speed: {Speed}, " +
                $"Damage Modifier: {DamageModifier}, " +
                $"Mana Cost: {ManaCost}, " +
                $"AttackDelay: {AttackDelay} ";
        }
    }
}
