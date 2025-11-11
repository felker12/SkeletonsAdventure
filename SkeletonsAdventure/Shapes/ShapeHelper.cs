
namespace SkeletonsAdventure.Shapes
{
    internal static class ShapeHelper
    {
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
