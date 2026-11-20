using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.States
{
    internal abstract class FrontScreen : State
    {
        private const int LayoutWidth = 1280;
        private const int LayoutHeight = 720;
        protected static readonly Color Brass = MenuStyle.Brass;
        protected static readonly Color Ivory = MenuStyle.Ivory;

        private readonly Texture2D _backdrop;
        private readonly Texture2D _pixel;
        private readonly SpriteFont _titleFont;
        private readonly List<(MenuButton Button, Rectangle Bounds)> _buttons = [];
        private float _scale;
        private Vector2 _origin;

        protected FrontScreen(Game1 game, GameManager assets) : base(game, assets)
        {
            _backdrop = assets.TexturesLibrary.TitleBackdrop;
            _pixel = assets.TexturesLibrary.StatusBarTexture;
            _titleFont = assets.FontsLibrary.Title;
        }

        protected void AddButton(Rectangle bounds, string text, Action action, bool primary = false)
        {
            var button = new MenuButton(_pixel, Assets.FontsLibrary.Arial16, text, action, primary)
            {
                HasFocus = _buttons.Count == 0
            };
            _buttons.Add((button, bounds));
            ControlManager.Add(button);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateLayout();
            ControlManager.Update(gameTime, PlayerIndexInControl);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            UpdateLayout();
            var viewport = GraphicsDevice.Viewport;
            float coverScale = Math.Max(viewport.Width / (float)_backdrop.Width, viewport.Height / (float)_backdrop.Height);
            int width = (int)Math.Ceiling(_backdrop.Width * coverScale);
            int height = (int)Math.Ceiling(_backdrop.Height * coverScale);
            var backdropBounds = new Rectangle((viewport.Width - width) / 2, (viewport.Height - height) / 2, width, height);

            spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
            spriteBatch.Draw(_backdrop, backdropBounds, Color.White);
            DrawForeground(spriteBatch);
            ControlManager.Draw(spriteBatch);
            spriteBatch.End();
        }

        protected abstract void DrawForeground(SpriteBatch spriteBatch);

        protected Rectangle GetButtonPanelBounds(int horizontalPadding, int verticalPadding)
        {
            Rectangle bounds = Rectangle.Empty;
            foreach (var (button, rectangle) in _buttons)
                if (button.Visible)
                    bounds = bounds.IsEmpty ? rectangle : Rectangle.Union(bounds, rectangle);

            if (!bounds.IsEmpty)
                bounds.Inflate(horizontalPadding, verticalPadding);
            return bounds;
        }

        protected void DrawTitle(SpriteBatch batch, int x, int y, float scale)
        {
            DrawText(batch, _titleFont, "THE ADVENTURES OF", x + 2, y, Brass, 0.35f * scale);
            DrawText(batch, _titleFont, "THE SKELETON", x + 2, y + 33 * scale,
                Color.Black * 0.8f, scale);
            DrawText(batch, _titleFont, "THE SKELETON", x, y + 30 * scale, Ivory, scale);
        }

        protected void DrawRule(SpriteBatch batch, int x, int y, int width)
        {
            Fill(batch, new(x, y, width, 1), Brass * 0.6f);
            Fill(batch, new(x, y - 1, 34, 3), Brass);
        }

        protected void Fill(SpriteBatch batch, Rectangle bounds, Color color) =>
            batch.Draw(_pixel, Place(bounds), color);

        protected void DrawText(SpriteBatch batch, SpriteFont font, string text, float x, float y, Color color, float scale = 1)
        {
            Vector2 position = _origin + new Vector2(x, y) * _scale;
            batch.DrawString(font, text, position, color, 0, Vector2.Zero, scale * _scale, SpriteEffects.None, 0);
        }

        private Rectangle Place(Rectangle bounds) => new(
            (int)Math.Round(_origin.X + bounds.X * _scale),
            (int)Math.Round(_origin.Y + bounds.Y * _scale),
            Math.Max(1, (int)Math.Round(bounds.Width * _scale)),
            Math.Max(1, (int)Math.Round(bounds.Height * _scale)));

        private void UpdateLayout()
        {
            var viewport = GraphicsDevice.Viewport;
            _scale = Math.Min(viewport.Width / (float)LayoutWidth, viewport.Height / (float)LayoutHeight);
            _origin = new((viewport.Width - LayoutWidth * _scale) / 2, (viewport.Height - LayoutHeight * _scale) / 2);

            foreach (var (button, bounds) in _buttons)
                button.Arrange(Place(bounds), _scale);
        }

        public override void HandleInput(PlayerIndex playerIndex) { }
        public override void PostUpdate(GameTime gameTime) { }
        public override void StateChangeToHandler() { }
        public override void StateChangeFromHandler() { }
    }
}

