using RpgLibrary.AttackData;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    internal class FireWave : ShootingAttack
    {
        public FireWave(AttackData attackData, Texture2D texture, Entity source = null) : base(attackData, texture, source)
        {
            Initialize();
        }

        public FireWave(ShootingAttack attack) : base(attack)
        {
            Initialize();
        }

        private void Initialize()
        {
            SetFrames(1, 64, 64, order: [AnimationKey.Right]);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override FireWave Clone()
        {
            return new FireWave(this);
        }

        public override void Offset()
        {
            AttackOffset = new(-Source.Width / 2 , 0);
        }
    }
}
