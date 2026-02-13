using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;
using System.Security.Cryptography.X509Certificates;

namespace SkeletonsAdventure.GameMenu
{
    internal class ActionBarMenu : BaseMenu
    {
        private static readonly string defaultText = "Choose an option..."; // Placeholder text for the dropdown
        private readonly static Keys[] keyOrder = GameManager.KeyOrder;
        private static Player Player { get; set; }
        private Dictionary<string, BasicAttack> LearnedAttacks { get; set; } = [];
        private List<string> ExcludedSkills { get; set; } = ["BasicAttack"]; //skills that should not be learnable by the player
        private DropdownList[] DropdownLists { get; set; } 
        private Label[] DropdownLabels { get; set; }

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
            DropdownLists = new DropdownList[keyOrder.Length];
            DropdownLabels = new Label[keyOrder.Length];

            Vector2 startLabelPos = new(120, 152);
            Vector2 startDropdownPos = new(150, 150);
            int dropdownWidth = 200;
            int dropdownHeight = 30;
            int spacing = 40; // Space between each dropdown and label

            Label dropdownLabel;
            DropdownList dropdownList;

            foreach (Keys key in keyOrder)
            {
                // Create and configure the label

                dropdownLabel = new()
                {
                    Position = startLabelPos,
                    Text = key.ToString(),
                    Visible = true,
                };

                ControlManager.Add(dropdownLabel);

                startLabelPos.Y += spacing; // Move the next label down

                // Create and configure the dropdown list
                dropdownList = new()
                {
                    Position = startDropdownPos,
                    Width = dropdownWidth,
                    Height = dropdownHeight,
                    Text = defaultText, // Placeholder text
                    Visible = true,
                };

                //TODO do this once and add it to the dropdowns 
                foreach (var attack in LearnedAttacks)
                    dropdownList.AddItem(attack.Key);

                // Subscribe to selection changes
                dropdownList.Selected += (sender, e) =>
                {
                    var selected = dropdownList.SelectedItem;
                    // Handle selection
                };

                ControlManager.Add(dropdownList);

                startDropdownPos.Y += spacing; // Move the next dropdown down
            }
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
