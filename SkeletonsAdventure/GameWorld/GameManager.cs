global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Content;
global using Microsoft.Xna.Framework.Graphics;
global using System;
global using System.Collections.Generic;
global using System.Diagnostics; //this is just for debugging purposes

using MonoGame.Extended.Tiled;
using RpgLibrary.AttackData;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.GameObjectClasses;
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.NPCs;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.Quests;
using System.IO;
using System.Linq;

namespace SkeletonsAdventure.GameWorld
{
    internal class GameManager
    {
        //SpriteFonts
        public static SpriteFont Arial10 { get; private set; }
        public static SpriteFont Arial12 { get; private set; }
        public static SpriteFont Arial14 { get; private set; }
        public static SpriteFont Arial16 { get; private set; }
        public static SpriteFont Arial18 { get; private set; }
        public static SpriteFont Arial20 { get; private set; }

        //Strings
        public static string GamePath { get; private set; }
        public static string SavePath { get; private set; }

        //Dictionaries
        private static Dictionary<string, Enemy> Enemies { get; set; } = [];
        public static Dictionary<string, Enemy> EnemiesClone => GetEnemiesClone();

        private static Dictionary<string, GameItem> Items { get; set; } = [];
        public static Dictionary<string, GameItem> ItemsClone => GetItemsClone();

        //This is a dictionary of items that can be dropped by enemies, it is used to create the drop table for the enemies
        private static Dictionary<string, DropTableItem> DropTableItems { get; set; } = [];
        public static Dictionary<string, DropTableItem> DropTableItemsClone => GetDropTableItemsClone();

        //TODO create a dictinary for Drop Tables
        private static Dictionary<string, DropTable> DropTables { get; set; } = [];
        public static Dictionary<string, DropTable> DropTablesClone => GetDropTablesClone();

        private static Dictionary<string, Chest> Chests { get; set; } = [];
        public static Dictionary<string, Chest> ChestsClone => GetChestsClone();

        private static Dictionary<string, BasicAttack> EntityAttacks { get; set; } = [];
        public static Dictionary<string, BasicAttack> EntityAttackClone => GetEntityAttacksClone();

        private static Dictionary<string, Quest> Quests { get; set; } = [];
        public static Dictionary<string, Quest> QuestsClone => GetQuestsClone();

        private static Dictionary<string, NPC> NPCs { get; set; } = [];
        public static Dictionary<string, NPC> NPCClone => GetNPCClone();

        private static Dictionary<string, TiledAnimation> TiledAnimations { get; set; } = [];
        public static Dictionary<string, TiledAnimation> TiledAnimationsClone => GetTileAnimationsClone();

        //=====Textures=====
        //Entity Textures
        public static Texture2D SkeletonTexture { get; private set; }
        public static Texture2D SpiderTexture { get; private set; }
        public static Texture2D GoblinTexture { get; private set; }
        public static Texture2D SkeletalBruiserTexture { get; private set; } 
        public static Texture2D ArmoredSkeletonTexture { get; private set; }
        public static Texture2D SkeletonWarriorTexture { get; private set; }
        public static Texture2D SkeletonMageTexture { get; private set; }

        //Attack Textures
        public static Texture2D AttackAreaTexture { get; private set; }
        public static Texture2D SkeletonAttackTexture { get; private set; }
        public static Texture2D FireBallTexture { get; private set; }
        public static Texture2D FireBallTexture2 { get; private set; }
        public static Texture2D IcePillarTexture { get; private set; }
        public static Texture2D IcePillarSpriteSheetTexture { get; private set; }
        public static Texture2D IceBulletTexture { get; private set; }

        //UI Textures
        public static Texture2D ButtonBoxTexture { get; private set; }
        public static Texture2D DefaultButtonTexture { get; private set; }
        public static Texture2D GameMenuTexture { get; set; }
        public static Texture2D BackpackBackground { get; set; }
        public static Texture2D StatusBarTexture { get; set; }
        public static Texture2D ButtonTexture { get; set; }
        public static Texture2D TextBoxTexture { get; set; }

