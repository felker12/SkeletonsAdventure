using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameEvents;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.Quests;

namespace SkeletonsAdventure.GameWorld
{
    internal class Level
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
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
        public TiledMapObjectLayer InteractableObjectLayerObjects { get; set; } = null;
        public TiledMapTileLayer InteractableObjectLayerTiles { get; set; } = null;
        public TiledMapObjectLayer TeleporterLayer { get; set; } = null;
        public TiledMapTileLayer ConditionalLayer { get; set; } = null;
        public LevelExit LevelExit { get; set; } = null;
        public LevelExit LevelEntrance { get; set; } = null;
        internal InteractableObjectManager InteractableObjectManager { get; set; } = new();
        public DamagePopUpManager DamagePopUpManager { get; } = new(); //used to show damage popups when an entity is hit by an attack
        public TeleporterManager TeleporterManager { get; set; } = new(); // used to manage teleporters in the level
        public GameEventManager GameEventManager { get; } = new(); //used to manage timed game events

        private TiledMapRenderer _tiledMapRenderer;
        private TiledMapTileLayer _mapCollisionLayer;
        private TiledMapObjectLayer _mapSpawnerLayer;
        private readonly Dictionary<string, Enemy> Enemies = [];
        TiledMapTileLayer[] collisionLayers = [];

        public List<Rectangle> EnterExitLayerObjectRectangles { get; set; } = []; //TODO used to temporarily see where hitboxes are for exits

        public Level(GraphicsDevice graphics, TiledMap tiledMap, Dictionary<string, Enemy> enemies, MinMaxPair enemyLevels)
        {
            GraphicsDevice = graphics;
            Enemies = enemies;
            CreateMap(tiledMap);

            LoadChestsFromTiledMap();
            CreateEntityManager(enemyLevels);
            LoadInteractableObjects();
            LoadTeleporters();
        }

