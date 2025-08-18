using RpgLibrary.MenuClasses;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameMenu
{
    internal class BaseMenu
    {
        public ControlManager ControlManager { get; set; } = new(GameManager.Arial20);
        protected PlayerIndex PlayerIndexInControl { get; set; } = PlayerIndex.One;
        public string Title { get; set; } = "Menu";
        public bool Visible { get; set; } = false;
        public Vector2 Position { get; set; } = new();
        public int Width { get; set; } = 400;
        public int Height { get; set; } = 400;
        public Texture2D Texture { get; set; } = GameManager.GameMenuTexture;
        public Rectangle Rectangle => new((int)Position.X, (int)Position.Y, Width, Height);
        public Color TintColor { get; set; } = Color.White;

        //This is just used to save the background color used by the SetBackgroundColor method to color the texture
        protected Color BackgroundColor { get; private set; } = Color.White;

        public BaseMenu()
        {
        }

        public BaseMenu(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public BaseMenu(MenuData menuData)
        {
            Visible = menuData.Visible;
            Position = menuData.Position;
            Width = menuData.Width;
            Height = menuData.Height;
            TintColor = menuData.TintColor;
            Title = menuData.Title;

            SetBackgroundColor(menuData.BackgroundColor);
        }

        public virtual void Update(GameTime gameTime)
        {
            if (Visible)
                ControlManager.Update(gameTime, PlayerIndexInControl);
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
        public void SetBackgroundColor(Color color)
        {
            Texture2D background = new(GameManager.GraphicsDevice, 1, 1);
            background.SetData([color]);

            Texture = background;
            BackgroundColor = color;
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
