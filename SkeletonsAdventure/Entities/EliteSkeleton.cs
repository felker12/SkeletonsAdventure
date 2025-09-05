using RpgLibrary.EntityClasses;

namespace SkeletonsAdventure.Entities
{
    internal class EliteSkeleton : Skeleton
    {
        public EliteSkeleton(EnemyData data) : base(data)
        {
            IsElite = true;
        }

        public override EliteSkeleton Clone()
        {
            EliteSkeleton skeleton = new(GetEntityData())
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                Level = Level,
                SpriteColor = SpriteColor,
                DefaultColor = DefaultColor,
            };
            return skeleton;
        }
    }
}
