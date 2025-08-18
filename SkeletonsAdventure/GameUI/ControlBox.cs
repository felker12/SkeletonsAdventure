using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameUI
{
    public abstract class ControlBox
    {
        public Vector2 Position { get; set; } = new();
        public Vector2 ControlOffset { get; set; } = new(4, 4);
        public Texture2D Texture { get; set; } = GameManager.ButtonBoxTexture;
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 100;
        public Color Color { get; set; } = Color.White;
        public bool Visible { get; set; } = false;
        public Rectangle Rectangle => new((int)Position.X, (int)Position.Y, Width, Height);
        public bool DrawOutline { get; set; } = false;
        public Color OutlineColor { get; set; } = Color.Black;

        public ControlBox(Vector2 pos, Texture2D texture, int width, int height)
        {
            Position = pos;
            Texture = texture;
            Width = width;
            Height = height;
        }

        public ControlBox()
        {
        }

        public abstract void Draw(SpriteBatch spriteBatch);

        public abstract void Update(GameTime gameTime);

        public abstract void HandleInput(PlayerIndex playerIndex);
    }
}
