using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Entities
{
    internal class SkeletonWarrior : Skeleton
    {
        public SkeletonWarrior(EnemyData data) : base(data)
        {
            Initialize();
        }

        private void Initialize()
        {
            //TODO
            Texture = GameManager.SkeletonWarriorTexture;
            SetFrames(5, 64, 64, order: [AnimationKey.Down, AnimationKey.Left, AnimationKey.Up, AnimationKey.Right],
                paddingX: 15, xOffset: 10); 
            BasicAttackColor = Color.DarkGray; //TODO
        }

        public override SkeletonWarrior Clone()
        {
            return new(GetEntityData())
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                SpriteColor = SpriteColor,
                DefaultColor = DefaultColor,
            };
        }
    }
}
