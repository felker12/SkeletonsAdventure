
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
            if(rectangle.Contains(Point1) ||
                rectangle.Contains(Point2) || 
                rectangle.Contains(Point3))
            {
                return true;
            }

            return false;
        }
    }
}
