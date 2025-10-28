using RpgLibrary.ItemClasses;
using System.Linq;

namespace SkeletonsAdventure.ItemClasses.ItemManagement
{
    internal class DroppedLootManager
    {
        public List<GameItem> Items { get; set; } = [];
        private List<GameItem> ItemsToRemove { get; set; } = [];

        public void Update()
        {
            foreach (GameItem item in ItemsToRemove)
                Items.Remove(item);
            ItemsToRemove.Clear();

            foreach (GameItem item in Items)
                item.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (GameItem item in Items)
                item.Draw(spriteBatch);
        }

        public void Add(GameItem item, Vector2 pos)
        {
            item.Position = pos + new Vector2(20,20);
            Items.Add(item.Clone());
        }

        public void Add(List<GameItem> items, Vector2 pos)
        {
            Vector2 offset = Vector2.Zero;
            foreach (GameItem item in items)
            {
                Add(item, pos + offset);
                offset += new Vector2(15, 15);
            }
        }

        public void Add(ItemList lootList, Vector2 pos)
        {
            Add(lootList.Items, pos);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public void AddToRemoveList(GameItem item)
        {
            ItemsToRemove.Add(item);
        }

        public List<ItemData> GetDroppedItemData()
        {
            List<ItemData> droppedItemData = [];

            foreach (var gameItem in Items)
                droppedItemData.Add(gameItem.ToData());

            return droppedItemData;
        }

        public void Remove(GameItem item)
        {
            ItemsToRemove.Add(item);
        }
    }
}
