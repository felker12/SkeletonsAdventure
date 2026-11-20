using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameWorld;
using System.IO;

namespace SkeletonsAdventure.States
{
    internal class MenuScreen : FrontScreen
    {
        private string _loadStatus = string.Empty;

        public MenuScreen(Game1 game, GameManager assets) : base(game, assets)
        {
            int row = 0;
            AddMenuButton("New Game", () => StartNewGame(), primary: true);
            if (Game1.IfDev)
                AddMenuButton("New Game (Test)", () => StartNewGame(testGame: true));
            AddMenuButton("Load Game", LoadGame);
            AddMenuButton("Quit Game", Game.Exit);

            void AddMenuButton(string text, Action activate, bool primary = false) =>
                AddButton(new(98, 291 + 58 * row++, 336, 48), text, activate, primary);
        }

        internal Rectangle ButtonPanelBounds => GetButtonPanelBounds(18, 19);

        protected override void DrawForeground(SpriteBatch batch)
        {
            DrawTitle(batch, 82, 134, 0.76f);
            DrawRule(batch, 84, 231, 365);
            var panel = ButtonPanelBounds;
            Fill(batch, panel, new Color(75, 98, 87));
            panel.Inflate(-1, -1);
            Fill(batch, panel, new Color(10, 28, 27));
            if (_loadStatus.Length > 0)
                DrawText(batch, Assets.FontsLibrary.Arial12, _loadStatus, panel.Left + 3, panel.Bottom + 18, Ivory);
        }

        private void LoadGame()
        {
            _loadStatus = string.Empty;
            try
            {
                SaveManager.LoadSavedGame(Game, Assets, GameManager.PathsLibrary.SavePath);
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                _loadStatus = "No complete save was found.";
                return;
            }
            catch (InvalidDataException ex)
            {
                Debug.WriteLine(ex);
                _loadStatus = "The saved game is incomplete or incompatible.";
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _loadStatus = "Could not load the saved game.";
                return;
            }
            //return to the game screen
            StateManager.ChangeState(Game.GameScreen);
        }

        internal void StartNewGame(bool testGame = false)
        {
            Game.GameScreen = new(Game, Assets, testGame);
            if (testGame)
                World.GiveTestInventory();
            else
                World.GiveStarterInventory();
            Game.ExitScreen = new(Game, Assets);

            StateManager.ChangeState(Game.GameScreen);
        }

    }
}
