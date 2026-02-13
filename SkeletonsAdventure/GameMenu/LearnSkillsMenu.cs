using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.Controls;

namespace SkeletonsAdventure.GameMenu
{
    internal class LearnSkillsMenu : BaseMenu
    {
        private static Player Player { get; set; } = World.Player;
        private static Dictionary<string, BasicAttack> Skills { get; } = GameManager.EntityAttackClone;
        private Dictionary<string, Button> SkillButtons { get; set; } = [];
        private List<string> ExcludedSkills { get; set; } = ["BasicAttack"]; //skills that should not be learnable by the player

        public LearnSkillsMenu() 
        {
            Button btn = new();

            //remove excluded skills
            foreach (string excludedSkill in ExcludedSkills)
                Skills.Remove(excludedSkill);

            //Get the longest length of any of the skill names to use for button sizing
            Vector2 largestBtnTxt = new(), currentText;
            foreach(string name in Skills.Keys)
            {
                currentText = btn.SpriteFont.MeasureString(name);
                if (currentText.X > largestBtnTxt.X)
                    largestBtnTxt = currentText;
            }

            int widthBuffer = 18; //extra width to add to button for padding
            foreach (var skill in Skills)
            {
                btn = new(skill.Key)
                {
                    Visible = true,
                    Name = skill.Key,
                    Width = (int)largestBtnTxt.X + widthBuffer,
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
            foreach (var skill in Skills)
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

        private void PositionSkillButtons()
        {
            Vector2 originalPos = new(40, 40);
            int maxCols = 5;
            int padding = 6;
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
            var skillToLearn = Skills[button.Name];

            Player.LearnAttack(skillToLearn);
            UpdateButtonsEnabled();
        }
    }
}
