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
        private readonly static Keys[] keyOrder = KeybindingsManager.KeyOrder;
        private static Player Player { get; set; }
        private Dictionary<string, BasicAttack> LearnedAttacks { get; set; } = [];
        private List<string> ExcludedSkills { get; set; } = ["BasicAttack"]; //skills that should not be learnable by the player
        private List<string> attackNames;
        private readonly Dictionary<Keys, DropdownList> DropdownDictionary = [];
        private readonly Dictionary<Keys, Label> _keyLabels = [];
        private readonly Texture2D _pixel;
        private readonly SpriteFont _font;

        public ActionBarMenu(Texture2D texture, Texture2D pixel, SpriteFont font, Color color)
            : base(texture, color)
        {
            _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
            _font = font ?? throw new ArgumentNullException(nameof(font));
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

        private void CreateControls() //TODO add functionality to load the player's current keybindings into the dropdowns when the menu is opened
        {
            Label dropdownLabel;
            DropdownList dropdownList;
            
            foreach (Keys key in keyOrder)
            {
                // Create and configure the label
                dropdownLabel = new(_font)
                {
                    Text = DisplayKey(key),
                    TextColor = MenuStyle.Brass,
                    Visible = true,
                };
                ControlManager.Add(dropdownLabel);

                _keyLabels[key] = dropdownLabel;

                // Create and configure the dropdown list
                dropdownList = new(_pixel, _font)
                {
                    Height = 36,
                    BackgroundColor = MenuStyle.Surface,
                    DropdownBackgroundColor = MenuStyle.Surface,
                    BorderColor = MenuStyle.Border,
                    HoverColor = MenuStyle.Selection,
                    SelectedItemColor = MenuStyle.Selection,
                    TextColor = MenuStyle.Ivory,
                    Text = defaultText, // Placeholder text
                    Visible = true,
                };

                dropdownList.AddItem(defaultText); // Add the default option to the dropdown
                dropdownList.AddItems(attackNames); // Add the attack names to the dropdown

                dropdownList.SelectedIndex = 0; // Set the default selected index to the placeholder 

                // Subscribe to selection changes - auto-save when selection changes
                Keys currentKey = key;
                DropdownList currentDropdown = dropdownList;

                currentDropdown.Selected += (sender, e) =>
                {
                    SaveKeybinding(currentKey, currentDropdown);
                };

                DropdownDictionary[key] = dropdownList; // Map the key to its corresponding dropdown for easy access later

            }

            //Add the dropdowns to the control manager in reverse order to ensure
            //they are drawn in the correct order (dropdowns on top of labels and the save button)
            var dropdowns = DropdownDictionary.Values.Reverse().ToList();
            foreach (var dropdown in dropdowns)
                ControlManager.Add(dropdown);

            ArrangeControls();
        }

        public override void Arrange(Rectangle bounds)
        {
            base.Arrange(bounds);
            ArrangeControls();
        }

        private void ArrangeControls()
        {
            int rowsPerColumn = (keyOrder.Length + 1) / 2;
            int columnWidth = Width / 2;
            for (int i = 0; i < keyOrder.Length; i++)
            {
                Keys key = keyOrder[i];
                var position = Position + new Vector2(26 + i / rowsPerColumn * columnWidth, 26 + i % rowsPerColumn * 50);
                _keyLabels[key].Position = position + new Vector2(0, 8);
                DropdownDictionary[key].Position = position + new Vector2(45, 0);
                DropdownDictionary[key].Width = Math.Clamp(columnWidth - 80, 90, 330);
            }
        }

        private static string DisplayKey(Keys key) => key is >= Keys.D0 and <= Keys.D9
            ? ((int)key - (int)Keys.D0).ToString()
            : key.ToString();

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

            // Create controls only once. On subsequent opens, just refresh the
            // dropdown items and load the player's current keybindings into them.
            if (DropdownDictionary.Count == 0)
                CreateControls();
            else
                UpdateDropdownItems();

            LoadedAttacksToDropdowns();
        }

        // Refresh the items in each dropdown without recreating the controls.
        private void UpdateDropdownItems()
        {
            // attackNames should already be updated by UpdateLearnedAttacks()
            if (attackNames is null)
                return;

            foreach (var dropdown in DropdownDictionary.Values)
            {
                // Reset items to default + available attacks
                dropdown.Items.Clear();
                dropdown.AddItem(defaultText);
                dropdown.AddItems(attackNames);

                // Ensure selected index is valid
                if (dropdown.SelectedIndex >= dropdown.Items.Count)
                    dropdown.SelectedIndex = 0;
            }
        }

        private void SaveKeybinding(Keys key, DropdownList dropdown)
        {
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
    }
}
