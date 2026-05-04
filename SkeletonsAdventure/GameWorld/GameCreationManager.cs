using Microsoft.Xna.Framework.Input;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.GameObjectClasses;
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.NPCs;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.ItemClasses.ItemManagement;
using SkeletonsAdventure.LibraryClasses;
using SkeletonsAdventure.Quests;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;

namespace SkeletonsAdventure.GameWorld
{
    internal static class GameCreationManager
    {
        //Create the enemies from the content folder
        internal static Dictionary<string, Enemy> CreateEnemies(ContentManager content, string gamePath)
        {
            // Use reflection to build a lookup table of all concrete Enemy subclasses in the current assembly.
            // This maps full type names (e.g., "SkeletonsAdventure.Entities.Skeleton") to Type objects,
            // allowing us to dynamically instantiate the correct enemy class based on the type name 
            // stored in each EnemyData XML file. Only non-abstract Enemy subclasses are included.
            var enemyTypes = typeof(Enemy).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Enemy)) && !t.IsAbstract)
                .ToDictionary(t => t.FullName, t => t);

            string EnemiesPath = Path.Combine(gamePath, "Content", "EntityData");
            string[] fileNames = Directory.GetFiles(EnemiesPath);

            Dictionary<string, Enemy> enemies = [];

            // Iterate through each enemy data file and create the corresponding enemy instance
            foreach (string s in fileNames)
            {
                string fileName = Path.GetFileNameWithoutExtension(s);
                EnemyData data = content.Load<EnemyData>($"EntityData/{fileName}");

                // Attempt to find the enemy type in our lookup table using the type name from the data file
                if (enemyTypes.TryGetValue(data.Type, out Type enemyType))
                {
                    // Dynamically create the specific enemy subclass and add to dictionary
                    Enemy en = (Enemy)Activator.CreateInstance(enemyType, data);
                    enemies.Add(en.GetType().FullName, en);

                    //Debug.WriteLine($"Adding enemy: {en.GetType().FullName}");
                }
                else
                {
                    Debug.WriteLine($"Warning: Type '{data.Type}' not found in {fileName}");
                }
            }

            /* string EnemiesPath = Path.Combine(GamePath, "Content", "EntityData");
             string[] fileNames = Directory.GetFiles(EnemiesPath);

             foreach (string s in fileNames)
             {
                 // Get just the filename without extension and path
                 string fileName = Path.GetFileNameWithoutExtension(s);

                 // Load using Content.Load with the correct content path format
                 EnemyData data = Content.Load<EnemyData>($"EntityData/{fileName}");
                 Enemy en = (Enemy)Activator.CreateInstance(Type.GetType(data.Type), data);
                 var en2 = new Enemy(data);

                 Debug.WriteLine($"Adding enemy: {en.GetType().FullName} & the test full name is: {en2.GetType().FullName}");

                 Enemies.Add(en.GetType().FullName, en); //Add the entity to the dictionary of enemies
             }*/

            return enemies;
        }

        //Create the items from the content folder
        internal static Dictionary<string, GameItem> CreateItems(ContentManager content)
        {
            string[] folders = Directory.GetDirectories(@"Content\Items");
            string[] names;
            string filePath;

            ItemData itemData;
            Texture2D itemTexure;
            GameItem gameItem;

            Dictionary<string, GameItem> items = [];

            foreach (string folder in folders)
            {
                //the name of the folder without extensions and the complete file path
                names = [.. Directory.GetFiles(folder).Select(fileName => Path.GetFileNameWithoutExtension(fileName))];

                foreach (string name in names)
                {
                    filePath = $@"..\{folder}\{name}"; //add the folder name to the path of the folder to get the file path without the extension

                    itemData = content.Load<ItemData>(filePath);
                    itemTexure = content.Load<Texture2D>(@$"{itemData.TexturePath}");

                    gameItem = CreateGameItemFromData(itemData);

                    if (items.ContainsKey(gameItem.Name) == false)
                        items.Add(gameItem.Name, gameItem);
                }
            }

            return items;
        }

        //TODO in the future load the drop tables from data
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

        internal static Dictionary<string, Chest> CreateChests() //TODO
        {
            Dictionary<string, Chest> chests = [];

            Chest BasicChest = new()
            {
                ID = 8,
                ChestType = ChestType.Basic,
                DropTableName = "BasicDropTable",
            };

            string name = nameof(BasicChest);

            if (chests.ContainsKey(name) == false)
                chests.Add(name, BasicChest);

            Chest BasicChest2 = new()
            {
                ID = 784,
                ChestType = ChestType.Basic,
                DropTableName = "BasicDropTable",
            };

            name = nameof(BasicChest2);
            if (chests.ContainsKey(name) == false)
                chests.Add(name, BasicChest2);


            return chests;
        }

        private static GameItem CreateGameItemFromData(ItemData itemData)
        {
            return itemData switch
            {
                WeaponData w => new Weapon(w.Clone()),
                ArmorData a => new Armor(a.Clone()),
                ShieldData s => new Shield(s.Clone()),
                ConsumableData c => new Consumable(c.Clone()),
                _ => new GameItem(itemData.Clone())
            };
        }

        internal static Dictionary<string, Quest> CreateQuests(Dictionary<string, GameItem> ItemsClone) //TODO
        {
            BaseTask task = new()
            {
                RequiredAmount = 3,
                TaskToComplete = "Kill that thing"
            };
            BaseTask task2 = new()
            {
                RequiredAmount = 5,
                TaskToComplete = "Do that thing"
            };
            BaseTask task3 = new()
            {
                RequiredAmount = 5,
                TaskToComplete = "talk to that person"
            };
            SlayTask slayTask = new()
            {
                RequiredAmount = 10,
                TaskToComplete = "Slay Entity: Skeleton",
                MonsterToSlay = typeof(Skeleton).FullName,
            };

            List<BaseTask> Tasks = [task.Clone(), task2.Clone(), task3.Clone(), slayTask.Clone()];

            LevelRequirements requirements = new()
            {
                Attack = 0,
                Defence = 0,
                Level = 0,
            };

            Quest quest = new()
            {
                Name = "Test Quest",
                Description = "This is a test quest to test the quest system.",
                Requirements = requirements,
                Tasks = Tasks,
            };

            Quest quest2 = quest.Clone();
            quest2.Name = "Test2";
            quest2.RequiredQuestNames.Add(quest.Name);

            Quest quest3 = new(quest.GetQuestData())
            {
                Name = "Test3",
            };

            QuestReward questReward = new()
            {
                Coins = 100,
                XP = 50,
                Items = [.. ItemsClone.Values] // Convert the Dictionary to a List
            };

            Quest SlaySkeletons = new()
            {
                Name = "SlaySkeletons",
                Description = "Kill 10 skeletons",
                Requirements = requirements,
                Reward = questReward,
            };
            SlaySkeletons.Tasks.Add(slayTask.Clone());

            List<Quest> quests = [quest, quest2, quest3, SlaySkeletons];
            Dictionary<string, Quest> Quests = [];

            foreach (var q in quests)
            {
                Quests.Add(q.Name, q);
            }

            return Quests;
        }

        internal static Dictionary<string, NPC> CreateNPCs(ContentManager content, string gamePath, Dictionary<string, Quest> quests) //TODO
        {
            Dictionary<string, NPC> npcs = [];

            //TODO

            /*
            NPCData data = new()
            {

            };
            */

            return npcs;
        }
    }
}