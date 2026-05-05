using RpgLibrary.AttackData;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.GameObjectClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.MenuClasses;
using RpgLibrary.SettingsClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using System.IO;

namespace SkeletonsAdventure.Engines
{
    internal class SaveManager
    {
        public static void SaveGame(Game1 game, ExitScreenMenu exitScreen)
        {
            string savePath = GameManager.PathsLibrary.SavePath;
            Player player = World.Player;

            try
            {
                if (Directory.Exists(savePath) == false)
                    Directory.CreateDirectory(savePath);

                MenuManagerData GameScreenMenuData = new();
                foreach (BaseMenu baseMenu in game.GameScreen.Menus)
                {
                    if (baseMenu is TabbedMenu tabbedMenu)
                        GameScreenMenuData.Menus.Add(tabbedMenu.GetTabbedMenuData());
                    else if (baseMenu is not null)
                        GameScreenMenuData.Menus.Add(baseMenu.GetMenuData());
                }

                //save the data
                XnaSerializer.Serialize<WorldData>(savePath + @"\World.xml", World.ToData());
                XnaSerializer.Serialize<MenuManagerData>(savePath + @"\GameScreenMenuData.xml", GameScreenMenuData);
                XnaSerializer.Serialize<TabbedMenuData>(savePath + @"\ExitScreenData.xml", exitScreen.GetTabbedMenuData());
                XnaSerializer.Serialize<List<String>>(Path.Combine(savePath, "MessageBox.xml"), game.GameScreen.MessageBox.Messages);

                XnaSerializer.Serialize<KeyBindingsManagerData>(savePath + @"\Keybindings.xml", player.KeybindingsManager.ToData());
                XnaSerializer.Serialize<LearnedAttackManagerData>(savePath + @"\LearnedAttackManager.Xml", player.LearnedAttackManager.ToData());
            }
            catch (Exception ex)
            {
                //TODO handle exception
                Debug.WriteLine(ex);
                return;
            }
        }

        //Load the world
        public static void LoadGame(WorldData worldData)
        {
            //Restore Player Stats and Basic Info
            World.Player.UpdatePlayerWithData(worldData.PlayerData);
            World.TotalTimeInWorld.TotalGameTime = worldData.TotalTimeInWorld;

            //Restore Backpack and Equipment
            RestorePlayerInventory(worldData.PlayerData.backpack);

            //Restore Levels State
            foreach (var levelEntry in World.Levels)
            {
                if (worldData.Levels.TryGetValue(levelEntry.Key, out LevelData levelData))
                {
                    levelEntry.Value.Player = World.Player;
                    LoadLevelDataFromLevelData(levelEntry.Value, levelData);
                }
            }

            //Set the current location
            if (World.Levels.TryGetValue(worldData.CurrentLevel, out Level targetLevel))
            {
                World.SetCurrentLevel(targetLevel, World.Player.Position);
            }
        }

        private static void RestorePlayerInventory(List<ItemData> itemDatas)
        {
            World.Player.Backpack.Items.Clear(); //Clear current items before loading

            foreach (ItemData data in itemDatas)
            {
                GameItem item = GameManager.GameItemLoadingManager.LoadGameItemFromItemData(data);
                World.Player.Backpack.Add(item);

                if (data.Equipped)
                {
                    //Always equip from the instance inside the backpack
                    var itemInBackpack = World.Player.Backpack.Items[^1];
                    World.Player.EquippedItems.TryEquipItem(itemInBackpack);
                }
            }
        }

        //Load the levels
        public static void LoadLevelDataFromLevelData(Level level, LevelData levelData)
        {
            //Set the visibility of the layers based on the saved data
            List<(string, bool)> layerVisibility = levelData.LayerVisibility;

            foreach (var layer in level.TiledMap.TileLayers)
            {
                foreach (var (name, visible) in layerVisibility)
                {
                    if (layer.Name == name)
                    {
                        layer.IsVisible = visible;
                        break;
                    }
                }
            }

            //Clear out the current entities and load in the saved ones
            level.EntityManager.Clear();
            level.EntityManager.Add(level.Player);
            level.EntityManager.DroppedLootManager.Items = GameManager.GameItemLoadingManager.LoadGameItemsFromItemData(levelData.DroppedItemDatas);
            LoadEnemies(level, levelData.EntityManagerData);

            //Load the chests and their contents
            UpdateChestManagerFromSave(level.ChestManager, levelData.Chests);

            //Load the interactable objects and their data
            level.InteractableObjectManager.LoadFromData(levelData.InteractableObjectManagerData);
        }

        private static void LoadEnemies(Level level, EntityManagerData entityManagerData)
        {
            foreach (Enemy enemy in level.Enemies.Values)
            {
                foreach (EntityData entityData in entityManagerData.EntityData)
                {
                    if (entityData is EnemyData data && enemy.GetType().FullName == data.Type)
                    {
                        Enemy en = (Enemy)Activator.CreateInstance(enemy.GetType(), data);
                        en.SetEnemyLevel(data.EntityLevel);
                        en.GuaranteedDrops.Add(GameManager.GameItemLoadingManager.LoadGameItemsFromItemBaseData(data.GuaranteedItems));
                        level.EntityManager.Add(en);
                    }
                }
            }
        }

        public static void UpdateChestManagerFromSave(ChestManager chestManager, List<ChestData> chestDatas)
        {
            foreach (Chest chest in chestManager.Chests)
            {
                foreach (ChestData data in chestDatas)
                {
                    if (chest.Position == data.Position)
                    {
                        chest.DropTable = GameManager.GetDropTableByName(chest.DropTableName);
                        chest.ChestEmptied = data.ChestEmptied;
                        chest.Items = GameManager.GameItemLoadingManager.LoadGameItemsFromItemData(data.ItemDatas);
                    }
                }
            }
        }
    }
}
