using MonoGame.Extended;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;

namespace SkeletonsAdventure.Entities
{
    public enum EnemyType //TODO add more types as needed
    {
        Skeleton,
        Spider,
        Slime,
        Zombie,
        Goblin,
    }

    public enum EnemyClass
    {
        Regular, Elite, Boss
    }

    internal class Enemy : Entity
    {
        private int x, y, x2, y2, walkDistance, detectionWidth, detectionHeight;

        public bool CheckedLastAtackArea { get; set; } = false;
        public Rectangle DetectionArea => new((int)Position.X - (detectionWidth - Width) / 2,
            (int)Position.Y - (detectionHeight - Height) / 2, detectionWidth, detectionHeight);
        public Rectangle AttackArea => new((int)Position.X - Width,
            (int)Position.Y - Width, Width * 3, Height + Width * 2);

        public EnemyType EnemyType { get; set; }
        public EnemyClass EnemyClass { get; set; } = EnemyClass.Regular;
        public string DropTableName { get; set; } = string.Empty;
        public int NumberOfItemsToDrop { get; set; } = 2; //Number of items to drop from the drop table
        public ItemList GuaranteedDrops { get; set; } = new();
        public string Name { get; set; }

        public Enemy(EnemyData data) : base(data)
        {
            Initialize();
            UpdateEntityWithData(data);
        }

        public Enemy() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            ResetSquare();
            walkDistance = 200;
            detectionWidth = 300;
            detectionHeight = 300;

            Name = this.GetType().Name;
        }

        public void SetEnemyLevel(MinMaxPair levels)
        {
            if (EnemyClass != EnemyClass.Regular) //If the enemy is elite or a boss, set the level to the max of the range
                Level = levels.Max;
            else //Otherwise, set the level to a random number in the range
                Level = levels.GetRandomNumberInRange();

            EnemyStatAdjustmentForLevel(); // Adjust the enemy's stats based on the level
        }

        public void SetEnemyLevel(int level) //TODO
        {
            Level = level;
            EnemyStatAdjustmentForLevel();
        }

        private void EnemyStatAdjustmentForLevel()
        {
            MaxHealth = baseHealth + Level * 2;
            Health = MaxHealth;
            Defence = (int)(baseDefence + Level * 1.5);
            Attack = (int)(baseAttack + Level * 1.5);
            XP = baseXP + Level * 2;

            if(EnemyClass is EnemyClass.Elite)
            {
                int healthBonus =(int)(Health * 1.5);
                int Bonus = (int)Math.Ceiling((double)Defence / 4);

                Health += healthBonus;
                MaxHealth += healthBonus;

                Defence += Bonus;
                Attack += Bonus;
                XP *= 2;
                NumberOfItemsToDrop += 1; //Elite enemies drop more items
            }
            else if(EnemyClass is EnemyClass.Boss)
            {
                //TODO
            }

            UpdateEntityWithData(GetEntityData()); //TODO: check if this is needed
        }
        public override void Update(GameTime gameTime)
        {
            //Hide health bar when entity hasn't been attacked in a while
            if (LastTimeAttacked.TotalMilliseconds != 0 && gameTime.TotalGameTime.TotalMilliseconds - LastTimeAttacked.TotalMilliseconds < 6000)
                HealthBarVisible = true;
            else
                HealthBarVisible = false;

            base.Update(gameTime);

            //TODO delete this
            Info.Text += "\nXP = " + XP;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            //spriteBatch.DrawRectangle(DetectionArea, Color.Aqua, 1, 0); //TODO
            //spriteBatch.DrawRectangle(AttackArea, Color.Red, 1, 0); //TODO
        }

        public override Enemy Clone()
        {
            return new(GetEntityData())
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                Level = Level,
                SpriteColor = SpriteColor,
                DefaultColor = DefaultColor,
                DropTableName = DropTableName,
            };
        }

        public override EnemyData GetEntityData()
        {
            return new(base.GetEntityData())
            {
                GuaranteedItems = GuaranteedDrops.GetItemListItemData(),
                DropTableName = DropTableName,
            };
        }

        public virtual void UpdateEntityWithData(EnemyData entityData)
        {
            base.UpdateEntityWithData(entityData);
            DropTableName = entityData.DropTableName;
        }

        //Get the drop table based on the enemies level
        //TODO add different drop tables for different level ranges
        protected virtual DropTable GetDropTable()
        {
            DropTable table = Level switch
            {
                >= 100 => GameManager.GetDropTableByName(DropTableName).Clone(),
                >= 90 => GameManager.GetDropTableByName(DropTableName).Clone(),
                >= 80 => GameManager.GetDropTableByName(DropTableName).Clone(),
                >= 70 => GameManager.GetDropTableByName(DropTableName).Clone(),
                >= 60 => GameManager.GetDropTableByName(DropTableName).Clone(),
                >= 50 => GameManager.GetDropTableByName(DropTableName).Clone(),
                >= 40 => GameManager.GetDropTableByName(DropTableName).Clone(),
                >= 30 => GameManager.GetDropTableByName(DropTableName).Clone(),
                >= 20 => GameManager.GetDropTableByName(DropTableName).Clone(),
                >= 10 => GameManager.GetDropTableByName(DropTableName).Clone(),
                _ => GameManager.GetDropTableByName(DropTableName).Clone()
            };

            //TODO add logic to return different drop tables based on the entity's level
            return table;
        }

        // Returns a list of items that the enemy drops when killed
        // this includes the items from the drop table and the guaranteed drops
        public virtual List<GameItem> GetDrops()
        {
            List<GameItem> items = GetDropTable().GetDropsList(NumberOfItemsToDrop);

            //Add guaranteed drops to the list
            foreach (GameItem item in GuaranteedDrops.Items)
                items.Add(item.Clone());

            return items;
        }

        public override void GetHitByAttack(BasicAttack attack, GameTime gameTime)
        {
            base.GetHitByAttack(attack, gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - LastTimeAttacked.TotalMilliseconds > 6000)
            {
                ResetSquare();
                CheckedLastAtackArea = false;
            }

            if (Health < 1 && attack.Source is Player player) //If the entity dies give xp to the player that killed it
            {
                player.GainXp(XP);
            }
        }

        public virtual void Respawn(MinMaxPair levelRange)
        { 
            ResetSquare();
            Motion = Vector2.Zero;
            SetEnemyLevel(levelRange);
            base.Respawn();
        }

        public virtual void AutoAttack(Player player, GameTime gameTime)
        {
            //attack the player if the player is close enough
            if (AttackArea.Intersects(player.Rectangle))
            {
                //TODO: add logic for other types of attacks (probably move this logic to the enemy)
                //so there can be attacks based on what the enemy is
                PerformAttack(gameTime, BasicAttack);
            }
        }

        public void WalkInSquare()
        {
            Vector2 movement = Vector2.Zero;

            if (x < walkDistance)
            {
                movement.Y = 0;
                movement.X = 1;
                x++;
            }
            else if (x == walkDistance)
            {
                if (y < walkDistance)
                {
                    movement.X = 0;
                    movement.Y = 1;
                    y++;
                }
                else
                {
                    if (x2 == walkDistance)
                    {
                        movement.X = 0;
                        movement.Y = -1;
                        y2++;

                        if (y2 == walkDistance)
                            ResetSquare();
                    }
                    else
                    {
                        movement.X = -1;
                        movement.Y = 0;
                        x2++;
                    }
                }
            }

            Motion = movement;
        }

        public void ResetSquare()
        {
            x = 0;
            y = 0;
            x2 = 0;
            y2 = 0;
        }
    }
}
