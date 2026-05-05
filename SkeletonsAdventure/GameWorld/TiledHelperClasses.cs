using MonoGame.Extended.Tiled;
using RpgLibrary.DataClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameObjects;

namespace SkeletonsAdventure.GameWorld
{
    internal static class TiledHelperClasses
    {
        public static List<TiledMapTile> TileLocations(int id, TiledMapTile[] tiles)
        {
            List<TiledMapTile> mapTiles = [];

            foreach (var tile in tiles)
            {
                if (tile.GlobalIdentifier == id)
                    mapTiles.Add(tile);
            }
            return mapTiles;
        }

        public static List<TiledMapObject> ObjectLocations(string name, TiledMapObject[] objects)
        {
            List<TiledMapObject> mapObjects = [];

            foreach (var obj in objects)
            {
                if (obj.Name == name)
                    mapObjects.Add(obj);
            }
            return mapObjects;
        }

        public static Dictionary<string, TiledAnimation> LoadTiledAnimations(ContentManager content)
        {
            TiledMapTileset tiledMapTileset = content.Load<TiledMapTileset>(@"TiledFiles/doors_lever_chest_animation");
            string tileName = tiledMapTileset.Name;

            Dictionary<string, TiledAnimation> tiledAnimations = [];

            foreach (var tile in tiledMapTileset.Tiles)
            {
                if (tile is TiledMapTilesetAnimatedTile animatedTile)
                {
                    //to have a unique key for each animated tile the name will be the texture name + "_" + the tile id
                    tiledAnimations.Add(tileName + "_" + tile.LocalTileIdentifier, new(tiledMapTileset, animatedTile));
                }
            }

            return tiledAnimations;
        }

        public static int GetLevelFromTiledMap(TiledMapObject obj)
        {
            obj.Properties.TryGetValue("level", out string level);

            if (level is null)
                obj.Properties.TryGetValue("lvl", out level);

            if (level is not null)
            {
                if (int.TryParse(level, out int lvl))//parse the level to an int to be used for the enemy level
                    return lvl;
            }

            return 0; //level not found or could not be parsed
        }

        public static List<TiledMapTile> GetTiledMapTiles(TiledMapTileLayer layer, Rectangle rec)
        {
            if (layer == null)
                return null;

            List<TiledMapTile> tiles = [];

            int startX = Math.Max(rec.Left / layer.TileWidth, 0);
            int endX = Math.Min((rec.Right - 1) / layer.TileWidth, layer.Width - 1);
            int startY = Math.Max(rec.Top / layer.TileHeight, 0);
            int endY = Math.Min((rec.Bottom - 1) / layer.TileHeight, layer.Height - 1);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    if (layer.TryGetTile((ushort)x, (ushort)y, out TiledMapTile? tile) && tile.Value.GlobalIdentifier != 0)
                        tiles.Add(tile.Value);
                }
            }

            return tiles;
        }

        public static List<Enemy> LoadEnemyFromTiledMap(Enemy enemy, TiledMapObjectLayer mapSpawnerLayer, MinMaxPair enemyLevels)
        {
            if (mapSpawnerLayer is null)
                return null;

            List<Enemy> enemies = [];

            foreach (TiledMapObject obj in ObjectLocations(enemy.Name, mapSpawnerLayer.Objects))
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

        private static List<Chest> GetChestsFromTiledMapTileLayer(TiledMapTileLayer layer, Chest chest)
        {
            List<Chest> chests = [];
            int width = layer.TileWidth;
            int height = layer.TileHeight;

            foreach (TiledMapTile tile in TileLocations(chest.ID, layer.Tiles))
            {
                chest.Position = new(tile.X * width, tile.Y * height);
                chest.DetectionArea = new Rectangle((int)chest.Position.X - 25, (int)chest.Position.Y - 25,
                    layer.TileWidth + 50, layer.TileHeight + 50);

                chests.Add(chest.Clone());
            }

            return chests;
        }

        public static List<Chest> LoadChestsFromTiledMap(TiledMapTileLayer layer, Dictionary<string, Chest> possibleChests)
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