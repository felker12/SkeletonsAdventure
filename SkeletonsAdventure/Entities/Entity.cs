using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.Quests;

namespace SkeletonsAdventure.Entities
{
    internal class Entity : AnimatedSprite
    {
        public TimeSpan LastDeathTime { get; set; } = new();
        public Vector2 RespawnPosition { get; set; } = Vector2.Zero;
        public string Type { get; set; } = string.Empty;
        public StatusBar HealthBar { get; set; } = new();
        public int AttackCoolDownLength { get; protected set; } = 600; //minimum length of the delay between attacks in milliseconds
        public AttackManager AttackManager { get; set; } 
        public BasicAttack BasicAttack { get; set; }
        public List<BasicAttack> AttacksHitBy { get; set; } = [];
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
        public bool CanAttack { get; set; } = true; //TODO add a check to see if the entity can attack or not because of a status effect
        public TimeSpan LastTimeAttacked { get; set; }
        public Vector2 PositionLastAttackedFrom { get; set; } // the position of the center of the entity that last attacked this entity

        public Entity() : base()
        {
            Health = BaseHealth;
            Position = new();
            Type = GetType().FullName;

            Initialize();
        }

        public Entity(EntityData data) : base()
        {
            Type = GetType().FullName;
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
            BasicAttack = new(GameManager.BasicAttackData, GameManager.SkeletonAttackTexture, this);
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

        public virtual EntityData GetEntityData()
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

        public virtual Entity Clone()
        {
            return new(GetEntityData())
            {
                Position = Position,
                Level = Level,
                SpriteColor = SpriteColor,
            };
        }

        public virtual void GetHitByAttack(BasicAttack attack, GameTime gameTime)
        {
            AttacksHitBy.Add(attack);
            LastTimeAttacked = gameTime.TotalGameTime;
            PositionLastAttackedFrom = attack.Source.Center;

            int dmg = (int)(DamageEngine.CalculateDamage(attack.Source, this) * attack.DamageModifier);
            DamagePopUp damagePopUp = new(dmg.ToString(), Center);

            if (this is Enemy)
                damagePopUp.Color = Color.Cyan;

            World.CurrentLevel.DamagePopUpManager.Add(damagePopUp);
            //TODO add logic for critical hits and color the attack orange if it is a critical

            Health -= dmg;

            if (Health < 1)
            {
                EntityDiedByAttack(attack);
            }
        }

        private void ClearOldAttacksHitBy()
        {
            List<BasicAttack> oldAttacks = [];

            foreach (var atk in AttacksHitBy)
            {
                if (atk.AttackTimedOut()) //removes attacks that have timed out
                    oldAttacks.Add(atk);
                else if (atk.Source.IsDead) //removes attacks from dead entities
                    oldAttacks.Add(atk);
            }

            if (oldAttacks.Count > 0)
            {
                foreach (var oldAttack in oldAttacks)
                    AttacksHitBy.Remove(oldAttack);
            }
        }
        public virtual void Respawn()
        {
            Health = MaxHealth;
            Position = RespawnPosition;
            Motion = Vector2.Zero;
            IsDead = false;
            AttackManager.ClearAttacks();
            LastTimeAttacked = TimeSpan.Zero;
            Info.Text = string.Empty;
        }

        public virtual void EntityDied(GameTime gameTime) //TODO change how the timer for dead entities works
        {
            IsDead = true;
            LastDeathTime = gameTime.TotalGameTime;
            Motion = Vector2.Zero;
            AttackManager.ClearAttacks();
        }

        public virtual void EntityDiedByAttack(BasicAttack attack) //TODO change how the timer for dead entities works
        {
            string message = $"{attack.Source.GetType().Name} killed {GetType().Name} level {Level} with {attack.GetType().Name}.";
            AttacksHitBy.Clear();

            if (attack.Source is Player player)
            {
                message += $" {XP} XP was gained.";
                //check if there is an active task that requires the player to kill this entity
                foreach (Quest quest in player.ActiveQuests)
                {
                    if (quest is null || quest.ActiveTask is null)
                        continue;

                    if (quest.ActiveTask is not null && quest.ActiveTask is SlayTask slayTask)
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
            if(AttackingIsOnCoolDown(gameTime) is false && entityAttack.IsOnCooldown(gameTime) is false)
            {
                if(this is Player player)
                {
                    if(player.Mana < entityAttack.ManaCost)
                        return;
                    else
                        player.Mana -= entityAttack.ManaCost;
                }

                //if the attack has speed it will move. If not it will  be stationary
                if (entityAttack.Speed > 0)
                    SetEntityAttackMotion(entityAttack);

                entityAttack.SetUpAttack(gameTime, BasicAttackColor, Position);
                AttackManager.AddAttack(entityAttack.Clone(), gameTime);
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
            entityAttack.Motion *= entityAttack.Speed;
        }

        public bool AttackingIsOnCoolDown(GameTime gameTime)
        {
            return (gameTime.TotalGameTime - AttackManager.LastAttackTime).TotalMilliseconds < AttackCoolDownLength;
        }

        public virtual void PathToPoint(Vector2 target)
        {
            Vector2 movement = new(0,0);

            if ((int)target.Y > (int)Position.Y)
                movement.Y = 1;
            else if ((int)target.Y < (int)Position.Y)
                movement.Y = -1;
            else if ((int)target.Y == (int)Position.Y)
                movement.Y = 0;
            if ((int)target.X > (int)Position.X)
                movement.X = 1;
            else if ((int)target.X < (int)Position.X)
                movement.X = -1;
            else if ((int)target.X == (int)Position.X)
                movement.X = 0;

            if (movement != Vector2.Zero)
                movement.Normalize();

            Motion = movement;
        }

        public void FaceTarget(Entity target)
        {
            //find the distance to the center of the target
            Vector2 distance = Position - target.Center;
            //convert the distance to an int so it can be used for the animation
            distance.X = (int)distance.X;
            distance.Y = (int)distance.Y;

            if (distance.X > -Width/2 && distance.Y >= 0) //TODO update if adding diagonal animations
            {
                CurrentAnimation = AnimationKey.Left;
                if (distance.Y > Width * .75)
                    CurrentAnimation = AnimationKey.Up;
            }
            else if (distance.X <= -Width/2 && distance.Y >= 0)
            {
                CurrentAnimation = AnimationKey.Right;
                if (distance.Y > Width * .75)
                    CurrentAnimation = AnimationKey.Up;
            }
            else if (distance.X >= -Width / 4 && distance.Y < 0)
            {
                CurrentAnimation = AnimationKey.Left;
                if (distance.Y < -Height - Height / 4)
                    CurrentAnimation = AnimationKey.Down;
            }
            else if (distance.X <= -Width / 2 && distance.Y < 0)
            {
                CurrentAnimation = AnimationKey.Right;
                if (distance.Y < -Height  -Height / 4)
                    CurrentAnimation = AnimationKey.Down;
            }
        }
    }
}
