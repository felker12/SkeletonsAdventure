using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.States
{
    internal abstract class State
    {
        protected GraphicsDevice GraphicsDevice { get; set; }
        protected Game1 Game {  get; set; }
        protected StateManager StateManager { get; set; }
        protected GameManager Assets { get; }
        protected ControlManager ControlManager { get; } = new();
        protected PlayerIndex PlayerIndexInControl { get; set; } = PlayerIndex.One;

        protected State(Game1 game, GameManager assets)
        {
            Game = game;
            Assets = assets ?? throw new ArgumentNullException(nameof(assets));
            GraphicsDevice = game.GraphicsDevice;
            StateManager = game.StateManager;
        }

        public abstract void StateChangeToHandler();
        public abstract void StateChangeFromHandler();
        public abstract void Update(GameTime gameTime);
        public abstract void HandleInput(PlayerIndex playerIndex);
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void PostUpdate(GameTime gameTime);
    }
}
