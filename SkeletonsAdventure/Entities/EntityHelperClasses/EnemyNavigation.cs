using SkeletonsAdventure.Engines;

namespace SkeletonsAdventure.Entities.EntityHelperClasses
{
    internal sealed class EnemyNavigation(int width, int height, int tileWidth, int tileHeight, Func<int, int, bool> blocked)
    {
        private sealed class Route
        {
            public Point Goal;
            public TimeSpan RefreshAt;
            public Queue<Vector2> Points = new();
        }
        private readonly System.Runtime.CompilerServices.ConditionalWeakTable<Enemy, Route> _routes = new();
        private Point Cell(Vector2 point) => new((int)MathF.Floor(point.X / tileWidth), (int)MathF.Floor(point.Y / tileHeight));
        private Vector2 Position(Point cell) => new(cell.X * tileWidth, cell.Y * tileHeight);
        private bool Solid(int x, int y) => x < 0 || y < 0 || x * tileWidth >= width || y * tileHeight >= height || blocked(x, y);

        // Supercover grid traversal also checks both neighbors when a ray crosses a tile corner.
        internal bool HasLineOfSight(Vector2 from, Vector2 to)
        {
            var cell = Cell(from);
            var goal = Cell(to);
            var delta = to - from;
            int sx = Math.Sign(delta.X), sy = Math.Sign(delta.Y);
            double stepX = sx == 0 ? double.PositiveInfinity : tileWidth / Math.Abs((double)delta.X);
            double stepY = sy == 0 ? double.PositiveInfinity : tileHeight / Math.Abs((double)delta.Y);
            double nextX = sx == 0 ? double.PositiveInfinity :
                ((sx > 0 ? (cell.X + 1) * tileWidth : cell.X * tileWidth) - from.X) / delta.X;
            double nextY = sy == 0 ? double.PositiveInfinity :
                ((sy > 0 ? (cell.Y + 1) * tileHeight : cell.Y * tileHeight) - from.Y) / delta.Y;
            while (true)
            {
                if (Solid(cell.X, cell.Y))
                    return false;
                if (cell == goal)
                    return true;
                if (Math.Min(nextX, nextY) >= 1 - 0.000000001)
                    return !Solid(goal.X, goal.Y);
                if (Math.Abs(nextX - nextY) < 0.0000001)
                {
                    if (Solid(cell.X + sx, cell.Y) || Solid(cell.X, cell.Y + sy))
                        return false;
                    cell.X += sx;
                    cell.Y += sy;
                    nextX += stepX;
                    nextY += stepY;
                }
                else if (nextX < nextY) { cell.X += sx; nextX += stepX; }
                else { cell.Y += sy; nextY += stepY; }
            }
        }

        private bool CanStand(Vector2 position, Enemy enemy)
        {
            if (position.X < 0 || position.Y < 0 || position.X + enemy.Width > width || position.Y + enemy.Height > height)
                return false;
            for (int y = (int)(position.Y / tileHeight); y < (int)Math.Ceiling((position.Y + enemy.Height) / tileHeight); y++)
                for (int x = (int)(position.X / tileWidth); x < (int)Math.Ceiling((position.X + enemy.Width) / tileWidth); x++)
                    if (Solid(x, y))
                        return false;
            return true;
        }

        private bool CanTravel(Enemy enemy, Vector2 from, Vector2 to)
        {
            if (!CanStand(to, enemy))
                return false;
            var result = CollisionDetection.Move(from, to - from, enemy.Width, enemy.Height,
                width, height, tileWidth, tileHeight, Solid);
            return Vector2.DistanceSquared(result, to) < 0.01f;
        }

        internal Vector2 DirectionTo(Enemy enemy, Vector2 target, GameTime time)
        {
            if (HasLineOfSight(enemy.Center, target + new Vector2(enemy.Width, enemy.Height) / 2) &&
                CanTravel(enemy, enemy.Position, target))
                return Direction(enemy, target, time);
            var route = _routes.GetOrCreateValue(enemy);
            var goal = Cell(target);
            while (route.Points.Count > 0 && Vector2.DistanceSquared(enemy.Position, route.Points.Peek()) < 0.25f)
                route.Points.Dequeue();
            if (route.Goal != goal || time.TotalGameTime >= route.RefreshAt ||
                (route.Points.Count > 0 && !CanTravel(enemy, enemy.Position, route.Points.Peek())))
            {
                route.Goal = goal;
                route.RefreshAt = time.TotalGameTime + TimeSpan.FromMilliseconds(500);
                route.Points = FindRoute(enemy, goal);
                while (route.Points.Count > 0 && Vector2.DistanceSquared(enemy.Position, route.Points.Peek()) < 0.25f)
                    route.Points.Dequeue();
            }
            return route.Points.Count == 0 ? Vector2.Zero : Direction(enemy, route.Points.Peek(), time);
        }

        private Queue<Vector2> FindRoute(Enemy enemy, Point goal)
        {
            var cell = Cell(enemy.Position);
            Point? anchor = null;
            float nearest = float.MaxValue;
            foreach (var candidate in new[] { cell, new Point(cell.X + 1, cell.Y), new Point(cell.X, cell.Y + 1), new Point(cell.X + 1, cell.Y + 1) })
            {
                float distance = Vector2.DistanceSquared(enemy.Position, Position(candidate));
                if (distance < nearest && CanTravel(enemy, enemy.Position, Position(candidate)))
                {
                    anchor = candidate;
                    nearest = distance;
                }
            }
            if (!anchor.HasValue)
                return new();
            var start = anchor.Value;
            var frontier = new PriorityQueue<Point, int>();
            var costs = new Dictionary<Point, int> { [start] = 0 };
            var previous = new Dictionary<Point, Point>();
            int Distance(Point p) => Math.Abs(p.X - goal.X) + Math.Abs(p.Y - goal.Y);
            frontier.Enqueue(start, Distance(start));
            Point best = start;
            int examined = 0;
            Point[] directions = [new(1, 0), new(0, 1), new(-1, 0), new(0, -1)];
            while (frontier.TryDequeue(out var current, out _) && examined++ < 2048)
            {
                if (Distance(current) < Distance(best))
                    best = current;
                if (current == goal) { best = current; break; }
                foreach (var offset in directions)
                {
                    var next = new Point(current.X + offset.X, current.Y + offset.Y);
                    if (!CanStand(Position(next), enemy))
                        continue;
                    int cost = costs[current] + 1;
                    if (costs.TryGetValue(next, out int oldCost) && oldCost <= cost)
                        continue;
                    costs[next] = cost;
                    previous[next] = current;
                    frontier.Enqueue(next, cost + Distance(next));
                }
            }
            var points = new List<Vector2> { Position(best) };
            while (best != start)
            {
                best = previous[best];
                points.Add(Position(best));
            }
            points.Reverse();
            return new(points);
        }

        internal static Vector2 Direction(Enemy enemy, Vector2 target, GameTime time)
        {
            var delta = target - enemy.Position;
            float distance = delta.Length();
            if (distance < 0.001f)
                return Vector2.Zero;
            float step = enemy.Speed * (float)time.ElapsedGameTime.TotalSeconds * Game1.BaseSpeedMultiplier;
            return delta / distance * (step > distance ? distance / step : 1);
        }
    }
}
