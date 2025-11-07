
using RpgLibrary.AttackData;

namespace SkeletonsAdventure.Attacks
{
    internal class IceBullets : MultiShotAttack
    {
        public IceBullets(IceBullet attack) : base(attack)
        {
        }

        public IceBullets(IceBullet attack, int shotCount, TimeSpan shotInterval) : base(attack, shotCount, shotInterval)
        {
        }

        protected IceBullets(IceBullets attack) : base(attack)
        {
        }

        public IceBullets(MultiShotAttackData data) : base(data)
        {

        }

        public override IceBullets Clone()
        {
            return new IceBullets(this);
        }
    }
}
