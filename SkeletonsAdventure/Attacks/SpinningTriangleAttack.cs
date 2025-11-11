using RpgLibrary.AttackData;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Shapes;

namespace SkeletonsAdventure.Attacks
{
    internal class SpinningTriangleAttack : TriangleAttack
    {
        protected float rotateAngle = 0f;
        protected readonly float rotateSpeed = .8f; // radians/sec adjust until looks 
        private readonly List<float> baseAngles = [];

        public SpinningTriangleAttack(AttackData attackData, Texture2D texture, Entity source = null) : base(attackData, texture, source)
        {
            Initialize();
        }

        protected SpinningTriangleAttack(BasicAttack attack) : base(attack)
        {
            Initialize();
        }

        private void Initialize()
        {
            count = 5;
            distance = 180;
            maxSpread = (MathHelper.TwoPi / count) * .3f;

            float step = MathHelper.TwoPi / count;
            for (int i = 0; i < count; i++)
                baseAngles.Add(i * step);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override SpinningTriangleAttack Clone()
        {
            return new(this);
        }

        protected override void UpdateTriangles(GameTime gameTime)
        {
            Triangles.Clear();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 center = Source.Center;

            if (spreadAngle < maxSpread)
                spreadAngle = MathF.Min(maxSpread, spreadAngle + dt * spreadSpeed);
            else
                rotateAngle += dt * rotateSpeed;

            for (int i = 0; i < count; i++)
            {
                float baseAngle = baseAngles[i] + rotateAngle;

                // widen out from the base angle
                Vector2 dirA = new(MathF.Cos(baseAngle), MathF.Sin(baseAngle));
                Vector2 dirB = new(MathF.Cos(baseAngle + spreadAngle), MathF.Sin(baseAngle + spreadAngle));

                Vector2 p2 = center + dirA * distance;
                Vector2 p3 = center + dirB * distance;

                Triangles.Add(new Triangle(center, p2, p3, Color.Red));
            }
        }
    }
}
