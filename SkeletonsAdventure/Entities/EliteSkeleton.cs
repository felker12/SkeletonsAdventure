using RpgLibrary.EntityClasses;

namespace SkeletonsAdventure.Entities
{
    internal class EliteSkeleton : Skeleton
    {
        public EliteSkeleton(EnemyData data) : base(data)
        {
            EnemyClass = EnemyClass.Elite;
            DefaultColor = new Color(Color.Black, 255);
            SpriteColor = DefaultColor;
        }

        public override EliteSkeleton Clone()
        {
            EliteSkeleton skeleton = new(ToData())
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
