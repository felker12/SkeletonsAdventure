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
        public bool Visible { get; set; } = true;
        public float CooldownTime { get; set; } = 1000; //Time in milliseconds
        public TimeSpan LastInteractedTime { get; set; } = new();

        public InteractableObjectData() { }

        public InteractableObjectData(InteractableObjectData interactableObjectData)
        {
            TypeOfObject = interactableObjectData.TypeOfObject;
            Position = interactableObjectData.Position;
            Width = interactableObjectData.Width;
            Height = interactableObjectData.Height;
            Active = interactableObjectData.Active;
            Visible = interactableObjectData.Visible;
            CooldownTime = interactableObjectData.CooldownTime;
            LastInteractedTime = interactableObjectData.LastInteractedTime;
        }

        public virtual InteractableObjectData Clone()
        {
            return new InteractableObjectData(this);
        }
    }
}
