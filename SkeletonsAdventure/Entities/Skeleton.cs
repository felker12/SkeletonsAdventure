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
            return CopyEnemyTo(new Skeleton(ToData(), Assets));
        }
    }
}
