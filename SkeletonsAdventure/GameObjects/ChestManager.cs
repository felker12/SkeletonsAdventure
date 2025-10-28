using MonoGame.Extended.Tiled;
using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameObjects
{
    internal class ChestManager(TiledMapTileLayer mapChestLayer)
    {
        public List<Chest> Chests { get; set; } = [];
        public TiledMapTileLayer TiledMapTileLayer { get; set; } = mapChestLayer;

        public void Add(List<Chest> chests)
        {
            foreach (Chest chest in chests)
                Add(chest);
        }

        public void Add(Chest chest)
        {
            Chests.Add(chest);
        }

        public void Clear()
        {
            Chests.Clear();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach(Chest chest in Chests)
            {
                chest.Draw(spriteBatch);
            }    
        }

        public void Update(GameTime gameTime)
        {
            foreach (Chest chest in Chests)
            {
                chest.Update(gameTime);
            }
        }

        public void HandleInput(PlayerIndex playerIndex)
        {
            foreach (Chest chest in Chests)
            {
                chest.HandleInput(playerIndex);
            }
        }

        public List<Chest> GetChestsFromTiledMapTileLayer(Chest chest)
        {
            List<Chest> chests = [];
            int width = TiledMapTileLayer.TileWidth;
            int height = TiledMapTileLayer.TileHeight;

            foreach (TiledMapTile tile in GameManager.TileLocations(chest.ID, TiledMapTileLayer.Tiles))
            {
                chest.Position = new(tile.X * width, tile.Y * height);
                chest.DetectionArea = new Rectangle((int)chest.Position.X - 25, (int)chest.Position.Y - 25,
                    TiledMapTileLayer.TileWidth + 50, TiledMapTileLayer.TileHeight + 50);

                chests.Add(chest.Clone());
            }

            return chests;
        }

        public List<ChestData> GetChestDatas()
        {
            List<ChestData> chestDatas = [];

            foreach (Chest chest in Chests)
                chestDatas.Add(chest.ToData());

            return chestDatas;
        }

        public void UpdateFromSave(List<ChestData> chestDatas)
        {
            foreach (Chest chest in Chests)
            {
                foreach(ChestData data in chestDatas)
                {
                    if(chest.Position == data.Position)
                    {
                        chest.DropTable = GameManager.GetDropTableByName(chest.DropTableName);
                        chest.ChestEmptied = data.ChestEmptied;
                        chest.Items = GameManager.LoadGameItemsFromItemData(data.ItemDatas);
                    }
                }
            }
        }
    }
}
