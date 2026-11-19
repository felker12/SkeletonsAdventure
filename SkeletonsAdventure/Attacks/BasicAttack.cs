using RpgLibrary.AttackData;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.HelperClasses;
using SkeletonsAdventure.Quests;

namespace SkeletonsAdventure.Attacks
{
    public class BasicAttack : AnimatedSprite, ICloneableGameClass<BasicAttack>
    {
        public int AttackLength { get; set; } //The length includes AttackDelay
        public TimeSpan StartTime { get; set; } //The time when the attack was started, and is used to determine if the attack has timed out or passed the delay
        public TimeSpan Duration { get; set; } //The duration is the time since the attack was started, and is used to determine if the attack has timed out or passed the delay
        public TimeSpan LastAttackTime { get; set; } //The time when the last attack was started, and is used to determine if the attack is on cooldown
        public Vector2 AttackOffset { get; set; } //The offset of the attack from the source entity's position
        public Entity Source { get; set; }
        public bool HasStarted { get; private set; }
        public Vector2 CastOriginPosition { get; private set; }
        internal double MovementSecondsThisFrame { get; private set; }
        internal List<(Vector2 From, Vector2 To)> MovementSegments { get; } = [];
        internal HashSet<Entity> HitTargets { get; } = [];
        public virtual Rectangle HitBounds => DamageHitBox;
        public int CoolDownLength { get; set; } //length of the delay between attacks in milliseconds
        public float DamageModifier { get; set; }
        public int ManaCost { get; set; }
        public bool AnimatedAttack { get; set; } = false;
        public Rectangle DamageHitBox { get; set; }
        public int AttackDelay { get; set; } //the length of time from when the attack is started until the attack actually happens in milliseconds. During this time, the attack is not visible and does not hit the target, but the source can still be locked in place if CanMoveDuringAttack is false
        public bool AttackVisible { get; set; } = true;
        public Vector2 StartPosition { get; set; } = new();
        public Vector2 InitialMotion { get; set; }
        public double CooldownRemaining { get; private set; } //The remaining cooldown time in milliseconds, or 0 if the cooldown has expired
        public float CooldownRemainingRatio { get; private set; } //The remaining cooldown time as a ratio of the total cooldown time, clamped between 0 and 1
        public LevelRequirements LevelRequirements { get; private set; } = new();
        public List<string> SkillRequirementsNames { get; private set; } = [];
        public string Name => GetType().Name;
        public int AttackLengthMinusDelay => AttackLength - AttackDelay;
        public bool CanMoveDuringAttack { get; protected set; } = true;
        internal AttackIcon AttackIcon { get; set; }

        public BasicAttack(AttackData attackData, Texture2D texture, SpriteFont infoFont, Entity source = null) : base(texture, infoFont)
        {
            ArgumentNullException.ThrowIfNull(attackData);

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

            Source = source;

            Initialize();
        }

        protected BasicAttack(BasicAttack attack) : base(attack.Texture, attack.Info.SpriteFont)
        {
            Width = attack.Width;
            Height = attack.Height;
            AttackLength = attack.AttackLength;
            AttackOffset = attack.AttackOffset;
            Frame = attack.Frame;
            Source = attack.Source;
            SpriteColor = attack.SpriteColor;
            Info.TextColor = attack.Info.TextColor;
            CoolDownLength = attack.CoolDownLength;
            Speed = attack.Speed;
            DamageModifier = attack.DamageModifier;
            ManaCost = attack.ManaCost;
            AttackDelay = attack.AttackDelay;
            AnimatedAttack = attack.AnimatedAttack;
            DefaultColor = attack.DefaultColor;
            CanMoveDuringAttack = attack.CanMoveDuringAttack;
            LevelRequirements = attack.LevelRequirements.Clone();
            SkillRequirementsNames = [.. attack.SkillRequirementsNames];

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

        public override BasicAttack Clone()
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

            MovementSegments.Clear();
            MovementSecondsThisFrame = 0;
            if (!HasStarted)
                return;
            Duration = gameTime.TotalGameTime - StartTime;
            double now = Duration.TotalSeconds;
            double previous = now - gameTime.ElapsedGameTime.TotalSeconds;
            double activeStart = AttackDelay / 1000.0;
            double end = AttackLength / 1000.0;
            MovementSecondsThisFrame = Math.Max(0, Math.Min(now, end) - Math.Max(previous, activeStart));
            AttackVisible = MovementSecondsThisFrame > 0 || (now >= activeStart && now <= end) ||
                (end == activeStart && previous <= end && now >= end);
            if (AttackVisible && previous <= activeStart && now >= activeStart)
            {
                Motion = InitialMotion;
            }

            if(AttackVisible is false)
                Motion = Vector2.Zero;
            else if (AttackVisible && AnimatedAttack)
                base.Update(new GameTime(gameTime.TotalGameTime, TimeSpan.FromSeconds(MovementSecondsThisFrame)));

            DamageHitBox = Rectangle;
            UpdateCooldown(gameTime);
        }

        public void UpdateCooldown(GameTime gameTime)
        {
            CooldownRemaining = GetRemainingCooldown(gameTime);
            CooldownRemainingRatio = CoolDownLength <= 0 ? 0 : (float)Math.Clamp(CooldownRemaining / CoolDownLength, 0f, 1f);
        }

        public virtual bool Hits(Entity entity)
        {
            return DamageHitBox.Intersects(entity.Rectangle);
        }

        public virtual bool HitsDuringFrame(Entity entity) => Hits(entity);

        public virtual void SetUpAttack(GameTime gameTime, Color attackColor, Vector2 originPosition)
        {
            ArgumentNullException.ThrowIfNull(Source);
            if (AttackDelay < 0 || AttackLength < AttackDelay)
                throw new ArgumentOutOfRangeException(nameof(AttackLength), "Attack length must include its nonnegative delay.");
            HasStarted = true;
            Duration = TimeSpan.Zero;
            MovementSecondsThisFrame = 0;
            MovementSegments.Clear();
            CastOriginPosition = originPosition;
            HitTargets.Clear();
            InitialMotion = Motion;
            AttackVisible = AttackDelay == 0;
            if (!AttackVisible)
                Motion = Vector2.Zero;
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
            return HasStarted && Duration.TotalMilliseconds >= AttackLength;
        }

        public bool AttackPassedDelay()
        {
            //return true if the source can move during the attack and the attack has passed the delay time, or if the attack doesn't allow movement during the attack and the attack has timed out
            return CanMoveDuringAttack && HasStarted && Duration.TotalMilliseconds >= AttackDelay;
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
