using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.Engines;

namespace SkeletonsAdventure.Controls
{
    public class ControlManager : List<Control>
    {
        #region Fields and Properties
        int selectedControl = 0;

        bool AcceptInput { get; set; } = true;
        public SpriteFont SpriteFont { get; set; }
        #endregion
        #region Constructors
        public ControlManager(SpriteFont spriteFont)
        : base()
        {
            SpriteFont = spriteFont;
        }

        public ControlManager(SpriteFont spriteFont, int capacity)
        : base(capacity)
        {
            SpriteFont = spriteFont;
        }

        public ControlManager(SpriteFont spriteFont, IEnumerable<Control> collection)
        : base(collection)
        {
            SpriteFont = spriteFont;
        }
        #endregion

        #region Event Region
        public event EventHandler FocusChanged;
        #endregion

        #region Methods
        public void Update(GameTime gameTime, PlayerIndex playerIndex)
        {
            //original update method, which caused issues with dropdown lists and other controls that require exclusive input handling
            /*if (Count == 0)
                return;

            foreach (Control c in this)
            {
                if (c.Enabled)
                {
                    c.Update(gameTime);
                    c.HandleInput(playerIndex);
                }
            }

            if (!AcceptInput)
                return;

            if (InputHandler.ButtonPressed(Buttons.LeftThumbstickUp, playerIndex) ||
            InputHandler.ButtonPressed(Buttons.DPadUp, playerIndex) ||
            InputHandler.KeyPressed(Keys.Up))
                PreviousControl();
            if (InputHandler.ButtonPressed(Buttons.LeftThumbstickDown, playerIndex) ||
            InputHandler.ButtonPressed(Buttons.DPadDown, playerIndex) ||
            InputHandler.KeyPressed(Keys.Down) || InputHandler.KeyPressed(Keys.Tab))
                NextControl();*/

            if (Count == 0)
                return;

            // First pass: Update all controls
            foreach (Control c in this)
            {
                if (c.Enabled)
                    c.Update(gameTime);
            }

            // Check if any dropdown is expanded
            DropdownList expandedDropdown = null;
            foreach (Control c in this)
            {
                if (c is DropdownList dropdown && dropdown.IsExpanded)
                {
                    expandedDropdown = dropdown;
                    break;
                }
            }

            // Second pass: Handle input (only for non-blocked controls)
            foreach (Control c in this)
            {
                if (c.Enabled)
                {
                    // If a dropdown is expanded, only allow input for that dropdown
                    if (expandedDropdown != null && c != expandedDropdown)
                        continue;

                    c.HandleInput(playerIndex);
                }
            }

            if (!AcceptInput)
                return;

            if (InputHandler.ButtonPressed(Buttons.LeftThumbstickUp, playerIndex) ||
                InputHandler.ButtonPressed(Buttons.DPadUp, playerIndex) ||
                InputHandler.KeyPressed(Keys.Up))
                PreviousControl();
            if (InputHandler.ButtonPressed(Buttons.LeftThumbstickDown, playerIndex) ||
                InputHandler.ButtonPressed(Buttons.DPadDown, playerIndex) ||
                InputHandler.KeyPressed(Keys.Down) || InputHandler.KeyPressed(Keys.Tab))
                NextControl();
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Control c in this)
            {
                if (c.Visible)
                    c.Draw(spriteBatch);
            }
        }

        public void NextControl()
        {
            if (Count == 0)
                return;
            int currentControl = selectedControl;
            this[selectedControl].HasFocus = false;
            do
            {
                selectedControl++;
                if (selectedControl == Count)
                    selectedControl = 0;
                if (this[selectedControl].TabStop && this[selectedControl].Enabled)
                {
                    FocusChanged?.Invoke(this[selectedControl], null);
                    break;
                }
            } while (currentControl != selectedControl);
            this[selectedControl].HasFocus = true;
        }

        public void PreviousControl()
        {
            if (Count == 0)
                return;

            int currentControl = selectedControl;
            this[selectedControl].HasFocus = false;
            do
            {
                selectedControl--;
                if (selectedControl < 0)
                    selectedControl = Count - 1;
                if (this[selectedControl].TabStop && this[selectedControl].Enabled)
                {
                    FocusChanged?.Invoke(this[selectedControl], null);
                    break;
                }
            } while (currentControl != selectedControl);
            this[selectedControl].HasFocus = true;
        }
        #endregion
    }
}