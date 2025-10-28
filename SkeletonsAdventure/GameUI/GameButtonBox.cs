using Microsoft.Xna.Framework;
using SkeletonsAdventure.Controls;

namespace SkeletonsAdventure.GameUI
{
    internal class GameButtonBox : ButtonBox
    {
        public new List<GameButton> Buttons { get; set; } = [];

        public GameButtonBox(Vector2 pos, Texture2D texture, int width, int height) : base(pos, texture, width, height)
        {
            Position = pos;
            Texture = texture;
            Width = width;
            Height = height;
        }

        public GameButtonBox() : base()
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Rectangle, Color);

            foreach (GameButton button in Buttons)
            {
                if (button.Visible)
                    button.Draw(spriteBatch);
            }
        }

        public void Update(GameTime gameTime, bool transformMouse, Matrix transformation)
        {
            if (VisibleButtonsCount() > 0)
            {
                Height = VisibleButtonsHeight() + (int)ControlOffset.Y * 2;
                Width = LongestButtonTextLength() + (int)ControlOffset.X * 2;
                Vector2 offset = ControlOffset;

                foreach (GameButton button in Buttons)
                {
                    if (button.Visible)
                    {
                        button.TransformMouse = transformMouse;
                        button.Transformation = transformation;
                        button.Update(gameTime);
                        button.Position = Position + offset;
                        offset += new Vector2(0, button.Height);
                    }
                }
            }
            else
            {
                Height = 0;
                Width = 0;
            }
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            foreach (GameButton button in Buttons)
            {
                if (button.Visible)
                {
                    button.HandleInput(playerIndex);
                }
            }
        }

        public void AddButton(GameButton button, string buttonText)
        {
            button.SetText(buttonText);
            button.Visible = false;

            Buttons.Add(button);
        }

        public void AddButtons(Dictionary<string, GameButton> buttons)
        {
            foreach (KeyValuePair<string, GameButton> button in buttons)
            {
                AddButton(button.Value, button.Key);
            }
        }

        public void ClearButtons()
        {
            Buttons.Clear();
        }

        public override int VisibleButtonsCount()
        {
            int count = 0;
            foreach (GameButton button in Buttons)
            {
                if (button.Visible == true)
                    count++;
            }
            return count;
        }

        public override int VisibleButtonsHeight()
        {
            int heightCount = 0;

            foreach (GameButton button in Buttons)
            {
                if (button.Visible == true)
                    heightCount += button.Height;
            }

            return heightCount;
        }

        public override int LongestButtonTextLength()
        {
            int length = 0;

            foreach (GameButton button in Buttons)
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

        public void RecalculateSize()
        {
            if (VisibleButtonsCount() > 0)
            {
                Height = VisibleButtonsHeight() + (int)ControlOffset.Y * 2;
                Width = LongestButtonTextLength() + (int)ControlOffset.X * 2;
                Vector2 offset = ControlOffset;

                foreach (GameButton button in Buttons)
                {
                    if (button.Visible)
                    {
                        button.Position = Position + offset;
                        offset += new Vector2(0, button.Height);
                    }
                }
            }
            else
            {
                Height = 0;
                Width = 0;
            }
        }
    }
}
