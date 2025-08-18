using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.Engines;

namespace SkeletonsAdventure.Controls
{
    public class LinkLabel : Control
    {
        #region Fields and Properties
        public Color SelectedColor{ get; set; } = Color.Red;
        #endregion
        #region Constructor Region
        public LinkLabel()
        {
            Initialize();
        }

        public LinkLabel(SpriteFont font) : base(font)
        {
            Initialize();
        }

        public LinkLabel(string text) : base(text)
        {
            Initialize();
        }

        private void Initialize()
        {
            TabStop = true;
            HasFocus = false;
            Position = Vector2.Zero;
        }

        #endregion
        #region Abstract Methods
        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (HasFocus)
                spriteBatch.DrawString(SpriteFont, Text, Position, SelectedColor);
            else
                spriteBatch.DrawString(SpriteFont, Text, Position, TextColor);
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if (InputHandler.CheckMouseReleased(MouseButton.Left))
            {
                Width = (int)SpriteFont.MeasureString(Text).X;
                Height = (int)SpriteFont.MeasureString(Text).Y;

                if (Rectangle.Contains(InputHandler.MouseAsPoint))
                    base.OnSelected(null);
            }

            if (!HasFocus)
                return;

            if (InputHandler.KeyReleased(Keys.Enter) ||
            InputHandler.ButtonReleased(Buttons.A, playerIndex))
                base.OnSelected(null);
        }
        #endregion
    }
}