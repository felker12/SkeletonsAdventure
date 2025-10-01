using MonoGame.Extended;
using RpgLibrary.AttackData;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    internal class BasicAttack : AnimatedSprite
    {
        public int AttackLength { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan LastAttackTime { get; set; }
        public Vector2 AttackOffset { get; set; }
        public Entity Source { get; set; }
        public int AttackCoolDownLength { get; protected set; } //length of the delay between attacks in milliseconds
        public float DamageModifier { get; set; }
        public int ManaCost { get; set; }
        public bool AnimatedAttack { get; set; } = false; //TODO
        public Rectangle DamageHitBox { get; set; }
        public int AttackDelay { get; set; }
        public bool AttackVisible { get; set; } = true;
        public Vector2 StartPosition { get; set; } = new();
        public Vector2 InitialMotion { get; set; }

        public BasicAttack(BasicAttack attack) : base()
        {
            Width = attack.Width;
            Height = attack.Height;
            Texture = attack.Texture;
            AttackLength = attack.AttackLength;
            StartTime = attack.StartTime;
            AttackOffset = attack.AttackOffset;
            Frame = attack.Frame;
            Source = attack.Source;
            Position = attack.Position;
            SpriteColor = attack.SpriteColor;
            Info.TextColor = attack.Info.TextColor;
            LastAttackTime = attack.LastAttackTime;
            AttackCoolDownLength = attack.AttackCoolDownLength;
            Motion = attack.Motion;
            Speed = attack.Speed;
            DamageModifier = attack.DamageModifier;
            ManaCost = attack.ManaCost;
            AttackDelay = attack.AttackDelay;
            AnimatedAttack = attack.AnimatedAttack;
            StartPosition = attack.StartPosition;
            DamageHitBox = attack.DamageHitBox;

            Initialize();
        }

        public BasicAttack(AttackData attackData, Texture2D texture, Entity source) : base()
        {
            AttackLength = attackData.AttackLength;
            StartTime = attackData.StartTime;
            Duration = attackData.Duration;
            AttackOffset = attackData.AttackOffset;
            LastAttackTime = attackData.LastAttackTime;
            AttackCoolDownLength = attackData.AttackCoolDownLength;
            Speed = attackData.Speed;
            DamageModifier = attackData.DamageModifier;
            ManaCost = attackData.ManaCost;
            AttackDelay = attackData.AttackDelay;

            Texture = texture;
            Source = source;

            Initialize();
        }
        
        private void Initialize()
        {
            DamageHitBox = new((int)Position.X, (int)Position.Y, Width, Height);

            if (AttackDelay > 0)
                AttackVisible = false;
        }

        public virtual BasicAttack Clone()
        {
            return new BasicAttack(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (AttackVisible is false)
            {
                //spriteBatch.Draw(GameManager.AttackAreaTexture, new Vector2(DamageHitBox.X, DamageHitBox.Y), DamageHitBox, Color.White * 0.5f); //TODO
            }

            if (AttackVisible)
            {
                spriteBatch.DrawRectangle(Rectangle, SpriteColor, 1, 0); //TODO
                spriteBatch.DrawRectangle(DamageHitBox, Color.OrangeRed, 1, 0); //TODO

                Vector2 FrameCenter = new(Frame.Width / 2, Frame.Height / 2);

                spriteBatch.Draw(Texture, Position + FrameCenter, Frame, SpriteColor, RotationAngle, FrameCenter, Scale, SpriteEffects.None, 1);
            }
        }

        public override void Update(GameTime gameTime)
        {
            DamageHitBox = new((int)Position.X, (int)Position.Y, Width, Height);

            if (StartTime > TimeSpan.Zero)
                Duration = gameTime.TotalGameTime - StartTime;

            if (AttackVisible is false && Duration.TotalMilliseconds > AttackDelay)
            {
                AttackVisible = true;
                Motion = InitialMotion;
            }

            if(AttackVisible is false)
                Motion = Vector2.Zero;
            else if (AttackVisible)
            {
                if (AnimatedAttack)
                    base.Update(gameTime);
            }

            //Prevent the source from moving during an attack with a build up
            if (Duration.TotalMilliseconds > AttackDelay)
                Source.CanMove = true;
            else
                Source.CanMove = false;
        }

        public virtual void SetUpAttack(GameTime gameTime, Color attackColor, Vector2 originPosition)
        {
            StartTime = gameTime.TotalGameTime;
            Offset();
            Position = originPosition + AttackOffset;
            DefaultColor = attackColor;
            SpriteColor = DefaultColor;
            LastAttackTime = gameTime.TotalGameTime;
            Info.Text = string.Empty;
            StartPosition = Position;

            DamageHitBox = new((int)Position.X, (int)Position.Y, Width, Height);
        }

        public bool IsOnCooldown(GameTime gameTime)
        {
            return ((gameTime.TotalGameTime - LastAttackTime).TotalMilliseconds < AttackCoolDownLength);
        }

        public virtual AttackData GetAttackData()
        {
            return new()
            {
                AttackLength = AttackLength,
                StartTime = StartTime,
                Duration = Duration,
                AttackOffset = AttackOffset,
                LastAttackTime = LastAttackTime,
                AttackCoolDownLength = AttackCoolDownLength,
                Speed = Speed,
                DamageModifier = DamageModifier,
                ManaCost = ManaCost,
            };
        }

        //TODO Overide this with the corret offset parameters based on the type of the entity calling the method 
        public virtual void Offset()
        {
            if (Source.CurrentAnimation == AnimationKey.Up)
            {
                Width = 48;
                Height = 32;
                AttackOffset = new(Source.Width / 2 - Width / 2, -Height);
                Frame = new(10, 160, Width, Height);
            }
            else if (Source.CurrentAnimation == AnimationKey.Down)
            {
                Width = 48;
                Height = 32;
                AttackOffset = new(Source.Width / 2 - Width / 2, Source.Height);
                Frame = new(10, 230, Width, Height);
            }
            if (Source.CurrentAnimation == AnimationKey.Left)
            {
                Width = 32;
                Height = 60;
                AttackOffset = new(-Width, Source.Height / 2 - Height / 2);
                Frame = new(15, 5, Width, Height);
            }
            else if (Source.CurrentAnimation == AnimationKey.Right)
            {
                Width = 32;
                Height = 60;
                AttackOffset = new(Source.Width, Source.Height / 2 - Height / 2);
                Frame = new(20, 80, Width, Height);
            }
        }

        public bool AttackTimedOut()
        {
            if(Duration.TotalMilliseconds > AttackLength)
                return true;

            return false;
        }

        public override string ToString()
        {
            string ToString = 
                $"Position: {Position}, " +
                $"Start Position: {StartPosition}, " +
                $"Attack Length: {AttackLength}, " +
                $"Start Time: {StartTime}, " +
                $"Duration: {Duration}, " +
                $"Attack Offset: {AttackOffset}, " +
                $"Last Attack Time: {LastAttackTime}, " +
                $"Attack Cool Down Length: {AttackCoolDownLength}, " +
                $"Speed: {Speed}, " +
                $"Damage Modifier: {DamageModifier}, " +
                $"Mana Cost: {ManaCost}, " +
                $"AttackDelay: {AttackDelay}, " +
                $"Visible: {AttackVisible}, " + 
                $"Motion: {Motion}, " +
                $"Initial Motion: {InitialMotion}, " +
                $"DamageHitBox: {DamageHitBox}, " +
                $"Rectangle: {Rectangle}, ";

            return ToString;
        }
    }
}
