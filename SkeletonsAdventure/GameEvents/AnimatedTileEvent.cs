using SkeletonsAdventure.Animations;

namespace SkeletonsAdventure.GameEvents
{
    internal class AnimatedTileEvent : GameEvent
    {
        public TiledAnimation TiledAnimation { get; }
        public Vector2 Position { get; set; } = Vector2.Zero;

        public AnimatedTileEvent(TiledAnimation animation, Vector2 position)
        {
            TiledAnimation = animation ?? throw new ArgumentNullException(nameof(animation));
            Duration = TiledAnimation.Animation.TotalDuration;

            Position = position;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            TiledAnimation.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TiledAnimation.Draw(spriteBatch, Position);
        }

        public override string ToString()
        {
            return $"AnimatedTileEvent(Duration: {Duration}, ElapsedTime: {ElapsedTime}, IsComplete: {IsComplete}, TiledAnimation: {TiledAnimation})";
        }
    }
}
