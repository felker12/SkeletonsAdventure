using MonoGame.Extended.Tilemaps;

namespace SkeletonsAdventure.GameWorld
{
    internal sealed class GameContentManager(IServiceProvider services) : ContentManager(services, "Content")
    {
        internal Tilemap LoadSessionMap(string assetName)
        {
            // Map layers hold session state. Evict the map itself while keeping shared assets
            // such as its textures in the game content cache.
            string key = assetName.Replace('\\', '/');
            Tilemap map = Load<Tilemap>(key);

            LoadedAssets.Remove(key);
            return map;
        }
    }
}
