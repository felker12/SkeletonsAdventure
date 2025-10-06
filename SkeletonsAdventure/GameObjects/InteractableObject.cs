using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.GameObjects
{
    internal class InteractableObject : AnimatedSprite
    {
        public string TypeOfObject { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;

        public InteractableObject(TiledMapObject obj) : base()
        {
            Position = new Vector2(obj.Position.X, obj.Position.Y);
            Width = (int)obj.Size.Width;
            Height = (int)obj.Size.Height;

            if (obj.Properties.TryGetValue("TypeOfObject", out TiledMapPropertyValue value))
                TypeOfObject = value.ToString();

            Initialize();
        }

        public InteractableObject(InteractableObject obj) : base()
        {
            TypeOfObject = obj.TypeOfObject;
            Info = obj.Info;
            Active = obj.Active;
            Width = obj.Width;
            Height = obj.Height;
            Position = obj.Position;
            Visible = obj.Visible;
        }

        public InteractableObject(InteractableObjectData obj)
        {
            TypeOfObject = obj.TypeOfObject;
            Active = obj.Active;
            Width = obj.Width;
            Height = obj.Height;
            Position = obj.Position;
            Visible = obj.Visible;

            Initialize();
        }

        private void Initialize()
        {
            Info.Position = Position;
            Info.Text = "Press R to Interact";
            Info.Visible = false;
            Info.TextColor = Color.GhostWhite;
        }

        public virtual void Update(GameTime gameTime, Player player)
        {
            if (!Visible || !Active)
                return;

            if (CheckPlayerNear(player))
                HandleInput(player);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible is false)
                return;

            if (Info.Visible)
                Info.Draw(spriteBatch);

            spriteBatch.DrawRectangle(Rectangle, SpriteColor, 1, 0); //TODO
        }

        public virtual InteractableObject Clone()
        {
            return new InteractableObject(this);
        }

        public InteractableObjectData GetInteractableObjectData()
        {
            return new InteractableObjectData()
            {
                TypeOfObject = TypeOfObject,
                Position = Position,
                Width = Width,
                Height = Height,
                Active = Active,
                Visible = Visible,
            };
        }

        public virtual bool CheckPlayerNear(Player player)
        {
            if (Rectangle.Intersects(player.Rectangle))
                Info.Visible = true;
            else
                Info.Visible = false;

            return Info.Visible;
        }

        public virtual void HandleInput(Player player)
        {
            // This method can be overridden in derived classes to handle specific input logic
            if (InputHandler.KeyReleased(Keys.R) || InputHandler.ButtonDown(Buttons.A, PlayerIndex.One))
            {
                Interact(player);
            }
        }

        public virtual void Interact(Player player)
        {
            // This method can be overridden in derived classes to provide specific interaction logic
            Debug.WriteLine($"Interacting with {TypeOfObject} at {Position}" +
                $", of type {this.GetType().Name}");
        }
    }
}
