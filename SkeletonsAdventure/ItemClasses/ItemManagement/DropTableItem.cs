
using RpgLibrary.ItemClasses;

namespace SkeletonsAdventure.ItemClasses.ItemManagement
{
    public class DropTableItem
    {
        public string ItemName { get; init; } = string.Empty;
        // Keep the serialized name for compatibility; runtime selection treats it as a weight.
        public int DropChance { get; init; } = 1;
        public int MinQuantity { get; init; } = 1;
        public int MaxQuantity { get; init; } = 1;

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
            return $"{ItemName} (Weight: {DropChance}, Quantity: {MinQuantity}-{MaxQuantity})";
        }
    }
}
