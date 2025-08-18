
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using System.Linq;

namespace SkeletonsAdventure.Engines
{
    internal class DropTable
    {
        //private List<DropTableItem> DropTableList { get; set; } = [];

        private Dictionary<string, DropTableItem> DropTableDictionary { get; set; } = [];
        private static Random Random { get; set; } = new Random(); // Random number generator
        private int RandomIndex { get; set; } // Index for random item selection

        private readonly int _maxDropChance = 100; // Represents the total drop chance, default is 100%
        public string[] ItemNames; // Array to hold item names based on drop chance

        public DropTable()
        {
            ItemNames = new string[_maxDropChance]; // Initialize the ItemNames array with the size of max drop chance
        }

        private DropTable(Dictionary<string, DropTableItem> dictionary)
        {
            DropTableDictionary = dictionary;
            ItemNames = new string[_maxDropChance]; // Initialize the ItemNames array with the size of max drop chance
            PopulateItemNames();
        }

        public DropTableData ToData()
        {
            return new()
            {
                DropTableList = [.. DropTableDictionary.Values.Select(item => item.ToData())] // Convert each DropTableItem to DropTableItemData
            };
        }

        public DropTable Clone()
        {
            return new(DropTableDictionary);
        }

        public GameItem[] GetDrops(int amount)
        {
            GameItem[] items = new GameItem[amount];

            for(int i = 0; i < amount + 1; i++)
                items[i] = GetDrop();

            return items;
        }

        public List<GameItem> GetDropsList(int amount)
        {
            List<GameItem> items = [];

            for (int i = 0; i < amount; i++)
            {
                items.Add(GetDrop()); // Add each drop to the list
            }
            return items;
        }

        public GameItem GetDrop()
        {
            GameItem gameItem = null;

            if (ValidateAndPopulateItemNames() is false)
                return null; // If item names are not populated, return null

            for (int i = 0; i < _maxDropChance; i++)
            {
                gameItem = GetRandomItem(); // Get a random item from the drop table

                if (gameItem is not null)
                    break; // If a valid item is found, exit the loop
            }

            return gameItem; // Return the game item if found, otherwise return null
        }

        private bool ValidateAndPopulateItemNames()
        {
            if (IsItemNamesPopulated() is false) // Check if ItemNames array is populated with item names
            {
                PopulateItemNames(); // Populate the ItemNames array if not already done

                if (IsItemNamesPopulated() is false) // Check if ItemNames array is populated with item names after populating
                {
                    return false; // If still not populated, return false
                }
            }

            return true;
        }

        private GameItem GetRandomItem()
        {
            if (DropTableDictionary.Count == 0)
                return null; // Return null if there are no items in the drop table

            RandomIndex = Random.Next(0, _maxDropChance); // Get a random index based on the length of ItemNames array

            string itemName = ItemNames[RandomIndex]; // Get the item name from the ItemNames array

            if (itemName == string.Empty)
                return null; // If the item name is empty, return null

            GameItem item = GameManager.GetItemByName(itemName); // Retrieve the item by name

            if(item.Stackable)
            {
                DropTableItem dropTableItem = DropTableDictionary[itemName];

                int numb = Random.Next(dropTableItem.MinQuantity, dropTableItem.MaxQuantity + 1);
                item.SetQuantity(numb);
            }

            return item;
        }

        private bool IsItemNamesPopulated()
        {
            // Check if ItemNames array is populated with at least one non-empty item name
            return ItemNames.Any(name => !string.IsNullOrEmpty(name));
        }

        //fills the ItemNames array with item names based on their drop chances
        public void PopulateItemNames()
        {
            for (int i = 0; i < _maxDropChance; i++)
            {
                ItemNames[i] = string.Empty; // Initialize all item names to empty
            }

            int index = 0; // Index to track the position in ItemNames array
            foreach (var item in DropTableDictionary.Values)
            {
                if (RemainingSpace() < item.DropChance)
                    return; // If remaining drop chance is less than the item's drop chance, exit the method

                for (int i = 0; i < item.DropChance; i++)
                {
                    ItemNames[index] = item.ItemName; // Assign the item name to the ItemNames array
                    index++; // Move to the next index in the ItemNames array
                }
            }
        }

        public int RemainingSpace()
        {
            return _maxDropChance - TotalDropChance(); // Returns the remaining chance. Used to check if more items can be added.
        }

        public int TotalDropChance()
        {
            int totalChance = 0;
            foreach (var item in DropTableDictionary.Values)
            {
                totalChance += item.DropChance;
            }

            return totalChance;
        }

        public bool AddItem(DropTableItem item)
        {
            if (TotalDropChance() >= 100)
            {
                return false; // Cannot add item if total drop chance is already 100% or more
            }

            DropTableDictionary.Add(item.ItemName, item);
            return true; // Item added successfully
        }

        public void RemoveItem(DropTableItem item)
        {
            DropTableDictionary.Remove(item.ItemName);
        }

        public void Clear()
        {
            DropTableDictionary.Clear(); // Clears the drop table list
            ItemNames = new string[_maxDropChance]; // Reinitialize the ItemNames array
        }

        public override string ToString()
        {
            return string.Join(", ", DropTableDictionary.Values.Select(item => item.ToString())); // Returns a string representation of the drop table items
        }
    }
}
