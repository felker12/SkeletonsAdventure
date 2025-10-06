using Assimp;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.Quests;
using SkeletonsAdventure.TileEngine;

namespace SkeletonsAdventure.GameWorld
{
    internal class Level2
    {
        TileMap TileMap { get; set; }

        public string Name { get; set; } = string.Empty;
        public Player Player { get; set; } = World.Player;
        public Camera Camera { get; set; } = World.Camera;
        public EntityManager EntityManager { get; set; }
        public MinMaxPair EnemyLevels { get; set; }
        private GraphicsDevice GraphicsDevice { get; }
        public Vector2 PlayerStartPosition { get; set; } = new(80, 80);
        public Vector2 PlayerEndPosition { get; set; } = new(80, 80);//location of the exit so if the player comes back to the level this is where they will be placed
        public Vector2 PlayerRespawnPosition { get; set; } = new(80, 80);
        public ChestManager ChestManager { get; set; }
        public LevelExit LevelExit { get; set; } = null;
        public LevelExit LevelEntrance { get; set; } = null; internal InteractableObjectManager InteractableObjectManager { get; set; } = new();
        public DamagePopUpManager DamagePopUpManager { get; } = new(); //used to show damage popups when an entity is hit by an attack
        public TeleporterManager TeleporterManager { get; set; } = new(); // used to manage teleporters in the level

        private readonly Dictionary<string, Enemy> Enemies = [];

        public Level2(GraphicsDevice graphics, TileMap tileMap , Dictionary<string, Enemy> enemies, MinMaxPair enemyLevels)
        {
            GraphicsDevice = graphics;
            Enemies = enemies;
            CreateMap(tileMap);

            LoadChestsFromTiledMap();
            CreateEntityManager(enemyLevels);
            LoadInteractableObjects();
            LoadTeleporters();
        }

        private void CreateMap(TileMap tileMap)
        {
            //Name = tileMap.Name[11..]; //trim "TiledFiles/" from the tiledmap name to use as the level name
            Name = tileMap.Name;






        }

        private void LoadChestsFromTiledMap()
        {
            if (ChestManager.TiledMapTileLayer is null)
                return;

            ChestManager.Clear();

            foreach (Chest chest in GameManager.ChestsClone.Values)
                ChestManager.Add(ChestManager.GetChestsFromTiledMapTileLayer(chest));
        }

        private void CreateEntityManager(MinMaxPair enemyLevels)
        {
            EntityManager = new();
            EnemyLevels = enemyLevels;
            EntityManager.Add(Player);

            AddEnemys();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp; //prevents wierd yellow lines between tiles
            //_tiledMapRenderer.Draw(Camera.Transformation);

            spriteBatch.Begin(
                       SpriteSortMode.Immediate,
                       BlendState.AlphaBlend,
                       SamplerState.PointClamp,
                       null,
                       null,
                       null,
                       Camera.Transformation);

            EntityManager.Draw(spriteBatch);
            ChestManager.Draw(spriteBatch);
            DamagePopUpManager.Draw(spriteBatch);

            InteractableObjectManager.Draw(spriteBatch);
            TeleporterManager.Draw(spriteBatch);


            if (LevelEntrance is not null && LevelEntrance.ExitTextVisible)
                spriteBatch.DrawString(GameManager.Arial12, LevelEntrance.ExitText, LevelEntrance.ExitPosition, Color.White);
            if (LevelExit is not null && LevelExit.ExitTextVisible)
                spriteBatch.DrawString(GameManager.Arial12, LevelExit.ExitText, LevelExit.ExitPosition, Color.White);

            spriteBatch.End();
        }

        public void Update(GameTime gameTime, GameTime totalTimeInWorld)
        {
            EntityManager.Update(gameTime, totalTimeInWorld);

            //EntityManager.CheckEntityBoundaryCollisions(_mapCollisionLayer, TileWidth, TileHeight);

            //EntityManager.CheckEntityBoundaryCollisions(collisionLayers, TileWidth, TileHeight);


            Camera.Update(Player.Position);


            ChestManager.Update(gameTime);
            CheckIfPlayerNearChest();

            InteractableObjectManager.Update(gameTime, Player);
            TeleporterManager.Update(Player);
            DamagePopUpManager.Update(gameTime);

            if (LevelExit != null)
                CheckIfPlayerIsNearExit(LevelExit, LevelExit.NextLevel.PlayerStartPosition);

            if (LevelEntrance != null)
                CheckIfPlayerIsNearExit(LevelEntrance, LevelEntrance.NextLevel.PlayerEndPosition);
        }

