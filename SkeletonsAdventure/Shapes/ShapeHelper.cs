namespace SkeletonsAdventure.Shapes
{
    internal static class ShapeHelper
    {
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
    }
}
