using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Entities
{
    internal class SkeletonMage : Skeleton
    {
        public SkeletonMage(EnemyData data) : base(data)
        {
            Initialize();
        }

        private void Initialize()
        {
            //TODO
            Texture = GameManager.SkeletonMageTexture;
            SetFrames(6, 64, 64, paddingX: 36, xOffset: 20, yOffset: 8, paddingY: 14);
            BasicAttackColor = Color.DarkBlue; //TODO
        }

        public override SkeletonMage Clone()
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
