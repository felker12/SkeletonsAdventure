using RpgLibrary.AttackData;
using SkeletonsAdventure.Attacks;

namespace SkeletonsAdventure.LibraryClasses
{
    internal class AttackLibrary
    {
        //Attacks
        public AttackData BasicAttackData { get; set; }
        public AttackData FireBallData { get; set; }
        public AttackData IcePillarData { get; set; }
        public AttackData IceBulletData { get; set; }
        public MultiShotAttackData IceBulletsData { get; set; }
        public AttackData WaterBallData { get; set; }
        public AttackData FireWallData { get; set; }
        public AttackData BlueFireWallData { get; set; }
        public AttackData TriangleAttackData { get; set; }
        public AttackData SpinningTriangleAttackData { get; set; }

        public Dictionary<string, BasicAttack> EntityAttacks { get; private set; } = [];

        public AttackLibrary(ContentManager content, TexturesLibrary textures)
        {
            LoadAttacks(content);

            EntityAttacks = CreateAttacks(textures);
        }

        private void LoadAttacks(ContentManager content)
        {
            BasicAttackData = content.Load<AttackData>(@"AttackData/BasicAttack");

            FireBallData = content.Load<AttackData>(@"AttackData/FireBall");
            FireWallData = content.Load<AttackData>(@"AttackData/FireWall");
            BlueFireWallData = content.Load<AttackData>(@"AttackData/BlueFireWall");

            IcePillarData = content.Load<AttackData>(@"AttackData/IcePillar");
            IceBulletData = content.Load<AttackData>(@"AttackData/IceBullet");
            IceBulletsData = content.Load<MultiShotAttackData>(@"AttackData/IceBullets");

            WaterBallData = content.Load<AttackData>(@"AttackData/WaterBall");

            TriangleAttackData = content.Load<AttackData>(@"AttackData/TriangleAttack");
            SpinningTriangleAttackData = content.Load<AttackData>(@"AttackData/SpinningTriangleAttack");
        }

        internal Dictionary<string, BasicAttack> CreateAttacks(TexturesLibrary textures)
        {
            Dictionary<string, BasicAttack> entityAttacks = [];

            //Create the attacks from the content folder
            BasicAttack attack = new(BasicAttackData, textures.SkeletonAttackTexture);
            entityAttacks.Add(attack.GetType().Name, attack);

            //Fire attacks
            FireBall fireball = new(FireBallData, textures.FireBallTexture2);
            entityAttacks.Add(fireball.GetType().Name, fireball);

            //FireWave fireWave = new(FireWallData, FireWallTexture);
            FireWave fireWave = new(FireWallData, textures.FireWallSpriteSheetTexture);
            entityAttacks.Add(fireWave.GetType().Name, fireWave);

            //BlueFireWave blueFireWave = new(BlueFireWallData, BlueFireWallTexture);
            BlueFireWave blueFireWave = new(BlueFireWallData, textures.BlueFireWallSpriteSheetTexture);
            entityAttacks.Add(blueFireWave.GetType().Name, blueFireWave);

            //Ice attacks
            IcePillar icePillar = new(IcePillarData, textures.IcePillarSpriteSheetTexture);
            entityAttacks.Add(icePillar.GetType().Name, icePillar);

            IceBullet iceBullet = new(IceBulletData, textures.IceBulletTexture);
            entityAttacks.Add(iceBullet.GetType().Name, iceBullet);

            IceBullets iceBullets = new(IceBulletsData, textures.IceBulletsTexture, iceBullet);
            entityAttacks.Add(iceBullets.GetType().Name, iceBullets);

            //Water attacks
            WaterBall waterBall = new(WaterBallData, textures.WaterBallSpriteSheetTexture);
            entityAttacks.Add(waterBall.GetType().Name, waterBall);

            //Non elemental attacks
            TriangleAttack triangleAttack = new(TriangleAttackData, textures.TriangleAttackTexture);
            entityAttacks.Add(triangleAttack.GetType().Name, triangleAttack);

            SpinningTriangleAttack spinningTriangleAttack = new(SpinningTriangleAttackData, textures.SpinningTriangleAttackTexture);
            entityAttacks.Add(spinningTriangleAttack.Name, spinningTriangleAttack);

            return entityAttacks;
        }
    }
}
