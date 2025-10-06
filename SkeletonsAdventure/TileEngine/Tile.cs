
using MonoGame.Extended.Tiled;

namespace SkeletonsAdventure.TileEngine
{
    internal class Tile(int x, int y, int width, int height, int globalTileId, TileSet tileSet)
    {
        public int Width { get; set; } = width;
        public int Height { get; set; } = height;
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
        public int GlobalTileId { get; } = globalTileId;
        public TileSet TileSet { get; } = tileSet;
        public Vector2 Position { get; } = new (x, y);
        public Rectangle Bounds { get; } = new(x, y, width, height);
        public int LocalTileId { get; } = globalTileId - tileSet.FirstGid;


        public bool Contains(Vector2 point) => Bounds.Contains(point);
        public bool Intersects(Rectangle rectangle) => Bounds.Intersects(rectangle);

        public override string ToString()
        {
            return $"Tile: Position=({X}, {Y}), Size=({Width}, {Height})";
        }
    }
}
