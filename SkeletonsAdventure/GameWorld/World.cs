using Microsoft.Xna.Framework.Input;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameEvents;
using SkeletonsAdventure.ItemClasses;

namespace SkeletonsAdventure.GameWorld
{
    internal class World
    {
        public static Dictionary<string, Level> Levels { get; private set; }
        public static Level CurrentLevel { get; set; }
        public static Player Player { get; set; }
        public static Camera Camera { get; set; } = new(Game1.ScreenWidth, Game1.ScreenHeight);
        public static GameTime TotalTimeInWorld { get; set; } = new();
        public static List<string> MessagesToAdd { get; private set; } = [];

        public World(ContentManager content, GraphicsDevice graphics)
        {
            Player = new();
            //Clear the levels dictionary because the levels are static and will persist between game instances
            Levels = [];

            LevelCreator levelCreator = new(content, graphics);
            Levels = levelCreator.Levels;

            //TODO
            //SetCurrentLevel(Levels["Level0"], Levels["Level0"].PlayerStartPosition);
            //SetCurrentLevel(Levels["TestLevel"], Levels["TestLevel"].PlayerStartPosition);
            //SetCurrentLevel(Levels["Dungeon\Dungeon"], new(100,100));
            //SetCurrentLevel(Levels["Catacombs"], new(100, 100));
            //SetCurrentLevel(Levels["Catacombs1"], new(100, 100));
            SetCurrentLevel(Levels["Level0"]);
        }

        public static void Update(GameTime gameTime)
        {
            TotalTimeInWorld.TotalGameTime += gameTime.ElapsedGameTime;

            CurrentLevel.Update(gameTime, TotalTimeInWorld); 

            //TODO delete this after testing
            if (InputHandler.KeyReleased(Keys.NumPad0))
            {
                SetCurrentLevel(Levels["Level0"]);
            }
            if (InputHandler.KeyReleased(Keys.NumPad1))
            {
                SetCurrentLevel(Levels["Level1"]);
            }
            if (InputHandler.KeyReleased(Keys.NumPad2))
            {
            }
            if (InputHandler.KeyReleased(Keys.NumPad3))
            {
            }
            if (InputHandler.KeyReleased(Keys.NumPad4))
            {
            }
            if (InputHandler.KeyReleased(Keys.NumPad5))
            {
            }
            if (InputHandler.KeyReleased(Keys.NumPad6))
            {
            }
            if (InputHandler.KeyReleased(Keys.NumPad7))
            {
            }
            if (InputHandler.KeyReleased(Keys.NumPad8))
            {
                SetCurrentLevel(Levels["Level0_Test"]);
            }
            if (InputHandler.KeyReleased(Keys.NumPad9))
            {
                SetCurrentLevel(Levels["TestLevel"], Levels["TestLevel"].PlayerStartPosition);
            }
            //=======================================================================
        }

        public static void HandleInput(PlayerIndex playerIndex)
        {
            CurrentLevel.HandleInput(playerIndex);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            CurrentLevel.Draw(spriteBatch);
        }

        public static void AddGameEventToCurrentLevel(GameEvent gameEvent)
        {
            CurrentLevel.GameEventManager.AddEvent(gameEvent);
        }

        public static WorldData ToData()
        {
            Dictionary<string, LevelData> levels = [];
            string name = string.Empty;

            foreach (var level in Levels)
            {
                levels.Add(level.Key, level.Value.ToData());

                if (level.Value == CurrentLevel)
                    name = level.Key;
            }

            return new()
            {
                TotalTimeInWorld = TotalTimeInWorld.TotalGameTime,
                PlayerData = Player.GetPlayerData(),
                Levels = levels,
                CurrentLevel = name
            };
        }

        public static void SetCurrentLevel(Level level, Vector2 playerPosition = new())
        {
            Player.Position = playerPosition;
            Player.RespawnPosition = level.PlayerRespawnPosition;

            CurrentLevel = level;
            Camera.SetBounds(CurrentLevel.TiledMap.WidthInPixels, CurrentLevel.TiledMap.HeightInPixels);
            CurrentLevel.Player = Player;
            CurrentLevel.Camera = Camera;
            CurrentLevel.EntityManager.Player = Player;
        }

        public static void SetCurrentLevel(Level level)
        {
            SetCurrentLevel(level, level.PlayerStartPosition);
        }

        public static void AddMessage(string message)
        {
            MessagesToAdd.Add(message);
        }

        public static void FillPlayerBackback() //TODO this is for testing
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (GameItem item in GameManager.ItemsClone.Values)
                {
                    Player.Backpack.Add(item);
                }
            }

            GameItem Coins = GameManager.ItemsClone["Coins"];
            Coins.SetQuantity(20);

            Player.Backpack.Add(Coins);

            Player.EquippedItems.TryEquipItem(Player.Backpack.Items[0]);
            Player.EquippedItems.TryEquipItem(Player.Backpack.Items[3]);
        }
    }
}
