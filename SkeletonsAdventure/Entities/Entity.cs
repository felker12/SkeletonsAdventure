using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.HelperClasses;
using SkeletonsAdventure.Quests;

namespace SkeletonsAdventure.Entities
{
    public class Entity : AnimatedSprite, ICloneableGameClass<Entity>
    {
        internal event Action<Entity> Died;
        private bool _attackDeathReported;
        public TimeSpan LastDeathTime { get; set; } = new();
        public Vector2 RespawnPosition { get; set; } = Vector2.Zero;
        public string Type { get; set; } = string.Empty;
        public StatusBar HealthBar { get; }
        internal EntityAssets Assets { get; }
        public int AttackCoolDownLength { get; protected set; } = 600; //minimum length of the delay between attacks in milliseconds
        public AttackManager AttackManager { get; set; } 
        public BasicAttack BasicAttack { get; set; }
        public HashSet<BasicAttack> AttacksHitBy { get; set; } = [];
        public int BaseDefence { get; set; } = 1; 
        public int BaseAttack { get; set; } = 1;
        public int BaseHealth { get; set; } = 1; 
        public int BaseXP { get; set; } = 1; 
        public int WeaponAttack { get; set; } = 0; 
        public int ArmourDefence { get; set; } = 0;
        public int XP { get; set; } //Xp gained for killing the entity
        public int MaxHealth { get; set; }
        public int Health { get; set; } 
        public int Defence { get; set; }
        public int Attack { get; set; }
        public int RespawnTime { get; set; } = 3; //time in seconds until the entity respawns
        public int Level { get; protected set; } = 0;
        public Color BasicAttackColor { get; set; } = Color.White; //Color of the basic attack
        public bool IsDead { get; set; } = false;
        public bool HealthBarVisible { get; set; } = true;
        public bool IsInvincible { get; set; } = false; //TODO add invincibility frames to the entity
        public bool CanAttack { get; set; } = true; //TODO add a check to see if the entity can attack or not because of a status effect or already attacking
        public TimeSpan LastTimeAttacked { get; set; }
        public Vector2 PositionLastAttackedFrom { get; set; } // the position of the center of the entity that last attacked this entity

        public Entity(EntityAssets assets) : this(null, assets)
        {
        }

        public Entity(EntityData data, EntityAssets assets)
            : base((assets ?? throw new ArgumentNullException(nameof(assets))).Texture, assets.InfoFont)
        {
            Assets = assets;
            HealthBar = new(assets.StatusBarTexture, assets.StatusBarFont);
            Type = GetType().FullName;
            if (data is not null)
                UpdateEntityWithData(data);

            Initialize();
        }

        private void Initialize()
        {
            AttackManager = new(this);
            Health = BaseHealth;
            MaxHealth = BaseHealth;
            Defence = BaseDefence;
            Attack = BaseAttack;
            XP = BaseXP;
            BasicAttack = new(Assets.BasicAttackData, Assets.BasicAttackTexture, Assets.InfoFont, this);
        }

        public override void Update(GameTime gameTime)
        {
            AttackManager.Update(gameTime);
            ClearOldAttacksHitBy();
            base.Update(gameTime);

            //make the entity red for a time when hit
            //Timespan.Zero check makes sure it isn't true when the game starts
            if ((gameTime.TotalGameTime - LastTimeAttacked).TotalMilliseconds < 200 && LastTimeAttacked != TimeSpan.Zero) 
                SpriteColor = Color.Red;
            else
                SpriteColor = DefaultColor;

            if (HealthBarVisible)
            {
                Vector2 healthBarOffset = new(HealthBar.Width / 2 - Width / 2, HealthBar.Height + HealthBar.BorderWidth + 4);
                HealthBar.Update(Health, MaxHealth, Position - healthBarOffset);
            }

            //TODO
            Info.Text += "\nLevel = " + Level;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(HealthBarVisible)
                HealthBar.Draw(spriteBatch);

            base.Draw(spriteBatch);
        }

        public virtual EntityData ToData()
        {
            return new()
            {
                Type = Type,
                BaseHealth = BaseHealth,
                BaseDefence = BaseDefence,
                BaseAttack = BaseAttack,
                CurrentHealth = Health,
                Position = Position,
                RespawnPosition = RespawnPosition,
                BaseXP = BaseXP,
                EntityLevel = Level,
                IsDead = IsDead,
                LastDeathTime = LastDeathTime,
            };
        }

        public virtual void UpdateEntityWithData(EntityData entityData)
        {
            Type = entityData.Type;
            BaseHealth = entityData.BaseHealth;
            BaseAttack = entityData.BaseAttack;
            BaseDefence = entityData.BaseDefence;
            BaseXP = entityData.BaseXP;
            Level = entityData.EntityLevel;
            Health = entityData.CurrentHealth;
            IsDead = entityData.IsDead;

            if (entityData.Position != null)
                Position = (Vector2)entityData.Position;
            if (entityData.RespawnPosition != null)
                RespawnPosition = (Vector2)entityData.RespawnPosition;
            if (entityData.LastDeathTime != null)
                LastDeathTime = (TimeSpan)entityData.LastDeathTime;
        }

