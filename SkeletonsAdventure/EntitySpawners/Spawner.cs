using MonoGame.Extended.Tiled;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.EntitySpawners
{
    internal class Spawner(TiledMapTileLayer mapSpawnerLayer)
    {
        public TiledMapTileLayer TiledMapTileLayer { get; set; } = mapSpawnerLayer;

        public List<Enemy> CreateEnemiesForSpawners(Enemy Enemy)
        {
            List<Enemy> enemies = [];
            int width = TiledMapTileLayer.TileWidth;
            int height = TiledMapTileLayer.TileHeight;

            foreach (TiledMapTile tile in GameManager.TileLocations(Enemy.ID, TiledMapTileLayer.Tiles))
            {
                Enemy enemy = Enemy.Clone();
                enemy.Position = new Vector2(tile.X * width - enemy.Width, tile.Y * height - enemy.Height);
                enemy.RespawnPosition = enemy.Position;
                enemies.Add(enemy);
            }
            return enemies;
        }
    }
}
