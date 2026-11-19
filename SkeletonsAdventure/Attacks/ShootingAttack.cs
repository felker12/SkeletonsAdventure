using MonoGame.Extended;
using RpgLibrary.AttackData;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    public class ShootingAttack : BasicAttack
    {
        public override Rectangle HitBounds
        {
            get
            {
                var bounds = DamageHitBox;
                foreach (var segment in MovementSegments)
                {
                    int x = (int)MathF.Floor(Math.Min(segment.From.X, segment.To.X));
                    int y = (int)MathF.Floor(Math.Min(segment.From.Y, segment.To.Y));
                    bounds = Rectangle.Union(bounds, new Rectangle(x, y,
                        (int)MathF.Ceiling(Math.Max(segment.From.X, segment.To.X) + Width) - x,
                        (int)MathF.Ceiling(Math.Max(segment.From.Y, segment.To.Y) + Height) - y));
                }
                return bounds;
            }
        }

        public override bool HitsDuringFrame(Entity entity)
        {
            if (Hits(entity))
                return true;
            foreach (var segment in MovementSegments)
                if (SweptHit(segment.From, segment.To, Width, Height, entity.Rectangle))
                    return true;
            return false;
        }

        internal static bool SweptHit(Vector2 from, Vector2 to, int width, int height, Rectangle target)
        {
            float enter = 0, exit = 1;
            var delta = to - from;
            bool Axis(float start, float motion, float min, float max)
            {
                if (motion == 0)
                    return start > min && start < max;
                float a = (min - start) / motion, b = (max - start) / motion;
                enter = Math.Max(enter, Math.Min(a, b));
                exit = Math.Min(exit, Math.Max(a, b));
                return enter < exit;
            }
            return Axis(from.X, delta.X, target.Left - width, target.Right) &&
                Axis(from.Y, delta.Y, target.Top - height, target.Bottom) && enter < 1 && exit > 0;
        }
        public List<Vector2> PathPoints { get; set; } = [];

        public ShootingAttack(AttackData attackData, Texture2D texture, SpriteFont infoFont, Entity source = null) : base(attackData, texture, infoFont, source)
        {
        }

        protected ShootingAttack(ShootingAttack attack) : base(attack)
        {
        }

        public override ShootingAttack Clone()
        {
            return new ShootingAttack(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            DrawPath(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Duration.TotalMilliseconds % 50 < 1)
                PathPoints.Add(Center);
        }

        private void DrawPath(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < PathPoints.Count - 1; i++)
                spriteBatch.DrawLine(PathPoints[i], PathPoints[i + 1], Color.Aquamarine, 1);
        }

        public override void Offset()
        {
            //start the attack at the center of the entity
            AttackOffset = new(Source.Width / 2 - Width / 2, Source.Height / 2 - Height / 2);
        }

        //TODO
        public virtual void MoveInDirectionOfPosition(Vector2 target)
        {
            var direction = target - Center;
            Motion = direction == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(direction);
            InitialMotion = Motion;
        }

        public void SetRotationBasedOffMotion()
        {
            // Angle in radians
            float angleRadians = (float)Math.Atan2(Motion.Y, Motion.X);
            //Previous angle
            float PreviousRotationAngle = RotationAngle;

            //if the angle changes and is stopped by something keep the rotation the same
            if (angleRadians == 0 && PreviousRotationAngle != 0)
            {
                if (Motion != Vector2.Zero)
                    RotationAngle = angleRadians;
            }
            else
                RotationAngle = angleRadians;

            if (RotationAngle != angleRadians)
                RotationAngle = PreviousRotationAngle;
        }
    }
}
