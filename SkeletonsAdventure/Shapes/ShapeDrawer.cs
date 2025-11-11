
using MonoGame.Extended;

namespace SkeletonsAdventure.Shapes
{
    internal static class ShapeDrawer
    {
        public static void DrawLine(Line line, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawLine(line.Point1, line.Point2, line.Color, line.Thickness);
        }

        public static void DrawLine(Line line, SpriteBatch spriteBatch, Color color, int thickness = 1)
        {
            spriteBatch.DrawLine(line.Point1, line.Point2, color, thickness);
        }

        public static void DrawTriangle(Triangle triangle, SpriteBatch spriteBatch)
        {
            foreach(Line line in triangle.Lines)
            {
                DrawLine(line, spriteBatch);
            }
        }
    }
}
