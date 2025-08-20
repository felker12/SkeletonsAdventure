using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.EntitySpawners;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.Quests;

namespace SkeletonsAdventure.GameWorld
{
    internal class Level
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; } = string.Empty;
        public Player Player { get; set; } = World.Player; 
        public Camera Camera { get; set; } = World.Camera; 
        public EntityManager EntityManager { get; set; }
        public TiledMap TiledMap { get; private set; }
        public MinMaxPair EnemyLevels { get; set; }
        private GraphicsDevice GraphicsDevice { get; }
        public Vector2 PlayerStartPosition { get; set; } = new(80, 80);
        public Vector2 PlayerEndPosition { get; set; } = new(80, 80);//location of the exit so if the player comes back to the level this is where they will be placed
        public Vector2 PlayerRespawnPosition { get; set; } = new(80, 80);
        public ChestManager ChestManager { get; set; }
        public TiledMapObjectLayer EnterExitLayer { get; set; } = null;
        public TiledMapObjectLayer InteractableObjectLayer { get; set; } = null;
        public TiledMapObjectLayer TeleporterLayer { get; set; } = null;
        public LevelExit LevelExit { get; set; } = null;
        public LevelExit LevelEntrance { get; set; } = null;
        internal InteractableObjectManager InteractableObjectManager { get; set; } = new();
        public DamagePopUpManager DamagePopUpManager { get; } = new(); //used to show damage popups when an entity is hit by an attack
        public TeleporterManager TeleporterManager { get; set; } = new(); // used to manage teleporters in the level

        private TiledMapRenderer _tiledMapRenderer;
        private TiledMapTileLayer _mapCollisionLayer, _mapSpawnerLayer;
        private readonly Dictionary<string, Enemy> Enemies = [];

        public List<Rectangle> EnterExitLayerObjectRectangles { get; set; } = []; //TODO used to temporarily see where hitboxes are for exits

        public Level(GraphicsDevice graphics, TiledMap tiledMap, Dictionary<string, Enemy> enemies, MinMaxPair enemyLevels)
        {
            GraphicsDevice = graphics;
            Enemies = enemies;
            CreateMap(tiledMap);

            if(ChestManager.TiledMapTileLayer != null)
                ChestManager.Chests = ChestManager.GetChestsFromTiledMapTileLayer(GameManager.ChestsClone["BasicChest"]);

            EntityManager = new();
            EnemyLevels = enemyLevels;
            EntityManager.Add(Player);

            if(_mapSpawnerLayer != null)
            AddEnemys();

            LoadInteractableObjects();
            LoadTeleporters();
        }

        private void CreateMap(TiledMap tiledMap)
        {
            TiledMap = tiledMap;
            _tiledMapRenderer = new(GraphicsDevice);
            _tiledMapRenderer.LoadMap(TiledMap);
            _mapCollisionLayer = TiledMap.GetLayer<TiledMapTileLayer>("CollisionLayer");
            _mapSpawnerLayer = tiledMap.GetLayer<TiledMapTileLayer>("SpawnerLayer");
            ChestManager = new(tiledMap.GetLayer<TiledMapTileLayer>("ChestLayer"));
            EnterExitLayer = TiledMap.GetLayer<TiledMapObjectLayer>("EnterExitLayer");
            InteractableObjectLayer = TiledMap.GetLayer<TiledMapObjectLayer>("InteractableObjectLayerObjects");
            TeleporterLayer = TiledMap.GetLayer<TiledMapObjectLayer>("TeleporterLayer");

            Width = tiledMap.WidthInPixels;
            Height = tiledMap.HeightInPixels;
            Name = tiledMap.Name[11..]; //trim "TiledFiles/" from the tiledmap name to use as the level name
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp; //prevents wierd yellow lines between tiles
            _tiledMapRenderer.Draw(Camera.Transformation);

            spriteBatch.Begin(
                       SpriteSortMode.Immediate,
                       BlendState.AlphaBlend,
                       SamplerState.PointClamp,
                       null,
                       null,
                       null,
                       Camera.Transformation);


            //foreach (var layer in TiledMap.Layers) //Draw entities on the correct layer
            //{
            //    _tiledMapRenderer.Draw(layer, Camera.Transformation);

            //    if (layer.Name == "Floor")
            //    {
            //        EntityManager.Draw(spriteBatch);
            //        DamagePopUpManager.Draw(spriteBatch);
            //    }
            //}

            ChestManager.Draw(spriteBatch);
            InteractableObjectManager.Draw(spriteBatch);
            TeleporterManager.Draw(spriteBatch);

            foreach(Rectangle rec in EnterExitLayerObjectRectangles) //TODO delete this 
                spriteBatch.DrawRectangle(rec, Color.White, 1, 0); //used to see where the hitboxes are for the exits

            if (LevelEntrance is not null && LevelEntrance.ExitTextVisible)
                spriteBatch.DrawString(GameManager.Arial12, LevelEntrance.ExitText, LevelEntrance.ExitPosition, Color.White);
            if (LevelExit is not null && LevelExit.ExitTextVisible)
                spriteBatch.DrawString(GameManager.Arial12, LevelExit.ExitText, LevelExit.ExitPosition, Color.White);

            EntityManager.Draw(spriteBatch); //TODO
            DamagePopUpManager.Draw(spriteBatch);

            spriteBatch.End();
        }

        public void Update(GameTime gameTime, GameTime totalTimeInWorld) 
        {
            EntityManager.Update(gameTime, totalTimeInWorld);
            EntityManager.CheckEntityBoundaryCollisions(TiledMap, _mapCollisionLayer);

            Camera.Update(Player.Position);

            _tiledMapRenderer.Update(gameTime);

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
            EntityManager.RemoveAll();

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
            foreach(Enemy enemy in Enemies.Values)
            {
                foreach(EntityData entityData in entityManagerData.EntityData)
                {
                    if(entityData is EnemyData data)
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

        private void LoadInteractableObjects()
        {
            if (InteractableObjectLayer != null) //TODO 
            {
                foreach (TiledMapObject obj in InteractableObjectLayer.Objects)
                {
                    if (obj.Properties.TryGetValue("TypeOfObject", out TiledMapPropertyValue value))
                    {
                        if (value == "Quest")
                        {
                            if (obj.Properties.TryGetValue("Quests", out TiledMapPropertyValue quests))
                            {
                                QuestNode questNode = new(obj);
                                string[] Quests = quests.ToString().Split(',', StringSplitOptions.TrimEntries);

                                foreach (string questName in Quests)
                                {
                                    if (GameManager.QuestsClone.TryGetValue(questName, out Quest quest))
                                        questNode.Quests.Add(quest.Clone()); //Clone the quest to prevent modifying the original quest
                                }

                                InteractableObjectManager.Add(questNode);
                            }
                        }
                        else if (value == "Resource")
                        {
                            InteractableObjectManager.Add(new ResourceNode(obj)); //TODO resource logic still needs added
                        }
                        else
                        {
                            InteractableObjectManager.Add(new InteractableObject(obj));
                        }
                    }
                }
            }
        }

        private void LoadTeleporters()
        {
            if (TeleporterLayer is null)
                return; //No teleporters in this level

            string name; 
            Teleporter teleporter;
            foreach (TiledMapObject obj in TeleporterLayer.Objects)
            {
                name = obj.Name ?? string.Empty;
                teleporter = new(name)
                {
                    Position = obj.Position,
                    Width = (int)obj.Size.Width,
                    Height = (int)obj.Size.Height,
                };
                teleporter.Info.Position = teleporter.Position;

                if (obj.Properties.TryGetValue("ToName", out TiledMapPropertyValue value))
                {
                    teleporter.DestinationName = value;
                }

                TeleporterManager.AddTeleporter(teleporter);
            }

            //TODO add the to destinations to the teleporters
            TeleporterManager.SetDestinationForAllTeleporters();
        }

        private void AddEnemys()
        {
            Spawner spawner = new(_mapSpawnerLayer);

            foreach (Enemy enemy in Enemies.Values)
            {
                foreach (Enemy spawnerEnemy in spawner.CreateEnemiesForSpawners(enemy))
                {
                    spawnerEnemy.SetEnemyLevel(EnemyLevels);
                    EntityManager.Add(spawnerEnemy);
                }
            }
        }

        private void CheckIfPlayerNearChest()
        {
            Chest chestToOpen = null;
            foreach (Chest chest in ChestManager.Chests)
            {
                if (chest.PlayerIntersects(Player.Rectangle))
                {
                    if(chest.Loot.Count > 0) //Cannot open empty chests
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
