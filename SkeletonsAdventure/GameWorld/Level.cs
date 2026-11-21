using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tilemaps;
using MonoGame.Extended.Tilemaps.Rendering;
using RpgLibrary.DataClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Content;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.EntityHelperClasses;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameEvents;
using SkeletonsAdventure.GameObjects;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.Quests;
using SkeletonsAdventure.ItemClasses.ItemManagement;

namespace SkeletonsAdventure.GameWorld
{
    internal class Level
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public Player Player { get; set; } = World.Player; 
        public Camera Camera { get; set; } = World.Camera; 
        public EntityManager EntityManager { get; private set; }
        public Tilemap TiledMap { get; private set; }
        public MinMaxPair EnemyLevels { get; private set; }
        public Vector2 PlayerStartPosition { get; set; } = new(80, 80);
        public Vector2 PlayerEndPosition { get; set; } = new(80, 80);//location of the exit so if the player comes back to the level this is where they will be placed
        public Vector2 PlayerRespawnPosition { get; set; } = new(80, 80);
        public ChestManager ChestManager { get; private set; }
        public TilemapObjectLayer EnterExitLayer { get; private set; } = null;
        public TilemapObjectLayer InteractableObjectLayerObjects { get; private set; } = null;
        public TilemapTileLayer InteractableObjectLayerTiles { get; private set; } = null;
        public TilemapObjectLayer TeleporterLayer { get; private set; } = null;
        public TilemapTileLayer ConditionalLayer { get; private set; } = null;
        public LevelExit LevelExit { get; set; } = null;
        public LevelExit LevelEntrance { get; set; } = null;
        internal InteractableObjectManager InteractableObjectManager { get; }
        internal QuestAreaTracker QuestAreas { get; }
        public DamagePopUpManager DamagePopUpManager { get; }
        private readonly SpriteFont _exitFont;
        public TeleporterManager TeleporterManager { get; private set; } = new(); // used to manage teleporters in the level
        public GameEventManager GameEventManager { get; } = new(); //used to manage timed game events
        public Dictionary<string, Enemy> Enemies { get; private set; } = [];

        private TilemapSpriteBatchRenderer _tiledMapRenderer;
        private TilemapTileLayer _mapCollisionLayer;
        private TilemapObjectLayer _mapSpawnerLayer;
        private TilemapTileLayer[] collisionLayers = [];
        internal EnemyNavigation Navigation { get; private set; }

        public List<Rectangle> EnterExitLayerObjectRectangles { get; private set; } = []; //TODO used to temporarily see where hitboxes are for exits

        public Level(Tilemap tiledMap, string name, Dictionary<string, Enemy> enemies, MinMaxPair enemyLevels,
            SpriteFont exitFont, SpriteFont damageFont, TiledAnimation leverAnimation)
        {
            _exitFont = exitFont ?? throw new ArgumentNullException(nameof(exitFont));
            DamagePopUpManager = new(damageFont);
            InteractableObjectManager = new(_exitFont);
            Enemies = enemies;
            CreateMap(tiledMap ?? throw new ArgumentNullException(nameof(tiledMap)), name);

            //Load the chests from the tiled map and add them to the chest manager
            ChestManager.Add(TiledHelperClasses.LoadChestsFromTiledMap(ChestManager.TilemapTileLayer, GameManager.ChestsClone));

            EnemyLevels = enemyLevels;

            CreateEntityManager();
            LoadInteractableObjects(leverAnimation);
            LoadTeleporters();
            QuestAreas = new(Name);
            if (TiledMap.Layers.GetLayer<TilemapObjectLayer>(MapCatalog.QuestAreaLayer) is { } areas)
                foreach (var obj in areas.Objects)
                    if (obj is TilemapRectangleObject && obj.Rotation == 0 && obj.Bounds.Width >= 1 && obj.Bounds.Height >= 1)
                        QuestAreas.Add(obj.Id, new Rectangle((int)obj.Position.X, (int)obj.Position.Y,
                            (int)obj.Bounds.Width, (int)obj.Bounds.Height));
        }

