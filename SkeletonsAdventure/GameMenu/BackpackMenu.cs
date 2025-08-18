using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;

namespace SkeletonsAdventure.GameMenu
{
    internal class BackpackMenu : BaseMenu
    {
        public List<GameItem> Items { get; set; } = [];

        public BackpackMenu()
        {
            Texture = GameManager.BackpackBackground;
            Width = Texture.Width;
            Height = Texture.Height;
            Position = new(0, 0);
            Title = "Backpack";

            Position = new(Game1.ScreenWidth - Width, 0);
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if(Visible)
            {
                //Draw the backback items
                spriteBatch.Begin();
                foreach (GameItem item in Items)
                {
                    item.Draw(spriteBatch);
                }
                spriteBatch.End();
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void Update(GameTime gameTime, List<GameItem> items)
        {
            base.Update(gameTime);

            Update(items);
        }

        public void Update(List<GameItem> items)
        {
            Items = items;

            //19 colums 4 rows
            int[,] inventory = new int[19, 4];
            Vector2 pos = new();

            int itmCnt = 0;
            for (int i = 0; i < inventory.GetLength(0); i++)
            {
                for (int j = 0; j < inventory.GetLength(1); j++)
                {
                    if (itmCnt < Items.Count)
                    {
                        pos.X = Position.X + (j + 1) * 32;
                        pos.Y = Position.Y + (i + 1) * 32;
                        Items[itmCnt].Position = pos;
                        Items[itmCnt].ItemRectangle = new Rectangle((int)pos.X, (int)pos.Y, 32, 32);

                        itmCnt++;
                    }
                    else break;
                }
            }
        }
    }
}
