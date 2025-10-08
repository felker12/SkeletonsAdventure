namespace SkeletonsAdventure.GameEvents
{
    internal class GameEvent
    {
        public float Duration { get; set; } = 0; //in milliseconds
        public float ElapsedTime { get; private set; } = 0;
        public bool IsComplete => ElapsedTime >= Duration;

        public GameEvent() { }

        public GameEvent(float duration)
        {
            Duration = duration;
        }

        public virtual void Update(GameTime gameTime)
        {
            ElapsedTime += gameTime.ElapsedGameTime.Milliseconds;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // Base implementation does nothing
        }

        public override string ToString()
        {
            return $"GameEvent(Duration: {Duration}, ElapsedTime: {ElapsedTime}, IsComplete: {IsComplete})";
        }
    }
}
