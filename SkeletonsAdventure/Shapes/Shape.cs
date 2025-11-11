
namespace SkeletonsAdventure.Shapes
{
    internal abstract class Shape
    {
        public Color Color { get; set; }
        public int Thickness { get; set; } = 1;
        public virtual List<Line> Lines { get; set; }

        public Shape() { }
    }
}
