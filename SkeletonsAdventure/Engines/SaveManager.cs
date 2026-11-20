using RpgLibrary.EntityClasses;
using RpgLibrary.GameObjectClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.MenuClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using System.IO;
using System.Linq;

namespace SkeletonsAdventure.Engines
{
    internal class SaveManager
    {
        internal static void RestoreGame(Game1 game, GameManager assets, SaveGameData data)
        {
            var rollback = World.CaptureRestorePoint();
            try
            {
                var texture = game.Content.Load<Texture2D>(data.World.PlayerData.TextureName);
                var screen = new States.GameScreen(game, assets, data.World, texture, data.IsTestGame);
                World.Player.LearnedAttackManager.UpdateWithData(data.LearnedAttacks);
                World.Player.SetKeybinding(data.Bindings.Keybindings);
                screen.MessageBox.AddMessages(data.Messages);
                foreach (var saved in data.Menus.Menus)
                    foreach (var menu in screen.Menus)
                        if (menu.Title == saved.Title)
                        {
                            if (menu is TabbedMenu tabbed && saved is TabbedMenuData tabbedData)
                                tabbed.SetMenuData(tabbedData);
                            else
                                menu.SetMenuData(saved);
                        }
                var exit = new States.ExitScreen(game, assets);
                exit.SetExitScreenMenuData(data.PauseMenu);
                game.GameScreen = screen;
                game.ExitScreen = exit;
            }
            catch
            {
                rollback();
                throw;
            }
        }

        internal static void LoadSavedGame(Game1 game, GameManager assets, string directory)
        {
            SaveStorage.Read(directory, data => RestoreGame(game, assets, data), out var primaryFailure);
            game.PreserveSaveBackup = primaryFailure is not null;
            if (primaryFailure is not null)
            {
                Debug.WriteLine($"Recovered '{Path.Combine(directory, SaveStorage.FileName)}' from backup. Primary failure: {primaryFailure}");
                game.GameScreen.MessageBox.AddMessage("Loaded the backup save.");
                game.GameScreen.MessageBox.Visible = true;
            }
        }

        public static bool SaveGame(Game1 game, ExitScreenMenu exitScreen, string directory = null)
        {
            try
            {
                SaveStorage.Write(directory ?? GameManager.PathsLibrary.SavePath,
                    CaptureGame(game, exitScreen), game.PreserveSaveBackup);
                game.PreserveSaveBackup = false;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        internal static SaveGameData CaptureGame(Game1 game, ExitScreenMenu exitScreen)
        {
            Player player = World.Player;
            MenuManagerData GameScreenMenuData = new();
            foreach (BaseMenu baseMenu in game.GameScreen.Menus)
            {
                if (baseMenu is TabbedMenu tabbedMenu)
                    GameScreenMenuData.Menus.Add(tabbedMenu.GetTabbedMenuData());
                else if (baseMenu is not null)
                    GameScreenMenuData.Menus.Add(baseMenu.GetMenuData());
            }

            var learned = player.LearnedAttackManager.ToData();
            foreach (var attack in player.KeybindingsManager.Keybindings.Values)
                if (attack is not null && !learned.LearnedAttackNames.Contains(attack.Name))
                    learned.LearnedAttackNames.Add(attack.Name);
            return new SaveGameData
            {
                IsTestGame = game.GameScreen.IsTestGame,
                World = World.ToData(),
                Menus = GameScreenMenuData,
                PauseMenu = exitScreen.GetTabbedMenuData(),
                Messages = [.. game.GameScreen.MessageBox.Messages, .. World.MessagesToAdd],
                Bindings = player.KeybindingsManager.ToData(),
                LearnedAttacks = learned
            };
        }

        //Load the world
        public static void LoadGame(WorldData worldData, Texture2D playerTexture)
        {
            if (!World.Levels.ContainsKey(worldData.CurrentLevel))
                throw new InvalidDataException($"Unknown saved level: {worldData.CurrentLevel}");
            foreach (var name in worldData.Levels.Keys)
                if (!World.Levels.ContainsKey(name))
                    throw new InvalidDataException($"Unknown saved level: {name}");
            //Restore Player Stats and Basic Info
            World.Player.UpdatePlayerWithData(worldData.PlayerData, playerTexture);
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
                if (worldData.PlayerData.RespawnPosition is Vector2 respawn)
                    World.Player.RespawnPosition = respawn;
            }
        }

        private static void RestorePlayerInventory(List<ItemData> itemDatas)
        {
            World.Player.EquippedItems.Clear();
            World.Player.Backpack.Items.Clear();

            foreach (ItemData data in itemDatas)
            {
                GameItem item = GameManager.GameItemLoadingManager.LoadGameItemFromItemData(data);
                if (item is EquipableItem equipment)
                    equipment.SetEquipped(false);
                // Preserve saved stacks and object identity; Add() clones and merges items.
                World.Player.Backpack.Items.Add(item);

                if (data.Equipped)
                {
                    World.Player.EquippedItems.TryEquipItem(item);
                    if (item is not EquipableItem equipped || !equipped.Equipped)
                        throw new InvalidDataException($"Cannot restore equipped item: {data.Name}");
                }
            }
            World.Player.RefreshEquipmentStats();
        }

        //Load the levels
        public static void LoadLevelDataFromLevelData(Level level, LevelData levelData)
        {
            //Set the visibility of the layers based on the saved data
            List<(string, bool)> layerVisibility = levelData.LayerVisibility;

            foreach (var layer in level.TiledMap.Layers)
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
            level.EntityManager.DroppedLootManager.RestoreDrops(
                GameManager.GameItemLoadingManager.LoadGameItemsFromItemData(levelData.DroppedItemDatas));
            LoadEnemies(level, levelData.EntityManagerData);

            //Load the chests and their contents
            UpdateChestManagerFromSave(level.ChestManager, levelData.Chests);

            //Load the interactable objects and their data
            level.InteractableObjectManager.LoadFromData(levelData.InteractableObjectManagerData);
        }

        private static void LoadEnemies(Level level, EntityManagerData entityManagerData)
        {
            foreach (EntityData entityData in entityManagerData.EntityData)
            {
                if (entityData is EnemyData data)
                {
                    var enemy = level.Enemies.Values.FirstOrDefault(e => e.GetType().FullName == data.Type)
                        ?? throw new InvalidDataException($"Unknown saved enemy: {data.Type}");
                    Enemy en = (Enemy)Activator.CreateInstance(enemy.GetType(), data, enemy.Assets);
                    en.SetEnemyLevel(data.EntityLevel);
                    en.UpdateEntityWithData(data);
                    en.GuaranteedDrops.Add(GameManager.GameItemLoadingManager.LoadGameItemsFromItemBaseData(data.GuaranteedItems));
                    level.EntityManager.Add(en);
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
                        chest.LootAmountRange = new(data.LootAmountRange.Min, data.LootAmountRange.Max);
                        bool generated = data.ContentsGenerated ?? (data.ChestEmptied || data.ItemDatas.Count > 0);
                        chest.Items = generated ? GameManager.GameItemLoadingManager.LoadGameItemsFromItemData(data.ItemDatas) : null;
                    }
                }
            }
        }
    }
}
