using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Controls
{
    internal class TextBox : Control
    {
        public Texture2D Background { get; set; }
        public Color BackgroundOutlineColor { get; set; } = Color.Black;

        public TextBox() : base()
        {
            Initialize();
        }

        public TextBox(SpriteFont font) : base(font)
        {
            Initialize();
        }

        private void Initialize()
        {
            TextColor = Color.Black;
            Background = GameManager.TextBoxTexture;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible is false)
                return;

            spriteBatch.Draw(Background, Rectangle, Color.White);
            spriteBatch.DrawRectangle(Rectangle, BackgroundOutlineColor, 2, 0);

            if (Text == string.Empty || Text is null)
                return;

            spriteBatch.DrawString(SpriteFont, Text, Position + new Vector2(5, 5), TextColor);
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if (Visible is false)
                return;

        }

        public override void Update(GameTime gameTime)
        {
            if (Visible is false)
                return;

        }
    }
}
