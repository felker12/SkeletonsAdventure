using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Animations;
using RpgLibrary.AttackData;

namespace SkeletonsAdventure.Attacks
{
    internal class FireBall : ShootingAttack
    {
        public FireBall(AttackData attackData, Texture2D texture, Entity source) : base(attackData, texture, source)
        {
            Initialize();
        }

        public FireBall(FireBall attack) : base(attack)
        {
            Initialize(); 
        }

        private void Initialize()
        {
            Width = 32;
            Height = 28;

            AnimatedAttack = true;

            SetFrames(3, Width, Height, 0, Height); 
        }

        public override FireBall Clone()
        {
            return new FireBall(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        //TODO Overide this with the corret offset parameters based on the type of the entity calling the method 
        public override void Offset()
        {
            //start the attack at the center of the entity
            AttackOffset = new(Source.Width / 2 - Width / 2, Source.Height / 2 - Height / 2);
        }
    }
}
