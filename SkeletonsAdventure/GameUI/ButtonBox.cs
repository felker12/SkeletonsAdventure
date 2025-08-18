using MonoGame.Extended;
using SkeletonsAdventure.Controls;

namespace SkeletonsAdventure.GameUI
{
    public class ButtonBox : ControlBox
    {
        public List<Button> Buttons { get; set; } = [];

        public ButtonBox(Vector2 pos, Texture2D texture, int width, int height) : base(pos, texture, width, height)
        {
        }

        public ButtonBox() : base()
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible is false)
                return;

            spriteBatch.Draw(Texture, Rectangle, Color);

            if (DrawOutline)
                spriteBatch.DrawRectangle(Rectangle, OutlineColor, 2, 0);

            foreach (Button button in Buttons)
            {
                if (button.Visible)
                    button.Draw(spriteBatch);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Visible is false)
                return;

            Vector2 offset = ControlOffset;
            foreach (Button button in Buttons)
            {
                if (button.Visible)
                {
                    button.Update(gameTime);
                    button.Position = Position + offset;
                    offset += new Vector2(0, button.Height);
                }
            }
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if (Visible is false)
                return;

            foreach (Button button in Buttons)
            {
                if (button.Visible)
                {
                    button.HandleInput(playerIndex);
                }
            }
        }

        public virtual void AddButton(Button button, string buttonText)
        {
            button.Text = buttonText;
            button.Visible = false;
            button.Width = (int)button.SpriteFont.MeasureString(buttonText).X;
            button.Height = (int)button.SpriteFont.MeasureString(buttonText).Y;

            Buttons.Add(button);
        }

        public virtual void AddButtons(Dictionary<string, Button> buttons)
        {
            foreach (KeyValuePair<string, Button> button in buttons)
            {
                AddButton(button.Value, button.Key);
            }
        }

        public virtual int VisibleButtonsCount()
        {
            int count = 0;
            foreach (Button button in Buttons)
            {
                if (button.Visible == true)
                    count++;
            }
            return count;
        }

        public virtual int VisibleButtonsHeight()
        {
            int heightCount = 0;

            foreach (Button button in Buttons)
            {
                if (button.Visible == true)
                    heightCount += button.Height;
            }

            return heightCount;
        }

        public virtual int LongestButtonTextLength()
        {
            int length = 0;

            foreach (Button button in Buttons)
            {
                if (button.Visible == true)
                {
                    if (button.SpriteFont.MeasureString(button.Text).X > length)
                    {
                        length = (int)button.SpriteFont.MeasureString(button.Text).X;
                    }
                }
            }

            return length;
        }
    }
}
