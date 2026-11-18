using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.HelperClasses;
using SkeletonsAdventure.Quests;

namespace SkeletonsAdventure.GameObjects
{
    internal class InteractableObject : ICloneableGameClass<InteractableObject>
    {
        public Vector2 Position { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Color SpriteColor { get; set; } = Color.White;
        public Label Info { get; }
        public Rectangle Rectangle => new((int)Position.X, (int)Position.Y, Width, Height);
        public string TypeOfObject { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;
        public float CooldownTime { get; set; } = 1000; //Time in milliseconds
        public TimeSpan LastInteractedTime { get; set; } = new();
        public LevelRequirements LevelRequirements { get; set; } = null;
        public KillRequirements KillRequirements { get; set; } = null;
        public string InfoText { get; set; } = "Press R to Interact";

        public InteractableObject(TiledMapObject obj, SpriteFont font)
        {
            Info = new(font);
            Position = new Vector2(obj.Position.X, obj.Position.Y);
            Width = (int)obj.Size.Width;
            Height = (int)obj.Size.Height;

            if (obj.Properties.TryGetValue("TypeOfObject", out TiledMapPropertyValue value))
                TypeOfObject = value.ToString();

            Initialize();
        }

        public InteractableObject(InteractableObject obj)
        {
            TypeOfObject = obj.TypeOfObject;
            Info = new(obj.Info.SpriteFont, obj.Info.Text)
            {
                Visible = obj.Info.Visible,
                Position = obj.Info.Position,
                TextColor = obj.Info.TextColor
            };
            Active = obj.Active;
            Width = obj.Width;
            Height = obj.Height;
            Position = obj.Position;
            Visible = obj.Visible;
            CooldownTime = obj.CooldownTime;
            LastInteractedTime = obj.LastInteractedTime;
            LevelRequirements = obj.LevelRequirements;
            KillRequirements = obj.KillRequirements;
        }

        public InteractableObject(InteractableObjectData obj, SpriteFont font)
        {
            Info = new(font);
            TypeOfObject = obj.TypeOfObject;
            Active = obj.Active;
            Width = obj.Width;
            Height = obj.Height;
            Position = obj.Position;
            Visible = obj.Visible;
            CooldownTime = obj.CooldownTime;
            LastInteractedTime = obj.LastInteractedTime;

            Initialize();
        }

        private void Initialize()
        {
            Info.Position = Position;
            Info.Text = InfoText;
            Info.Visible = false;
            Info.TextColor = Color.GhostWhite;
        }

        public virtual void Update(GameTime gameTime, Player player)
        {
            if (!Visible || !Active)
                return;

            if (CheckPlayerNear(player))
            {
                if (LevelRequirements is not null)
                {
                    if (LevelRequirements.CheckRequirements(player) is false)
                    {
                        Info.Text = LevelRequirements.MissingRequirementsText(player);
                        return;
                    }
                }
                
                if (KillRequirements is not null)
                {
                    if (KillRequirements.CheckRequirements(player) is false)
                    {
                        Info.Text = KillRequirements.MissingRequirementsText(player);
                        return;
                    }
                }


                Info.Text = InfoText; //reset in case it was changed by requirements
                HandleInput(gameTime, player);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
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

        public InteractableObjectData ToData()
        {
            return new InteractableObjectData()
            {
                TypeOfObject = TypeOfObject,
                Position = Position,
                Width = Width,
                Height = Height,
                Active = Active,
                Visible = Visible,
                CooldownTime = CooldownTime,
                LastInteractedTime = LastInteractedTime
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

        public virtual void HandleInput(GameTime gameTime, Player player)
        {
            // This method can be overridden in derived classes to handle specific input logic
            if (InputHandler.KeyReleased(Keys.R) || InputHandler.ButtonDown(Buttons.A, PlayerIndex.One))
            {
                Interact(gameTime, player);
            }
        }

        public virtual void Interact(GameTime gameTime, Player player)
        {
            // Check for cooldown
            if (LastInteractedTime + TimeSpan.FromMilliseconds(CooldownTime) < gameTime.TotalGameTime is false)
                return;

            LastInteractedTime = gameTime.TotalGameTime;

            //TODO
            /*
            Debug.WriteLine($"Interacting with {TypeOfObject} at {Position}" +
                $", of type {this.GetType().Name}, at GameTime: {gameTime.TotalGameTime}");
            */
        }
    }
}
