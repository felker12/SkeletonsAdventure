using RpgLibrary.AttackData;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Attacks
{
    internal class MultiShotAttack : ShootingAttack
    {
        public virtual ShootingAttack Shot { get; private set; }
        public int ShotCount { get; set; } = 3;
        public TimeSpan ShotInterval { get; set; } = TimeSpan.FromMilliseconds(500);
        public int ShotsFired { get; set; } = 0;
        public List<ShootingAttack> Shots { get; private set; } = [];

        public MultiShotAttack(MultiShotAttackData data, Texture2D iconImage, ShootingAttack baseAttack) : base(data, iconImage, baseAttack.Info.SpriteFont)
        {
            Shot = baseAttack.Clone();
            ShotCount = data.ShotCount;
            ShotInterval = data.ShotInterval;
            Initialize();
        }

        protected MultiShotAttack(MultiShotAttack attack) : base(attack)
        {
            Shot = attack.Shot.Clone();
            ShotCount = attack.ShotCount;
            ShotInterval = attack.ShotInterval;
            ShotsFired = 0; //reset the number of shots fired when cloning a multi-shot attack, since the clone will be used as a new attack
            Shots = []; //reset the shots list when cloning a multi-shot attack, since the clone will be used as a new attack
            Initialize();
        }

        private void Initialize()
        {
            Shot.DamageModifier = DamageModifier;
            Shot.ManaCost = 0; // Only the multi-shot attack consumes mana

            //multi shot attacks will just have the 1 frame to be used as an icon for them
            SetFrames(1, 32, 32, order: [AnimationKey.Right]);

            AttackLength = CalculateLifetime();

            CanMoveDuringAttack = false;
        }

        private int CalculateLifetime()
        {
            if (ShotCount < 1 || ShotInterval < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(ShotCount));
            return checked((int)((ShotCount - 1) * ShotInterval.TotalMilliseconds + AttackDelay + Shot.AttackLength));
        }

        public override void SetUpAttack(GameTime gameTime, Color attackColor, Vector2 originPosition)
        {
            AttackLength = CalculateLifetime();
            ShotsFired = 0;
            Shots.Clear();
            base.SetUpAttack(gameTime, attackColor, originPosition);
        }

        public override MultiShotAttack Clone()
        {
            return new MultiShotAttack(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            AttackVisible = false;
            Motion = Vector2.Zero;
            if (!HasStarted)
                return;

            //check if it's time to fire the next shot
            if(Duration.TotalMilliseconds < AttackDelay)
                return;

            while (ShotsFired < ShotCount && Duration.TotalMilliseconds >= ShotsFired * ShotInterval.TotalMilliseconds + AttackDelay)
            {
                ShootingAttack newShot = Shot.Clone();
                newShot.Source = Source;
                newShot.DamageModifier = DamageModifier;
                newShot.ManaCost = 0;
                newShot.Motion = InitialMotion;
                var scheduledAt = StartTime + TimeSpan.FromMilliseconds(AttackDelay + ShotsFired * ShotInterval.TotalMilliseconds);
                newShot.SetUpAttack(new GameTime(scheduledAt, TimeSpan.Zero), DefaultColor, CastOriginPosition);

                Shots.Add(newShot);
                ShotsFired++;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //nothing is to be drawn
        }

        public override MultiShotAttackData ToData()
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
                ShotName = Shot.Name,
                ShotCount = ShotCount,
                ShotInterval = ShotInterval,
                ShotsFired = ShotsFired
            };
        }
    }
}
