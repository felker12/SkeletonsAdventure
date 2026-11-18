using RpgLibrary.MenuClasses;
using SkeletonsAdventure.Controls;

namespace SkeletonsAdventure.GameMenu
{
    internal class BaseMenu
    {
        public ControlManager ControlManager { get; } = new();
        protected PlayerIndex PlayerIndexInControl { get; set; } = PlayerIndex.One;
        public string Title { get; set; } = "Menu";
        public bool Visible { get; set; } = false;
        public Vector2 Position { get; set; } = new();
        public int Width { get; set; } = 400;
        public int Height { get; set; } = 400;
        public Texture2D Texture { get; }
        public Rectangle Rectangle => new((int)Position.X, (int)Position.Y, Width, Height);
        public Color TintColor { get; set; } = Color.White;

        protected Color BackgroundColor { get; }

        public BaseMenu(Texture2D texture, Color? backgroundColor = null)
        {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            BackgroundColor = backgroundColor ?? Color.White;
        }

        public BaseMenu(int width, int height, Texture2D texture, Color? backgroundColor = null)
            : this(texture, backgroundColor)
        {
            Width = width;
            Height = height;
        }

        public BaseMenu(MenuData menuData, Texture2D texture)
            : this(texture, menuData.BackgroundColor)
        {
            Visible = menuData.Visible;
            Position = menuData.Position;
            Width = menuData.Width;
            Height = menuData.Height;
            TintColor = menuData.TintColor;
            Title = menuData.Title;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (Visible)
                ControlManager.Update(gameTime, PlayerIndexInControl);
        }

        public virtual void Arrange(Rectangle bounds)
        {
            Position = new(bounds.X, bounds.Y);
            Width = bounds.Width;
            Height = bounds.Height;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(Texture, Rectangle, TintColor);
                ControlManager.Draw(spriteBatch);
                spriteBatch.End();
            }
        }

        public virtual void HandleInput(PlayerIndex playerIndex)
        {

        }

        public virtual void MenuOpened()
        {
            //Overridden by child classes
        }

        public void SetMenuData(MenuData menuData)
        {
            Visible = menuData.Visible;
            Position = menuData.Position;
            Width = menuData.Width;
            Height = menuData.Height;
            TintColor = menuData.TintColor;
            Title = menuData.Title;
        }
        public void ToggleVisibility()
        {
            Visible = !Visible;
        }

        public MenuData GetMenuData()
        {
            return new MenuData
            {
                Visible = Visible,
                Title = Title,
                Width = Width,
                Height = Height,
                Position = Position,
                TintColor = TintColor,
                BackgroundColor = BackgroundColor
            };
        }
    }
}
