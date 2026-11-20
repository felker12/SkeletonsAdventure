using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.States;

namespace SkeletonsAdventure
{
    internal class Game1 : Game
    {
        public GraphicsDeviceManager Graphics { get; set; }
        internal GameContentManager GameContent { get; }
        public SpriteBatch SpriteBatch { get; private set; }
        public GameScreen GameScreen { get; set; }
        public ExitScreen ExitScreen { get; internal set; }
        internal bool PreserveSaveBackup { get; set; }
        public StateManager StateManager { get; private set; } = new();
        public static int ScreenWidth { get; set; } = 1280;
        public static int ScreenHeight { get; set; } = 720;
        //public static GameTime GameTime { get; private set; }
        //public static float DeltaTime => (float)GameTime.ElapsedGameTime.TotalSeconds;
        public static int BaseSpeedMultiplier { get; set; } = 50;
        public static PlayerIndex PlayerIndexInControl { get; set; } = PlayerIndex.One;

        #if IFDEV
        internal static bool IfDev => true;
        #else
        internal static bool IfDev => false;
        #endif

        public Game1()
        {
            GameContent = new(Services);
            Content.Dispose();
            Content = GameContent;
            Window.Title = "The Adventures of the Skeleton";

            Graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = ScreenWidth,
                PreferredBackBufferHeight = ScreenHeight,
            };
            Graphics.ApplyChanges();

            //TODO delete this: it uncaps the FPS
            //IsFixedTimeStep = false;
            //Graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
            IsMouseVisible = true;

            var gameManager = new GameManager(Content, GraphicsDevice);
            Components.Add(new InputHandler(this));
            GameScreen = new(this, gameManager);
            ExitScreen = new(this, gameManager);
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            StateManager.ChangeState(new StartScreen(this, gameManager));
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            //GameTime = gameTime;

            if (InputHandler.KeyReleased(Keys.Escape) ||
            InputHandler.ButtonDown(Buttons.Back, PlayerIndex.One))
            {
                if (StateManager.CurrentState == GameScreen)
                    StateManager.ChangeState(ExitScreen);
                else if (StateManager.CurrentState == ExitScreen) 
                    StateManager.ChangeState(GameScreen);
                else 
                    Exit();
            }

            StateManager.HandleInput(PlayerIndexInControl);
            StateManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            StateManager.Draw(SpriteBatch);
        }
    }
}
