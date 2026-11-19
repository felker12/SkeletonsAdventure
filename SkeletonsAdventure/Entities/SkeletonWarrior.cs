using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Animations;

namespace SkeletonsAdventure.Entities
{
    internal class SkeletonWarrior : Skeleton
    {
        public SkeletonWarrior(EnemyData data, EntityAssets assets) : base(data, assets)
        {
            Initialize();
        }

        private void Initialize()
        {
            //TODO
            SetFrames(5, 64, 64, order: [AnimationKey.Down, AnimationKey.Left, AnimationKey.Up, AnimationKey.Right],
                paddingX: 15, xOffset: 10); 
            BasicAttackColor = Color.DarkGray; //TODO
        }

        public override SkeletonWarrior Clone()
        {
            return CopyEnemyTo(new SkeletonWarrior(ToData(), Assets));
        }
    }
}
