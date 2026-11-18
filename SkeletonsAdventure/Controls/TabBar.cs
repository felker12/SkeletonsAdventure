using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.GameMenu;

namespace SkeletonsAdventure.Controls
{
    internal class TabBar
    {
        private readonly Texture2D _tabTexture;
        public Dictionary<Tab, BaseMenu> TabMenus { get; private set; } = [];
        public Vector2 Position { get; set; } = new();
        public SpriteFont SpriteFont { get; }
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 100;
        public BaseMenu ActiveMenu { get; set; } = null;

        public event EventHandler TabClicked;
        protected MouseState _currentMouse, _previousMouse;

        public TabBar(Texture2D tabTexture, SpriteFont font)
        {
            _tabTexture = tabTexture ?? throw new ArgumentNullException(nameof(tabTexture));
            SpriteFont = font ?? throw new ArgumentNullException(nameof(font));
            TabClicked += HandleTabSelected;
        }

        public void Update(GameTime gameTime)
        {
            ArrangeTabs();
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();
            var mouseRectangle = new Rectangle(_currentMouse.X, _currentMouse.Y, 1, 1);

            foreach (Tab tab in TabMenus.Keys)
            {
                if(tab.Visible)
                {
                    if (mouseRectangle.Intersects(tab.Rectangle))
                    {
                        tab.IsHovering = true;

                        if (_currentMouse.LeftButton == ButtonState.Released &&
                            _previousMouse.LeftButton == ButtonState.Pressed)
                        {
                            TabClicked?.Invoke(tab, new EventArgs());
                        }
                    }
                    else
                        tab.IsHovering = false;
                }
            }

            ActiveMenu?.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ArrangeTabs();
            foreach (Tab tab in TabMenus.Keys)
            {
                if(tab.Visible)
                {
                    tab.Draw(spriteBatch);
                }
            }
        }

        private void ArrangeTabs()
        {
            Vector2 position = Position;
            foreach (Tab tab in TabMenus.Keys)
            {
                if (!tab.Visible)
                    continue;

                tab.Position = position;
                position.X += tab.Width + 8;
            }
        }

        public void HandleTabSelected(object sender, EventArgs e)
        {
            Tab tab = (Tab)sender;
            if (!tab.Active)
            {
                SetActiveTab(TabMenus[tab]);
            }
        }

        public void AddMenu(BaseMenu menu)
        {
            Tab tab = new(_tabTexture, SpriteFont)
            {
                Text = menu.Title,
                Width = Math.Max(88, (int)Math.Ceiling(SpriteFont.MeasureString(menu.Title).X) + 32),
                Height = 38,
                Visible = true,
            };

            TabMenus.Add(tab, menu);

            Height = MaxTabHeight();
        }

        public void SetActiveTab(BaseMenu menu)
        {
            if (menu == null || !TabMenus.ContainsValue(menu))
                return;

            foreach (Tab t in TabMenus.Keys)
            {
                if (TabMenus[t] == menu)
                {
                    t.Active = true;
                    ActiveMenu = menu;
                    ActiveMenu.MenuOpened();
                }
                else
                {
                    t.Active = false;
                }
            }
        }

        public int MaxTabHeight()
        {
            int maxHeight = 0;

            foreach(Tab tab in TabMenus.Keys)
            {
                if (tab.Height > maxHeight)
                    maxHeight = tab.Height;
            }

            return maxHeight;
        }

        public BaseMenu GetFirstVisibleMenu()
        {
            foreach (Tab tab in TabMenus.Keys)
            {
                if (tab.Visible)
                {
                    return TabMenus[tab];
                }
            }

            return null;
        }
    }
}
