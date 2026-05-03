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

        public MultiShotAttack(MultiShotAttackData data, Texture2D iconImage) : base(data, iconImage)
        {
            Shot = (ShootingAttack)GameManager.GetAttackByName(data.ShotName);
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
            Shot.Source = Source;
            Shot.AttackLength = Shot.AttackLengthMinusDelay; //The length of the shot attack is the time it takes for the shot to be fired and disappear, so we set the multi-shot attack length to be that plus the delay between shots
            //Shot.AttackDelay = 0;

            //multi shot attacks will just have the 1 frame to be used as an icon for them
            SetFrames(1, 32, 32, order: [AnimationKey.Right]);

            //the length of the multi-shot attack is the time it takes to fire all the shots
            //plus the initial delay before the first shot, plus the delay of the last shot
            AttackLength = (int)(ShotCount * ShotInterval.TotalMilliseconds + AttackDelay + Shot.AttackDelay);

            CanMoveDuringAttack = false;
        }

        public override MultiShotAttack Clone()
        {
            return new MultiShotAttack(this);
        }

        public override void Update(GameTime gameTime)
        {
            if (StartTime > TimeSpan.Zero)
                Duration = gameTime.TotalGameTime - StartTime;

            UpdateCooldown(gameTime);

            //check if it's time to fire the next shot
            if(Duration.TotalMilliseconds < AttackDelay)
                return;

            while (ShotsFired < ShotCount && Duration.TotalMilliseconds >= ShotsFired * ShotInterval.TotalMilliseconds + AttackDelay)
            {
                ShootingAttack newShot = Shot.Clone();
                newShot.Position = StartPosition;
                newShot.Motion = InitialMotion;
                newShot.SetUpAttack(gameTime, DefaultColor, StartPosition);

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
