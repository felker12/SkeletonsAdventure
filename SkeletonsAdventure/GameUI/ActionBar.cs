using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using SkeletonsAdventure.Attacks;

namespace SkeletonsAdventure.GameUI
{
    internal class ActionBar(Dictionary<Keys, BasicAttack> keyBindings, SpriteFont font, Texture2D pixel,
        Vector2 position = new(), int slotSize = 64, int slotPadding = 4,
        Color? background = null, Color? border = null, Color? text = null)
    {
        private readonly SpriteFont _font = font ?? throw new ArgumentNullException(nameof(font));
        private readonly Texture2D _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel)); // 1x1 white pixel texture used for drawing rects
        private readonly Color _backGroundColor = background ?? new Color(20, 20, 20, 200);
        private readonly Color _borderColor = border ?? Color.Black;
        private readonly Color _textColor = text ?? Color.White;

        // Draw keys in order: 1..9 then 0
        private readonly static Keys[] keyOrder = [Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0];

        public static int SlotCount => keyOrder.Length;
        public Dictionary<Keys, BasicAttack> KeyBindings { get; private set; } = keyBindings ?? [];
        public Vector2 Position { get; set; } = position;
        public int SlotSize { get; private set; } = Math.Max(16, slotSize);
        public int SlotPadding { get; private set; } = Math.Max(0, slotPadding);
        public int TotalWidth => SlotCount * SlotSize + (SlotCount - 1) * SlotPadding;
        public int TotalHeight => SlotSize;


        // Replace or set the keybindings dictionary used for display.
        public void SetKeyBindings(Dictionary<Keys, BasicAttack> keyBindings)
        {
            KeyBindings = keyBindings ?? [];
        }

        // Draw the action bar showing keys 0..9 (Keys.D0..Keys.D9) left-to-right.
        public void Draw(SpriteBatch spriteBatch)
        {
            if (spriteBatch == null) return;

            // Draw 10 slots for keys D0..D9
            for (int i = 0; i < SlotCount; i++)
            {
                Keys key = keyOrder[i];
                Vector2 slotPos = Position + new Vector2(i * (SlotSize + SlotPadding), 0);
                Rectangle slotRect = new((int)slotPos.X, (int)slotPos.Y, SlotSize, SlotSize);

                // Background
                spriteBatch.Draw(_pixel, slotRect, _backGroundColor);
                spriteBatch.DrawRectangle(slotRect, _borderColor, 1f);

                // Draw attack icon if bound
                if (KeyBindings?.TryGetValue(key, out BasicAttack boundAttack) == true && boundAttack != null)
                {
                    boundAttack.DrawIcon(spriteBatch, slotPos + new Vector2(2, 2), SlotSize - 4);

                    // --- Cooldown Overlay ---
                    // Option A: if you have a bool IsOnCooldown
                    //if (boundAttack.OnCooldown)
                    //{
                        // simple grey overlay
                        //spriteBatch.Draw(_pixel, slotRect, new Color(0, 0, 0, 120));
                    //}

                    // Option B: if you have remaining time ratio
                    // Example: draw a vertical cooldown fill (from bottom up)
                    if (boundAttack.CooldownRemaining > 0 && boundAttack.CoolDownLength > 0)
                    {
                        int fillHeight = (int)(SlotSize * boundAttack.CooldownRemainingRatio);
                        Rectangle fillRect = new(slotRect.X, slotRect.Y + (SlotSize - fillHeight), SlotSize, fillHeight);
                        spriteBatch.Draw(_pixel, fillRect, new Color(0, 0, 0, 160));
                    }
                }

                // Key label (small, top-left of slot)
                string keyLabel = KeyToLabel(key);
                Vector2 keyLabelPos = slotPos + new Vector2(4, 2);
                spriteBatch.DrawString(_font, keyLabel, keyLabelPos, _textColor);
            }
        }

        private static string KeyToLabel(Keys key)
        {
            // keys D0..D9 display as 0..9; fallback to key.ToString() otherwise.
            if (key >= Keys.D0 && key <= Keys.D9)
                return ((int)(key - Keys.D0)).ToString();
            return key.ToString();
        }
    }
}
