using Microsoft.Xna.Framework.Input;

namespace SkeletonsAdventure.Controls
{
    public class GameButton : Button
    {
        public bool TransformMouse { get; set; } = false;
        public Matrix Transformation { get; set; }

        public GameButton(Texture2D texture) : base(texture)
        {
            Position = new(-1000, -1000); // Start off-screen
        }

        public GameButton(Texture2D texture, SpriteFont font) : base(texture, font)
        {
            Position = new(-1000, -1000); // Start off-screen
        }

        public override void Update(GameTime gameTime)
        {
            if (TransformMouse)
                IsMouseHovering(TransformMouse, Transformation);
            else
                IsMouseHovering();

            Update();
        }

        public void IsMouseHovering(bool transformMouse, Matrix transformation)
        {
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();

            Vector2 mousePos = new(_currentMouse.X, _currentMouse.Y);
            Rectangle mouseRectangle = new(_currentMouse.X, _currentMouse.Y, 1, 1);

            if (transformMouse)
            {
                Vector2 transformedmousePos = Vector2.Transform(mousePos, Matrix.Invert(transformation)); //Mouse position in the world
                Rectangle transformedMouseRectangle = new((int)transformedmousePos.X, (int)transformedmousePos.Y, 1, 1);

                mouseRectangle = transformedMouseRectangle;
            }

            _isHovering = mouseRectangle.Intersects(Rectangle);
        }

        public void SetText(string text)
        {
            Text = text;
            Width = (int)SpriteFont.MeasureString(text).X;
            Height = (int)SpriteFont.MeasureString(text).Y;
        }
    }
}
