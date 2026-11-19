using RpgLibrary.ItemClasses;
using System.Linq;

namespace SkeletonsAdventure.ItemClasses.ItemManagement
{
    internal class DroppedLootManager
    {
        public List<GameItem> Items { get; set; } = [];
        public GroundLootPlacement Placement { get; set; }
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
            if (item is null)
                return;
            Add([item], pos);
        }

        public void Add(List<GameItem> items, Vector2 pos)
        {
            var drops = items.Where(item => item is not null).Select(item => item.Clone()).ToList();
            if (drops.Count == 0)
                return;
            var positions = Placement?.Place(pos, drops.Count, Items.Select(item => item.Position));
            for (int i = 0; i < drops.Count; i++)
            {
                var drop = drops[i];
                drop.Position = positions is null ? pos : positions[i];
                drop.Update();
                Items.Add(drop);
            }
        }

        public void RepairPlacement()
        {
            if (Placement is null || Items.Count == 0)
                return;
            foreach (var item in Items)
            {
                if (!Placement.IsSafe(item.Position))
                    item.Position = Placement.Place(item.Position, 1,
                        Items.Where(other => other != item).Select(other => other.Position))[0];
                item.Update();
            }
        }

        public void Add(ItemList lootList, Vector2 pos)
        {
            Add(lootList.Items, pos);
        }

        public void Clear()
        {
            Items.Clear();
            ItemsToRemove.Clear();
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