        //Miscellaneous Textures
        public static Texture2D DoorLeverAndChestAnimationTexture { get; private set; }


        //==========

        //Colors
        public static Color TextBoxColor { get; set; }
        //=========


        //Attacks
        public static AttackData BasicAttackData { get; set; }
        public static AttackData FireBallData { get; set; }
        public static AttackData IcePillarData { get; set; }
        public static AttackData IceBulletData { get; set; }

        //Miscellaneous Variables
        public static Game1 Game { get; private set; }
        public static GraphicsDevice GraphicsDevice { get; private set; }
        public static QuestManager QuestManager { get; set; } = new(); //TODO this isn't used
        public static ContentManager Content { get; private set; }
        public static List<int> PlayerLevelXPs { get; private set; } = [];

        public GameManager(Game1 game)
        {
            Game = game;
            Content = game.Content;
            GraphicsDevice = game.GraphicsDevice;

            SetPaths();
            CreatePlayerLevelXPs();

            SetColors();
            LoadFonts();
            LoadTextures();

            LoadAttacks();
            LoadTiledAnimations();

            CreateItems();
            CreateDropTables();

            CreateEnemies();
            //CreateEnemiesManually();

            CreateChests();
            CreateAttacks();
            CreateQuests();
            CreateNPCs();


            foreach (var item in Items)
            {
                Debug.WriteLine(item.ToString()); //TODO
            }
        }

        public static Texture2D CreateTextureFromColor(Color color)
        {
            Texture2D texture = new(GraphicsDevice, 1, 1);
            texture.SetData([color]);

            return texture;
        }

        private static void SetPaths()
        {
            GamePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName; //Project Directory
            SavePath = Path.GetFullPath(Path.Combine(GamePath, @"..\SaveFiles")); //Directory of the saved files

            /*
            //TODO
            string path = Path.Combine(GamePath, "Content", "EntityData");
            string contentDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Content.RootDirectory);
            Debug.WriteLine($"AppDomain: {AppDomain.CurrentDomain.BaseDirectory}");
            Debug.WriteLine($"Path: {path}");
            Debug.WriteLine($"Content Path: {contentDirectoryPath}");
            */
        }

        //Load data from saved files
        public static List<GameItem> LoadGameItemsFromItemData(List<ItemData> itemDatas)
        {
            List<GameItem> items = [];

            foreach (ItemData item in itemDatas)
                items.Add(LoadGameItemFromItemData(item));

            return items;
        }

        public static GameItem LoadGameItemFromItemData(ItemData itemData)
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

        //Get a clone of the dictionaries
        private static Dictionary<string, GameItem> GetItemsClone()
        {
            Dictionary<string, GameItem> items = [];

            foreach (var item in Items)
                items.Add(item.Key, item.Value.Clone());

            return items;
        }

        private static Dictionary<string, DropTableItem> GetDropTableItemsClone()
        {
            Dictionary<string, DropTableItem> dropTableItems = [];

            foreach (var item in DropTableItems)
                dropTableItems.Add(item.Key, item.Value.Clone());

            return dropTableItems;
        }

        private static Dictionary<string, DropTable> GetDropTablesClone()
        {
            Dictionary<string, DropTable> dropTables = [];

            foreach (var item in DropTables)
                dropTables.Add(item.Key, item.Value.Clone());

            return dropTables;
        }

        private static Dictionary<string, Chest> GetChestsClone()
        {
            Dictionary<string, Chest> chests = [];

            foreach (var item in Chests)
                chests.Add(item.Key, item.Value.Clone());

            return chests;
        }


        private static Dictionary<string, Enemy> GetEnemiesClone()
        {
            Dictionary<string, Enemy> enemy = [];

            foreach (var item in Enemies)
                enemy.Add(item.Key, item.Value.Clone());

            return enemy;
        }

        private static Dictionary<string, BasicAttack> GetEntityAttacksClone()
        {
            Dictionary<string, BasicAttack> attack = [];

            foreach (KeyValuePair<string, BasicAttack> item in EntityAttacks)
                attack.Add(item.Key, item.Value.Clone());

            return attack;
        }

