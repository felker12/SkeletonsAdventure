using RpgLibrary.AttackData;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Attacks
{
    internal class MultiShotAttack : ShootingAttack
    {
        public virtual ShootingAttack Shot { get; set; }
        public int ShotCount { get; set; } = 3;
        public TimeSpan ShotInterval { get; set; } = TimeSpan.FromMilliseconds(500);
        public int ShotsFired { get; set; } = 0;
        public int RemainingShots => ShotCount - ShotsFired;
        public List<ShootingAttack> Shots { get; private set; } = [];

        public MultiShotAttack(MultiShotAttackData data, Texture2D iconImage) : base(data, iconImage)
        {
            Shot = (ShootingAttack)GameManager.EntityAttackClone[data.ShotName];
            ShotCount = data.ShotCount;
            ShotInterval = data.ShotInterval;
            Initialize();
        }

        protected MultiShotAttack(MultiShotAttack attack) : base(attack)
        {
            Shot = attack.Shot;
            ShotCount = attack.ShotCount;
            ShotInterval = attack.ShotInterval;
            ShotsFired = attack.ShotsFired;
            Shots = [.. attack.Shots];
            Initialize();
        }

        private void Initialize()
        {
            Shot.DamageModifier = DamageModifier;
            Shot.ManaCost = 0; // Only the multi-shot attack consumes mana
            Shot.Source = Source;
            Shot.AttackLength = Shot.AttackLengthMinusDelay;
            Shot.AttackDelay = 0;

            //multi shot attacks will just have the 1 frame to be used as an icon for them
            SetFrames(1, 32, 32, order: [AnimationKey.Right]);
        }

        public override MultiShotAttack Clone()
        {
            return new MultiShotAttack(this);
        }

        public override void Update(GameTime gameTime)
        {
            if (StartTime > TimeSpan.Zero)
                Duration = gameTime.TotalGameTime - StartTime;

            //Prevent the source from moving while shooting
            if (Duration.TotalMilliseconds > ShotCount * ShotInterval.TotalMilliseconds + AttackDelay)
                Source.CanMove = true;
            else
                Source.CanMove = false;

            UpdateCooldown(gameTime);

            // Check if it's time to fire the next shot
            if(Duration.TotalMilliseconds < AttackDelay)
                return;

            if (ShotsFired < ShotCount && Duration.TotalMilliseconds >= ShotsFired * ShotInterval.TotalMilliseconds + AttackDelay)
            {
               /* Debug.WriteLine($"Time to shoot. shots fired: {ShotsFired}, " +
                    $"duration: {Duration.TotalMilliseconds} >= " +
                    $"{ShotsFired} * {ShotInterval.TotalMilliseconds} + {AttackDelay} " +
                    $"({ShotsFired * ShotInterval.TotalMilliseconds + AttackDelay})");*/

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

        public void ClearExpiredAttacks()
        {
            List<ShootingAttack> toRemove = [];

            foreach (var attack in Shots)
            {
                if (attack.AttackTimedOut() || attack.Source.IsDead)
                {
                    attack.AttackVisible = false;
                    toRemove.Add(attack);
                }
            }

            foreach (var atk in toRemove)
                Shots.Remove(atk);
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
