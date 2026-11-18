using RpgLibrary.AttackData;
using RpgLibrary.DataClasses;
using RpgLibrary.MenuClasses;
using RpgLibrary.SettingsClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameWorld;
using System.IO;

namespace SkeletonsAdventure.States
{
    internal class MenuScreen : FrontScreen
    {
        private string _loadStatus = string.Empty;

        public MenuScreen(Game1 game, GameManager assets) : base(game, assets)
        {
            AddButton(new(98, 291, 336, 48), "New Game", NewGame, primary: true);
            AddButton(new(98, 349, 336, 48), "Load Game", LoadGame);
            AddButton(new(98, 407, 336, 48), "Quit Game", Game.Exit);
        }

        protected override void DrawForeground(SpriteBatch batch)
        {
            DrawTitle(batch, 82, 134, 0.76f);
            DrawRule(batch, 84, 231, 365);
            Fill(batch, new(80, 272, 372, 205), new Color(75, 98, 87));
            Fill(batch, new(81, 273, 370, 203), new Color(10, 28, 27));
            DrawText(batch, Assets.FontsLibrary.Arial12, "Up / Down to select    |    Enter or A to confirm",
                84, 499, MutedText);
            if (_loadStatus.Length > 0)
                DrawText(batch, Assets.FontsLibrary.Arial12, _loadStatus, 84, 533, Ivory);
        }

        private void LoadGame()
        {
            WorldData worldData;
            Texture2D playerTexture;
            MenuManagerData gameScreenMenuData;
            TabbedMenuData exitScreenData;
            List<string> messageBoxText;
            KeyBindingsManagerData bindings;
            LearnedAttackManagerData learnedAttacks;

            _loadStatus = string.Empty;
            try
            {
                string savePath = GameManager.PathsLibrary.SavePath;

                if (Directory.Exists(savePath) == false)
                    Directory.CreateDirectory(savePath);

                worldData = XnaSerializer.Deserialize<WorldData>(savePath + @"\World.xml");
                gameScreenMenuData = XnaSerializer.Deserialize<MenuManagerData>(savePath + @"\GameScreenMenuData.xml");
                exitScreenData = XnaSerializer.Deserialize<TabbedMenuData>(savePath + @"\ExitScreenData.xml");
                messageBoxText = XnaSerializer.Deserialize<List<String>>(Path.Combine(savePath, "MessageBox.xml"));
                bindings = XnaSerializer.Deserialize<KeyBindingsManagerData>(savePath + @"\Keybindings.xml");
                learnedAttacks = XnaSerializer.Deserialize<LearnedAttackManagerData>(savePath + @"\LearnedAttackManager.xml");
                playerTexture = Game.Content.Load<Texture2D>(worldData.PlayerData.TextureName);
            }
            catch (FileNotFoundException)
            {
                _loadStatus = "No complete save was found.";
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _loadStatus = "Could not load the saved game.";
                return;
            }

            //Update the game with the saved information
            Game.GameScreen = new(Game, Assets, worldData, playerTexture);
            Game.GameScreen.MessageBox.AddMessages(messageBoxText);
            UpdateMenusFromSave(gameScreenMenuData);
            Game.ExitScreen.SetExitScreenMenuData(exitScreenData);
            World.Player.SetKeybinding(bindings.Keybindings);
            World.Player.LearnedAttackManager.UpdateWithData(learnedAttacks);

            //return to the game screen
            StateManager.ChangeState(Game.GameScreen);
        }

        public void UpdateMenusFromSave(MenuManagerData menuManagerData)
        {
            foreach (MenuData menuData in menuManagerData.Menus)
            {
                foreach(BaseMenu gameMenu in Game.GameScreen.Menus)
                {
                    if(menuData.Title == gameMenu.Title)
                    {
                        if (menuData is TabbedMenuData tabbedMenuData && gameMenu is TabbedMenu tabbedMenu)
                        {
                            //update tabbed menus from loaded data
                            tabbedMenu.SetMenuData(tabbedMenuData);
                        }
                        else if (menuData is not null)
                        {
                            //update regular menus from loaded data
                            gameMenu.SetMenuData(menuData);
                        }
                    }
                }
            }
        }

        private void NewGame()
        {
            Game.GameScreen = new(Game, Assets);
            World.FillPlayerBackback(); //TODO

            StateManager.ChangeState(Game.GameScreen);
        }

    }
}