        private static Dictionary<string, Quest> GetQuestsClone()
        {
            Dictionary<string, Quest> Quest = [];

            foreach (var quest in Quests)
                Quest.Add(quest.Key, quest.Value.Clone());

            return Quest;
        }

        private static Dictionary<string, NPC> GetNPCClone()
        {
            Dictionary<string, NPC> NPC = [];

            foreach (var npc in NPCs)
                NPC.Add(npc.Key, npc.Value.Clone());

            return NPC;
        }

        private static Dictionary<string, TiledAnimation> GetTileAnimationsClone()
        {
            Dictionary<string, TiledAnimation> animations = [];

            foreach (var animation in TiledAnimations)
                animations.Add(animation.Key, animation.Value.Clone());

            return animations;
        }

        //Create the player levels and their XP values
        public static void CreatePlayerLevelXPs() //TODO adjust the xp as needed
        {
            string levelsSavePath = Path.Combine(SavePath, "PlayerLevels.txt");

            if (File.Exists(levelsSavePath)) //If the file exists load the data
            {
                List<string> lines = [.. File.ReadAllLines(levelsSavePath)];

                foreach (var line in lines)
                {
                    string[] parts = line.Split(',');
                    if (int.TryParse(parts[1], out int xp))
                    {
                        PlayerLevelXPs.Add(xp);
                    }
                }
            }
            else
            {
                File.CreateText(levelsSavePath).Close(); //create the file if it doesn't exist
                string levels = string.Empty;

                for (int i = 0; i < 101; i++)
                {
                    if (i == 0)
                        PlayerLevelXPs.Add(0);
                    else
                        PlayerLevelXPs.Add((int)Math.Pow(i + 1, 2) * 20);

                    levels += $"{i},{PlayerLevelXPs[i]}" + Environment.NewLine;
                }

                File.WriteAllText(levelsSavePath, levels);
            }
        }

        //Get the level the player is at given the XP
        public static int GetPlayerLevelAtXP(int XP)
        {
            int level = 0;

            foreach (var levelXP in PlayerLevelXPs)
                if (XP > levelXP)
                    level = PlayerLevelXPs.IndexOf(levelXP);

            return level;
        }

        //Get the XP needed for the level
        public static int GetLevelXPAtLevel(int level)
        {
            return PlayerLevelXPs[level];
        }

        //Get an item from the items dictionary by its name
        public static GameItem GetItemByName(string name)
        {
            if (ItemsClone.TryGetValue(name, out GameItem item))
                return item.Clone();
            else
                return null;
        }

        public static DropTable GetDropTableByName(string name)
        {
            if (DropTablesClone.TryGetValue(name, out DropTable dropTable))
                return dropTable.Clone();
            else
                return null;
        }

        public static Enemy GetEnemyByName(string name)
        {
            if (EnemiesClone.TryGetValue("SkeletonsAdventure.Entities." + name, out Enemy enemy))
                return enemy.Clone();

            return null;
        }

        //Set the Colors
        private static void SetColors()
        {
            TextBoxColor = new Color(210, 210, 210, 220);
        }

        //Load the data from the content folder
        private static void LoadFonts()
        {
            Arial10 = Content.Load<SpriteFont>("Fonts/Arial10");
            Arial12 = Content.Load<SpriteFont>("Fonts/Arial12");
            Arial14 = Content.Load<SpriteFont>("Fonts/Arial14");
            Arial16 = Content.Load<SpriteFont>("Fonts/Arial16");
            Arial18 = Content.Load<SpriteFont>("Fonts/Arial18");
            Arial20 = Content.Load<SpriteFont>("Fonts/Arial20");
        }

