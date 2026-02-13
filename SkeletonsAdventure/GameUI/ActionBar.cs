using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameUI
{
    internal class ActionBar(Dictionary<Keys, BasicAttack> keyBindings, SpriteFont font, Texture2D pixel,
        Vector2 position = new(), int slotSize = 64, int slotPadding = 4, Color? frameBackground = null,
        Color? KeyBackground = null, Color? keyBorder = null, Color? text = null)
    {
        private readonly SpriteFont _font = font ?? throw new ArgumentNullException(nameof(font));
        private readonly Texture2D _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel)); // 1x1 white pixel texture used for drawing rects
        private readonly Color _backGroundColor = KeyBackground ?? new Color(30, 30, 30, 220);
        private readonly Color _borderColor = keyBorder ?? Color.Black;
        private readonly Color _textColor = text ?? Color.White;
        private readonly Color _frameBackground = frameBackground ?? new(15, 15, 15, 200);

        // Draw keys in order: 1..9 then 0
        private readonly static Keys[] keyOrder = GameManager.KeyOrder;

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

            Rectangle frameRect = new((int)Position.X, (int)Position.Y, TotalWidth, TotalHeight);
            frameRect.Inflate(4, 4);

            // Draw frame background
            spriteBatch.Draw(_pixel, frameRect, _frameBackground);
            spriteBatch.DrawRectangle(frameRect, _borderColor, 1f);

            // Draw 10 slots for keys D0..D9
            for (int i = 0; i < SlotCount; i++)
            {
                Keys key = keyOrder[i];
                Vector2 slotPos = Position + new Vector2(i * (SlotSize + SlotPadding), 0);
                Rectangle slotRect = new((int)slotPos.X, (int)slotPos.Y, SlotSize, SlotSize);

                // Background
                spriteBatch.Draw(_pixel, slotRect, _backGroundColor);
                spriteBatch.DrawRectangle(slotRect, _borderColor * .7f, 1f);

                // Draw attack icon if bound
                if (KeyBindings?.TryGetValue(key, out BasicAttack boundAttack) == true && boundAttack != null)
                {
                    boundAttack.DrawIcon(spriteBatch, slotPos + new Vector2(2, 2), SlotSize - 4);

                    // --- Cooldown Overlay ---
                    // simple grey overlay
                    //if (boundAttack.OnCooldown)
                        //spriteBatch.Draw(_pixel, slotRect, new Color(0, 0, 0, 120));

                    // draw a vertical cooldown fill (from bottom up)
                    if (boundAttack.CooldownRemaining > 0 && boundAttack.CoolDownLength > 0)
                    {
                        int fillHeight = (int)(SlotSize * boundAttack.CooldownRemainingRatio);
                        Rectangle fillRect = new(slotRect.X, slotRect.Y + (SlotSize - fillHeight), SlotSize, fillHeight);
                        spriteBatch.Draw(_pixel, fillRect, new Color(0, 0, 0, 160));
                    }
                }

                // Key label (small, top-left of slot)
                string keyLabel = KeyToString(key);
                Vector2 keyLabelPos = slotPos + new Vector2(4, 2);
                spriteBatch.DrawString(_font, keyLabel, keyLabelPos, _textColor);
            }
        }

        public static string KeyToString(Keys key)
        {
            // keys D0..D9 display as 0..9; fallback to key.ToString() otherwise.
            if (key >= Keys.D0 && key <= Keys.D9)
                return ((int)(key - Keys.D0)).ToString();
            return key.ToString();
        }
    }
}
