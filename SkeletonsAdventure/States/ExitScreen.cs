using RpgLibrary.AttackData;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.MenuClasses;
using RpgLibrary.SettingsClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameWorld;
using System.IO;

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
            string savePath = GameManager.SavePath;
            Player player = World.Player;

            try
            {
                if (Directory.Exists(savePath) == false)
                    Directory.CreateDirectory(savePath);

                MenuManagerData GameScreenMenuData = new();
                foreach (BaseMenu baseMenu in Game.GameScreen.Menus)
                {
                    if (baseMenu is TabbedMenu tabbedMenu)
                        GameScreenMenuData.Menus.Add(tabbedMenu.GetTabbedMenuData());
                    else if (baseMenu is not null)
                        GameScreenMenuData.Menus.Add(baseMenu.GetMenuData());
                }

                //save the data
                XnaSerializer.Serialize<WorldData>(savePath + @"\World.xml", World.GetWorldData());
                XnaSerializer.Serialize<MenuManagerData>(savePath + @"\GameScreenMenuData.xml", GameScreenMenuData);
                XnaSerializer.Serialize<TabbedMenuData>(savePath + @"\ExitScreenData.xml", ExitScreenMenu.GetTabbedMenuData());
                XnaSerializer.Serialize<List<String>>(Path.Combine(savePath, "MessageBox.xml"), Game.GameScreen.MessageBox.Messages);
                XnaSerializer.Serialize<KeyBindingsManagerData>(savePath + @"\Keybindings.xml", player.KeybindingsManager.ToData());
                XnaSerializer.Serialize<LearnedAttackManagerData>(savePath + @"\LearnedAttackManager.Xml", player.LearnedAttackManager.ToData());
            }
            catch (Exception ex)
            {
                //TODO handle exception
                Debug.WriteLine(ex);
                return;
            }
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