        private static void LoadTextures()
        {
            //Entity Textures
            SkeletonTexture = Content.Load<Texture2D>(@"Player/SkeletonSpriteSheet");
            SkeletonAttackTexture = Content.Load<Texture2D>(@"Player/SkeletonAttackSprites");
            SpiderTexture = Content.Load<Texture2D>(@"EntitySprites/spider");
            GoblinTexture = Content.Load<Texture2D>(@"EntitySprites/goblin");
            SkeletalBruiserTexture = Content.Load<Texture2D>(@"EntitySprites/SkeletalBruiser");
            SkeletonWarriorTexture = Content.Load<Texture2D>(@"EntitySprites/SkeletonWarrior");
            SkeletonMageTexture = Content.Load<Texture2D>(@"EntitySprites/SkeletonMage");

            //Attack Textures
            FireBallTexture = Content.Load<Texture2D>(@"AttackSprites/FireBall_01");
            FireBallTexture2 = Content.Load<Texture2D>(@"AttackSprites/FireBallSpriteSheet");
            IcePillarTexture = Content.Load<Texture2D>(@"AttackSprites/IcePillar");
            IcePillarSpriteSheetTexture = Content.Load<Texture2D>(@"AttackSprites/IcePillarSpriteSheet");
            IceBulletTexture = Content.Load<Texture2D>(@"AttackSprites/IceBullet");

            AttackAreaTexture = new(GraphicsDevice, 1, 1);
            AttackAreaTexture.SetData([new Color(153, 29, 20, 250)]);

            //UI Textures
            ButtonBoxTexture = new(GraphicsDevice, 1, 1);
            ButtonBoxTexture.SetData([new Color(83, 105, 140, 230)]);

            DefaultButtonTexture = new(GraphicsDevice, 1, 1);
            DefaultButtonTexture.SetData([new Color(83, 105, 140, 230)]);

            GameMenuTexture = new(GraphicsDevice, 1, 1);
            GameMenuTexture.SetData([new Color(171, 144, 91, 250)]);

            BackpackBackground = Content.Load<Texture2D>(@"TiledFiles/BackpackBackground");

            StatusBarTexture = CreateTextureFromColor(Color.White);

            ButtonTexture = Content.Load<Texture2D>("Controls/Button");

            TextBoxTexture = CreateTextureFromColor(TextBoxColor);

            //Miscellaneous Textures
            DoorLeverAndChestAnimationTexture = Content.Load<Texture2D>(@"TiledFiles/doors_lever_chest_animation_0");
        }

        private static void LoadAttacks()
        {
            BasicAttackData = Content.Load<AttackData>(@"AttackData/BasicAttack");
            FireBallData = Content.Load<AttackData>(@"AttackData/FireBall");
            IcePillarData = Content.Load<AttackData>(@"AttackData/IcePillar");
            IceBulletData = Content.Load<AttackData>(@"AttackData/IceBullet");
        }

        private static void LoadTiledAnimations()
        {
            TiledMapTileset tiledMapTileset = Content.Load<TiledMapTileset>(@"TiledFiles/doors_lever_chest_animation");
            string tileName = tiledMapTileset.Name;

            foreach (var tile in tiledMapTileset.Tiles)
            {
                if(tile is TiledMapTilesetAnimatedTile animatedTile)
                {
                    //to have a unique key for each animated tile the name will be the texture name + "_" + the tile id
                    TiledAnimations.Add(tileName + "_" + tile.LocalTileIdentifier, new(tiledMapTileset, animatedTile));
                }
            }
        }

