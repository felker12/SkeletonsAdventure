using System.Linq;

namespace SkeletonsAdventure.GameEvents
{
    internal class GameEventManager
    {
        public List<GameEvent> ActiveEvents { get; } = [];
        public List<TimeDelayedGameEvent> DelayedEvents { get; } = [];
        public string ActiveCount => $"GameEventManager(ActiveEvents Count: {ActiveEvents.Count})";

        public GameEventManager() { }

        public void Update(GameTime gameTime)
        {
            for (int i = DelayedEvents.Count - 1; i >= 0; i--)
            {
                var delayedEvent = DelayedEvents[i];
                delayedEvent.Update(gameTime);

                if (delayedEvent.IsComplete)
                {
                    ActiveEvents.Add(delayedEvent.StoredEvent);
                    DelayedEvents.RemoveAt(i);
                    Debug.WriteLine("delayed event completed");
                }
            }

            //iterate backwards to safely remove completed events
            for (int i = ActiveEvents.Count - 1; i >= 0; i--)
            {
                var gameEvent = ActiveEvents[i];
                gameEvent.Update(gameTime);

                if (gameEvent.IsComplete)
                    ActiveEvents.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var gameEvent in ActiveEvents)
                gameEvent.Draw(spriteBatch);
        }

        public void AddEvent(GameEvent gameEvent)
        {
            if (gameEvent == null)
                return;

            if (gameEvent is TimeDelayedGameEvent delayedEvent)
            {
                DelayedEvents.Add(delayedEvent);
                Debug.WriteLine("Delayed event added");
            }
            else
                ActiveEvents.Add(gameEvent);
        }

        public void ClearEvents()
        {
            ActiveEvents.Clear();
        }

        public override string ToString()
        {
            return $"Active Events: {string.Join(", ", ActiveEvents.Select(q => q.ToString()))}";
        }
    }
}
