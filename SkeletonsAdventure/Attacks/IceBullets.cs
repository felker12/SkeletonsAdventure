
using RpgLibrary.AttackData;

namespace SkeletonsAdventure.Attacks
{
    internal class IceBullets : MultiShotAttack
    {
        public IceBullets(MultiShotAttackData data, Texture2D iconImage, ShootingAttack baseAttack) : base(data, iconImage, baseAttack)
        {
        }

        protected IceBullets(IceBullets attack) : base(attack)
        {
        }

        public override IceBullets Clone()
        {
            return new IceBullets(this);
        }
    }
}
