using RpgLibrary.EntityClasses;

namespace SkeletonsAdventure.Entities
{
    internal class Spider : Enemy
    {
        public Spider(EnemyData data, EntityAssets assets) : base(data, assets)
        {
            Initialize();
        }

        public Spider(EntityAssets assets) : base(assets)
        {
            Initialize();
        }

        private void Initialize()
        {
            SetFrames(6, 32, 32);
            BasicAttackColor = Color.Gray;
            EnemyType = EnemyType.Spider;
        }

        public override Spider Clone()
        {
            Spider spider = new(ToData(), Assets)
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                SpriteColor = SpriteColor,
                DefaultColor = DefaultColor,
            };
            return spider;
        }
    }
}
