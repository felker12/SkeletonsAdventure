using RpgLibrary.MenuClasses;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.States
{
    internal class ExitScreen : State
    {
        public ExitScreenMenu ExitScreenMenu { get; }

        public ExitScreen(Game1 game, GameManager assets) : base(game, assets)
        {
            ExitScreenMenu = new(Game1.ScreenWidth, Game1.ScreenHeight, Assets.TexturesLibrary.StatusBarTexture,
                Assets.FontsLibrary, Assets.AttackTemplates, SaveGame, ResumeGame, ReturnToMenu);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(MenuStyle.Canvas);
            ExitScreenMenu.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            var viewport = GraphicsDevice.Viewport;
            if (ExitScreenMenu.Width != viewport.Width || ExitScreenMenu.Height != viewport.Height)
                ExitScreenMenu.Arrange(new(0, 0, viewport.Width, viewport.Height));

            ExitScreenMenu.Update(gameTime);
        }

        public override void HandleInput(PlayerIndex playerIndex) => ExitScreenMenu.HandleInput(playerIndex);

        public void SetExitScreenMenuData(TabbedMenuData settingsMenu)
        {
            ExitScreenMenu.SetMenuData(settingsMenu);
            var viewport = GraphicsDevice.Viewport;
            ExitScreenMenu.Arrange(new(0, 0, viewport.Width, viewport.Height));
        }

        private bool SaveGame() => SaveManager.SaveGame(Game, ExitScreenMenu);
        private void ResumeGame() => StateManager.ChangeState(Game.GameScreen);
        private void ReturnToMenu() => StateManager.ChangeState(new MenuScreen(Game, Assets));

        public override void StateChangeToHandler() => ExitScreenMenu.MenuOpened();
        public override void StateChangeFromHandler() { }
        public override void PostUpdate(GameTime gameTime) { }
    }
}
