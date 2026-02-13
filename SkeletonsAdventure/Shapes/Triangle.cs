namespace SkeletonsAdventure.Shapes
{
    internal class Triangle : Shape
    {
        public Vector2 Point1 { get; set; } = new Vector2();
        public Vector2 Point2 { get; set; } = new Vector2();
        public Vector2 Point3 { get; set; } = new Vector2();
        public override List<Line> Lines =>
        [
            new(Point1, Point2, Color, Thickness),
            new(Point2, Point3, Color, Thickness),
            new(Point3, Point1, Color, Thickness),
        ];

        public Triangle() { }

        public Triangle(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }

        public Triangle(Vector2 point1, Vector2 point2, Vector2 point3, Color color, int thickness = 1)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
            Color = color;
            Thickness = thickness;
        }

        public Triangle(Color color, int thickness = 1)
        {
            Color = color;
            Thickness = thickness;
        }

        public void SetPoints(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }

        public bool Intersects(Rectangle rectangle)
        {
            // 1) any triangle vertex inside rectangle
            if (rectangle.Contains(Point1) || rectangle.Contains(Point2) || rectangle.Contains(Point3))
                return true;

            // 2) any rectangle vertex inside triangle
            if (ContainsPoint(rectangle.Location.ToVector2())) return true;
            if (ContainsPoint(new Vector2(rectangle.Right, rectangle.Top))) return true;
            if (ContainsPoint(new Vector2(rectangle.Right, rectangle.Bottom))) return true;
            if (ContainsPoint(new Vector2(rectangle.Left, rectangle.Bottom))) return true;

            // 3) line intersection (edges)
            var triLines = Lines;
            var rectLines = ShapeHelper.RectanglePerimeterLines(rectangle, Color.White);

            foreach (var tl in triLines)
                foreach (var rl in rectLines)
                    if (tl.Intersects(rl))
                        return true;

            return false;
        }

        // barycentric check
        public bool ContainsPoint(Vector2 p)
        {
            var v0 = Point3 - Point1;
            var v1 = Point2 - Point1;
            var v2 = p - Point1;

            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v2, v0);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v2, v1);

            float invDen = 1f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDen;
            float v = (dot00 * dot12 - dot01 * dot02) * invDen;

            return (u >= 0) && (v >= 0) && (u + v < 1);
        }
    }
}
