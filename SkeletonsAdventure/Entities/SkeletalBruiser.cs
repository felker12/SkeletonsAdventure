using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Animations;

namespace SkeletonsAdventure.Entities
{
    internal class SkeletalBruiser : Skeleton
    {
        public SkeletalBruiser(EnemyData data, EntityAssets assets) : base(data, assets)
        {
            Initialize();
        }

        private void Initialize()
        {
            SetFrames(4, 48, 48, paddingX: 20, order: [AnimationKey.Up, AnimationKey.Right, AnimationKey.Down, AnimationKey.Left]);
            BasicAttackColor = Color.Bisque;
            EnemyType = EnemyType.Skeleton;
        }

        public override SkeletalBruiser Clone()
        {
            SkeletalBruiser skeletalBruiser = new(ToData(), Assets)
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                SpriteColor = this.SpriteColor,
                DefaultColor = this.DefaultColor,
            };
            return skeletalBruiser;
        }
    }
}
