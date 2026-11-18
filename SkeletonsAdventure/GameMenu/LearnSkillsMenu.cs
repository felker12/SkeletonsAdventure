using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.Controls;
using System.Linq;

namespace SkeletonsAdventure.GameMenu
{
    internal class LearnSkillsMenu : BaseMenu
    {
        private static Player Player { get; set; } = World.Player;
        private readonly Dictionary<string, BasicAttack> _skills;
        private Dictionary<string, Button> SkillButtons { get; set; } = [];

        public LearnSkillsMenu(Texture2D texture, Texture2D buttonTexture, SpriteFont buttonFont,
            Color color, IReadOnlyDictionary<string, BasicAttack> attackTemplates)
            : base(texture, color)
        {
            ArgumentNullException.ThrowIfNull(attackTemplates);
            _skills = attackTemplates.Where(entry => entry.Key != nameof(BasicAttack))
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            //Get the longest length of any of the skill names to use for button sizing
            Vector2 largestBtnTxt = new(), currentText;
            foreach(string name in _skills.Keys)
            {
                currentText = buttonFont.MeasureString(name);
                if (currentText.X > largestBtnTxt.X)
                    largestBtnTxt = currentText;
            }

            int widthBuffer = 18; //extra width to add to button for padding
            foreach (var skill in _skills)
            {
                Button btn = new(buttonTexture, buttonFont)
                {
                    Text = skill.Key,
                    Visible = true,
                    Name = skill.Key,
                    Width = Math.Max(138, (int)largestBtnTxt.X + widthBuffer),
                    Height = 42,
                };

                btn.Click += LearnSkillButton_Click;

                UpdateButtonEnabled(btn, skill);

                SkillButtons.Add(skill.Key, btn);
                ControlManager.Add(btn);
            }

            PositionSkillButtons();
        }


        public void UpdateButtonsEnabled()
        {
            foreach (var skill in _skills)
                UpdateButtonEnabled(SkillButtons[skill.Key], skill);
        }

        private static void UpdateButtonEnabled(Button btn, KeyValuePair<string, BasicAttack> skill)
        {
            //disable the button if the player has learned it already or is missing the requirements. 
            if (Player.LearnedAttackManager.Contains(skill.Key))
                btn.Enabled = false;
            else
                btn.Enabled = Player.LearnedAttackManager.CheckRequirements(skill.Value);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void MenuOpened()
        {
            Player = World.Player;
            UpdateButtonsEnabled();
        }

        public override void Arrange(Rectangle bounds)
        {
            base.Arrange(bounds);
            PositionSkillButtons();
        }

        private void PositionSkillButtons()
        {
            Vector2 originalPos = Position + new Vector2(26, 26);
            int padding = 10;
            int buttonWidth = SkillButtons.Values.FirstOrDefault()?.Width ?? 138;
            int maxCols = Math.Max(1, (Width - 42) / (buttonWidth + padding));
            int count = 0;

            foreach (var btn in SkillButtons.Values)
            {
                int col = count % maxCols;
                int row = count / maxCols;

                btn.Position = new Vector2(
                    originalPos.X + col * (btn.Width + padding),
                    originalPos.Y + row * (btn.Height + padding)
                );

                count++;
            }
        }

        private void LearnSkillButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            var skillToLearn = _skills[button.Name].Clone();

            Player.LearnAttack(skillToLearn);
            UpdateButtonsEnabled();
        }
    }
}
