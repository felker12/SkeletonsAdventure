using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.Entities.PlayerClasses;

namespace SkeletonsAdventure.GameObjects
{
    internal class InteractableObjectManager
    {
        private List<InteractableObject> InteractableObjects { get; set; } = [];
        public int Count => InteractableObjects.Count;
        public IReadOnlyList<InteractableObject> Objects => InteractableObjects;

        private readonly SpriteFont _font;

        public InteractableObjectManager(SpriteFont font)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
        }

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
            foreach (var objData in data.InteractableObjectsData)
            {
                // Keep map-created subtypes, quests, requirements, animations and callbacks.
                var obj = InteractableObjects.Find(candidate =>
                    candidate.Position == objData.Position && candidate.TypeOfObject == objData.TypeOfObject);
                if (obj is null)
                    throw new System.IO.InvalidDataException($"Unknown saved interactable: {objData.TypeOfObject}");
                obj.Active = objData.Active;
                obj.Visible = objData.Visible;
                obj.CooldownTime = objData.CooldownTime;
                obj.LastInteractedTime = objData.LastInteractedTime;
            }
        }
    }
}
