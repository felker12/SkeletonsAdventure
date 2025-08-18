using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameUI
{
    public class StatusBar()
    {
        public int Value { get; set; } = 1;
        public int MaxValue { get; set; } = 10;
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 12; //Don't have the Height less than 12 if Text is Visible
        public Vector2 Position { get; set; } = new(10, 10);
        public int BorderWidth { get; set; } = 1;
        public Color BorderColor { get; set; } = Color.Black;
        public Color BackgroundColor { get; set; } = Color.Gray;
        public Color BarColor { get; set; } = Color.Red;
        public Texture2D Texture { get; set; } = GameManager.StatusBarTexture;
        public bool Visible { get; set; } = true;
        public bool TextVisible { get; set; } = true;
        public float Transparency { get; set; } = 0.5f;

        private string statusBarText = string.Empty;
        

        public void Update(int value, int Max, Vector2 position)
        {
            if(Visible)
            {
                //Some Validation of variables
                if (Height < 10)
                    TextVisible = false;

                if (Transparency > 1)
                    Transparency = 1;
                else if (Transparency < 0)
                    Transparency = 0;

                //Update the status bar with the input variables
                Value = value;
                MaxValue = Max;
                Position = position;

                if(TextVisible)
                    statusBarText = $"{Value} / {MaxValue}";
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(Visible)
            {
                // Draw the border
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X - BorderWidth, (int)Position.Y - BorderWidth, Width + BorderWidth * 2, Height + BorderWidth * 2), BorderColor * Transparency);
                // Draw the background
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Width, Height), BackgroundColor * Transparency);
                // Draw the value
                float percentage = (float)Value / MaxValue;
                int barWidth = (int)(Width * percentage);
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, barWidth, Height), BarColor * Transparency);

                // Draw the text
                if(TextVisible)
                {
                    var textPosition = new Vector2(Position.X + Width / 2 - GameManager.Arial10.MeasureString(statusBarText).X / 2, Position.Y + Height / 2 - GameManager.Arial10.MeasureString(statusBarText).Y / 2);
                    spriteBatch.DrawString(GameManager.Arial10, statusBarText, textPosition, Color.White * Transparency);
                }
            }
        }

        public void ToggleVisibility()
        {
            Visible = !Visible;
        }
    }
}
