global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Content;
global using Microsoft.Xna.Framework.Graphics;
global using System;
global using System.Collections.Generic;
global using System.Diagnostics; //this is just for debugging purposes

using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.NPCs;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.ItemClasses.ItemManagement;
using SkeletonsAdventure.Quests;
using System.Linq;
using SkeletonsAdventure.LibraryClasses;
using SkeletonsAdventure.HelperClasses;

namespace SkeletonsAdventure.GameWorld
{
    internal class GameManager
    {
        //Libraries
        public static FontsLibrary FontsLibrary { get; private set; }
        public static TexturesLibrary TexturesLibrary { get; private set; }
        public static AttackLibrary AttackLibrary { get; private set; }
        public static PathsLibrary PathsLibrary { get; private set; }
        public static XPTable XPTable { get; private set; }

        //Loading classes
        public static GameItemLoadingManager GameItemLoadingManager { get; private set; }

        //Dictionaries
        private static Dictionary<string, Enemy> Enemies { get; set; } = [];
        public static Dictionary<string, Enemy> EnemiesClone => CloneDictionary(Enemies);

        private static Dictionary<string, GameItem> Items { get; set; } = [];
        public static Dictionary<string, GameItem> ItemsClone => CloneDictionary(Items);

        private static Dictionary<string, DropTable> DropTables { get; set; } = [];
        public static Dictionary<string, DropTable> DropTablesClone => CloneDictionary(DropTables);

        private static Dictionary<string, Chest> Chests { get; set; } = [];
        public static Dictionary<string, Chest> ChestsClone => CloneDictionary(Chests);

        private static Dictionary<string, BasicAttack> EntityAttacks { get; set; } = [];
        public static Dictionary<string, BasicAttack> EntityAttackClone => CloneDictionary(EntityAttacks);

        private static Dictionary<string, Quest> Quests { get; set; } = [];
        public static Dictionary<string, Quest> QuestsClone => CloneDictionary(Quests);

        private static Dictionary<string, NPC> NPCs { get; set; } = [];
        public static Dictionary<string, NPC> NPCClone => CloneDictionary(NPCs);

        private static Dictionary<string, TiledAnimation> TiledAnimations { get; set; } = [];
        public static Dictionary<string, TiledAnimation> TiledAnimationsClone => CloneDictionary(TiledAnimations);

        //Miscellaneous Variables
        public static QuestManager QuestManager { get; set; } = new(); //TODO this isn't used
        public static ContentManager Content { get; private set; }

        public GameManager(ContentManager content, GraphicsDevice graphicsDevice)
        {
            Content = content;

            PathsLibrary = new();
            FontsLibrary = new(content);
            TexturesLibrary = new(content, graphicsDevice);
            AttackLibrary = new(content, TexturesLibrary);

            EntityAttacks = AttackLibrary.EntityAttacks;

            TiledAnimations = TiledHelperClasses.LoadTiledAnimations(Content);

            //The order of these is important because some of the data relies on other data to be created first.
            XPTable = new(PathsLibrary.SavePath);
            Items = GameCreationManager.CreateItems(Content);

            GameItemLoadingManager = new GameItemLoadingManager(Items);

            DropTables = GameCreationManager.CreateDropTables();
            Enemies = GameCreationManager.CreateEnemies(Content);
            Chests = GameCreationManager.CreateChests();
            Quests = GameCreationManager.CreateQuests(ItemsClone);
            NPCs = GameCreationManager.CreateNPCs(Content, QuestsClone);
        }

        public GameManager(Game1 game) : this(game.Content, game.GraphicsDevice)
        {
        }

        //Get a clone of the dictionaries
        private static Dictionary<string, T> CloneDictionary<T>(Dictionary<string, T> source) where T : class, ICloneableGameClass<T>
        {
            return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone());
        }

        public static T GetClonedValue<T>(Dictionary<string, T> source, string name) where T : class, ICloneableGameClass<T>
        {
            return source.TryGetValue(name, out T value) ? value.Clone() : null;
        }

        public static void DictionaryWriteTest<T>(Dictionary<string, T> dictionary) where T : class
        {
            foreach (var item in dictionary)
            {
                Debug.WriteLine($"Key: {item.Key}, Value: {item.Value}");
            }
        }

        //Get an item from the items dictionary by its name
        public static Enemy GetEnemyByName(string name) => GetClonedValue(Enemies, "SkeletonsAdventure.Entities." + name);
        public static GameItem GetItemByName(string name) => GetClonedValue(Items, name);
        public static DropTable GetDropTableByName(string name) => GetClonedValue(DropTables, name);
        public static BasicAttack GetAttackByName(string name) => GetClonedValue(EntityAttacks, name);
        public static Quest GetQuestByName(string name) => GetClonedValue(Quests, name);
    }
}