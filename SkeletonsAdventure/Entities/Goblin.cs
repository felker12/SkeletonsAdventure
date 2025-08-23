using RpgLibrary.EntityClasses;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Entities
{
    internal class Goblin : Enemy
    {
        public Goblin(EnemyData data) : base(data)
        {
            Initialize();
        }

        public Goblin() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            Texture = GameManager.GoblinTexture;
            SetFrames(4, 23, 40);
            BasicAttackColor = Color.DarkGreen;
            EnemyType = EnemyType.Goblin;
        }

        public override Goblin Clone()
        {
            Goblin goblin = new(GetEntityData())
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                SpriteColor = this.SpriteColor,
                DefaultColor = this.DefaultColor,
            };
            return goblin;
        }
    }
}
