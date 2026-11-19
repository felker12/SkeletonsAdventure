using MonoGame.Extended.Tilemaps;
using RpgLibrary.DataClasses;
using System.IO;
using SkeletonsAdventure.Animations;

namespace SkeletonsAdventure.GameWorld
{
    internal class LevelCreator
    {
        public Dictionary<string, Level> Levels { get; private set; } = [];

        public LevelCreator(ContentManager content, GraphicsDevice graphics,
            SpriteFont exitFont, SpriteFont damageFont, TiledAnimation leverAnimation)
        { 
            CreateLevels(content, graphics, exitFont, damageFont, leverAnimation);
        }

        private void CreateLevels(ContentManager content, GraphicsDevice graphics,
            SpriteFont exitFont, SpriteFont damageFont, TiledAnimation leverAnimation)
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
                    Tilemap tiledMap = content.Load<Tilemap>(relativePath);
                    MinMaxPair pair = GetMinMaxPairFromTiledMap(tiledMap);
                    string levelName = Path.ChangeExtension(Path.GetRelativePath(tiledFilesPath, filePath), null);
                    Level level = new(graphics, tiledMap, levelName, GameManager.EnemiesClone, pair,
                        exitFont, damageFont, leverAnimation);

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

        private static MinMaxPair GetMinMaxPairFromTiledMap(Tilemap tiledMap)
        {
            MinMaxPair pair = new();
            if (tiledMap.Properties.TryGetValue("MinLevel", out var value) && TiledHelperClasses.TryReadInt(value, out int min))
                pair.Min = min;
            if (tiledMap.Properties.TryGetValue("MaxLevel", out value) && TiledHelperClasses.TryReadInt(value, out int max))
                pair.Max = max;
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

            foreach (TilemapObject obj in level.EnterExitLayer.Objects)
            {
                if (obj.Name == "Entrance" || obj.Name == "Enter")
                {
                    if (obj.Properties.TryGetValue("ToLocation", out TilemapPropertyValue value))
                        level.LevelEntrance = new(obj, Levels[value.AsString()]);

                    level.PlayerStartPosition = new((int)obj.Position.X, (int)obj.Position.Y);
                    level.PlayerRespawnPosition = level.PlayerStartPosition;
                }
                else if (obj.Name == "Exit")
                {
                    level.LevelExit = new(obj, Levels[obj.Properties["ToLocation"].AsString()]);
                    level.PlayerEndPosition = new((int)obj.Position.X, (int)obj.Position.Y);
                }

                rec = new((int)obj.Position.X, (int)obj.Position.Y, (int)obj.Bounds.Width, (int)obj.Bounds.Height);
                level.EnterExitLayerObjectRectangles.Add(rec);
            }

            //if there is no level exit positin set it to the level entrance position
            if (level.LevelExit is null)
                level.PlayerEndPosition = level.PlayerStartPosition;
        }
    }
}
