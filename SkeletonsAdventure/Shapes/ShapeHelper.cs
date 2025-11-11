
using System.Linq;

namespace SkeletonsAdventure.Shapes
{
    internal static class ShapeHelper
    {
        public static Vector2 RotatePointAroundCenter(Vector2 point, Vector2 center, float angle)
        {
            Vector2 translated = point - center;
            translated = Vector2.Transform(translated, Matrix.CreateRotationZ(angle));
            return center + translated;
        }

        public static List<Line> RectanglePerimeterLines(Rectangle rectangle, Color? color)
        {
            Vector2 position = new(rectangle.X, rectangle.Y);

            Vector2[] Points =
            [
                position,
                position + new Vector2(rectangle.Width, 0),
                position + new Vector2(rectangle.Width, rectangle.Height),
                position + new Vector2(0, rectangle.Height),
                position
            ];

            return [.. ToLines(Points, color ?? Color.White)];
        }

        public static Line[] ToLines(Vector2[] vectors, Color color)
        {
            int lineCount = vectors.Length - 1;
            var lines = new Line[lineCount];

            for (int i = 0; i < lineCount; i++)
            {
                lines[i] = new Line(vectors[i], vectors[i + 1], color);
            }

            return lines;
        }

        public static List<Line> RadialLines(Vector2 position, float distance, int count, Color? color = null, int thickness = 1)
        {
            const int min = 1, max = 360;
            count = MathHelper.Clamp(count, min, max);

            Color lineColor = color ?? Color.White;

            float step = MathHelper.TwoPi / count;

            List<Line> lines = new(count);

            for (int i = 0; i < count; i++)
            {
                float angle = i * step;

                Vector2 dir = new(MathF.Cos(angle), MathF.Sin(angle));
                Vector2 end = position + dir * distance;

                lines.Add(new Line(position, end, lineColor, thickness));
            }

            return lines;
        }

        public static List<Triangle> RadialTriangles(Vector2 position, float distance, int count, Color? color = null, int thickness = 1)
        {
            const int min = 1, max = 360;
            count = MathHelper.Clamp(count, min, max);

            Color lineColor = color ?? Color.White;

            float step = MathHelper.TwoPi / count;

            List<Triangle> triangles = new(count);

            for (int i = 0; i < count; i++)
            {
                float angle = i * step;

                Vector2 dir = new(MathF.Cos(angle), MathF.Sin(angle));
                Vector2 end = position + dir * distance;

                triangles.Add(new Triangle(position, end, end, lineColor, thickness));
            }

            return triangles;
        }
    }
}