        //Create the items from the content folder
        private static void CreateItems()
        {
            string[] folders = Directory.GetDirectories(@"Content\Items");
            string[] names;
            string filePath;

            ItemData itemData;
            Texture2D itemTexure;
            GameItem gameItem;
            Weapon weapon;
            Armor armor;
            Shield shield;
            Consumable consumable;

            foreach (string folder in folders)
            {
                //the name of the folder without extensions and the complete file path
                names = [.. Directory.GetFiles(folder).Select(fileName => Path.GetFileNameWithoutExtension(fileName))];

                foreach (string name in names)
                {
                    filePath = $@"..\{folder}\{name}"; //add the folder name to the path of the folder to get the file path without the extension

                    itemData = Content.Load<ItemData>(filePath);
                    itemTexure = Content.Load<Texture2D>(@$"{itemData.TexturePath}");

                    //cast the itemData to the correct type
                    if (itemData is WeaponData weaponData)
                    {
                        weapon = new(weaponData.Clone());
                        gameItem = weapon;
                    }
                    else if (itemData is ArmorData armorData)
                    {
                        armor = new(armorData.Clone());
                        gameItem = armor;
                    }
                    else if(itemData is ShieldData shieldData)
                    {
                        shield = new(shieldData.Clone());
                        gameItem = shield;
                    }
                    else if (itemData is ConsumableData consumableData)
                    {
                        consumable = new(consumableData.Clone());
                        gameItem = consumable;
                    }
                    else
                    {
                        gameItem = new(itemData.Clone());
                    }

                    if (Items.ContainsKey(gameItem.Name) == false)
                        Items.Add(gameItem.Name, gameItem);

                    CreateDropTableItemFromGameItem(gameItem); //Create a drop table item from the game item
                }
            }
        }

        private static void CreateDropTables() //TODO: Drop tables should be created from the content folder
        {
            DropTable BasicDropTable = new(); //Create a basic drop table
            BasicDropTable.AddItem(new DropTableItem(ItemsClone["Coins"].Name, 10, 1, 12));
            BasicDropTable.AddItem(new DropTableItem(ItemsClone["Robes"].Name, 10, 1, 1)); 
            BasicDropTable.AddItem(new DropTableItem(ItemsClone["Bones"].Name, 10, 1, 2)); 
            BasicDropTable.AddItem(new DropTableItem(ItemsClone["Sword"].Name, 10, 1, 1)); 
            BasicDropTable.PopulateItemNames(); //Populate the item names in the drop table

            DropTables.Add("BasicDropTable", BasicDropTable); //Add the basic drop table to the dictionary

            //TODO : Create more drop tables as needed
        }

        private static bool CreateDropTableItemFromGameItem(GameItem item)
        {
            if (item is not null && item.Name is not null)
            {
                DropTableItem dropTableItem = new(item.Name, 1, 1, 1);
                DropTableItems.Add(dropTableItem.ItemName, dropTableItem);
                return true; // Return true if the item was successfully added
            }

            return false; // Return false if the item was null or invalid
        }

        //Create the enemies from the content folder
        private static void CreateEnemies()
        {
            string EnemiesPath = Path.Combine(GamePath, "Content", "EntityData");
            string[] fileNames = Directory.GetFiles(EnemiesPath);

            foreach (string s in fileNames)
            {
                // Get just the filename without extension and path
                string fileName = Path.GetFileNameWithoutExtension(s);
                
                // Load using Content.Load with the correct content path format
                EnemyData data = Content.Load<EnemyData>($"EntityData/{fileName}");
                Enemy en = (Enemy)Activator.CreateInstance(Type.GetType(data.Type), data);

                Enemies.Add(en.GetType().FullName, en); //Add the entity to the dictionary of enemies
            }

            //TODO this is used for testing
            /*

            string EnemyPath = Path.Combine(SavePath, "Enemies"); //Directory of the enemies

            //if (Path.Exists(ItemPath) == false)
            //    Directory.CreateDirectory(ItemPath); //Create the directory if it doesn't exist
            if (Path.Exists(EnemyPath) == false)
                Directory.CreateDirectory(EnemyPath); //Create the directory if it doesn't exist

            //TODO test this
            foreach (var enemy in Enemies)//shouldn't be needed now
            {
                XnaSerializer.Serialize($@"{EnemyPath}\{enemy.Value.GetType().Name}Data.xml", enemy.Value.GetEntityData());
            }

            */
        }

