using RpgLibrary.EntityClasses;

namespace SkeletonsAdventure.Entities
{
    internal class Skeleton : Enemy
    {
        public Skeleton(EnemyData data, EntityAssets assets) : base(data, assets)
        {
            EnemyType = EnemyType.Skeleton;
        }

        public override Skeleton Clone()
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
