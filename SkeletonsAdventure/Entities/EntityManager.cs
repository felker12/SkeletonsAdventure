using MonoGame.Extended.Tiled;
using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;

namespace SkeletonsAdventure.Entities
{
    internal class EntityManager
    {
        private List<Entity> Entities { get; } = []; //list of all entities in the level, including the player
        public DroppedLootManager DroppedLootManager { get; } = new(); //used to manage the loot dropped by dead entities
        public Player Player { get; set; } = World.Player;

        public EntityManager(){}

        public void Add(Entity entity)
        {
            Entities.Add(entity);

            if (entity is Player player)
                Player = player;
        }

        public void Remove(Entity entity)
        {
            Entities.Remove(entity);

            /*if (entity is Player)
                Player = World.Player;*/
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DroppedLootManager.Draw(spriteBatch);

            foreach (Entity entity in Entities)
            {
                if(entity.IsDead == false)
                    entity.Draw(spriteBatch);
            }

            //draw the attacks after the entities so the attacks are always on top
            foreach (Entity entity in Entities)
                entity.AttackManager.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime, GameTime totalTimeInWorld)
        {
            UpdateEntities(gameTime, totalTimeInWorld);
            CheckIfEnemyDetectPlayer(gameTime);

            DroppedLootManager.Update();
        }

        private void UpdateEntities(GameTime gameTime, GameTime totalTimeInWorld)
        {
            foreach (var entity in Entities)
            {
                entity.AttackManager.ClearExpiredAttacks(gameTime);

                if (entity.IsDead == false) //Ensure dead enemies cannot be hit
                {
                    entity.AttackManager.CheckAttackHit(Entities, gameTime); //TODO: this might have to be changed to totalTimeInWorld instead later

                    CheckEntityHealth(totalTimeInWorld, entity);

                    entity.Update(gameTime);

                    if (entity == Player)
                        PickUpLoot(); //If the player walks over loot pick it up
                }
                else if (entity.IsDead && totalTimeInWorld.TotalGameTime - entity.LastDeathTime > new TimeSpan(0, 0, entity.RespawnTime))
                {
                    entity.Respawn();
                }
            }
        }

        private void CheckEntityHealth(GameTime totalTimeInWorld, Entity entity)
        {
            if (entity.Health < 1)
            {
                entity.EntityDied(totalTimeInWorld);

                if (entity is Enemy enemy && enemy.DropTableName != string.Empty && enemy.GetDrops().Count > 0)
                {
                    //if the enemy has a drop table then drop the loot
                    DroppedLootManager.Add(enemy.GetDrops(), entity.Position);
                }
            }
        }

        public void Clear()
        {
            Entities?.Clear();
        }

        public void PickUpLoot()
        {
            foreach (GameItem item in DroppedLootManager.Items)
            {
                if (Player.Rectangle.Intersects(item.ItemRectangle) && Player.Backpack.Add(item) == true)
                    DroppedLootManager.Remove(item);
            }
        }

        public EntityManagerData GetEnemyData()
        {
            EntityManagerData entityManagerData = new();

            foreach (Entity entity in Entities)
            {
                if (entity is Enemy enemy)
                    entityManagerData.EntityData.Add(enemy.GetEntityData());
            }

            return entityManagerData;
        }

        private void CheckIfEnemyDetectPlayer(GameTime gameTime)
        {
            foreach (Entity entity in Entities)
            {
                if (entity is not Enemy enemy || enemy.IsDead)
                    continue;

                HandleEnemyDetection(gameTime, enemy);
            }
        }

        private void HandleEnemyDetection(GameTime gameTime, Enemy enemy)
        {
            //if the player is close then go to the player
            if (CheckForPlayer(enemy))
                enemy.AutoAttack(Player, gameTime);

            //if the enemy has attacked by an out of sight enemy then move to the point the enemy was at when  
            else if (enemy.LastTimeAttacked.TotalMilliseconds != 0
                && gameTime.TotalGameTime.TotalMilliseconds - enemy.LastTimeAttacked.TotalMilliseconds < 6000
                && enemy.CheckedLastAtackArea == false)
            {
                InvestigateLastAttackLocation(gameTime, enemy);
            }
            else //if no player in sight do something
                EnemyIdle(enemy);
        }

        private void InvestigateLastAttackLocation(GameTime gameTime, Enemy enemy)
        {
            //if the enemy detects player on way then mark area checked so the enemy doesn't keep moving towards that point
            if (CheckForPlayer(enemy))
                enemy.CheckedLastAtackArea = true;

            //move to the point the enemy was attacked from
            if (enemy.Rectangle.Intersects(new((int)enemy.PositionLastAttackedFrom.X, (int)enemy.PositionLastAttackedFrom.Y, 1, 1)))
            {
                enemy.CheckedLastAtackArea = true;
                if (CheckForPlayer(enemy))
                    enemy.AutoAttack(Player, gameTime);
                else
                    EnemyIdle(enemy);
            }
            else
                enemy.PathToPoint(enemy.PositionLastAttackedFrom);
        }


        //Do something when the enemy is idle
        private static void EnemyIdle(Enemy enemy)
        {
            enemy.WalkInSquare();
        }

        public bool CheckForPlayer(Enemy enemy)
        {
            bool playerInRange = false;

            if (enemy.DetectionArea.Intersects(Player.Rectangle))
                playerInRange = true;

            enemy.Motion = Vector2.Zero; //Stops any motion caused by another method

            //if the enemy detects the player then move towerds the player
            if (!enemy.Rectangle.Intersects(Player.Rectangle))
                enemy.PathToPoint(Player.Position);
            else
                enemy.FaceTarget(Player);

            return playerInRange;
        }

       

