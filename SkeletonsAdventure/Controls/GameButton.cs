using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SkeletonsAdventure.Controls
{
    public class GameButton : Button
    {
        public bool TransformMouse { get; set; } = false;
        public Matrix Transformation { get; set; }

        public GameButton(Texture2D texture) : base(texture)
        {
        }

        public GameButton(Texture2D texture, SpriteFont font) : base(texture, font)
        {
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
    }
}
