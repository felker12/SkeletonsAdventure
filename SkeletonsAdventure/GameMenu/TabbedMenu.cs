using RpgLibrary.MenuClasses;
using SkeletonsAdventure.Controls;

namespace SkeletonsAdventure.GameMenu
{
    internal class TabbedMenu : BaseMenu
    {
        public TabBar TabBar { get; set; } = new();

        public TabbedMenu() : base()
        {
            Width = 600;
            Height = 500;
            Initialize();
        }

        public TabbedMenu(int width, int height) : base(width, height)
        {
            Initialize();
        }

        private void Initialize()
        {
            Position = new Vector2(0, 0);
            TabBar.Width = Width;
            TabBar.Height = TabBar.MaxTabHeight();
            TabBar.Position = Position;
        }

        public TabbedMenu(TabbedMenuData tabbedMenuData) : base(tabbedMenuData)
        {
            BaseMenu activeMenu = null;

            foreach(MenuData menuData in tabbedMenuData.MenuDatas)
            {
                TabBar.AddMenu(new BaseMenu(menuData));
            }

            foreach(BaseMenu menu in TabBar.TabMenus.Values)
            {
                if(menu.Title == tabbedMenuData.ActiveMenu)
                    activeMenu = menu;
            }

            TabBar.Position = Position;
            TabBar.Width = Width;
            TabBar.Height = TabBar.MaxTabHeight();
            TabBar.SetActiveTab(activeMenu);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Visible)
                TabBar.Update(gameTime);
        }

        public override void MenuOpened()
        {
            TabBar.ActiveMenu?.MenuOpened();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(Texture, Rectangle, TintColor);
                TabBar.Draw(spriteBatch);
                ControlManager.Draw(spriteBatch);
                spriteBatch.End();

                TabBar.ActiveMenu?.Draw(spriteBatch);
            }
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if (Visible is false)
                return;

            TabBar.ActiveMenu?.HandleInput(playerIndex);
        }

        public void SetMenuData(TabbedMenuData tabbedMenuData)
        {
            base.SetMenuData(tabbedMenuData);
            BaseMenu activeMenu = null;

            foreach (MenuData menuData in tabbedMenuData.MenuDatas)
            {
                foreach (BaseMenu menu in TabBar.TabMenus.Values)
                {
                    if (menu.Title == tabbedMenuData.ActiveMenu)
                        activeMenu = menu;

                    if(menuData.Title == menu.Title)
                        menu.SetMenuData(menuData);
                }
            }

            TabBar.Width = Width;
            TabBar.Height = TabBar.MaxTabHeight();
            TabBar.SetActiveTab(activeMenu);
        }

        public void AddMenu(BaseMenu menu)
        {
            TabBar.AddMenu(menu);

            foreach(BaseMenu baseMenu in TabBar.TabMenus.Values) //This way all the menus will the same size and position if the tabbar changes after a menu is added
            {
                baseMenu.Position = TabBar.Position + new Vector2(0, TabBar.Height);
                baseMenu.Width = Width;
                baseMenu.Height = Height - TabBar.Height;
            }
        }

        public TabbedMenuData GetTabbedMenuData()
        {
            List<MenuData> menuDatas = [];
            foreach(BaseMenu menu in TabBar.TabMenus.Values)
            {
                menuDatas.Add(menu.GetMenuData());
            }

            return new TabbedMenuData()
            {
                Visible = Visible,
                Title = Title,
                Width = Width,
                Height = Height,
                Position = Position,
                TintColor = TintColor,
                BackgroundColor = BackgroundColor,
                ActiveMenu = TabBar.ActiveMenu?.Title ?? string.Empty,
                MenuDatas = menuDatas
            };
        }
    }
}
