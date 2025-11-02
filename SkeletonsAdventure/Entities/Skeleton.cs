using RpgLibrary.EntityClasses;

namespace SkeletonsAdventure.Entities
{
    internal class Skeleton : Enemy
    {
        public Skeleton(EnemyData data) : base(data)
        {
            EnemyType = EnemyType.Skeleton;
        }

        public override Skeleton Clone()
        {
            Skeleton skeleton = new(ToData())
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                SpriteColor = SpriteColor,
                DefaultColor = DefaultColor,
           };
            return skeleton;
        }
    }
}
