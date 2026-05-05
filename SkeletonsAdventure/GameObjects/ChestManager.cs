using MonoGame.Extended.Tiled;
using RpgLibrary.GameObjectClasses;

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

        public List<ChestData> GetChestDatas()
        {
            List<ChestData> chestDatas = [];

            foreach (Chest chest in Chests)
                chestDatas.Add(chest.ToData());

            return chestDatas;
        }
    }
}
