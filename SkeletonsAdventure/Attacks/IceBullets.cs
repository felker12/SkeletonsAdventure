
using RpgLibrary.AttackData;

namespace SkeletonsAdventure.Attacks
{
    internal class IceBullets : MultiShotAttack
    {
        public IceBullets(MultiShotAttackData data, Texture2D iconImage) : base(data, iconImage)
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
