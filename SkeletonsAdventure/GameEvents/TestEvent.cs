
namespace SkeletonsAdventure.GameEvents
{
    internal class TestEvent : GameEvent
    {
        public string DisplayText { get; set; } = "Test Event";

        public TestEvent() 
        {
            Duration = 400;
        }

        public TestEvent(string text, float duration)
        {
            DisplayText = text;
            Duration = duration;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


            Debug.WriteLine($"TestEvent: {DisplayText}, ElapsedTime: {ElapsedTime}/{Duration}, IsComplete: {IsComplete}");
            // Additional update logic can go here
        }

    }
}
