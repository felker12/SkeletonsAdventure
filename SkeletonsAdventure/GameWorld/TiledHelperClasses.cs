using MonoGame.Extended.Tilemaps;
using RpgLibrary.DataClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameObjects;

namespace SkeletonsAdventure.GameWorld
{
    internal static class TiledHelperClasses
    {
        public static List<TilemapObject> ObjectLocations(string name, IEnumerable<TilemapObject> objects)
        {
            List<TilemapObject> mapObjects = [];

            foreach (var obj in objects)
            {
                if (obj.Name == name)
                    mapObjects.Add(obj);
            }
            return mapObjects;
        }

        public static TiledAnimation CreateTiledAnimation(TilemapTileset tileset, int tileId)
        {
            var animation = tileset.GetTileData(tileId)?.Animation ?? throw new InvalidOperationException($"Missing animation for tile {tileId}.");
            return new TiledAnimation(tileset, animation);
        }

        public static bool TryReadInt(TilemapPropertyValue value, out int number)
        {
            if (value.Type == TilemapPropertyType.Int)
            {
                number = value.AsInt();
                return true;
            }

            number = 0;
            return value.Type == TilemapPropertyType.String && int.TryParse(value.AsString(), out number);
        }

        public static int GetLevelFromTiledMap(TilemapObject obj)
        {
            bool hasLevel = obj.Properties.TryGetValue("level", out var level);

            if (!hasLevel)
                hasLevel = obj.Properties.TryGetValue("lvl", out level);

            if (hasLevel)
            {
                if (TryReadInt(level, out int lvl))
                    return lvl;
            }

            return 0; //level not found or could not be parsed
        }

        public static List<Point> GetTiledMapTiles(TilemapTileLayer layer, Rectangle rec)
        {
            if (layer == null)
                return [];

            List<Point> tiles = [];

            int startX = Math.Max(rec.Left / layer.TileWidth, 0);
            int endX = Math.Min((rec.Right - 1) / layer.TileWidth, layer.Width - 1);
            int startY = Math.Max(rec.Top / layer.TileHeight, 0);
            int endY = Math.Min((rec.Bottom - 1) / layer.TileHeight, layer.Height - 1);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    if (layer.GetTile(x, y).HasValue)
                        tiles.Add(new(x, y));
                }
            }

            return tiles;
        }

        public static List<Enemy> LoadEnemyFromTiledMap(Enemy enemy, TilemapObjectLayer mapSpawnerLayer, MinMaxPair enemyLevels)
        {
            if (mapSpawnerLayer is null)
                return null;

            List<Enemy> enemies = [];

            foreach (TilemapObject obj in ObjectLocations(enemy.Name, mapSpawnerLayer.Objects))
            {
                Enemy enemyClone = enemy.Clone();
                enemyClone.Position = obj.Position;
                enemyClone.RespawnPosition = enemyClone.Position;

                int levelFromMap = GetLevelFromTiledMap(obj);
                //Clamp the level to be within the max range
                if (levelFromMap > enemyLevels.Max)
                    levelFromMap = enemyLevels.Max;

                enemyClone.SetEnemyLevel(levelFromMap);

                //If the level was not set from the map, set it to the default for the level
                if (enemyClone.Level == 0)
                    enemyClone.SetEnemyLevel(enemyLevels);

                enemies.Add(enemyClone);
            }

            return enemies;
        }

        private static List<Chest> GetChestsFromTiledMapTileLayer(TilemapTileLayer layer, Chest chest)
        {
            List<Chest> chests = [];
            int width = layer.TileWidth;
            int height = layer.TileHeight;

            foreach (var (x, y, tile) in layer.GetTiles())
            {
                if (tile.GlobalId != chest.ID)
                    continue;
                chest.Position = new(x * width, y * height);
                chest.DetectionArea = new Rectangle((int)chest.Position.X - 25, (int)chest.Position.Y - 25,
                    layer.TileWidth + 50, layer.TileHeight + 50);

                chests.Add(chest.Clone());
            }

            return chests;
        }

        public static List<Chest> LoadChestsFromTiledMap(TilemapTileLayer layer, Dictionary<string, Chest> possibleChests)
        {
            List<Chest> chests = [];    

            if (layer is null)
                return chests;

            foreach (Chest chest in possibleChests.Values)
            {
                chests.AddRange(GetChestsFromTiledMapTileLayer(layer, chest));
            }

            return chests;
        }
    }
}
