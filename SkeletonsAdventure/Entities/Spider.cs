using RpgLibrary.EntityClasses;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Entities
{
    internal class Spider : Enemy
    {
        public Spider(EnemyData data) : base(data)
        {
            Initialize();
        }

        public Spider() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            Texture = GameManager.SpiderTexture;
            SetFrames(6, 32, 32);
            BasicAttackColor = Color.Gray;
            EnemyType = EnemyType.Spider;
        }

        public override Spider Clone()
        {
            Spider spider = new(GetEntityData())
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                SpriteColor = this.SpriteColor,
                DefaultColor = this.DefaultColor,
            };
            return spider;
        }
    }
}
