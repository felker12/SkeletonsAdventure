using RpgLibrary.AttackData;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.Quests;
using System.Linq;

namespace SkeletonsAdventure.Attacks
{
    public class BasicAttack : AnimatedSprite
    {
        public int AttackLength { get; set; } //The length includes AttackDelay
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan LastAttackTime { get; set; }
        public Vector2 AttackOffset { get; set; }
        public Entity Source { get; set; }
        public int CoolDownLength { get; set; } //length of the delay between attacks in milliseconds
        public float DamageModifier { get; set; }
        public int ManaCost { get; set; }
        public bool AnimatedAttack { get; set; } = false;
        public Rectangle DamageHitBox { get; set; }
        public int AttackDelay { get; set; }
        public bool AttackVisible { get; set; } = true;
        public Vector2 StartPosition { get; set; } = new();
        public Vector2 InitialMotion { get; set; }
        public double CooldownRemaining { get; private set; }
        public float CooldownRemainingRatio { get; private set; }
        public LevelRequirements LevelRequirements { get; private set; } = new();
        public List<string> SkillRequirementsNames { get; private set; } = [];
        public string Name => GetType().Name;
        public int AttackLengthMinusDelay => AttackLength - AttackDelay;
        public bool CanMoveDuringAttack { get; protected set; } = true;
        internal AttackIcon AttackIcon { get; set; }

        public BasicAttack(AttackData attackData, Texture2D texture, Entity source = null) : base()
        {
            attackData ??= new(); // Use default values if attackData is null

            AttackLength = attackData.AttackLength;
            StartTime = attackData.StartTime;
            Duration = attackData.Duration;
            AttackOffset = attackData.AttackOffset;
            LastAttackTime = attackData.LastAttackTime;
            CoolDownLength = attackData.AttackCoolDownLength;
            Speed = attackData.Speed;
            DamageModifier = attackData.DamageModifier;
            ManaCost = attackData.ManaCost;
            AttackDelay = attackData.AttackDelay;
            LevelRequirements = new(attackData.LevelRequirement);
            SkillRequirementsNames = attackData.SkillRequirementsNames;

            Texture = texture;
            Source = source;

            Initialize();
        }

        protected BasicAttack(BasicAttack attack) : base()
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
            CoolDownLength = attack.CoolDownLength;
            Motion = attack.Motion;
            Speed = attack.Speed;
            DamageModifier = attack.DamageModifier;
            ManaCost = attack.ManaCost;
            AttackDelay = attack.AttackDelay;
            AnimatedAttack = attack.AnimatedAttack;
            StartPosition = attack.StartPosition;
            DamageHitBox = attack.DamageHitBox;
            LevelRequirements = attack.LevelRequirements;
            SkillRequirementsNames = attack.SkillRequirementsNames;

            Initialize();
        }

        private void Initialize()
        {
            DamageHitBox = new((int)Position.X, (int)Position.Y, Width, Height);

            if (AttackDelay > 0)
                AttackVisible = false;

            LastAttackTime = TimeSpan.FromMilliseconds(-CoolDownLength);

            AttackIcon = new();
        }

        public virtual BasicAttack Clone()
        {
            return new BasicAttack(this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (AttackVisible)
            {
                //spriteBatch.DrawRectangle(Rectangle, SpriteColor, 1, 0); //TODO
                //spriteBatch.DrawRectangle(DamageHitBox, Color.OrangeRed, 1, 0); //TODO

                spriteBatch.Draw(Texture, Center, Frame, SpriteColor, RotationAngle, SpriteCenter, Scale, SpriteEffects.None, 1);
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
            else if (AttackVisible && AnimatedAttack)
                base.Update(gameTime);

            //Prevent the source from moving during an attack with a build up
            if (CanMoveDuringAttack && Duration.TotalMilliseconds > AttackDelay)
                Source.CanMove = true;
            //Prevent the source from moving during an attack that locks the source in place
            else if (AttackTimedOut())
                Source.CanMove = true;
            else
                Source.CanMove = false;

            Source.CanAttack = Source.CanMove; //prevent a source from being able to attack while already doing an attack

            UpdateCooldown(gameTime);
        }

        public void UpdateCooldown(GameTime gameTime)
        {
            CooldownRemaining = GetRemainingCooldown(gameTime);
            CooldownRemainingRatio = (float)Math.Clamp(CooldownRemaining / CoolDownLength, 0f, 1f);
        }

        public virtual bool Hits(Entity entity)
        {
            return DamageHitBox.Intersects(entity.Rectangle);
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
            return ((gameTime.TotalGameTime - LastAttackTime).TotalMilliseconds < CoolDownLength);
        }

        //Returns the remaining cooldown time in milliseconds, or 0 if the cooldown has expired.
        public double GetRemainingCooldown(GameTime gameTime)
        {
            double remainingCooldown = CoolDownLength - (gameTime.TotalGameTime - LastAttackTime).TotalMilliseconds;
            return Math.Max(0, remainingCooldown);
        }

        public virtual AttackData ToData()
        {
            return new()
            {
                AttackLength = AttackLength,
                StartTime = StartTime,
                Duration = Duration,
                AttackOffset = AttackOffset,
                LastAttackTime = LastAttackTime,
                AttackCoolDownLength = CoolDownLength,
                Speed = Speed,
                DamageModifier = DamageModifier,
                ManaCost = ManaCost,
                AttackDelay = AttackDelay,
                LevelRequirement = LevelRequirements.ToData(),
                SkillRequirementsNames = [.. SkillRequirementsNames],
            };
        }

        //Overide this with the corret offset parameters based on the type of the entity calling the method 
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
            return Duration.TotalMilliseconds > AttackLength;
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
                $"Attack Cool Down Length: {CoolDownLength}, " +
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

        public virtual void DrawIcon(SpriteBatch spriteBatch, Vector2 position, int size = 32, Color tint = default)
        {
            AttackIcon.DrawIcon(this, spriteBatch, position, size, tint);
        }

        public void AddRequiredSkill(BasicAttack attack)
        {
            if (attack is null)
                return;

            AddRequriedSkillName(attack.Name);
        }

        private void AddRequriedSkillName(string name)
        {
            //Don't add the skill requirement if the name is null or empty, or if it already exists in the list
            if (string.IsNullOrEmpty(name))
                return;

            if (SkillRequirementsNames.Contains(name))
                return;

            SkillRequirementsNames.Add(name);
        }
    }
}