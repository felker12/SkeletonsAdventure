
namespace SkeletonsAdventure.Shapes
{
    internal class Line
    {
        public Color Color { get; set; } = Color.White;
        public int Thickness { get; set; } = 1;
        public Vector2 Point1 { get; set; } = new Vector2();
        public Vector2 Point2 { get; set; } = new Vector2();

        public Line() { }

        public Line(Vector2 point1, Vector2 point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public Line(Vector2 point1, Vector2 point2, Color color, int thickness = 1) : this(color, thickness)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public Line(Color color, int thickness = 1)
        {
            Color = color;
            Thickness = thickness;
        }

        public void SetPoints(Vector2 point1, Vector2 point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public bool Intersects(Line other)
        {
            return Intersects(Point1, Point2, other.Point1, other.Point2);
        }

        public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            // based on orientation/general segment intersection algorithm
            float d = (a2.X - a1.X) * (b2.Y - b1.Y) - (a2.Y - a1.Y) * (b2.X - b1.X);

            if (d == 0)
                return false; // parallel or collinear (you can expand for collinear overlap if needed)

            float u = ((b1.X - a1.X) * (b2.Y - b1.Y) - (b1.Y - a1.Y) * (b2.X - b1.X)) / d;
            float v = ((b1.X - a1.X) * (a2.Y - a1.Y) - (b1.Y - a1.Y) * (a2.X - a1.X)) / d;

            // segment intersection (0-1 range)
            return (u >= 0 && u <= 1) && (v >= 0 && v <= 1);
        }
    }
}
