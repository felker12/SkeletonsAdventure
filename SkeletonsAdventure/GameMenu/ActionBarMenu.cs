using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;
using System.Linq;

namespace SkeletonsAdventure.GameMenu
{
    internal class ActionBarMenu : BaseMenu
    {
        private static readonly string defaultText = "None"; // Placeholder text for the dropdown
        private readonly static Keys[] keyOrder = GameManager.KeyOrder;
        private static Player Player { get; set; }
        private Dictionary<string, BasicAttack> LearnedAttacks { get; set; } = [];
        private List<string> ExcludedSkills { get; set; } = ["BasicAttack"]; //skills that should not be learnable by the player
        private List<string> attackNames;
        private readonly Dictionary<Keys, DropdownList> DropdownDictionary = [];
        private Button saveBtn;

        public ActionBarMenu()
        {
            UpdateLearnedAttacks();
            CreateControls();
            LoadedAttacksToDropdowns();
        }

        public void UpdateLearnedAttacks()
        {
            Player = World.Player;
            LearnedAttacks = Player.LearnedAttackManager.LearnedAttacks;

            //remove excluded skills
            foreach (string excludedSkill in ExcludedSkills)
                LearnedAttacks.Remove(excludedSkill);

            attackNames = [.. LearnedAttacks.Keys];
        }

        private void CreateControls() //TODO add a save button and functionality to save the selected attacks to the player's keybindings, also add functionality to load the player's current keybindings into the dropdowns when the menu is opened
        {
            Vector2 startLabelPos = new(60, 65);
            Vector2 startDropdownPos = new(90, 60);
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

                dropdownList.AddItem(defaultText); // Add the default option to the dropdown
                dropdownList.AddItems(attackNames); // Add the attack names to the dropdown

                dropdownList.SelectedIndex = 0; // Set the default selected index to the placeholder 

                // Subscribe to selection changes
               /* dropdownList.Selected += (sender, e) =>
                {
                    //var selected = dropdownList.SelectedItem;
                };*/

                DropdownDictionary[key] = dropdownList; // Map the key to its corresponding dropdown for easy access later

                startDropdownPos.Y += spacing; // Move the next dropdown down
            }

            // Create and configure the save button
            saveBtn = new()
            {
                Position = new Vector2(90, startDropdownPos.Y),
                Width = 80,
                Height = 30,
                Text = "Save",
                Visible = true,
            };

            saveBtn.Click += SaveBtn_Click; // Subscribe to the click event

            ControlManager.Add(saveBtn);

            //Add the dropdowns to the control manager in reverse order to ensure
            //they are drawn in the correct order (dropdowns on top of labels and the save button)
            var dropdowns = DropdownDictionary.Values.Reverse().ToList();
            foreach (var dropdown in dropdowns)
                ControlManager.Add(dropdown);
        }

        private void LoadedAttacksToDropdowns()
        {
            foreach (var kvp in DropdownDictionary)
            {
                Keys key = kvp.Key;
                DropdownList dropdown = kvp.Value;

                if (Player.KeybindingsManager.Keybindings.TryGetValue(key, out BasicAttack boundAttack) 
                    && boundAttack != null)
                {
                    //Debug.WriteLine($"Loaded {boundAttack.Name} into dropdown for key {key}");

                    // Find the index of the bound attack name in the dropdown items
                    int selectedIndex = -1;
                    for (int i = 0; i < dropdown.Items.Count; i++)
                    {
                        if (dropdown.Items[i] == boundAttack.Name)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }

                    // Set the dropdown to the found index, or default (0) if not found
                    if (selectedIndex >= 0)
                    {
                        dropdown.SelectedIndex = selectedIndex;
                        //Debug.WriteLine($"Set dropdown index to {selectedIndex} for {boundAttack.Name}");
                    }
                    else
                    {
                        dropdown.SelectedIndex = 0; // Reset to "None" if attack not found
                        //Debug.WriteLine($"Attack {boundAttack.Name} not found in dropdown, reset to default");
                    }
                }
                else
                {
                    // No attack bound to this key, set to default "None"
                    dropdown.SelectedIndex = 0;
                    //Debug.WriteLine($"No attack bound for key {key}, set to default");
                }
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
            CreateControls();
            LoadedAttacksToDropdowns();
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            //TODO save the selected attacks in the dropdowns to the player's keybindings
            //Debug.WriteLine("Save button clicked. Saving keybindings...");

            // When the save button is clicked, iterate through the dropdowns and save the selected attacks to the player's keybindings
            foreach (var kvp in DropdownDictionary)
            {
                Keys key = kvp.Key;
                DropdownList dropdown = kvp.Value;
                string selectedAttackName = dropdown.SelectedItem;

                if (selectedAttackName != defaultText && LearnedAttacks.TryGetValue(selectedAttackName, out BasicAttack selectedAttack))
                {
                    Player.KeybindingsManager.SetKeybinding(key, selectedAttack);
                    //Debug.WriteLine($"Bound {selectedAttack.Name} to key {key}");
                }
                else
                {
                    Player.KeybindingsManager.RemoveKeybinding(key); // Remove keybinding if "None" is selected
                    //Debug.WriteLine($"Removed keybinding for key {key}");
                }
            }
            // Optionally, add feedback to the player here (e.g., a message that keybindings were saved)
            //Debug.WriteLine("Keybindings saved.");
        }
    }
}
