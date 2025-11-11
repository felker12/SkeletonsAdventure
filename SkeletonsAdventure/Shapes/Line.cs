
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
    }
}