        private void CreateMap(Tilemap tiledMap, string name)
        {
            TiledMap = tiledMap;
            _tiledMapRenderer = new() { BlendState = BlendState.AlphaBlend, SamplerState = SamplerState.PointClamp };
            _tiledMapRenderer.LoadTilemap(TiledMap);
            _mapCollisionLayer = TiledMap.Layers.GetLayer<TilemapTileLayer>("CollisionLayer");
            _mapSpawnerLayer = tiledMap.Layers.GetLayer<TilemapObjectLayer>("SpawnerLayer");
            ChestManager = new(tiledMap.Layers.GetLayer<TilemapTileLayer>("ChestLayer"));
            EnterExitLayer = TiledMap.Layers.GetLayer<TilemapObjectLayer>("EnterExitLayer");
            InteractableObjectLayerObjects = TiledMap.Layers.GetLayer<TilemapObjectLayer>(MapCatalog.InteractableLayer);
            InteractableObjectLayerTiles = TiledMap.Layers.GetLayer<TilemapTileLayer>("InteractableObjectLayer");
            TeleporterLayer = TiledMap.Layers.GetLayer<TilemapObjectLayer>("TeleporterLayer");
            ConditionalLayer = TiledMap.Layers.GetLayer<TilemapTileLayer>("ConditionalLayer");

            Width = tiledMap.Width * tiledMap.TileWidth;
            Height = tiledMap.Height * tiledMap.TileHeight;
            TileWidth = tiledMap.TileWidth;
            TileHeight = tiledMap.TileHeight;
            Name = name;

            if(ConditionalLayer is not null)
                collisionLayers = [_mapCollisionLayer, ConditionalLayer];
            else
                collisionLayers = [_mapCollisionLayer];
            Navigation = new(Width, Height, TileWidth, TileHeight,
                (x, y) => CollisionDetection.IsTileBlocked(collisionLayers, x, y));
        }

        private void CreateEntityManager()
        {
            EntityManager = new();
            EntityManager.DroppedLootManager.Placement = new(Width, Height, TileWidth, TileHeight,
                (x, y) => CollisionDetection.IsTileBlocked(collisionLayers, x, y),
                () => (Player.Position, Player.Width, Player.Height),
                () => ConditionalLayer?.IsVisible == true ? 1 : 0);
            EntityManager.Add(Player);

            AddEnemies();
        }

        public void Draw(SpriteBatch spriteBatch)  
        {
            _tiledMapRenderer.Draw(spriteBatch, Camera.View);

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
                spriteBatch.DrawString(_exitFont, LevelEntrance.ExitText, LevelEntrance.ExitPosition, Color.White);
            if (LevelExit is not null && LevelExit.ExitTextVisible)
                spriteBatch.DrawString(_exitFont, LevelExit.ExitText, LevelExit.ExitPosition, Color.White);

            spriteBatch.End();
        }

