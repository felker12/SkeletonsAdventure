
using RpgLibrary.ItemClasses;

namespace SkeletonsAdventure.ItemClasses.ItemManagement
{
    public class DropTableItem
    {
        public string ItemName { get; set; } = string.Empty;
        public int DropChance { get; set; } = 1; // Represents the chance of the item dropping, e.g., 10 for 10%
        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 1;

        public DropTableItem() { }

        public DropTableItem(string itemName, int dropChance, int minQuantity = 1, int maxQuantity = 1)
        {
            ItemName = itemName;
            DropChance = dropChance;
            MinQuantity = minQuantity;
            MaxQuantity = maxQuantity;
        }

        public DropTableItem(string itemName, int dropChance)
        {
            ItemName = itemName;
            DropChance = dropChance;
        }

        public DropTableItem(DropTableItemData data)
        {
            ItemName = data.ItemName;
            DropChance = data.DropChance;
            MinQuantity = data.MinQuantity;
            MaxQuantity = data.MaxQuantity;
        }

        public DropTableItem Clone()
        {
            return new(ItemName, DropChance, MinQuantity, MaxQuantity);
        }

        public DropTableItemData ToData()
        {
            return new(ItemName, DropChance, MinQuantity, MaxQuantity);
        }

        public override string ToString()
        {
            return $"{ItemName} (Chance: {DropChance}%, Quantity: {MinQuantity}-{MaxQuantity})";
        }
    }
}
