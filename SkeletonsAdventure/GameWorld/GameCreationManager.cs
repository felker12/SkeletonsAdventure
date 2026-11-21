using RpgLibrary.EntityClasses;
using RpgLibrary.GameObjectClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;
using SkeletonsAdventure.Content;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.NPCs;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.ItemClasses.ItemManagement;
using SkeletonsAdventure.LibraryClasses;
using SkeletonsAdventure.Quests;
using System.IO;
using System.Linq;

namespace SkeletonsAdventure.GameWorld
{
    internal static class GameCreationManager
    {
        //Create the enemies from the content folder
        internal static Dictionary<string, Enemy> CreateEnemies(ContentManager content, EntityAssets defaultAssets, TexturesLibrary textures,
            GameItem skeletonDrop, GameItemLoadingManager itemLoader, IEnumerable<string> assetPaths)
        {
            var enemyTypes = EnemyClassCatalog.Entries.ToDictionary(entry => entry.Type, entry =>
                (Type: typeof(Enemy).Assembly.GetType(entry.Type, throwOnError: true), entry.DefaultBones));

            Dictionary<string, Enemy> enemies = [];

            //Iterate through each enemy data file and create the corresponding enemy instance
            foreach (string contentPath in assetPaths)
            {
                EnemyData data = content.Load<EnemyData>(contentPath);

                //Attempt to find the enemy type in our lookup table using the type name from the data file
                if (enemyTypes.TryGetValue(data.Type, out var registration))
                {
                    Type enemyType = registration.Type;
                    Texture2D texture;
                    try
                    {
                        texture = textures.GetTexture(data.TextureName);
                    }
                    catch (ContentLoadException ex)
                    {
                        throw new ContentLoadException($"Could not load texture '{data.TextureName}' for enemy asset '{contentPath}'. {ex.Message}", ex);
                    }
                    Enemy en = (Enemy)Activator.CreateInstance(enemyType, data, defaultAssets with { Texture = texture });
                    en.GuaranteedDrops.Add(itemLoader.LoadGameItemsFromItemBaseData(data.GuaranteedItems));
                    if (registration.DefaultBones && skeletonDrop is not null && !data.GuaranteedItems.Any(item => item.Name == skeletonDrop.Name))
                        en.GuaranteedDrops.Add(skeletonDrop);

                    enemies.Add(en.GetType().FullName, en);
                }
                else
                {
                    throw new ContentLoadException($"Unknown enemy type '{data.Type}' in '{contentPath}'.");
                }
            }

            return enemies;
        }

        //Create the items from the content folder
        internal static Dictionary<string, GameItem> CreateItems(ContentManager content, SpriteFont tooltipFont, IEnumerable<string> assetPaths)
        {
            Dictionary<string, GameItem> items = [];

            foreach (string contentPath in assetPaths)
            {
                string texturePath = null;
                try
                {
                    ItemData itemData = content.Load<ItemData>(contentPath);
                    texturePath = itemData.TexturePath;
                    Texture2D itemTexture = content.Load<Texture2D>(texturePath);
                    GameItem gameItem = CreateGameItemFromData(itemData, itemTexture, tooltipFont);

                    if (!items.TryAdd(gameItem.Name, gameItem))
                        throw new InvalidDataException($"Duplicate item name '{gameItem.Name}'.");
                }
                catch (Exception ex)
                {
                    string texture = texturePath is null ? "" : $" with texture '{texturePath}'";
                    throw new ContentLoadException($"Could not load item '{contentPath}'{texture}. {ex.Message}", ex);
                }
            }

            return items;
        }

        //TODO in the future load the drop tables from data
        internal static Dictionary<string, DropTable> CreateDropTables()
        {
            Dictionary<string, DropTable> dropTables = [];

            var basicTable = DataAssetCatalog.BasicDrops.Select(entry =>
                new DropTableItem(entry.Name, entry.Weight, entry.Minimum, entry.Maximum)).ToList();

            dropTables.Add(DataAssetCatalog.BasicDropTable, new(basicTable, noDropWeight: 35));

            //TODO : Create more drop tables as needed
            return dropTables;
        }

        internal static Dictionary<string, Chest> CreateChests(Texture2D menuTexture, Texture2D buttonTexture, SpriteFont buttonFont, SpriteFont infoFont) //TODO
        {
            Dictionary<string, Chest> chests = [];

            Chest BasicChest = new(menuTexture, buttonTexture, buttonFont, infoFont)
            {
                ID = 8,
                ChestType = ChestType.Basic,
                DropTableName = DataAssetCatalog.BasicDropTable,
            };

            string name = nameof(BasicChest);

            if (chests.ContainsKey(name) == false)
                chests.Add(name, BasicChest);

            Chest BasicChest2 = new(menuTexture, buttonTexture, buttonFont, infoFont)
            {
                ID = 784,
                ChestType = ChestType.Basic,
                DropTableName = DataAssetCatalog.BasicDropTable,
            };

            name = nameof(BasicChest2);
            if (chests.ContainsKey(name) == false)
                chests.Add(name, BasicChest2);


            return chests;
        }

        private static GameItem CreateGameItemFromData(ItemData itemData, Texture2D texture, SpriteFont tooltipFont)
        {
            return itemData switch
            {
                WeaponData w => new Weapon(w.Clone(), texture, tooltipFont),
                ArmorData a => new Armor(a.Clone(), texture, tooltipFont),
                ShieldData s => new Shield(s.Clone(), texture, tooltipFont),
                ConsumableData c => new Consumable(c.Clone(), texture, tooltipFont),
                _ => new GameItem(itemData.Clone(), texture, tooltipFont)
            };
        }

        internal static Dictionary<string, Quest> CreateQuests(ContentManager content, Dictionary<string, GameItem> ItemsClone,
            IEnumerable<string> assetPaths)
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
                MonsterToSlay = DataAssetCatalog.SkeletonType,
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
                Name = BuiltInQuests.TestQuest,
                Description = "This is a test quest to test the quest system.",
                Requirements = requirements,
                Tasks = Tasks,
            };

            Quest quest2 = quest.Clone();
            quest2.Name = BuiltInQuests.Test2;
            quest2.RequiredQuestNames.Add(quest.Name);

            Quest quest3 = new(quest.GetQuestData())
            {
                Name = BuiltInQuests.Test3,
            };

            QuestReward questReward = new()
            {
                Coins = 100,
                XP = 250,
                Items = [.. ItemsClone.Values] // Convert the Dictionary to a List
            };

            Quest SlaySkeletons = new()
            {
                Name = BuiltInQuests.SlaySkeletons,
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

            var authored = new HashSet<string>(StringComparer.Ordinal);
            foreach (string asset in assetPaths)
            {
                QuestData data = content.Load<QuestData>(asset);
                if (!authored.Add(data.Name))
                    throw new ContentLoadException($"Duplicate quest name '{data.Name}' in '{asset}'.");
                Quests[data.Name] = new Quest(data);
            }
            return Quests;
        }

        internal static Dictionary<string, NPC> CreateNPCs(ContentManager content, Dictionary<string, Quest> quests) //TODO
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
