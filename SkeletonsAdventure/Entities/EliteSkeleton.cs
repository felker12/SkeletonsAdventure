using RpgLibrary.EntityClasses;

namespace SkeletonsAdventure.Entities
{
    internal class EliteSkeleton : Skeleton
    {
        public EliteSkeleton(EnemyData data, EntityAssets assets) : base(data, assets)
        {
            EnemyClass = EnemyClass.Elite;
            DefaultColor = new Color(Color.Black, 255);
            SpriteColor = DefaultColor;
        }

        public override EliteSkeleton Clone()
        {
            EliteSkeleton skeleton = new(ToData(), Assets)
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
