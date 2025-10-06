using System.Linq;
using SkeletonsAdventure.ShapeEngine;

namespace SkeletonsAdventure.TileEngine
{
    internal enum MapObjectShape { Rectangle, Ellipse, Polygon }

    internal class MapObject()
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visible { get; set; } = true;
        public List<MapObjectProperties> Properties { get; set; } = [];
        public MapObjectShape MapObjectShape { get; set; }
        public Vector2[] PolygonPoints { get; set; }
        public Vector2 Position => new(X, Y);

        private Color color = Color.White;

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (spriteBatch == null || pixel == null || !Visible)
                return;

            OutlineDrawer.DrawPolygonOutline(spriteBatch, pixel, PolygonPoints, color, 1f);
        }

        public override string ToString()
        {
            return $"Id: {ID}, Name: {Name}, X: {X}, Y: {Y}, " +
                $"Width: {Width}, Height: {Height}, Visible: {Visible}, " +
                $"Properties: {string.Join(", ", Properties.Select(t => t.ToString()))}, " +
                $"MapObjectShape: {MapObjectShape}" +
                "PolygonPoints: " + string.Join(" ", PolygonPoints.Select(p => $"({p.X},{p.Y})"));
        }
    }
}
