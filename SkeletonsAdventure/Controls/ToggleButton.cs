using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SkeletonsAdventure.Controls
{
    internal class ToggleButton : Button
    {
        public bool Toggled { get; private set; } = false;
        public Color ToggledColor { get; set; } = Color.LightGray;
        public string ToggledText { get; set; } = string.Empty;

        public ToggleButton() : base()
        {

        }

        public ToggleButton(SpriteFont font) : base(font)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color color;
            string text;

            if (Toggled)
            {
                color = ToggledColor;
                text = ToggledText;
            }
            else
            {
                color = BackgroundColor; 
                text = Text;
            }

            spriteBatch.Draw(Texture, Rectangle, color);

            if (!string.IsNullOrEmpty(text))
            {
                var x = (Rectangle.X + (Rectangle.Width / 2)) - (SpriteFont.MeasureString(Text).X / 2);
                var y = (Rectangle.Y + (Rectangle.Height / 2)) - (SpriteFont.MeasureString(Text).Y / 2);

                spriteBatch.DrawString(SpriteFont, text, new Vector2(x, y), TextColor);
            }
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if (Clicked)
                Toggle();

            base.HandleInput(playerIndex);
        }

        private void Toggle()
        {
            if (Toggled)
                Toggled = false;
            else
                Toggled = true;
        }
    }
}
