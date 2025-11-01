using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.ItemClasses.ItemManagement;

namespace SkeletonsAdventure.GameWorld
{
    internal static class GameCreationManager
    {
        internal static Dictionary<string, DropTable> CreateDropTables()
        {
            Dictionary<string, DropTable> dropTables = [];

            List<DropTableItem> basicTable = [
                new ("Coins", 20, 1, 12),
                new ("Robes", 5, 1, 1),
                new ("Bones", 20, 1, 2),
                new ("Sword", 5, 1, 1),
                new("Shoes", 5, 1, 1),
                new("Hood", 5),
                new("Round Shield", 5),
            ];

            dropTables.Add("BasicDropTable", new(basicTable)); //Add the basic drop table to the dictionary

            //TODO : Create more drop tables as needed
            return dropTables;
        }

        internal static Dictionary<string, Chest> CreateChests(Dictionary<string, DropTable> dropTables) //TODO
        {
            Dictionary<string, Chest> chests = [];

            Chest BasicChest = new()
            {
                ID = 8,
                ChestType = ChestType.Basic,
                DropTableName = "BasicDropTable",
                //DropTable = new(dropTables["BasicDropTable"]),
            };

            string name = nameof(BasicChest);
            if (chests.ContainsKey(name) == false)
                chests.Add(name, BasicChest);

            Chest BasicChest2 = new()
            {
                ID = 784,
                ChestType = ChestType.Basic,
                DropTableName = "BasicDropTable",
                //DropTable = new(dropTables["BasicDropTable"]),
            };

            name = nameof(BasicChest2);
            if (chests.ContainsKey(name) == false)
                chests.Add(name, BasicChest2);


            return chests;
        }
    }
}
