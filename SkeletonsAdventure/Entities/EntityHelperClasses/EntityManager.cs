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
            ArgumentNullException.ThrowIfNull(entity);
            if (Entities.Contains(entity))
                return;
            Entities.Add(entity);
            entity.Died += OnEntityDied;

            if (entity is Player player)
                Player = player;
        }

        public void Remove(Entity entity)
        {
            if (Entities.Remove(entity))
                entity.Died -= OnEntityDied;
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
                    CheckEntityHealth(totalTimeInWorld, entity);
                    if (entity.IsDead)
                        continue;

                    entity.AttackManager.CheckAttackHit(Entities, gameTime);

                    entity.Update(gameTime);

                    if (entity == Player)
                        PickUpLoot(); //If the player walks over loot pick it up
                }
                else if (totalTimeInWorld.TotalGameTime - entity.LastDeathTime >= TimeSpan.FromSeconds(entity.RespawnTime))
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

            }
        }

        private void OnEntityDied(Entity entity)
        {
            if (entity is Enemy enemy)
                DroppedLootManager.Add(enemy.GetDrops(), entity.Position);
        }

        public void Clear()
        {
            foreach (var entity in Entities)
                entity.Died -= OnEntityDied;
            Entities.Clear();
        }

        public void PickUpLoot()
        {
            if (Player is null || Player.IsDead || Player.Health <= 0)
                return;
            // Consume pickups immediately, so repeated pickup requests cannot duplicate them.
            for (int i = DroppedLootManager.Items.Count - 1; i >= 0; i--)
            {
                var item = DroppedLootManager.Items[i];
                if (Player.Rectangle.Intersects(item.ItemRectangle) && Player.Backpack.Add(item) == true)
                    DroppedLootManager.Items.RemoveAt(i);
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
