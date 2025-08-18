using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameWorld;
using System;
using System.Collections.Generic;

namespace SkeletonsAdventure.Controls
{
    internal class TabBar
    {
        public Dictionary<Tab, BaseMenu> TabMenus { get; private set; } = [];
        public Vector2 Position { get; set; } = new();
        public SpriteFont SpriteFont { get; set; } = GameManager.Arial20;
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 100;
        public BaseMenu ActiveMenu { get; set; } = null;

        public event EventHandler TabClicked;
        protected MouseState _currentMouse, _previousMouse;

        public TabBar()
        {
            TabClicked += HandleTabSelected;
        }

        public void Update(GameTime gameTime)
        {
            Vector2 position = new(), spacer = new(5, 0);
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();
            var mouseRectangle = new Rectangle(_currentMouse.X, _currentMouse.Y, 1, 1);

            foreach (Tab tab in TabMenus.Keys)
            {
                if(tab.Visible)
                {
                    tab.Position = Position + position;
                    position += new Vector2(tab.Width, 0) + spacer;
                    tab.Update();
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
            foreach (Tab tab in TabMenus.Keys)
            {
                if(tab.Visible)
                {
                    tab.Draw(spriteBatch);
                }
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
            Tab tab = new()
            {
                Text = menu.Title,
                Width = (int)SpriteFont.MeasureString(menu.Title).X,
                Height = (int)SpriteFont.MeasureString(menu.Title).Y,
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

        public void SetAllTabsTextures(Texture2D texture)
        {
            foreach (Tab tab in TabMenus.Keys)
            {
                tab.Texture = texture;
            }
        }

        public void SetAllTabsColors(Color color)
        {
            Texture2D background = new(GameManager.GraphicsDevice, 1, 1);
            background.SetData([color]);

            foreach (Tab tab in TabMenus.Keys)
            {
                tab.Texture = background;
            }
        }
    }
}