        public override Entity Clone()
        {
            return new(ToData(), Assets)
            {
                Position = Position,
                Level = Level,
                SpriteColor = SpriteColor,
            };
        }

        public virtual void GetHitByAttack(BasicAttack attack, GameTime gameTime)
        {
            if (IsDead || Health <= 0 || IsInvincible || attack?.Source is null ||
                attack.Source.IsDead || AttacksHitBy.Contains(attack))
                return;

            AttacksHitBy.Add(attack);
            LastTimeAttacked = gameTime.TotalGameTime;
            PositionLastAttackedFrom = attack.Source.Center;

            int dmg = (int)(DamageEngine.CalculateDamage(attack.Source, this) * attack.DamageModifier);
            World.CurrentLevel.DamagePopUpManager.Add(dmg.ToString(), Center, this is Enemy ? Color.Cyan : Color.White);
            //TODO add logic for critical hits and color the attack orange if it is a critical

            Health = Math.Max(0, Health - dmg);

            if (Health < 1)
            {
                EntityDied(World.TotalTimeInWorld);
                EntityDiedByAttack(attack);
            }
        }

        private void ClearOldAttacksHitBy()
        {
            /*for (int i = AttacksHitBy.Count - 1; i >= 0; i--)
            {
                var atk = AttacksHitBy[i];

                if (atk.AttackTimedOut() || atk.Source.IsDead) //removes attacks that have timed out or from dead entities
                    AttacksHitBy.Remove(atk);
            }*/

            AttacksHitBy.RemoveWhere(atk => atk.AttackTimedOut() || atk.Source.IsDead);
        }

        public virtual void Respawn()
        {
            Health = MaxHealth;
            Position = RespawnPosition;
            Motion = Vector2.Zero;
            IsDead = false;
            CanMove = true;
            CanAttack = true;
            _attackDeathReported = false;
            AttackManager.ClearAttacks();
            AttacksHitBy.Clear();
            LastTimeAttacked = TimeSpan.Zero;
            Info.Text = string.Empty;
        }

        public virtual void EntityDied(GameTime gameTime) //TODO change how the timer for dead entities works
        {
            if (IsDead)
                return;
            IsDead = true;
            Health = 0;
            CanMove = false;
            CanAttack = false;
            LastDeathTime = gameTime.TotalGameTime;
            Motion = Vector2.Zero;
            AttackManager.ClearAttacks();
            Died?.Invoke(this);
        }

        public virtual void EntityDiedByAttack(BasicAttack attack) //TODO change how the timer for dead entities works
        {
            if (!IsDead || _attackDeathReported)
                return;
            _attackDeathReported = true;
            string message = $"{attack.Source.GetType().Name} killed {GetType().Name} level {Level} with {attack.GetType().Name}.";
            AttacksHitBy.Clear();

            if (this is Enemy && attack.Source is Player player)
            {
                player.GainXp(XP);
                player.KillCounter.RecordKill(GetType().Name);
                message += $" {XP} XP was gained.";
                //check if there is an active task that requires the player to kill this entity
                foreach (Quest quest in player.ActiveQuests)
                {
                    if (quest is null || quest.ActiveTask is null)
                        continue;

                    if (quest.ActiveTask is SlayTask slayTask)
                    {
                        if (slayTask.GetEntityToSlay().GetType() == GetType())
                            quest.ProgressTask();
                    }
                }
            }

            World.AddMessage(message);
        }

        public virtual void PerformAttack(GameTime gameTime, BasicAttack entityAttack)
        {
            if (IsDead || Health <= 0 || !CanAttack || entityAttack is null)
                return;

            if (AttackManager.AttackingIsOnCoolDown(gameTime) is false && entityAttack.IsOnCooldown(gameTime) is false)
            {
                if(this is Player player)
                {
                    if(player.Mana < entityAttack.ManaCost)
                        return;
                    else
                        player.Mana -= entityAttack.ManaCost;
                }

                var fired = entityAttack.Clone();
                fired.Source = this;
                if (fired.Speed > 0)
                    SetEntityAttackMotion(fired);
                fired.SetUpAttack(gameTime, BasicAttackColor, Position);
                entityAttack.LastAttackTime = gameTime.TotalGameTime;
                AttackManager.AddAttack(fired, gameTime);
            }
        }

        private void SetEntityAttackMotion(BasicAttack entityAttack)
        {
            if (CurrentAnimation == AnimationKey.Up)
                entityAttack.Motion = new(0, -1);
            else if (CurrentAnimation == AnimationKey.Down)
                entityAttack.Motion = new(0, 1);
            if (CurrentAnimation == AnimationKey.Left)
                entityAttack.Motion = new(-1, 0);
            else if (CurrentAnimation == AnimationKey.Right)
                entityAttack.Motion = new(1, 0);

            entityAttack.Motion.Normalize();
        }
    }
}
