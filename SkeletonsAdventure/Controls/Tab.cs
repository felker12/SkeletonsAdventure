using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SkeletonsAdventure.Controls
{
    public class Tab(Texture2D texture, SpriteFont font)
    {
        public int Width { get; set; } = 30;
        public int Height { get; set; } = 30;
        public Vector2 Position { get; set; } = new();
        public string Text { get; set; } = "Tab";
        public Texture2D Texture { get; } = texture ?? throw new ArgumentNullException(nameof(texture));
        public SpriteFont SpriteFont { get; } = font ?? throw new ArgumentNullException(nameof(font));
        public bool Active { get; set; } = false;
        public bool Visible { get; set; } = false;
        public Rectangle Rectangle => new((int)Position.X, (int)Position.Y, Width, Height);
        public bool IsHovering { get; set; } = false;

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle bounds = Rectangle;
            spriteBatch.Draw(Texture, bounds, Active ? MenuStyle.Brass : MenuStyle.Border);
            bounds.Inflate(-1, -1);
            spriteBatch.Draw(Texture, bounds, Active || IsHovering ? MenuStyle.Selection : MenuStyle.Surface);

            if (!string.IsNullOrEmpty(Text))
            {
                Vector2 size = SpriteFont.MeasureString(Text);
                var position = new Vector2(bounds.Center.X - size.X / 2, bounds.Center.Y - SpriteFont.LineSpacing / 2f);
                spriteBatch.DrawString(SpriteFont, Text, position, Active || IsHovering ? MenuStyle.Ivory : MenuStyle.MutedText);
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
