using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.States
{
    internal sealed class StartScreen : FrontScreen
    {
        public StartScreen(Game1 game, GameManager assets) : base(game, assets)
        {
            AddButton(new(84, 405, 249, 54), "Begin",
                () => StateManager.ChangeState(new MenuScreen(Game, Assets)), primary: true);
        }

        protected override void DrawForeground(SpriteBatch batch)
        {
            DrawTitle(batch, 82, 216, 1f);
            DrawRule(batch, 84, 341, 445);
            DrawText(batch, Assets.FontsLibrary.Arial12, "Enter or A", 86, 476, MutedText);
        }
    }
}
