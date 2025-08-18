
using RpgLibrary.ItemClasses;

namespace SkeletonsAdventure.Engines
{
    internal class DropTableItem
    {
        public string ItemName { get; set; } = string.Empty;
        public int DropChance { get; set; } = 1; // Represents the chance of the item dropping, e.g., 10 for 10%
        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 1;

        public DropTableItem() { }

        public DropTableItem(string itemName, int dropChance, int minQuantity, int maxQuantity)
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
