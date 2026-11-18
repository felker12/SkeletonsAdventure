using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Animations;


namespace SkeletonsAdventure.Entities
{
    internal class Minotaur : Enemy
    {
        public Minotaur(EnemyData data, EntityAssets assets) : base(data, assets)
        {
            Initialize();
        }

        public Minotaur(EntityAssets assets) : base(assets)
        {
            Initialize();
        }

        private void Initialize()
        {
            SetFrames(6, 96, 96, order: [AnimationKey.Down, AnimationKey.Right, AnimationKey.Up, AnimationKey.Left]);
            EnemyType = EnemyType.Humanoid;
        }

        public override Minotaur Clone()
        {
            Minotaur minotaur = new(ToData(), Assets)
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
