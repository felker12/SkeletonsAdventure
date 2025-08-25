using MonoGame.Extended;
using RpgLibrary.AttackData;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    internal class ShootingAttack : BasicAttack
    {
        public List<Vector2> PathPoints { get; set; } = [];

        public ShootingAttack(AttackData attackData, Texture2D texture, Entity source) : base(attackData, texture, source)
        {
        }

        public ShootingAttack(ShootingAttack attack) : base(attack)
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

        //TODO Overide this with the corret offset parameters based on the type of the entity calling the method 
        public override void Offset()
        {
            Width = 32;
            Height = 28;
            Frame = new(0, 0, Width, Height);

            //start the attack at the center of the entity
            AttackOffset = new(Source.Width / 2 - Width / 2, Source.Height / 2 - Height / 2);
        }

        //TODO
        public void MoveInPositionDirection(Vector2 target)
        {
            Motion = Vector2.Normalize(target - Center) * Speed;
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
