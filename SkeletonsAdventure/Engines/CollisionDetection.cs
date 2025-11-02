using MonoGame.Extended.Tiled;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Engines
{
    internal static class CollisionDetection
    {
        public static void CheckEntityBoundaryCollisions(List<Entity> entities, TiledMapTileLayer[] mapCollisionLayers, int tileWidth, int tileHeight)
        {
            foreach (Entity entity in entities)
            {
                //Check entity collision with the map boundaries
                if (entity.CanMove)
                    CheckCollision(entity, mapCollisionLayers, tileWidth, tileHeight);

                //Check entity attacks collision with the map boundaries
                foreach (BasicAttack entityAttack in entity.AttackManager.Attacks)
                {
                    if (entityAttack.CanMove)
                        CheckCollision(entityAttack, mapCollisionLayers, tileWidth, tileHeight);
                }
            }
        }

        private static void CheckCollision(AnimatedSprite entity, TiledMapTileLayer[] mapCollisionLayers, int tileWidth, int tileHeight)
        {
            if (entity.Motion == Vector2.Zero)
                return;

            if (mapCollisionLayers == null)
            {
                entity.Position += entity.Motion * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier;
                return;
            }

            Vector2 pos = entity.Position,
                motion = entity.Motion;
            Rectangle rec = entity.Rectangle;
            bool xBlocked = false, yBlocked = false;

            foreach (var layer in mapCollisionLayers)
            {
                if (layer is null)
                    continue;

                if (layer.Name == "ConditionalLayer" && layer.IsVisible is false)
                    continue;

                // --- Y axis ---
                if (motion.Y != 0)
                {
                    if (yBlocked is false)
                        yBlocked = CheckYAxis(entity, layer, pos, motion, rec, tileWidth, tileHeight);

                    if (yBlocked)
                        motion.Y = 0;
                }

                // --- X axis ---
                if (motion.X != 0)
                {
                    if (xBlocked is false)
                        xBlocked = CheckXAxis(entity, layer, pos, motion, rec, tileWidth, tileHeight);

                    if (xBlocked)
                        motion.X = 0;
                }

                // Break out of loop early if both axes are blocked or no motion left
                if (motion == Vector2.Zero
                    || (xBlocked && yBlocked))
                    break;
            }

            entity.Motion = motion;
            entity.Position += entity.Motion * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier;
        }

        //--- Axis Collision Checks ---
        private static bool CheckXAxis(AnimatedSprite entity, TiledMapTileLayer mapCollisionLayer, Vector2 pos, Vector2 motion, Rectangle rec, int width, int height)
        {
            Vector2 newPosX = new(pos.X + motion.X * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier, pos.Y + motion.Y * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier);
            Rectangle newRectX = new((int)newPosX.X, (int)newPosX.Y, rec.Width, rec.Height);
            int checkX = motion.X > 0 ? (newRectX.Right - 1) / width : newRectX.Left / width;

            for (int y = newRectX.Top / height; y <= (newRectX.Bottom - 1) / height; y++)
            {
                if (IsTileBlocked(checkX, y, mapCollisionLayer))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CheckYAxis(AnimatedSprite entity, TiledMapTileLayer mapCollisionLayer, Vector2 pos, Vector2 motion, Rectangle rec, int width, int height)
        {
            Vector2 newPosY = new(pos.X, pos.Y + motion.Y * entity.Speed * Game1.DeltaTime * Game1.BaseSpeedMultiplier);
            Rectangle newRectY = new((int)newPosY.X, (int)newPosY.Y, rec.Width, rec.Height);
            int checkY = motion.Y > 0 ? (newRectY.Bottom - 1) / height : newRectY.Top / height;

            for (int x = newRectY.Left / width; x <= (newRectY.Right - 1) / width; x++)
            {
                if (IsTileBlocked(x, checkY, mapCollisionLayer))
                {
                    return true;
                }
            }

            return false;
        }

        //--- Tile Check ---
        private static bool IsTileBlocked(int x, int y, TiledMapTileLayer mapCollisionLayer)
        {
            return mapCollisionLayer.TryGetTile((ushort)x, (ushort)y, out TiledMapTile? tile) && tile.Value.IsBlank is false;
        }
    }
}
