using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.GameWorld;
using System;

namespace SkeletonsAdventure.Controls
{
    public class Button : Control
    {
        #region Fields
        #endregion
        #region Properties
        public event EventHandler Click;
        public Texture2D Texture { get; set; } = GameManager.ButtonTexture;
        public Color DisabledColor { get; set; } = Color.DarkGray;
        public Color HoverColor { get; set; } = Color.Gray;
        public bool Clicked { get; private set; } = false;
        #endregion
        #region Constructors
        public Button()
        {
            Initialize();
        }

        public Button(string text) : base(text)
        {
            Initialize();
        }

        public Button(SpriteFont font) : base(font)
        {
            Initialize();
        }

        public Button(Texture2D texture)
        {
            Texture = texture;
            Initialize();
        }

        public Button(Texture2D texture, SpriteFont font) : base(font)
        {
            Texture = texture;
            Initialize();
        }

        private void Initialize()
        {
            Width = Texture.Width;
            Height = Texture.Height;
            TextColor = Color.Black;
        }

        #endregion
        #region Methods

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color color = BackgroundColor;

            if (_isHovering)
                color = HoverColor;

            if (Enabled is false)
                color = DisabledColor;

            spriteBatch.Draw(Texture, Rectangle, color);

            if (!string.IsNullOrEmpty(Text))
            {
                var x = (Rectangle.X + (Rectangle.Width / 2)) - (SpriteFont.MeasureString(Text).X / 2);
                var y = (Rectangle.Y + (Rectangle.Height / 2)) - (SpriteFont.MeasureString(Text).Y / 2);

                spriteBatch.DrawString(SpriteFont, Text, new Vector2(x, y), TextColor);
            }
        }

        public override void Update(GameTime gameTime)
        {
            IsMouseHovering();
            Update();
        }

        //This method wont work with the button being used in the ControlManager
        protected void Update()
        {
            if (_isHovering && _currentMouse.LeftButton == ButtonState.Released 
                && _previousMouse.LeftButton == ButtonState.Pressed)
            {
                Clicked = true;
            }
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if(Clicked)
            {
                Click?.Invoke(this, new EventArgs());
                Clicked = false; //Reset the clicked state after handling the click event
            }
        }
        #endregion
    }
}
