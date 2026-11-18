namespace SkeletonsAdventure.Controls
{
    public class Label : Control
    {
        public Label(SpriteFont font, string text = null) : base(font)
        {
            Text = text;
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
