
namespace SkeletonsAdventure.GameEvents
{
    internal class TimeDelayedGameEvent : GameEvent
    {
        public GameEvent StoredEvent { get; set; } = null;

        public TimeDelayedGameEvent() { }

        public TimeDelayedGameEvent(GameEvent storedEvent, float duration = 200) : base(duration)
        {
            StoredEvent = storedEvent;
        }

        public override void Update(GameTime gameTime)
        {
            if (StoredEvent is null)
                return;

            base.Update(gameTime);
        }
    }
}
