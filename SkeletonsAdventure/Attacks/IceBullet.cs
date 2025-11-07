using RpgLibrary.AttackData;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    internal class IceBullet : ShootingAttack
    {
        public IceBullet(AttackData attackData, Texture2D texture, Entity source = null) : base(attackData, texture, source)
        {
            Initialize();
        }

        protected IceBullet(IceBullet attack) : base(attack)
        {
            Initialize();
        }

        private void Initialize()
        {
            SetFrames(1, 32, 32, order: [AnimationKey.Right]);
        }

        public override IceBullet Clone()
        {
            return new IceBullet(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SetRotationBasedOffMotion();
        }

        public override void Offset()
        {
            //start the attack at the center of the entity
            AttackOffset = new(Source.Width / 2 - Width / 2, Source.Height / 2 - Height / 2);
        }
    }
}
