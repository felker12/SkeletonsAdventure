using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.EntityHelperClasses;

namespace SkeletonsAdventure.TileEngine
{
    internal class EntityLayer : Layer
    {
        EntityManager EntityManager { get; set; } = new();

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw all entities in the layer
            EntityManager.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime, GameTime totalTimeInWorld)
        {
            EntityManager.Update(gameTime, totalTimeInWorld);
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void AddEntity(Entity entity)
        {
            EntityManager.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            EntityManager.Remove(entity);
        }

        public void ClearEntities()
        {
            EntityManager.Clear();
        }
    }
}
