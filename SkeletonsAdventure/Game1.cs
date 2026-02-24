using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.States;

namespace SkeletonsAdventure
{
    internal class Game1 : Game
    {
        public static GraphicsDeviceManager Graphics { get; set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public GameScreen GameScreen { get; set; }
        public ExitScreen ExitScreen { get; private set; }
        public StateManager StateManager { get; private set; } = new();
        public static int ScreenWidth { get; set; } = 1280;
        public static int ScreenHeight { get; set; } = 720;
        public static GameManager GameManager { get; set; }
        public static GameTime GameTime { get; private set; }
        public static float DeltaTime => (float)GameTime.ElapsedGameTime.TotalSeconds;
        public static int BaseSpeedMultiplier { get; set; } = 50;
        public static PlayerIndex PlayerIndexInControl { get; set; } = PlayerIndex.One;

        public Game1()
        {
            Content.RootDirectory = "Content";

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

            GameManager = new(this);
            Components.Add(new InputHandler(this));
            GameScreen = new(this);
            ExitScreen = new(this);
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            //TODO
            //StateManager.CurrentState = new StartScreen(this);
            StateManager.ChangeState(new MenuScreen(this));
            //StateManager.CurrentState = GameScreen;
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            GameTime = gameTime;

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
