using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SkeletonsAdventure.Controls
{
    public class Label : Control
    {
        public Label()
        {
            Initialize();
        }
        public Label(string text)
        {
            Text = text;
            Initialize();
        }

        public Label(SpriteFont font) : base(font)
        {
            Initialize();
        }

        private void Initialize()
        {
            TabStop = false;
        }

        public override void Update(GameTime gameTime)
        {
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!string.IsNullOrEmpty(Text))
                spriteBatch.DrawString(SpriteFont, Text, Position, TextColor);
        }
        public override void HandleInput(PlayerIndex playerIndex)
        {
        }
    }
}
