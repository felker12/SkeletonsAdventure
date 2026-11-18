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
            _loadStatus = string.Empty;
            try
            {
                var data = SaveStorage.Read(GameManager.PathsLibrary.SavePath);
                SaveManager.RestoreGame(Game, Assets, data);
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
            //return to the game screen
            StateManager.ChangeState(Game.GameScreen);
        }

        private void NewGame()
        {
            Game.GameScreen = new(Game, Assets);
            World.FillPlayerBackback(); //TODO
            Game.ExitScreen = new(Game, Assets);

            StateManager.ChangeState(Game.GameScreen);
        }

    }
}
