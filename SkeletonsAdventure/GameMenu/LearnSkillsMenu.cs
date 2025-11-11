using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.Controls;

namespace SkeletonsAdventure.GameMenu
{
    internal class LearnSkillsMenu : BaseMenu
    {
        public static Player Player { get; set; } = World.Player;
        private static Dictionary<string, BasicAttack> Skills { get; set; } = GameManager.EntityAttackClone;
        private Dictionary<string, Button> SkillButtons { get; set; } = [];
        private List<Button> VisibleSkillButtons { get; set; } = [];

        public LearnSkillsMenu() 
        {
            Button btn;

            foreach(var skill in Skills)
            {
                btn = new(skill.Key)
                {
                    Visible = true,
                    Name = skill.Key,
                };

                btn.Click += LearnSkillButton_Click;

                UpdateButtonEnabled(btn, skill);

                SkillButtons.Add(skill.Key, btn);
                ControlManager.Add(btn);
            }

            SetVisibleSkillButtons();
        }


        public void UpdateButtonsEnabled()
        {
            foreach (var skill in Skills)
            {
                UpdateButtonEnabled(SkillButtons[skill.Key], skill);
            }
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

        public void SetVisibleSkillButtons()
        {
            VisibleSkillButtons.Clear();
            //todo only show skills that can be learned?

            foreach (var skillButton in SkillButtons)
            {
                VisibleSkillButtons.Add(skillButton.Value);
            }

            PositionSkillButtons();
        }

        private void PositionSkillButtons()
        {
            Vector2 originalPos = new(40, 40);
            int maxCols = 5;
            int padding = 6;
            int count = 0;

            foreach (var btn in VisibleSkillButtons)
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

            Player.LearnAttack(button.Name, skillToLearn);
            UpdateButtonsEnabled();
        }
    }
}
