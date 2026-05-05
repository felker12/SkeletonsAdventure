using MonoGame.Extended.Tiled;
using RpgLibrary.DataClasses;
using System.IO;

namespace SkeletonsAdventure.GameWorld
{
    internal class LevelCreator
    {
        public Dictionary<string, Level> Levels { get; private set; } = [];

        public LevelCreator(ContentManager content, GraphicsDevice graphics)
        { 
            CreateLevels(content, graphics);
        }

        private void CreateLevels(ContentManager content, GraphicsDevice graphics)
        {
            string contentPath = content.RootDirectory;
            string tiledFilesPath = Path.Combine(contentPath, "TiledFiles");
            string[] tiledMapFiles = Directory.GetFiles(tiledFilesPath, "*.xnb", SearchOption.AllDirectories); //Note: this will include files that are not just map files (but also images)

            foreach (string filePath in tiledMapFiles)
            {
                //get the relative path for Content.Load (remove GamePath\Content\ and extension)
                string relativePath = Path.GetRelativePath(contentPath, filePath);
                relativePath = Path.ChangeExtension(relativePath, null); // Remove extension

                Debug.WriteLine($"Loading Tiled Map: {relativePath}");

                //try catch in case the .xnb file is not actually a TiledMap (but an image/tileset .xnb)
                try
                {
                    TiledMap tiledMap = content.Load<TiledMap>(relativePath);
                    MinMaxPair pair = GetMinMaxPairFromTiledMap(tiledMap);
                    Level level = new(graphics, tiledMap, GameManager.EnemiesClone, pair);

                    Levels.Add(level.Name, level);
                }
                catch (InvalidCastException)
                {
                    //This file was likely an image/tileset .xnb, not a TiledMap .xnb.
                    //Debug.WriteLine($"Skipping non-map asset: {relativePath}");
                }
            }

            //Initialize Levels: this should happen after all levels have been added
            //so that a level can reference another level as the enter or exit level
            foreach (Level lvl in Levels.Values)
                InitializeLevel(lvl);
        }

        private static MinMaxPair GetMinMaxPairFromTiledMap(TiledMap tiledMap)
        {
            MinMaxPair pair = new();
            if (tiledMap.Properties.TryGetValue("MinLevel", out TiledMapPropertyValue value))
                pair.Min = int.Parse(value.ToString());
            if (tiledMap.Properties.TryGetValue("MaxLevel", out value))
                pair.Max = int.Parse(value.ToString());
            else
                pair.Max = pair.Min; //the max level is the same as the min level if not specified
            return pair;
        }

        private void InitializeLevel(Level level)
        {
            if (level.EnterExitLayer == null)
                return;

            //TODO just used to temporarily provide a way to see where the hitboxes are for the exits
            Rectangle rec;

            foreach (TiledMapObject obj in level.EnterExitLayer.Objects)
            {
                if (obj.Name == "Entrance" || obj.Name == "Enter")
                {
                    if (obj.Properties.TryGetValue("ToLocation", out TiledMapPropertyValue value))
                        level.LevelEntrance = new(obj, Levels[value]);

                    level.PlayerStartPosition = new((int)obj.Position.X, (int)obj.Position.Y);
                    level.PlayerRespawnPosition = level.PlayerStartPosition;
                }
                else if (obj.Name == "Exit")
                {
                    level.LevelExit = new(obj, Levels[obj.Properties["ToLocation"]]);
                    level.PlayerEndPosition = new((int)obj.Position.X, (int)obj.Position.Y);
                }

                rec = new((int)obj.Position.X, (int)obj.Position.Y, (int)obj.Size.Width, (int)obj.Size.Height);
                level.EnterExitLayerObjectRectangles.Add(rec);
            }

            //if there is no level exit positin set it to the level entrance position
            if (level.LevelExit is null)
                level.PlayerEndPosition = level.PlayerStartPosition;
        }
    }
}
