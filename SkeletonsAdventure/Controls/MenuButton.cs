using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.Engines;

namespace SkeletonsAdventure.Controls
{
    internal sealed class MenuButton : Control
    {
        private readonly Texture2D _pixel;
        private readonly Action _activate;
        private readonly bool _primary;
        private float _scale;
        private bool _pressedHere;
        private bool _clicked;

        public MenuButton(Texture2D pixel, SpriteFont font, string text, Action activate, bool primary = false)
            : base(font)
        {
            _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
            _activate = activate ?? throw new ArgumentNullException(nameof(activate));
            _primary = primary;
            Text = text;
            TabStop = true;
        }

        public void Arrange(Rectangle bounds, float scale = 1f)
        {
            Position = new(bounds.X, bounds.Y);
            Width = bounds.Width;
            Height = bounds.Height;
            _scale = scale;
        }

        public override void Update(GameTime gameTime)
        {
            IsMouseHovering();
            if (_currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
                _pressedHere = _isHovering;
            _clicked = _pressedHere && _isHovering &&
                _currentMouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed;
            if (_currentMouse.LeftButton == ButtonState.Released)
                _pressedHere = false;
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if (Enabled && (_clicked || (HasFocus && (InputHandler.KeyPressed(Keys.Enter) || InputHandler.ButtonPressed(Buttons.A, playerIndex)))))
            {
                _clicked = false;
                _activate();
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            Rectangle bounds = Rectangle;
            bool highlighted = Enabled && (HasFocus || _isHovering);
            Color border = highlighted ? MenuStyle.Brass : MenuStyle.Border;
            Color fill = !Enabled ? MenuStyle.Panel : highlighted
                ? MenuStyle.Selection
                : _primary ? new Color(46, 35, 25) : MenuStyle.Surface;
            int inset = 1;
            batch.Draw(_pixel, bounds, border);
            bounds.Inflate(-inset, -inset);
            batch.Draw(_pixel, bounds, fill);
            if (highlighted)
                batch.Draw(_pixel, new Rectangle(bounds.X, bounds.Y, Math.Max(2, (int)(4 * _scale)), bounds.Height), MenuStyle.Brass);

            Vector2 label = new(bounds.X + 19 * _scale, bounds.Center.Y - SpriteFont.LineSpacing * _scale / 2);
            batch.DrawString(SpriteFont, Text, label, Enabled ? MenuStyle.Ivory : MenuStyle.MutedText,
                0, Vector2.Zero, _scale, SpriteEffects.None, 0);
        }
    }
}
