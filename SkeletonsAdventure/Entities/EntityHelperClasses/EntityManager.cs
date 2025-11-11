using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.ItemClasses.ItemManagement;

namespace SkeletonsAdventure.Entities.EntityHelperClasses
{
    internal class EntityManager()
    {
        public List<Entity> Entities { get; } = []; //list of all entities in the level, including the player
        public DroppedLootManager DroppedLootManager { get; } = new(); //used to manage the loot dropped by dead entities
        public Player Player { get; set; } = World.Player;

        public void Add(Entity entity)
        {
            Entities.Add(entity);

            if (entity is Player player)
                Player = player;
        }

        public void Remove(Entity entity)
        {
            Entities.Remove(entity);
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
            EnemyAI.CheckIfEnemyDetectPlayer(gameTime, Entities, Player);

            DroppedLootManager.Update();
        }

        private void UpdateEntities(GameTime gameTime, GameTime totalTimeInWorld)
        {
            foreach (var entity in Entities)
            {
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

                entity.AttackManager.ClearExpiredAttacks();
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

        public EntityManagerData ToData()
        {
            EntityManagerData entityManagerData = new();

            foreach (Entity entity in Entities)
            {
                if (entity is Enemy enemy)
                    entityManagerData.EntityData.Add(enemy.ToData());
            }

            return entityManagerData;
        }
    }
}
