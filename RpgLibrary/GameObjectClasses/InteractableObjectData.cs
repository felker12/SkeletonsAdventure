using Microsoft.Xna.Framework;

namespace RpgLibrary.GameObjectClasses
{
    public class InteractableObjectData
    {
        public string TypeOfObject { get; set; } = string.Empty;
        public Vector2 Position { get; set; } = new();
        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;
        public bool Active { get; set; } = true;

        public InteractableObjectData() { }

        public InteractableObjectData(InteractableObjectData interactableObjectData)
        {
            TypeOfObject = interactableObjectData.TypeOfObject;
            Position = interactableObjectData.Position;
            Width = interactableObjectData.Width;
            Height = interactableObjectData.Height;
            Active = interactableObjectData.Active;
        }

        public virtual InteractableObjectData Clone()
        {
            return new InteractableObjectData(this);
        }
    }
}
