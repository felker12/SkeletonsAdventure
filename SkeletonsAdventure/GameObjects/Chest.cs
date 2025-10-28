using RpgLibrary.DataClasses;
using RpgLibrary.GameObjectClasses;
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.ItemClasses.ItemManagement;

namespace SkeletonsAdventure.GameObjects
{
    internal class Chest
    {
        public ChestType ChestType { get; set; } //TODO add different types of chests
        public DropTable DropTable { get; set; } = null;//TODO
        public string DropTableName { get; set; } = string.Empty; 
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
        public List<GameItem> Items { get; set; } = null;
        public int LootCount => Items.Count;
        public bool ChestEmptied { get; set; } = false;
        public MinMaxPair LootAmountRange { get; set; } = new(2, 4);

        public Chest()
        {
        }

        public Chest(Chest chest)
        {
            Position = chest.Position;
            DetectionArea = chest.DetectionArea;
            ID = chest.ID;
            ChestType = chest.ChestType;
            DropTableName = chest.DropTableName;
            DropTable = GameManager.GetDropTableByName(chest.DropTableName);
            Info.Position = chest.Position;
            LootAmountRange = new MinMaxPair(chest.LootAmountRange.Min, chest.LootAmountRange.Max);
            Items = chest.Items is not null ? [.. chest.Items] : null;
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
            if(DropTable is null && DropTableName != string.Empty)
                DropTable = GameManager.GetDropTableByName(DropTableName); 

            Items ??= DropTable.GetRandomAmountOfUniqueDrops(LootAmountRange);

            if (Info.Visible)
                Info.Text = Items.Count > 0 ? "Press R to Open" : "Chest Empty";

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
                ChestMenu.Position = Position + new Vector2(6, 6);

                // Create buttons for each item in the chest
                Dictionary<string, GameButton> buttons = [];

                foreach (var item in Items)
                {
                    GameButton btn = CreateGameButton(item);
                    buttons.Add(item.Name, btn);
                }

                ChestMenu.ClearButtons();
                ChestMenu.AddButtons(buttons);

                // Recalculate size after adding buttons
                ChestMenu.RecalculateSize();

                // Make visible after everything is set up
                ChestMenu.Visible = true;

                // Make all buttons visible
                foreach (GameButton button in ChestMenu.Buttons)
                    button.Visible = true;
            }
            else
                ChestMenu.Visible = false;
        }

        private GameButton CreateGameButton(GameItem item)
        {
            GameButton btn = new(GameManager.DefaultButtonTexture, GameManager.Arial10)
            {
                Text = $"{item.Name} x{item.Quantity}"  // Add text to show item name and quantity
            };

            btn.Click += (sender, e) =>
            {
                if (World.CurrentLevel.Player.Backpack.Add(item))
                {
                    btn.Visible = false;
                    DropTable.DropTableDictionary.Remove(item.Name);
                    Items.Remove(item);

                    if (Items.Count == 0)
                    {
                        ChestEmptied = true;
                    }
                }
            };

            return btn;
        }

        public Chest Clone()
        {
            return new(this);
        }

        public ChestData ToData()
        {
            List<ItemData> itemDatas = [];
            if (Items is not null)
            {
                foreach (var item in Items)
                {
                    itemDatas.Add(item.ToData());
                }
            }

            return new()
            {
                DropTableName = DropTableName,
                ID = ID,
                ChestType = ChestType,
                Position = Position,
                ItemDatas = itemDatas,
                ChestEmptied = ChestEmptied,
                LootAmountRange = new MinMaxPair(LootAmountRange.Min, LootAmountRange.Max)
            };
        }
    }
}
