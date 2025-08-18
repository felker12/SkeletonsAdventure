using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.States
{
    internal abstract class State
    {
        protected ContentManager Content { get; set; }
        protected GraphicsDevice GraphicsDevice { get; set; }
        protected Game1 Game {  get; set; }
        protected StateManager StateManager { get; set; }
        public SpriteFont MenuFont { get; set; } = GameManager.Arial20;
        protected ControlManager ControlManager { get; set; }
        protected PlayerIndex PlayerIndexInControl { get; set; } = PlayerIndex.One;

        public State(Game1 game)
        {
            Game = game;
            GraphicsDevice = game.GraphicsDevice;
            Content = game.Content;
            StateManager = game.StateManager;

            ControlManager = new ControlManager(MenuFont);
        }

        public abstract void StateChangeToHandler();
        public abstract void StateChangeFromHandler();
        public abstract void Update(GameTime gameTime);
        public abstract void HandleInput(PlayerIndex playerIndex);
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void PostUpdate(GameTime gameTime);
    }
}
