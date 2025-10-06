using System.Linq;

namespace SkeletonsAdventure.TileEngine
{
    internal class TileMap
    {
        public List<Layer> Layers { get; private set; } = [];
        public List<TileSet> TileSets { get; private set; } = [];
        private ContentManager Content { get; set; }
        public float LongestLayerWidthInTiles { get; set; } = 0;
        public float LongestLayerWidthInPixels { get; set; } = 0;
        public float LongestLayerHeightInTiles { get; set; } = 0;
        public float LongestLayerHeightInPixels { get; set; } = 0;

        public TileMap(ContentManager content, string tmxPath, GraphicsDevice graphicsDevice)
        {
            Content = content;
            XDoc xDoc = new(tmxPath);

            // Load tilesets first as they're needed for tile layers
            TileSets = TmxReader.LoadTileSetsFromTmx(xDoc, tmxPath, content);
            Layers = TmxReader.LoadLayersFromTmx(xDoc, TileSets, graphicsDevice);

            CalculateLongestLayerDimensions();
        }

        public TileMap(List<Layer> layers, List<TileSet> tileSets, ContentManager content)
        {
            Layers = layers;
            TileSets = tileSets;
            Content = content;

            CalculateLongestLayerDimensions();
        }

        public TileMap(TileMap tileMap)
        {
            Layers = [.. tileMap.Layers];
            TileSets = [.. tileMap.TileSets];
            Content = tileMap.Content;
            LongestLayerWidthInTiles = tileMap.LongestLayerWidthInTiles;
            LongestLayerHeightInTiles = tileMap.LongestLayerHeightInTiles;
        }

        public void Update(GameTime gameTime)
        {
            foreach (var layer in Layers)
            {
                // Only update layers that are visible
                if (!layer.Visible)
                    continue;

                layer.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var layer in Layers)
            {
                // Only draw layers that are visible
                if (!layer.Visible)
                    continue;

                layer.Draw(spriteBatch);
            }
        }

        public Layer GetLayerByName(string name)
        {
            return Layers.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void InsertLayerAt(int index, Layer layer)
        {
            if (index < 0)
            {
                index = 0; // Insert at the beginning if index is out of range
            }
            else if (index > Layers.Count)
            {
                index = Layers.Count; // Append to the end if index is out of range
            }

            Layers.Insert(index, layer);

            CalculateLongestLayerDimensions();
        }

        public void CalculateLongestLayerDimensions()
        {
            float longestWidth = 0;
            float longestHeight = 0;
            float longestWidthInPixels = 0;
            float longestHeightInPixels = 0;

            foreach (var layer in Layers)
            {
                if (layer is TileLayer tileLayer)
                {
                    if (tileLayer.Width > longestWidth)
                    {
                        longestWidth = tileLayer.Width;
                        longestWidthInPixels = tileLayer.Width * tileLayer.TileWidth;
                    }
                    if (tileLayer.Height > longestHeight)
                    {
                        longestHeight = tileLayer.Height;
                        longestHeightInPixels = tileLayer.Height * tileLayer.TileHeight;
                    }
                }
            }

            LongestLayerWidthInTiles = longestWidth;
            LongestLayerHeightInTiles = longestHeight;
            LongestLayerWidthInPixels = longestWidthInPixels;
            LongestLayerHeightInPixels = longestHeightInPixels;
        }

        public void AddLayer(Layer layer)
        {
            Layers.Add(layer);
        }

        public override string ToString()
        {
            return $"TileMap with {Layers.Count} layers and {TileSets.Count} tile sets.\n" +
                $"Layers: {string.Join(", ", Layers.Select(t => t.Name))}\n" +
                $"TileSets: {string.Join(", ", TileSets.Select(t => t.Source))}";
        }
    }
}
