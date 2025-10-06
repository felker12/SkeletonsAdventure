using System.IO;

namespace SkeletonsAdventure.TileEngine
{
    internal static class TileMapReader
    {
        public static TileMap LoadTileMap(string tmxPath, ContentManager contentManager)
        {
            XDoc xDoc = new(tmxPath);
            List<TileSet> tileSets = TmxReader.LoadTileSetsFromTmx(xDoc, tmxPath, contentManager);
            List<Layer> layers = TmxReader.LoadLayersFromTmx(xDoc, tileSets);

            TileMap tileMap = new()
            {
                TileSets = tileSets,
                Layers  = layers,
                Name = Path.GetFileNameWithoutExtension(tmxPath)
            };
            tileMap.CalculateLongestLayerDimensions();

            return tileMap;
        }
    }
}
