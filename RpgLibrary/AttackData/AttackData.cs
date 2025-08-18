using Microsoft.Xna.Framework;

namespace RpgLibrary.AttackData
{
    public class AttackData
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
                $"AttackDelay: {AttackDelay} ";
        }
    }
}
