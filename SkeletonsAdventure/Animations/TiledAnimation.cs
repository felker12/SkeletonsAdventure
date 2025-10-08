

namespace SkeletonsAdventure.Animations
{
    internal class TiledAnimation
    {
        public string Name { get; set; } ="TiledAnimation";
        public int TileWidth { get; set; } = 16;
        public int TileHeight { get; set; } = 16;
        public Texture2D Texture { get; set; } = null;
        public TileAnimation Animation { get; set; } = null;
        private float elapsedTime = 0f;
        private bool animate = true;

        private Rectangle Frame
        {
            get
            {
                if (Animation is null || Texture is null)
                    return Rectangle.Empty;

                TileAnimationFrame currentFrame = Animation.GetCurrentFrame();

                if (currentFrame is null)
                    return Rectangle.Empty;

                int columns = Texture.Width / TileWidth;
                int x = (currentFrame.TileId % columns) * TileWidth;
                int y = (currentFrame.TileId / columns) * TileHeight;

                return new Rectangle(x, y, TileWidth, TileHeight);
            }
        }

        public TiledAnimation() { }

        public TiledAnimation(string name, Texture2D texture, TileAnimation animation, int tileWidth = 16, int tileHeight = 16)
        {
            Name = name;
            Texture = texture;
            Animation = animation;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        public void Update(GameTime gameTime)
        {
            // Implementation for playing the animation

            if (Animation is null || Texture is null || animate is false)
                return;

            elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            TileAnimationFrame currentFrame = Animation.GetCurrentFrame();

            if (currentFrame is not null)
                if(currentFrame.Duration < elapsedTime)
                {
                    elapsedTime = 0f;
                    Animation.CurrentFrameIndex++;

                    if (Animation.CurrentFrameIndex >= Animation.Frames.Count)
                        Animation.CurrentFrameIndex = 0;
                }

            if (elapsedTime >= Animation.TotalDuration)
            {
                elapsedTime = 0f;
                animate = false; // Uncomment this line to stop after one playthrough
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            if (Animation == null || Texture == null || animate is false)
                return;

            spriteBatch.Draw(Texture, position, Frame, Color.White);
        }

        public override string ToString()
        {
            return $"Name: {Name}, Texture: {Texture}, Animation: {Animation}";
        }
    }
}
