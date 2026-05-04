using MonoGame.Extended.Tiled;
using SkeletonsAdventure.Animations;

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
    }
}