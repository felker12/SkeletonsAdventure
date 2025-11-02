using RpgLibrary.EntityClasses;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Entities
{
    internal class Skeleton : Enemy
    {
        public Skeleton(EnemyData data) : base(data)
        {
            EnemyType = EnemyType.Skeleton;
            GuaranteedDrops.Add(GameManager.GetItemByName("Bones"));
        }

        public override Skeleton Clone()
        {
            return new(ToData())
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                SpriteColor = SpriteColor,
                DefaultColor = DefaultColor,
           };
        }
    }
}
