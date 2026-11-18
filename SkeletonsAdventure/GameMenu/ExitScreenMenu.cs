using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.LibraryClasses;

namespace SkeletonsAdventure.GameMenu
{
    internal sealed class ExitScreenMenu : TabbedMenu
    {
        private const int Margin = 56;
        private const int TabTop = 123;
        private const int PanelTop = 171;
        private static readonly TimeSpan SaveFeedbackDuration = TimeSpan.FromSeconds(1.5);
        private readonly MenuButton[] _saveActions;
        private readonly MenuButton _saveButton;
        private TimeSpan _saveFeedbackRemaining;
        private readonly Label _settingsMessage;
        private readonly BaseMenu _saveMenu;
        private readonly EvolutionMenu _evolutionMenu;
        private readonly SpriteFont _titleFont;
        private readonly SpriteFont _hintFont;

        public ExitScreenMenu(int width, int height, Texture2D pixel, FontsLibrary fonts,
            IReadOnlyDictionary<string, BasicAttack> attackTemplates,
            Func<bool> saveGame, Action resumeGame, Action returnToMenu)
            : base(width, height, pixel, pixel, fonts.Arial14, MenuStyle.Panel)
        {
            Title = "ExitScreenMenu";
            Visible = true;
            _titleFont = fonts.Title;
            _hintFont = fonts.Arial12;

            _saveMenu = new BaseMenu(pixel, MenuStyle.Panel) { Title = "Save", Visible = true };
            _saveButton = new(pixel, fonts.Arial16, "Save Game", () => Save(saveGame), primary: true) { HasFocus = true };
            _saveActions =
            [
                _saveButton,
                new(pixel, fonts.Arial16, "Resume Game", resumeGame),
                new(pixel, fonts.Arial16, "Return to Main Menu", returnToMenu)
            ];
            _saveMenu.ControlManager.AddRange(_saveActions);
            AddMenu(_saveMenu);

            var settings = new BaseMenu(pixel, MenuStyle.Panel) { Title = "Settings", Visible = true };
            _settingsMessage = new Label(fonts.Arial16, "No settings available.") { TextColor = MenuStyle.MutedText };
            settings.ControlManager.Add(_settingsMessage);
            AddMenu(settings);

            AddMenu(new PlayerInfoMenu(pixel, pixel, fonts.Arial12, MenuStyle.Panel) { Title = "Player", Visible = true });
            AddMenu(new QuestMenu(pixel, pixel, pixel, fonts.Arial12, fonts.Arial16, MenuStyle.Panel)
                { Title = "Quests", Visible = true });

            _evolutionMenu = new EvolutionMenu(pixel, pixel, fonts.Arial12, MenuStyle.Panel) { Title = "Evolution" };
            AddMenu(_evolutionMenu);
            AddMenu(new LearnSkillsMenu(pixel, pixel, fonts.Arial12, MenuStyle.Panel, attackTemplates)
                { Title = "Skills", Visible = true });
            AddMenu(new ActionBarMenu(pixel, pixel, fonts.Arial12, MenuStyle.Panel)
                { Title = "Action Bar", Visible = true });

            foreach (BaseMenu menu in TabBar.TabMenus.Values)
            {
                foreach (Control control in menu.ControlManager)
                {
                    if (control is Button button)
                        MenuStyle.Apply(button);
                }
            }

            Arrange(new Rectangle(0, 0, width, height));
            TabBar.SetActiveTab(_saveMenu);
        }

        private void Save(Func<bool> saveGame)
        {
            _saveButton.Enabled = false;
            _saveButton.Text = saveGame() ? "Game saved" : "Could not save game";
            _saveFeedbackRemaining = SaveFeedbackDuration;
        }

        public override void Update(GameTime gameTime)
        {
            if (_saveFeedbackRemaining > TimeSpan.Zero)
            {
                _saveFeedbackRemaining -= gameTime.ElapsedGameTime;
                if (_saveFeedbackRemaining <= TimeSpan.Zero)
                {
                    _saveButton.Text = "Save Game";
                    _saveButton.Enabled = true;
                }
            }

            base.Update(gameTime);
        }

        public override void Arrange(Rectangle bounds)
        {
            base.Arrange(bounds);
            TabBar.Position = new(bounds.X + Margin, bounds.Y + TabTop);
            TabBar.Width = Math.Max(1, bounds.Width - Margin * 2);
            var panel = new Rectangle(bounds.X + Margin + 1, bounds.Y + PanelTop + 1,
                Math.Max(1, TabBar.Width - 2), Math.Max(1, bounds.Height - PanelTop - 40));

            foreach (BaseMenu menu in TabBar.TabMenus.Values)
            {
                menu.Arrange(panel);
                menu.TintColor = MenuStyle.Panel;
            }

            for (int i = 0; i < _saveActions.Length; i++)
                _saveActions[i].Arrange(new(panel.X + 26, panel.Y + 26 + i * 58, Math.Clamp(panel.Width - 52, 3, 340), 46));

            _settingsMessage.Position = new(panel.X + 26, panel.Y + 26);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;

            spriteBatch.Begin();
            var titlePosition = Position + new Vector2(Margin, 30);
            spriteBatch.DrawString(_titleFont, "Paused", titlePosition, MenuStyle.Ivory, 0, Vector2.Zero,
                0.7f, SpriteEffects.None, 0);

            const string hint = "Esc to resume";
            var hintPosition = new Vector2(Rectangle.Right - Margin - _hintFont.MeasureString(hint).X, Position.Y + 56);
            spriteBatch.DrawString(_hintFont, hint, hintPosition, MenuStyle.MutedText);

            int ruleY = (int)Position.Y + 103;
            spriteBatch.Draw(Texture, new Rectangle((int)TabBar.Position.X, ruleY, TabBar.Width, 1), MenuStyle.Border);
            spriteBatch.Draw(Texture, new Rectangle((int)TabBar.Position.X, ruleY - 1, 38, 3), MenuStyle.Brass);
            TabBar.Draw(spriteBatch);

            BaseMenu activeMenu = TabBar.ActiveMenu;
            if (activeMenu is not null)
            {
                Rectangle border = activeMenu.Rectangle;
                border.Inflate(1, 1);
                spriteBatch.Draw(Texture, border, MenuStyle.Border);
            }

            spriteBatch.End();
            activeMenu?.Draw(spriteBatch);
            if (activeMenu == _saveMenu && _saveMenu.Visible)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(_hintFont, "Tab to select  |  Enter or A to confirm",
                    _saveActions[^1].Position + new Vector2(0, 63), MenuStyle.MutedText);
                spriteBatch.End();
            }
        }

        public override void MenuOpened()
        {
            _evolutionMenu.Player = World.Player;
            _evolutionMenu.Visible = World.Player.CanEvolve;
            foreach (var (tab, menu) in TabBar.TabMenus)
            {
                if (menu == _evolutionMenu)
                {
                    tab.Visible = _evolutionMenu.Visible;
                    break;
                }
            }

            if (!_evolutionMenu.Visible && TabBar.ActiveMenu == _evolutionMenu)
                TabBar.SetActiveTab(_saveMenu);
            else
                base.MenuOpened();
        }
    }
}
