using RpgLibrary.EntityClasses;

namespace SkeletonsAdventure.Entities
{
    internal class Goblin : Enemy
    {
        public Goblin(EnemyData data, EntityAssets assets) : base(data, assets)
        {
            Initialize();
        }

        public Goblin(EntityAssets assets) : base(assets)
        {
            Initialize();
        }

        private void Initialize()
        {
            SetFrames(4, 23, 40);
            BasicAttackColor = Color.DarkGreen;
            EnemyType = EnemyType.Goblin;
        }

        public override Goblin Clone()
        {
            return CopyEnemyTo(new Goblin(ToData(), Assets));
        }
    }
}