        public void Update(GameTime gameTime)
        {
            GameEventManager.Update(gameTime);
            EntityManager.Update(gameTime, Navigation);

            CollisionDetection.CheckEntityBoundaryCollisions(EntityManager.Entities, collisionLayers,
                TileWidth, TileHeight, Width, Height, gameTime);
            EntityManager.ResolveAttackHits(gameTime);

            Camera.Update(Player.Position);

            _tiledMapRenderer.Update(gameTime);

            ChestManager.Update(gameTime);
            CheckIfPlayerNearChest();

            InteractableObjectManager.Update(gameTime, Player);
            TeleporterManager.Update(Player);
            QuestAreas.Update(Player);
            DamagePopUpManager.Update(gameTime);

            if (LevelExit != null)
                CheckIfPlayerIsNearExit(LevelExit, LevelExit.NextLevel.PlayerStartPosition);

            if (World.CurrentLevel == this && LevelEntrance != null)
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

        public LevelData ToData()
        {
            //Get the visibility of all of the layers in the tiled map
            List<(string, bool)> layerVisibility = [];

            foreach (TilemapLayer layer in TiledMap.Layers)
            {
                bool visible = layer.IsVisible;
                // Animations are transient; load their completed door/lever state.
                foreach (var gameEvent in GameEventManager.ActiveEvents)
                    if (gameEvent is ToggleLayerVisibility toggle && toggle.Layer == layer && !toggle.IsComplete)
                        visible = !visible;
                foreach (var delayed in GameEventManager.DelayedEvents)
                    if (delayed.StoredEvent is ToggleLayerVisibility toggle && toggle.Layer == layer && !toggle.IsComplete)
                        visible = !visible;
                layerVisibility.Add((layer.Name, visible));
            }

            //Create and return the level data
            return new()
            {
                MinMaxPair = EnemyLevels,
                EntityManagerData = EntityManager.ToData(),
                DroppedItemDatas = EntityManager.DroppedLootManager.GetDroppedItemData(),
                Chests = ChestManager.GetChestDatas(),
                LayerVisibility = layerVisibility,
                InteractableObjectManagerData = InteractableObjectManager.ToData(),
            };
        }

        private void LoadInteractableObjects(TiledAnimation leverAnimation)
        {
            if (InteractableObjectLayerObjects is not null)
            {
                foreach (TilemapObject obj in InteractableObjectLayerObjects.Objects)
                {
                    if (obj.Properties.TryGetValue("TypeOfObject", out TilemapPropertyValue value))
                    {
                        if (value.AsString() == "Quest")
                        {
                            if (obj.Properties.TryGetValue("Quests", out TilemapPropertyValue quests))
                            {
                                QuestNode questNode = new(obj, _exitFont);
                                string[] Quests = quests.AsString().Split(',', StringSplitOptions.TrimEntries);

                                foreach (string questName in Quests)
                                {
                                    if (GameManager.QuestsClone.TryGetValue(questName, out Quest quest))
                                        questNode.Quests.Add(quest.Clone()); //Clone the quest to prevent modifying the original quest
                                }

                                Register(questNode);
                            }
                        }
                        else if (value.AsString() == "Resource")
                        {
                            Register(new ResourceNode(obj, _exitFont)); //TODO resource logic still needs added
                        }
                        else if (value.AsString() == "Lever")
                        {
                            Lever lever = new(obj, _exitFont, leverAnimation);
                            SetLeverProperties(obj, lever);

                            Register(lever);
                        }
                        else
                        {
                            Register(new InteractableObject(obj, _exitFont));
                        }
                    }
                }
            }

            void Register(InteractableObject obj)
            {
                obj.MapName = Name;
                InteractableObjectManager.Add(obj);
            }
        }

        private void SetLeverProperties(TilemapObject obj, Lever lever)
        {
            if (obj.Properties.TryGetValue("LeverPurpose", out TilemapPropertyValue purpose))
                lever.LeverPurpose = purpose.AsString();

            if (obj.Properties.TryGetValue("GameEventName", out TilemapPropertyValue eventName))
                lever.GameEventName = eventName.AsString();

            if (obj.Properties.TryGetValue("LevelRequired", out TilemapPropertyValue levelRequired))
            {
                if (TiledHelperClasses.TryReadInt(levelRequired, out int lvl))
                {
                    lever.LevelRequirements = new()
                    {
                        Level = lvl
                    };
                }
            }

            //get all of the tiles from the layer that the interactable objects area contains
            List<Point> tiledMapTiles = TiledHelperClasses.GetTiledMapTiles(InteractableObjectLayerTiles, lever.Rectangle);

            if (tiledMapTiles.Count == 1)
            {
                //position where the lever should be displayed.
                //(not the location of the interactable object which could be offset from the actual lever)
                lever.LeverPosition = new(tiledMapTiles[0].X * TileWidth, tiledMapTiles[0].Y * TileHeight);
            }
            else
            {
                //TODO some logic to pick which tile to map to if there is more than 1 tile in the area
                //(like for if something needs more than 1 tile to be drawn like the fire animation)
            }

            //TODO this is just temporary logic to add a kill requirement to the lever in level 1 for testing purposes,
            //eventually there will be a better way to add requirements to levers
            //that doesn't involve hardcoding specific levers in specific levels
            if (Name == "Level1")
            {
                if(lever.Position == new Vector2(7152,4608))
                {
                    var enemy = GameManager.GetEnemyByName(DataAssetCatalog.EliteSkeletonName);

                    lever.KillRequirements = new();
                    lever.KillRequirements.AddKillRequirement(enemy);
                }
            }
        }

        private void LoadTeleporters()
        {
            if (TeleporterLayer is null)
                return; //No teleporters in this level

            string name; 
            Teleporter teleporter;
            foreach (TilemapObject obj in TeleporterLayer.Objects)
            {
                name = obj.Name ?? string.Empty;
                teleporter = new(_exitFont)
                {
                    Name = name,
                    Position = obj.Position,
                    Width = (int)obj.Bounds.Width,
                    Height = (int)obj.Bounds.Height,
                };
                teleporter.Info.Position = teleporter.Position;

                if (obj.Properties.TryGetValue("ToName", out TilemapPropertyValue value))
                {
                    teleporter.DestinationName = value.AsString();
                }

                TeleporterManager.AddTeleporter(teleporter);
            }

            //TODO add the to destinations to the teleporters
            TeleporterManager.SetDestinationForAllTeleporters();
        }

        private void AddEnemies()
        {
            foreach (Enemy enemy in Enemies.Values)
            {
                List<Enemy> enemiesFromMap = TiledHelperClasses.LoadEnemyFromTiledMap(enemy, _mapSpawnerLayer, EnemyLevels);
                if (enemiesFromMap is null)
                    continue;

                foreach (Enemy e in enemiesFromMap)
                    EntityManager.Add(e);
            }
        }

        private void CheckIfPlayerNearChest()
        {
            Chest chestToOpen = null;
            foreach (Chest chest in ChestManager.Chests)
            {
                if (chest.PlayerIntersects(Player.Rectangle))
                {
                    if(chest.LootCount > 0) //Cannot open empty chests
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
