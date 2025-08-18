using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameUI
{
    internal class DamagePopUp
    {
        public string Text { get; set; } = string.Empty;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public Vector2 Motion { get; set; } = new Vector2(0, -1); // Default motion upwards
        public float Speed { get; set; } = 160f; // Default speed in pixels per second
        public float Duration { get; set; } = 0.8f; // Default duration
        public float ElapsedTime { get; private set; } = 0f;
        public SpriteFont Font { get; } = GameManager.Arial14;
        public bool TimedOut { get; private set; } = false; // Property to check if the pop-up has timed out
        public Color Color { get; set; } = Color.White; // Default color for the text

        public DamagePopUp(string text, Vector2 position)
        {
            Text = text;
            Position = position;
        }

        public DamagePopUp() { }

        public void Update(GameTime gameTime)
        {
            if(TimedOut == true)
                return; // If the pop-up has timed out, do not update

            ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ElapsedTime >= Duration)
            {
                TimedOut = true;
                return; // Stop updating if the duration has elapsed
            }

            Position += Motion * Speed * Game1.DeltaTime;
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (ElapsedTime < Duration && TimedOut == false)
            {
                spriteBatch.DrawString(Font, Text, Position, Color);
            }
        }

        public override string ToString()
        {
            return $"Position: {Position}, Duration: {Duration}, Elapsed Time: {ElapsedTime}";
        }
    }
}
