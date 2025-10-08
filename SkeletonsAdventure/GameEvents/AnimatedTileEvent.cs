using SkeletonsAdventure.Animations;

namespace SkeletonsAdventure.GameEvents
{
    internal class AnimatedTileEvent : GameEvent
    {
        public TiledAnimation TiledAnimation { get; set; } = null;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public AnimatedTileEvent() { }

        public AnimatedTileEvent(TiledAnimation animation, Vector2 position)
        {
            SetAnimation(animation);

            Position = position;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            TiledAnimation?.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TiledAnimation?.Draw(spriteBatch, Position);
        }

        public void SetAnimation(TiledAnimation animation)
        {
            TiledAnimation = animation;
            if (TiledAnimation?.Animation?.Frames is not null)
                Duration = TiledAnimation.Animation.TotalDuration;
        }

        public override string ToString()
        {
            return $"AnimatedTileEvent(Duration: {Duration}, ElapsedTime: {ElapsedTime}, IsComplete: {IsComplete}, TiledAnimation: {TiledAnimation})";
        }
    }
}
