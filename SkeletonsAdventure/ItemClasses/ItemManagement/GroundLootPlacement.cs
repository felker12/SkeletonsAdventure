using SkeletonsAdventure.Engines;
using System.Linq;

namespace SkeletonsAdventure.ItemClasses.ItemManagement
{
    internal sealed class GroundLootPlacement(int mapWidth, int mapHeight, int tileWidth, int tileHeight,
        Func<int, int, bool> blocked, Func<(Vector2 Position, int Width, int Height)> player, Func<int> collisionVersion)
    {
        private readonly HashSet<Point> _reachable = [];
        private int _version = int.MinValue, _width, _height;
        private static readonly Point[] Neighbors = [new(1, 0), new(0, 1), new(-1, 0), new(0, -1)];
        private Point Cell(Vector2 position) => new((int)MathF.Floor(position.X / tileWidth), (int)MathF.Floor(position.Y / tileHeight));
        private Vector2 Position(Point cell) => new(cell.X * tileWidth, cell.Y * tileHeight);
        private Vector2 ItemPosition(Point cell) => Position(cell) + new Vector2((_width - GameItem.Width) / 2f, (_height - GameItem.Height) / 2f);

        private bool Clear(Vector2 position, int width, int height)
        {
            if (position.X < 0 || position.Y < 0 || position.X + width > mapWidth || position.Y + height > mapHeight)
                return false;
            for (int y = (int)Math.Floor(position.Y / tileHeight); y < (int)Math.Ceiling((position.Y + height) / tileHeight); y++)
                for (int x = (int)Math.Floor(position.X / tileWidth); x < (int)Math.Ceiling((position.X + width) / tileWidth); x++)
                    if (blocked(x, y))
                        return false;
            return true;
        }

        private void RefreshReachableGround()
        {
            var actor = player();
            var cell = Cell(actor.Position);
            var seeds = new List<Point>();
            foreach (var candidate in new[] { cell, new Point(cell.X + 1, cell.Y), new Point(cell.X, cell.Y + 1), new Point(cell.X + 1, cell.Y + 1) })
            {
                var position = Position(candidate);
                if (!Clear(position, actor.Width, actor.Height))
                    continue;
                var end = CollisionDetection.Move(actor.Position, position - actor.Position, actor.Width, actor.Height,
                    mapWidth, mapHeight, tileWidth, tileHeight, blocked);
                if (Vector2.DistanceSquared(end, position) < 0.01f)
                    seeds.Add(candidate);
            }
            int version = collisionVersion();
            if (_version == version && _width == actor.Width && _height == actor.Height && seeds.Any(_reachable.Contains))
                return;
            _version = version;
            _width = actor.Width;
            _height = actor.Height;
            _reachable.Clear();
            var visited = new HashSet<Point>(seeds);
            var frontier = new Queue<Point>(seeds);
            while (frontier.TryDequeue(out var current))
            {
                _reachable.Add(current);
                foreach (var offset in Neighbors)
                {
                    var next = new Point(current.X + offset.X, current.Y + offset.Y);
                    if (visited.Add(next) && Clear(Position(next), _width, _height))
                        frontier.Enqueue(next);
                }
            }
        }

        public List<Vector2> Place(Vector2 origin, int count, IEnumerable<Vector2> existing)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count == 0)
                return [];
            RefreshReachableGround();
            Point? nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (var cell in _reachable)
            {
                var position = ItemPosition(cell);
                float distance = Vector2.DistanceSquared(position, origin);
                if (distance < nearestDistance && Clear(position, GameItem.Width, GameItem.Height))
                {
                    nearest = cell;
                    nearestDistance = distance;
                }
            }
            if (!nearest.HasValue)
                throw new InvalidOperationException("No player-reachable ground is available for loot.");

            // Spread within a small connected neighborhood, never through a nearby dividing wall.
            float radius = Math.Max(64, MathF.Sqrt(count) * GameItem.Width);
            var center = Position(nearest.Value);
            var local = new List<Vector2>();
            var seen = new HashSet<Point> { nearest.Value };
            var frontier = new Queue<Point>();
            frontier.Enqueue(nearest.Value);
            while (frontier.TryDequeue(out var current))
            {
                var position = ItemPosition(current);
                if (Clear(position, GameItem.Width, GameItem.Height))
                    local.Add(position);
                foreach (var offset in Neighbors)
                {
                    var next = new Point(current.X + offset.X, current.Y + offset.Y);
                    if (_reachable.Contains(next) && Vector2.DistanceSquared(Position(next), center) <= radius * radius && seen.Add(next))
                        frontier.Enqueue(next);
                }
            }
            local = local.OrderBy(position => Vector2.DistanceSquared(position, origin)).ToList();
            var occupied = existing.Select(RectangleAt).ToList();
            List<Vector2> result = [];
            for (int i = 0; i < count; i++)
            {
                // Overlap safely when there is no space left; never discard drops or push them into walls.
                var chosen = local.FirstOrDefault(position => !occupied.Any(rectangle => rectangle.Intersects(RectangleAt(position))), local[0]);
                result.Add(chosen);
                occupied.Add(RectangleAt(chosen));
            }
            return result;
        }

        public bool IsSafe(Vector2 position)
        {
            RefreshReachableGround();
            if (!Clear(position, GameItem.Width, GameItem.Height))
                return false;
            var rectangle = RectangleAt(position);
            var center = Cell(position);
            int reachX = (GameItem.Width + _width + tileWidth - 1) / tileWidth;
            int reachY = (GameItem.Height + _height + tileHeight - 1) / tileHeight;
            for (int y = center.Y - reachY; y <= center.Y + reachY; y++)
                for (int x = center.X - reachX; x <= center.X + reachX; x++)
                    if (_reachable.Contains(new(x, y)) && new Rectangle(x * tileWidth, y * tileHeight, _width, _height).Intersects(rectangle))
                        return true;
            return false;
        }

        private static Rectangle RectangleAt(Vector2 position) => new((int)position.X, (int)position.Y, GameItem.Width, GameItem.Height);
    }
}
