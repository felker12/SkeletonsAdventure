using RpgLibrary.EntityClasses;

namespace SkeletonsAdventure.Entities
{
    internal class SkeletonMage : Skeleton
    {
        public SkeletonMage(EnemyData data, EntityAssets assets) : base(data, assets)
        {
            Initialize();
        }

        private void Initialize()
        {
            //TODO
            SetFrames(6, 64, 64, paddingX: 36, xOffset: 20, yOffset: 8, paddingY: 14);
            BasicAttackColor = new(11,29,131); //TODO
        }

        public override SkeletonMage Clone()
        {
            return new(ToData(), Assets)
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                SpriteColor = SpriteColor,
                DefaultColor = DefaultColor,
            };
        }
    }
}