        //This is the old method for creating enemies. It is kept for reference and testing purposes
        private static void CreateEnemiesManually()
        {
            //Create the entities from the data and add their items to their loot list
            EnemyData entityData = new(Content.Load<EnemyData>(@"EntityData/SkeletonData"));

            Skeleton skeleton = new(entityData);
            EliteSkeleton eliteSkeleton = new(entityData);

            entityData = new(Content.Load<EnemyData>(@"EntityData/SpiderData"));

            Spider spider = new(entityData);

            //Add the entities to the dictionary
            Enemies.Add(skeleton.GetType().FullName, skeleton);
            Enemies.Add(eliteSkeleton.GetType().FullName, eliteSkeleton);
            Enemies.Add(spider.GetType().FullName, spider);

            //TODO these are temprorary paths, the files created here are not
            //saved in the content folder and will need to be moved to the content folder
            //ItemPath = Path.Combine(SavePath, "Items"); //Directory of the items
            string EnemyPath = Path.Combine(SavePath, "Enemies"); //Directory of the enemies

            //if (Path.Exists(ItemPath) == false)
            //    Directory.CreateDirectory(ItemPath); //Create the directory if it doesn't exist
            if (Path.Exists(EnemyPath) == false)
                Directory.CreateDirectory(EnemyPath); //Create the directory if it doesn't exist

            //TODO test this
            foreach (var enemy in Enemies)//shouldn't be needed now
            {
                XnaSerializer.Serialize($@"{EnemyPath}\{enemy.Value.GetType().Name}Data.xml", enemy.Value.GetEntityData());
            }
        }

        //Create the chests from the content folder
        private static void CreateChests() //TODO
        {
            ItemsClone.TryGetValue("Coins", out GameItem Coins);
            Coins.SetQuantity(10);

            ItemList loots = new();
            loots.Add(ItemsClone["Robes"]);
            loots.Add(ItemsClone["Bones"]);
            loots.Add(Coins.Clone());
            loots.Add(ItemsClone["Sword"]);

            Chest BasicChest = new()
            {
                ID = 8,
                ChestType = ChestType.Basic,
                Loot = loots.Clone()
            };

            string name = nameof(BasicChest);
            if (Chests.ContainsKey(name) == false)
                Chests.Add(name, BasicChest);

            Chest BasicChest2 = new()
            {
                ID = 784,
                ChestType = ChestType.Basic,
                Loot = loots.Clone()
            };

            name = nameof(BasicChest2);
            if (Chests.ContainsKey(name) == false)
                Chests.Add(name, BasicChest2);
        }

        private static void CreateAttacks()
        {
            //Create the attacks from the content folder
            BasicAttack attack = new(BasicAttackData, SkeletonAttackTexture, null);
            EntityAttacks.Add(attack.GetType().Name, attack);

            FireBall fireball = new(FireBallData, FireBallTexture2, null);
            EntityAttacks.Add(fireball.GetType().Name, fireball);

            IcePillar icePillar = new(IcePillarData, IcePillarSpriteSheetTexture, null);
            EntityAttacks.Add(icePillar.GetType().Name, icePillar);

            IceBullet iceBullet = new(IceBulletData, IceBulletTexture, null);
            EntityAttacks.Add(iceBullet.GetType().Name, iceBullet);
        }

        private static void CreateQuests() //TODO
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

            List <BaseTask> Tasks = [task.Clone(), task2.Clone(), task3.Clone(), slayTask.Clone()];

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
                Items = [..ItemsClone.Values] // Convert the Dictionary to a List
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

            foreach(var q in quests)
            {
                Quests.Add(q.Name, q);
            }
        }

        private static void CreateNPCs() //TODO
        {
            /*
            NPCData data = new()
            {

            };
            */
        }

        public static List<TiledMapTile> TileLocations(int id, TiledMapTile[] tiles)
        {
            List<TiledMapTile> mapTiles = [];

            foreach (var tile in tiles)
            {
                if (tile.GlobalIdentifier == id)
                    mapTiles.Add(tile);
            }
            return mapTiles;
        }

        public static List<TiledMapObject> ObjectLocations(string name, TiledMapObject[] objects)
        {
            List<TiledMapObject> mapObjects = [];
            foreach (var obj in objects)
            {
                if (obj.Name == name)
                    mapObjects.Add(obj);
            }
            return mapObjects;
        }
    }
}
