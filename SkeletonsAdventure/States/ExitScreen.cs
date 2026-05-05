using RpgLibrary.MenuClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameMenu;

namespace SkeletonsAdventure.States
{
    internal class ExitScreen : State
    {
        public ExitScreenMenu ExitScreenMenu { get; set; }

        public ExitScreen(Game1 game) : base(game)
        {
            ExitScreenMenu = new(Game1.ScreenWidth, Game1.ScreenHeight)
            {
                Position = new(0, 0),
                Visible = true,
            };
            ExitScreenMenu.SetBackgroundColor(Color.MidnightBlue);
            ExitScreenMenu.TabBar.SetAllTabsColors(Color.MidnightBlue);

            //Add event handlers to the controls in the Settings Menu
            ExitScreenMenu.SaveGameLabel.Selected += SaveGameButton_Clicked;
            ExitScreenMenu.ReturnToGameLabel.Selected += ReturnToGameLabel_Selected;
            ExitScreenMenu.ReturnToMenuLabel.Selected += ReturnToMenuLabel_Selected; 
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.MidnightBlue);
            ExitScreenMenu.Draw(spriteBatch);
        }
        public override void Update(GameTime gameTime)
        {
            ExitScreenMenu.Update(gameTime);
        }
        public override void HandleInput(PlayerIndex playerIndex)
        {
            ExitScreenMenu.HandleInput(playerIndex);
        }

        public override void PostUpdate(GameTime gameTime){}

        private void SaveGame()
        {
            SaveManager.SaveGame(Game, ExitScreenMenu);
        }

        public void SetExitScreenMenuData(TabbedMenuData settingsMenu)
        {
            ExitScreenMenu.SetMenuData(settingsMenu);
        }

        private void ReturnToMenuLabel_Selected(object sender, EventArgs e)
        {
            StateManager.ChangeState(new MenuScreen(Game));
        }

        private void ReturnToGameLabel_Selected(object sender, EventArgs e)
        {
            StateManager.ChangeState(Game.GameScreen);
        }

        private void SaveGameButton_Clicked(object sender, EventArgs e)
        {
            SaveGame();
        }

        public override void StateChangeToHandler()
        {
            ExitScreenMenu.MenuOpened();
        }

        public override void StateChangeFromHandler()
        {
        }
    }
}
