using Microsoft.Xna.Framework;
using RpgLibrary.DataClasses;

namespace RpgLibrary.AttackData
{
    public class AttackData
    {
        public int AttackLength { get; set; } = 1500; //total length of time the attack goes for
        public TimeSpan StartTime { get; set; } //time when the attack starts
        public TimeSpan Duration { get; set; } //how long the attack has lasted so far
        public Vector2 AttackOffset { get; set; } //how much to offset the attack from the source by
        public TimeSpan LastAttackTime { get; set; } //when the attack was used last
        public int AttackCoolDownLength { get; set; } = 3000; //how long between uses of the attack in milliseconds
        public float Speed { get; set; } = 2; //how fast the attack moves. if it moves
        public float DamageModifier { get; set; } = 1; 
        public int ManaCost { get; set; } = 1;
        public int AttackDelay { get; set; } = 0; //how long from when the attack is used until the attack happens
        public LevelRequirementData LevelRequirement { get; set; } = new();
        public List<string> SkillRequirementsNames { get; set; } = new();

        public AttackData() { }

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
                $"AttackDelay: {AttackDelay}, " + 
                $"Level Requirements: {LevelRequirement}, " +
                $"Skill Requirements: {string.Join(";", SkillRequirementsNames.Select(item => item.ToString()))}";
        }
    }
}
