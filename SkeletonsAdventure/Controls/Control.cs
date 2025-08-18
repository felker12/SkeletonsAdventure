using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.GameWorld;
using System;

namespace SkeletonsAdventure.Controls
{
    public abstract class Control
    {
        #region Event Region
        public event EventHandler Selected;

        protected MouseState _currentMouse;
        protected bool _isHovering = false;
        protected MouseState _previousMouse;
        #endregion
        #region Property Region
        public string Name { get; set; }
        public string Text { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector2 Position { get; set; }
        public virtual bool HasFocus { get; set; } = false;
        public bool Enabled { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool TabStop { get; set; } = false;
        public SpriteFont SpriteFont{ get; set; } = GameManager.Arial12;
        public Color TextColor { get; set; } = Color.White;
        public Color BackgroundColor { get; set; } = Color.White;
        public string Type { get; set; }
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            }
        }
        #endregion
        #region Constructor Region
        public Control()
        {
        }
        public Control(SpriteFont font)
        {
            SpriteFont = font;
        }
        public Control(string text)
        {
            Text = text;
        }

        #endregion
        #region Abstract Methods

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void HandleInput(PlayerIndex playerIndex);
        #endregion
        #region Virtual Methods
        protected virtual void OnSelected(EventArgs e)
        {
            Selected?.Invoke(this, e);
        }

        protected void IsMouseHovering()
        {
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();

            Rectangle mouseRectangle = new(_currentMouse.X, _currentMouse.Y, 1, 1);

            _isHovering = mouseRectangle.Intersects(Rectangle);
        }
        #endregion
    }
}