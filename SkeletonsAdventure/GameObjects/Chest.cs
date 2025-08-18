using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;

namespace SkeletonsAdventure.GameObjects
{
    internal class Chest
    {
        public ChestType ChestType { get; set; } //TODO add different types of chests
        public ItemList Loot { get; set; } = new();
        public Vector2 Position { get; set; } = new();
        public int ID { get; set; } = -1;
        public Rectangle DetectionArea { get; set; }
        public GameButtonBox ChestMenu { get; set; } = new()
        {
            Visible = false,
            Texture = GameManager.ButtonBoxTexture,
        };
        public Label Info { get; set; } = new()
        {
            Text = "",
            Visible = false,
            SpriteFont = GameManager.Arial12
        };

        public Chest()
        {
        }

        public Chest(Chest chest)
        {
            Position = chest.Position;
            DetectionArea = chest.DetectionArea;
            ID = chest.ID;
            ChestType = chest.ChestType;
            Loot = chest.Loot.Clone();
            Info.Position = chest.Position;
        }

        public Chest(ItemList loot)
        {
            Loot = loot;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.DrawRectangle(DetectionArea, Color.White, 1, 0); //TODO

            if (Info.Visible)
                Info.Draw(spriteBatch); 

            if (ChestMenu.Visible)
                ChestMenu.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            if (Info.Visible)
                Info.Text = Loot.Count > 0 ? "Press R to Open" : "Chest Empty";

            ChestMenu.Update(gameTime, true, World.Camera.Transformation);
        }

        public void HandleInput(PlayerIndex playerIndex)
        {
            ChestMenu.HandleInput(playerIndex);
        }

        public bool PlayerIntersects(Rectangle playerRec)
        {
            bool intersects = false;

            if (playerRec.Intersects(DetectionArea))
            {
                Info.Visible = true;
                intersects = true;
            }
            else
            {
                Info.Visible = false;
                ChestMenu.Visible = false;
            }

            return intersects;
        }

        public void ChestOpened()
        {
            if (ChestMenu.Visible == false && Info.Visible == true)
            {
                // Set position before making visible
                ChestMenu.Position = Position - new Vector2(ChestMenu.Width / 2, ChestMenu.Height + 10);
                
                ChestMenu.Buttons.Clear();

                Dictionary<string, GameButton> buttons = [];
                foreach (GameItem gameItem in Loot.Items)
                {
                    GameButton btn = new(GameManager.DefaultButtonTexture, GameManager.Arial10)
                    {
                        Text = $"{gameItem.Name} x{gameItem.Quantity}"  // Add text to show item name and quantity
                    };

                    btn.Click += (sender, e) =>
                    {
                        if (World.CurrentLevel.Player.Backpack.Add(gameItem))
                        {
                            btn.Visible = false;
                            Loot.Remove(gameItem);
                        }
                    };

                    buttons.Add(gameItem.Name, btn);
                }

                ChestMenu.AddButtons(buttons);

                foreach (GameButton button in ChestMenu.Buttons)
                    button.Visible = true;

                // Make visible after everything is set up
                ChestMenu.Visible = true;
            }
            else
                ChestMenu.Visible = false;
        }

        public Chest Clone()
        {
            return new(this);
        }

        public ChestData GetChestData()
        {
            return new()
            {
                ItemDatas = Loot.GetItemListItemData(),
                ID = ID,
                ChestType = ChestType,
                Position = Position
            };
        }
    }
}