        private void CreateMap(TiledMap tiledMap)
        {
            TiledMap = tiledMap;
            _tiledMapRenderer = new(GraphicsDevice);
            _tiledMapRenderer.LoadMap(TiledMap);
            _mapCollisionLayer = TiledMap.GetLayer<TiledMapTileLayer>("CollisionLayer");
            _mapSpawnerLayer = tiledMap.GetLayer<TiledMapObjectLayer>("SpawnerLayer");
            ChestManager = new(tiledMap.GetLayer<TiledMapTileLayer>("ChestLayer"));
            EnterExitLayer = TiledMap.GetLayer<TiledMapObjectLayer>("EnterExitLayer");
            InteractableObjectLayerObjects = TiledMap.GetLayer<TiledMapObjectLayer>("InteractableObjectLayerObjects");
            InteractableObjectLayerTiles = TiledMap.GetLayer<TiledMapTileLayer>("InteractableObjectLayer");
            TeleporterLayer = TiledMap.GetLayer<TiledMapObjectLayer>("TeleporterLayer");
            ConditionalLayer = TiledMap.GetLayer<TiledMapTileLayer>("ConditionalLayer");

            Width = tiledMap.WidthInPixels;
            Height = tiledMap.HeightInPixels;
            TileWidth = tiledMap.TileWidth;
            TileHeight = tiledMap.TileHeight;
            Name = tiledMap.Name[11..]; //trim "TiledFiles/" from the tiledmap name to use as the level name

            if(ConditionalLayer is not null)
                collisionLayers = [_mapCollisionLayer, ConditionalLayer];
            else
                collisionLayers = [_mapCollisionLayer];
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

        public void Draw(SpriteBatch spriteBatch)  {
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

            EntityManager.Draw(spriteBatch);
            ChestManager.Draw(spriteBatch);
            DamagePopUpManager.Draw(spriteBatch);

            InteractableObjectManager.Draw(spriteBatch);
            TeleporterManager.Draw(spriteBatch);

            GameEventManager.Draw(spriteBatch);

            foreach (Rectangle rec in EnterExitLayerObjectRectangles) //TODO delete this 
                spriteBatch.DrawRectangle(rec, Color.White, 1, 0); //used to see where the hitboxes are for the exits

            if (LevelEntrance is not null && LevelEntrance.ExitTextVisible)
                spriteBatch.DrawString(GameManager.Arial12, LevelEntrance.ExitText, LevelEntrance.ExitPosition, Color.White);
            if (LevelExit is not null && LevelExit.ExitTextVisible)
                spriteBatch.DrawString(GameManager.Arial12, LevelExit.ExitText, LevelExit.ExitPosition, Color.White);

            spriteBatch.End();
        }

        public void Update(GameTime gameTime, GameTime totalTimeInWorld) 
        {
            GameEventManager.Update(gameTime);
            EntityManager.Update(gameTime, totalTimeInWorld);

            //EntityManager.CheckEntityBoundaryCollisions(_mapCollisionLayer, TileWidth, TileHeight);

            EntityManager.CheckEntityBoundaryCollisions(collisionLayers, TileWidth, TileHeight);


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

        public void AddGameEvent(GameEvent gameEvent)
        {
            GameEventManager.AddEvent(gameEvent);
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

        private void LoadInteractableObjects()//TODO 
        {
            if (InteractableObjectLayerObjects is not null)
            {
                foreach (TiledMapObject obj in InteractableObjectLayerObjects.Objects)
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
                        else if (value == "Lever")
                        {
                            Lever lever = new(obj);

                            if (obj.Properties.TryGetValue("LeverPurpose", out TiledMapPropertyValue purpose))
                                lever.LeverPurpose = purpose;

                            if (obj.Properties.TryGetValue("GameEventName", out TiledMapPropertyValue eventName))
                                lever.GameEventName = eventName;

                            InteractableObjectManager.Add(lever);

                            //get all of the tiles from the layer that the interactable objects area contains
                            List<TiledMapTile> tiledMapTiles = GetTiledMapTiles(InteractableObjectLayerTiles, lever.Rectangle);

                            if(tiledMapTiles.Count == 1)
                            {
                                //position where the lever should be displayed.
                                //(not the location of the interactable object which could be offset from the actual lever)
                                lever.LeverPosition = new(tiledMapTiles[0].X * 16, tiledMapTiles[0].Y * 16);
                            }
                            else
                            {
                                //TODO some logic to pick which tile to map to if there is more than 1 tile in the area
                                //(like for if something needs more than 1 tile to be drawn like the fire animation)
                            }
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
            if (_mapSpawnerLayer is null)
                return null;

            List<Enemy> enemies = [];

            foreach (TiledMapObject obj in GameManager.ObjectLocations(Enemy.Name, _mapSpawnerLayer.Objects))
            {
                Enemy enemy = Enemy.Clone();
                enemy.Position = obj.Position;
                enemy.RespawnPosition = enemy.Position;

                int levelFromMap = GetLevelFromTiledMap(obj);
                //Clamp the level to be within the max range
                if (levelFromMap > EnemyLevels.Max)
                    levelFromMap = EnemyLevels.Max;

                enemy.SetEnemyLevel(levelFromMap);

                //If the level was not set from the map, set it to the default for the level
                if (enemy.Level == 0) 
                    enemy.SetEnemyLevel(EnemyLevels);

                enemies.Add(enemy);
            }

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

        public static List<TiledMapTile> GetTiledMapTiles(TiledMapTileLayer layer, Rectangle rec)
        {
            if (layer == null)
                return null;

            List<TiledMapTile> tiles = [];

            int startX = Math.Max(rec.Left / layer.TileWidth, 0);
            int endX = Math.Min((rec.Right - 1) / layer.TileWidth, layer.Width - 1);
            int startY = Math.Max(rec.Top / layer.TileHeight, 0);
            int endY = Math.Min((rec.Bottom - 1) / layer.TileHeight, layer.Height - 1);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    if (layer.TryGetTile((ushort)x, (ushort)y, out TiledMapTile? tile) && tile.Value.GlobalIdentifier != 0)
                        tiles.Add(tile.Value);
                }
            }

            return tiles;
        }
    }
}
