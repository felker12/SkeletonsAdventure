using RpgLibrary.AttackData;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Shapes;

namespace SkeletonsAdventure.Attacks
{
    internal class TriangleAttack : BasicAttack
    {
        public Triangle Triangle { get; set; } = new(Color.Red);
        public List<Line> Lines { get; set; } = [];
        public List<Triangle> Triangles { get; set; } = [];

        float spreadAngle = 0f;
        readonly int count = 4;
        readonly int distance = 140;
        readonly float spreadSpeed = 1f; // radians/sec
        readonly float maxSpread;

        public TriangleAttack(AttackData attackData, Texture2D texture, Entity source = null) : base(attackData, texture, source)
        {
            SetFrames(1, 32, 32, order: [AnimationKey.Right]);
            maxSpread = (MathHelper.TwoPi / count) * .6f;
        }

        protected TriangleAttack(BasicAttack attack) : base(attack)
        {
            SetFrames(1, 32, 32, order: [AnimationKey.Right]);
            maxSpread = (MathHelper.TwoPi / count) * .6f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach(var triangle in Triangles)
            {
                ShapeDrawer.DrawTriangle(triangle, spriteBatch);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateTriangles(gameTime);
        }

        public override bool Hits(Entity entity)
        {
            foreach (var triangle in Triangles)
            {
                if (triangle.Intersects(entity.Rectangle))
                {
                    Debug.WriteLine("intersects");
                    return true;
                }
            }

            return false;
        }

        private void UpdateTriangles(GameTime gameTime)
        {
            Vector2 position = Source.Center;
            spreadAngle = MathF.Min(maxSpread, spreadAngle + (float)gameTime.ElapsedGameTime.TotalSeconds * spreadSpeed);

            Triangles.Clear();

            float step = MathHelper.TwoPi / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * step;

                Vector2 dirA = new(MathF.Cos(angle), MathF.Sin(angle));
                Vector2 dirB = new(MathF.Cos(angle + spreadAngle), MathF.Sin(angle + spreadAngle));

                Vector2 p2 = position + dirA * distance;
                Vector2 p3 = position + dirB * distance;

                Triangles.Add(new Triangle(position, p2, p3, Color.Red));
            }
        }

        public override void DrawIcon(SpriteBatch spriteBatch, Vector2 position, int size = 32, Color tint = default)
        {
            base.DrawIcon(spriteBatch, position, size, tint);
        }

        public override TriangleAttack Clone()
        {
            return new(this);
        }


    }
}
