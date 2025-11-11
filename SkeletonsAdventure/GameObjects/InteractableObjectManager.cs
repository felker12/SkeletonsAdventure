using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.Entities.PlayerClasses;

namespace SkeletonsAdventure.GameObjects
{
    internal class InteractableObjectManager
    {
        private List<InteractableObject> InteractableObjects { get; set; } = [];
        public int Count => InteractableObjects.Count;

        public InteractableObjectManager() { }

        public void Update(GameTime gameTime, Player player)
        {
            foreach (var obj in InteractableObjects)
            {
                obj.Update(gameTime, player);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var obj in InteractableObjects)
            {
                obj.Draw(spriteBatch);
            }
        }

        public void Add(InteractableObject obj)
        {
            InteractableObjects.Add(obj);
        }
        public void Remove(InteractableObject obj)
        {
            InteractableObjects.Remove(obj);
        }

        public InteractableObjectManagerData ToData()
        {
            List<InteractableObjectData> data = [];

            foreach (var obj in InteractableObjects)
                data.Add(obj.ToData());

            return new InteractableObjectManagerData { InteractableObjectsData = data };
        }

        public void LoadFromData(InteractableObjectManagerData data)
        {
            InteractableObjects.Clear();

            foreach (var objData in data.InteractableObjectsData)
                InteractableObjects.Add(new(objData));
        }
    }
}
