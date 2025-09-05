using MonoGame.Extended.Tiled;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.EntitySpawners
{
    internal class Spawner(TiledMapObjectLayer mapSpawnerLayer)
    {
        //public TiledMapTileLayer TiledMapTileLayer { get; set; } = mapSpawnerLayer;

        public TiledMapObjectLayer TiledMapObjectLayer { get; set; } = mapSpawnerLayer;

        public List<Enemy> CreateEnemiesForSpawners(Enemy Enemy)
        {
            List<Enemy> enemies = [];

            foreach (TiledMapObject obj in GameManager.ObjectLocations(Enemy.Name, TiledMapObjectLayer.Objects))
            {
                Enemy enemy = Enemy.Clone();
                enemy.Position = obj.Position;
                enemy.RespawnPosition = enemy.Position;
                enemies.Add(enemy);
            }

            return enemies;
        }
    }
}
