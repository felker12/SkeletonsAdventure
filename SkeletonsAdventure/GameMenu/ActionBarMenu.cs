using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameMenu
{
    internal class ActionBarMenu : BaseMenu
    {
        private static readonly string defaultText = "Choose an option..."; // Placeholder text for the dropdown
        private static Player Player { get; set; }
        private Dictionary<string, BasicAttack> LearnedAttacks { get; set; } = [];
        private List<string> ExcludedSkills { get; set; } = ["BasicAttack"]; //skills that should not be learnable by the player

        public Label DropdownLabel { get; set; } = new()
        {
            Position = new Vector2(120, 152),
            Text = "0:",
            Visible = true,
        };

        private DropdownList DropdownList { get; set; } = new()
        {
                Position = new Vector2(150, 150),
                Width = 200,
                Height = 30,
                Text = defaultText, // Placeholder text
        };

        public ActionBarMenu()
        {
            UpdateLearnedAttacks();

            CreateControls();
        }

        public void UpdateLearnedAttacks()
        {
            Player = World.Player;
            LearnedAttacks = Player.LearnedAttackManager.LearnedAttacks;

            //remove excluded skills
            foreach (string excludedSkill in ExcludedSkills)
                LearnedAttacks.Remove(excludedSkill);
        }

        private void CreateControls()
        {
            DropdownList.Clear();

            foreach (var attack in LearnedAttacks)
            {
                Debug.WriteLine(attack.Key);
                DropdownList.AddItem(attack.Key);
            }

            // Subscribe to selection changes
            DropdownList.Selected += (sender, e) =>
            {
                var selected = DropdownList.SelectedItem;
                // Handle selection
            };

            ControlManager.Add(DropdownLabel);
            ControlManager.Add(DropdownList);
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
            UpdateLearnedAttacks();
        }


    }
}