        public void CheckEntityBoundaryCollisions(TiledMapTileLayer mapCollisionLayer, int tileWidth, int tileHeight)
        {
            foreach (Entity entity in Entities)
            {
                //Check entity collision with the map boundaries
                if (entity.CanMove)
                    CheckCollision(entity, mapCollisionLayer, tileWidth, tileHeight);

                //Check entity attacks collision with the map boundaries
                foreach (BasicAttack entityAttack in entity.AttackManager.Attacks)
                {
                    if (entityAttack.CanMove)
                        CheckCollision(entityAttack, mapCollisionLayer, tileWidth, tileHeight);
                }
            }
        }

        private static void CheckCollision(AnimatedSprite entity,TiledMapTileLayer mapCollisionLayer, int tileWidth, int tileHeight)
        {
            if (entity.Motion == Vector2.Zero)
                return;

            if (mapCollisionLayer == null)
            {
                entity.Position += entity.Motion * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier;
                return;
            }

            Vector2 pos = entity.Position,
                motion = entity.Motion;
            Rectangle rec = entity.Rectangle;
            bool xBlocked, yBlocked;

            // --- Y axis ---
            if(motion.Y != 0)
            {
                yBlocked = CheckYAxis(entity, mapCollisionLayer, pos, motion, rec, tileWidth, tileHeight);

                if (yBlocked)
                    motion.Y = 0;
            }

            // --- X axis ---
            if(motion.X != 0)
            {
                xBlocked = CheckXAxis(entity, mapCollisionLayer, pos, motion, rec, tileWidth, tileHeight);

                if (xBlocked)
                    motion.X = 0;
            }

            entity.Motion = motion;
            entity.Position += entity.Motion * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier;
        }

        public void CheckEntityBoundaryCollisions(TiledMapTileLayer[] mapCollisionLayers, int tileWidth, int tileHeight)
        {
            foreach (Entity entity in Entities)
            {
                //Check entity collision with the map boundaries
                if (entity.CanMove)
                    CheckCollision(entity, mapCollisionLayers, tileWidth, tileHeight);

                //Check entity attacks collision with the map boundaries
                foreach (BasicAttack entityAttack in entity.AttackManager.Attacks)
                {
                    if (entityAttack.CanMove)
                        CheckCollision(entityAttack, mapCollisionLayers, tileWidth, tileHeight);
                }
            }
        }

        private static void CheckCollision(AnimatedSprite entity, TiledMapTileLayer[] mapCollisionLayers, int tileWidth, int tileHeight)
        {
            if (entity.Motion == Vector2.Zero)
                return;

            if (mapCollisionLayers == null)
            {
                entity.Position += entity.Motion * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier;
                return;
            }

            Vector2 pos = entity.Position,
                motion = entity.Motion;
            Rectangle rec = entity.Rectangle;
            bool xBlocked = false, yBlocked = false;

            foreach(var layer in mapCollisionLayers)
            {
                if (layer is null)
                    continue;

                if(layer.Name == "ConditionalLayer" && layer.IsVisible is false)
                    continue;

                // --- Y axis ---
                if (motion.Y != 0)
                {
                    if (yBlocked is false)
                        yBlocked = CheckYAxis(entity, layer, pos, motion, rec, tileWidth, tileHeight);

                    if (yBlocked)
                        motion.Y = 0;
                }

                // --- X axis ---
                if (motion.X != 0)
                {
                    if (xBlocked is false)
                    {
                        xBlocked = CheckXAxis(entity, layer, pos, motion, rec, tileWidth, tileHeight);

                        if (xBlocked)
                            motion.X = 0;
                    }
                }

                if (motion == Vector2.Zero
                    || (xBlocked && yBlocked))
                    break;
            }
           
            entity.Motion = motion;
            entity.Position += entity.Motion * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier;
        }

        private static bool CheckXAxis(AnimatedSprite entity, TiledMapTileLayer mapCollisionLayer, Vector2 pos, Vector2 motion, Rectangle rec, int width, int height)
        {
            Vector2 newPosX = new(pos.X + motion.X * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier, pos.Y + motion.Y * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier);
            Rectangle newRectX = new((int)newPosX.X, (int)newPosX.Y, rec.Width, rec.Height);
            int checkX = motion.X > 0 ? (newRectX.Right - 1) / width : newRectX.Left / width;
            for (int y = newRectX.Top / height; y <= (newRectX.Bottom - 1) / height; y++)
            {
                if (IsTileBlocked(checkX, y, mapCollisionLayer))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CheckYAxis(AnimatedSprite entity, TiledMapTileLayer mapCollisionLayer, Vector2 pos, Vector2 motion, Rectangle rec, int width, int height)
        {
            Vector2 newPosY = new(pos.X, pos.Y + motion.Y * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier);
            Rectangle newRectY = new((int)newPosY.X, (int)newPosY.Y, rec.Width, rec.Height);
            int checkY = motion.Y > 0 ? (newRectY.Bottom - 1) / height : newRectY.Top / height;
            for (int x = newRectY.Left / width; x <= (newRectY.Right - 1) / width; x++)
            {
                if (IsTileBlocked(x, checkY, mapCollisionLayer))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTileBlocked(int x, int y, TiledMapTileLayer mapCollisionLayer)
        {
            return mapCollisionLayer.TryGetTile((ushort)x, (ushort)y, out TiledMapTile? tile) && tile.Value.IsBlank is false;
        }
    }
}