        public void HandleInput(PlayerIndex playerIndex)
        {
            ChestManager.HandleInput(playerIndex);
        }

        public void LoadLevelDataFromLevelData(LevelData levelData)
        {
            EntityManager.Clear();

            EntityManager.Add(Player);
            EntityManager.DroppedLootManager.Items = GameManager.LoadGameItemsFromItemData(levelData.DroppedItemDatas);
            LoadEnemies(levelData.EntityManagerData);

            ChestManager.UpdateFromSave(levelData.Chests);
        }

        public LevelData GetLevelData()
        {
            return new()
            {
                MinMaxPair = EnemyLevels,
                EntityManagerData = EntityManager.GetEnemyData(),
                DroppedItemDatas = EntityManager.DroppedLootManager.GetDroppedItemData(),
                Chests = ChestManager.GetChestDatas()
            };
        }

        private void LoadEnemies(EntityManagerData entityManagerData)
        {
            foreach (Enemy enemy in Enemies.Values)
            {
                foreach (EntityData entityData in entityManagerData.EntityData)
                {
                    if (entityData is EnemyData data)
                    {
                        if (enemy.GetType().FullName == data.Type)
                        {
                            Enemy en = (Enemy)Activator.CreateInstance(enemy.GetType(), data);
                            en.SetEnemyLevel(data.EntityLevel);
                            en.GuaranteedDrops.Add(GameManager.LoadGameItemsFromItemData(data.GuaranteedItems));

                            EntityManager.Add(en);
                        }
                    }
                }
            }
        }

        private void LoadInteractableObjects()//TODO 
        {
            //TODO
        }

        private void LoadTeleporters()
        {
            //TODO

            //TODO add the to destinations to the teleporters
            TeleporterManager.SetDestinationForAllTeleporters();
        }

        private void AddEnemys()
        {
            foreach (Enemy enemy in Enemies.Values)
            {
                List<Enemy> enemiesFromMap = LoadEnemyFromTiledMap(enemy);
                if (enemiesFromMap is null)
                    continue;

                foreach (Enemy e in enemiesFromMap)
                    EntityManager.Add(e);
            }
        }

        private List<Enemy> LoadEnemyFromTiledMap(Enemy Enemy)
        {
            List<Enemy> enemies = [];

            //TODO


            return enemies;
        }

        private static int GetLevelFromTiledMap(TiledMapObject obj)
        {
            obj.Properties.TryGetValue("level", out string level);
            if (level is null)
                obj.Properties.TryGetValue("lvl", out level);

            if (level is not null)
            {
                if (int.TryParse(level, out int lvl))//parse the level to an int to be used for the enemy level
                    return lvl;
            }

            return 0; //level not found or could not be parsed
        }

        private void CheckIfPlayerNearChest()
        {
            Chest chestToOpen = null;
            foreach (Chest chest in ChestManager.Chests)
            {
                if (chest.PlayerIntersects(Player.Rectangle))
                {
                    if (chest.Loot.Count > 0) //Cannot open empty chests
                    {
                        //input handler is here instead of in the chest class so that multipe chests can be opened at once
                        if (InputHandler.KeyReleased(Keys.R) ||
                            InputHandler.ButtonDown(Buttons.A, PlayerIndex.One))
                        {
                            chestToOpen = chest;
                        }
                    }
                }
            }
            chestToOpen?.ChestOpened(); //if not null open the chest
        }

        public void CheckIfPlayerIsNearExit(LevelExit exit, Vector2 targetPosition)
        {
            if (exit.ExitArea.Intersects(Player.Rectangle))
            {
                exit.ExitTextVisible = true;

                if (InputHandler.KeyReleased(Keys.R)
                    || InputHandler.ButtonDown(Buttons.A, PlayerIndex.One))
                {
                    World.SetCurrentLevel(exit.NextLevel, targetPosition);
                }
            }
            else
                exit.ExitTextVisible = false;
        }
    }
}
