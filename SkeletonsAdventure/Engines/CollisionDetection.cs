using MonoGame.Extended.Tilemaps;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Engines
{
    internal static class CollisionDetection
    {
        internal static bool IsTileBlocked(TilemapTileLayer[] layers, int x, int y)
        {
            if (layers is null)
                return false;
            foreach (var layer in layers)
            {
                if (layer is null || (layer.Name == "ConditionalLayer" && !layer.IsVisible))
                    continue;
                if (layer.GetTile(x, y).HasValue)
                    return true;
            }
            return false;
        }
        public static void CheckEntityBoundaryCollisions(List<Entity> entities, TilemapTileLayer[] layers,
            int tileWidth, int tileHeight, int mapWidth, int mapHeight, GameTime gameTime)
        {
            bool Blocked(int x, int y) => IsTileBlocked(layers, x, y);

            foreach (var entity in entities)
            {
                if (entity.IsDead)
                    continue;
                if (entity.CanMove)
                    MoveSprite(entity);
                foreach (var attack in entity.AttackManager.Attacks)
                    if (attack.CanMove && attack.AttackVisible && attack.Motion != Vector2.Zero && attack.MovementSecondsThisFrame > 0)
                    {
                        MoveSprite(attack);
                    }
            }

            void MoveSprite(AnimatedSprite sprite)
            {
                var direction = sprite.Motion;
                if (direction.LengthSquared() > 1)
                    direction.Normalize();
                double seconds = sprite is BasicAttack movingAttack ? movingAttack.MovementSecondsThisFrame : gameTime.ElapsedGameTime.TotalSeconds;
                var displacement = direction * sprite.Speed * (float)seconds * Game1.BaseSpeedMultiplier;
                var start = sprite.Position;
                var end = Move(start, displacement, sprite.Width, sprite.Height,
                    mapWidth, mapHeight, tileWidth, tileHeight, Blocked, sprite is BasicAttack,
                    sprite is BasicAttack projectile ? (from, to) => projectile.MovementSegments.Add((from, to)) : null);
                sprite.Position = end;
                if (sprite is BasicAttack movedAttack)
                {
                    var hitBox = movedAttack.DamageHitBox;
                    movedAttack.DamageHitBox = new(hitBox.X + (int)end.X - (int)start.X,
                        hitBox.Y + (int)end.Y - (int)start.Y, hitBox.Width, hitBox.Height);
                }
                // Actors can slide along walls. Projectiles stop on contact instead of rounding corners.
                if (sprite is BasicAttack attack && Vector2.DistanceSquared(end, start + displacement) > 0.0001f)
                {
                    attack.Motion = Vector2.Zero;
                    attack.InitialMotion = Vector2.Zero;
                }
            }
        }

        // The solver uses floating-point positions and checks every crossed tile edge.
        // Small steps preserve diagonal sliding and corner collisions during long frames.
        internal static Vector2 Move(Vector2 position, Vector2 displacement, int width, int height,
            int mapWidth, int mapHeight, int tileWidth, int tileHeight, Func<int, int, bool> blocked, bool stopOnCollision = false,
            Action<Vector2, Vector2> moved = null)
        {
            if (width <= 0 || height <= 0 || mapWidth <= 0 || mapHeight <= 0 || tileWidth <= 0 || tileHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) ||
                !float.IsFinite(displacement.X) || !float.IsFinite(displacement.Y))
                throw new ArgumentOutOfRangeException(nameof(displacement));
            float maxX = Math.Max(0, mapWidth - width), maxY = Math.Max(0, mapHeight - height);
            position = Vector2.Clamp(position, Vector2.Zero, new(maxX, maxY));
            displacement = Vector2.Clamp(displacement, new(-mapWidth, -mapHeight), new(mapWidth, mapHeight));
            int steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Abs(displacement.X), Math.Abs(displacement.Y)) /
                (Math.Min(tileWidth, tileHeight) * 0.5)));
            var step = displacement / steps;
            for (int i = 0; i < steps; i++)
            {
                float nextX = Math.Clamp(position.X + step.X, 0, maxX);
                int top = (int)Math.Floor(position.Y / tileHeight);
                int bottom = LastTile(position.Y + height, tileHeight);
                if (step.X > 0)
                {
                    for (int x = LastTile(position.X + width, tileWidth); x <= LastTile(nextX + width, tileWidth); x++)
                        for (int y = top; y <= bottom; y++)
                            if (blocked(x, y))
                                nextX = Math.Max(position.X, Math.Min(nextX, x * tileWidth - width));
                }
                else if (step.X < 0)
                {
                    for (int x = (int)Math.Floor(position.X / tileWidth); x >= (int)Math.Floor(nextX / tileWidth); x--)
                        for (int y = top; y <= bottom; y++)
                            if (blocked(x, y))
                                nextX = Math.Min(position.X, Math.Max(nextX, (x + 1) * tileWidth));
                }
                bool hitX = Math.Abs(nextX - (position.X + step.X)) > 0.00001f;
                var previous = position;
                position.X = nextX;
                if (position != previous)
                    moved?.Invoke(previous, position);
                if (stopOnCollision && hitX)
                    return position;
                float nextY = Math.Clamp(position.Y + step.Y, 0, maxY);
                int left = (int)Math.Floor(position.X / tileWidth);
                int right = LastTile(position.X + width, tileWidth);
                if (step.Y > 0)
                {
                    for (int y = LastTile(position.Y + height, tileHeight); y <= LastTile(nextY + height, tileHeight); y++)
                        for (int x = left; x <= right; x++)
                            if (blocked(x, y))
                                nextY = Math.Max(position.Y, Math.Min(nextY, y * tileHeight - height));
                }
                else if (step.Y < 0)
                {
                    for (int y = (int)Math.Floor(position.Y / tileHeight); y >= (int)Math.Floor(nextY / tileHeight); y--)
                        for (int x = left; x <= right; x++)
                            if (blocked(x, y))
                                nextY = Math.Min(position.Y, Math.Max(nextY, (y + 1) * tileHeight));
                }
                bool hitY = Math.Abs(nextY - (position.Y + step.Y)) > 0.00001f;
                previous = position;
                position.Y = nextY;
                if (position != previous)
                    moved?.Invoke(previous, position);
                if (stopOnCollision && hitY)
                    return position;
            }
            return position;
        }

        private static int LastTile(float edge, int tileSize) => (int)Math.Ceiling((double)edge / tileSize) - 1;
    }
}
