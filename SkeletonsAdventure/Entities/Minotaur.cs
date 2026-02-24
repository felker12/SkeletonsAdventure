using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.GameWorld;


namespace SkeletonsAdventure.Entities
{
    internal class Minotaur : Enemy
    {
        public Minotaur(EnemyData data) : base(data)
        {
            Initialize();
        }

        public Minotaur() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            Texture = GameManager.MinotaurTexture;
            SetFrames(6, 96, 96, order: [AnimationKey.Down, AnimationKey.Right, AnimationKey.Up, AnimationKey.Left]);
            EnemyType = EnemyType.Humanoid;
        }

        public override Minotaur Clone()
        {
            Minotaur minotaur = new(ToData())
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                SpriteColor = SpriteColor,
                DefaultColor = DefaultColor,
            };
            return minotaur;
        }
    }
}
