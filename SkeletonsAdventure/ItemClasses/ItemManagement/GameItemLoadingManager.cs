using RpgLibrary.ItemClasses;

namespace SkeletonsAdventure.ItemClasses.ItemManagement
{
    internal class GameItemLoadingManager
    {
        private Dictionary<string, GameItem> Items { get; set; } = [];

        public GameItemLoadingManager(Dictionary<string, GameItem> items)
        {
            Items = items;
        }

        //Load data from saved files
        public GameItem LoadGameItemFromItemData(ItemData itemData)
        {
            GameItem item = null;

            foreach (GameItem gameItem in Items.Values)
            {
                if (itemData.Name == gameItem.Name)
                {
                    item = gameItem.Clone();
                    item.SetQuantity(itemData.Quantity);
                    item.Position = itemData.Position;
                }
            }

            return item;
        }

        public List<GameItem> LoadGameItemsFromItemData(List<ItemData> itemDatas)
        {
            List<GameItem> items = [];

            foreach (ItemData item in itemDatas)
                items.Add(LoadGameItemFromItemData(item));

            return items;
        }

        public GameItem LoadGameItemFromItemBaseData(ItemBaseData itemBaseData)
        {
            GameItem item = null;

            foreach (GameItem gameItem in Items.Values)
            {
                if (itemBaseData.Name == gameItem.Name)
                {
                    item = gameItem.Clone();
                    item.SetQuantity(itemBaseData.Quantity);
                }
            }

            return item;
        }

        public List<GameItem> LoadGameItemsFromItemBaseData(List<ItemBaseData> itemDatas)
        {
            List<GameItem> items = [];

            foreach (ItemBaseData item in itemDatas)
                items.Add(LoadGameItemFromItemBaseData(item));

            return items;
        }
    }
}
