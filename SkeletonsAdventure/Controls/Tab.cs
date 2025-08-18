using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Controls
{
    public class Tab()
    {
        public int Width { get; set; } = 30;
        public int Height { get; set; } = 30;
        public Vector2 Position { get; set; } = new();
        public string Text { get; set; } = "Tab";
        public Texture2D Texture { get; set; } = GameManager.GameMenuTexture;
        public SpriteFont SpriteFont { get; set; } = GameManager.Arial20;
        public bool Active { get; set; } = false;
        public bool Visible { get; set; } = false;
        private Color TintColor { get; set; } = Color.White;
        public Rectangle Rectangle => new((int)Position.X, (int)Position.Y, Width, Height);
        public bool IsHovering { get; set; } = false;

        public void Update()
        {
            if(Active)
            {
                TintColor = Color.Gray;
            }
            else
            {
                if (IsHovering)
                    TintColor = Color.LightGray;
                else
                    TintColor = Color.White;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Rectangle, TintColor);

            if (!string.IsNullOrEmpty(Text))
            {
                var x = (Rectangle.X + (Rectangle.Width / 2)) - (SpriteFont.MeasureString(Text).X / 2);
                var y = (Rectangle.Y + (Rectangle.Height / 2)) - (SpriteFont.MeasureString(Text).Y / 2);

                spriteBatch.DrawString(SpriteFont, Text, new Vector2(x, y), TintColor);
            }
        }

        public void ToggleActive()
        {
            Active = !Active;
        }

        public void ToggleVisibility()
        {
            Visible = !Visible;
        }
    }
}
