using MonoGame.Extended.Tilemaps;
using SkeletonsAdventure.HelperClasses;

namespace SkeletonsAdventure.Animations
{
    internal class TiledAnimation : ICloneableGameClass<TiledAnimation>
    {
        public string Name { get; set; } = "TiledAnimation";
        public int TileWidth { get; set; } = 16;
        public int TileHeight { get; set; } = 16;
        public Texture2D Texture { get; }
        public TileAnimation Animation { get; }
        private float elapsedTime = 0f;
        private bool animate = true;

        private Rectangle Frame
        {
            get
            {
                if (animate is false)
                    return Rectangle.Empty;

                TileAnimationFrame currentFrame = Animation.GetCurrentFrame();

                if (currentFrame is null)
                    return Rectangle.Empty;

                int columns = Texture.Width / TileWidth;
                int x = (currentFrame.LocalTileId % columns) * TileWidth;
                int y = (currentFrame.LocalTileId / columns) * TileHeight;

                return new Rectangle(x, y, TileWidth, TileHeight);
            }
        }

        public TiledAnimation(string name, Texture2D texture, TileAnimation animation, int tileWidth = 16, int tileHeight = 16)
        {
            Name = name;
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            Animation = animation ?? throw new ArgumentNullException(nameof(animation));
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        public TiledAnimation(TilemapTileset tiledMapTileset, TilemapTileAnimation animation)
            : this(tiledMapTileset.Name, tiledMapTileset.Texture, new TileAnimation(animation),
                  tiledMapTileset.TileWidth, tiledMapTileset.TileHeight)
        {
        }

        public void Update(GameTime gameTime)
        {
            // Implementation for playing the animation
            if (animate is false)
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
                animate = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            if (animate is false)
                return;

            spriteBatch.Draw(Texture, position, Frame, Color.White);
        }

        public override string ToString()
        {
            return $"Name: {Name}, Texture: {Texture}, Animation: {Animation}";
        }

        public TiledAnimation Clone()
        {
            return new TiledAnimation(Name, Texture, Animation.Clone(), TileWidth, TileHeight);
        }
    }
}
