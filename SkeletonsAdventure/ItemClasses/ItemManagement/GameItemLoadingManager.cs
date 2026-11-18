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
            GameItem item = LoadItem(itemData.Name, itemData.Quantity);
            item.Position = itemData.Position;
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
            return LoadItem(itemBaseData.Name, itemBaseData.Quantity);
        }

        private GameItem LoadItem(string name, int quantity)
        {
            if (string.IsNullOrEmpty(name) || !Items.TryGetValue(name, out var template))
                throw new System.IO.InvalidDataException($"Unknown saved item: {name}");
            if (quantity < 1 || (!template.Stackable && quantity != 1))
                throw new System.IO.InvalidDataException($"Invalid saved quantity for {name}: {quantity}");
            var item = template.Clone();
            item.SetQuantity(quantity);
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
