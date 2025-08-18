using MonoGame.Extended;
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.ItemClasses
{
    internal class GameItem
    {
        public Vector2 Position { get; set; } = Vector2.Zero;
        public Texture2D Image { get; }
        public Rectangle SourceRectangle { get; }
        public string Type { get; } = string.Empty;
        public Rectangle ItemRectangle { get; set; }
        public static int Width { get; } = 32;
        public static int Height { get; } = 32;
        public int Quantity { get; private set; } = 1;
        public int Price { get; set; } = 0;
        public float Weight { get; set;} = 0f;
        public bool Stackable { get; set; } = false;
        public Label ToolTip { get; set; } = new()
        {
            Visible = false,
            SpriteFont = GameManager.Arial10,
            TextColor = Color.Aqua
        };
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TexturePath { get; set; } = string.Empty;

        public GameItem(ItemData item, int quantity, Texture2D texture) : this(item)
        {
            Image = texture;
            Quantity = quantity;
        }

        public GameItem(ItemData item)
        {
            Image = GameManager.Content.Load<Texture2D>(@$"{item.TexturePath}");
            SourceRectangle = item.SourceRectangle;
            Type = item.Type;
            Quantity = item.Quantity;
            Name = item.Name;
            Stackable = item.Stackable;
            Description = item.Description;
            TexturePath = item.TexturePath;
            Price = item.Price;
            Weight = item.Weight;

            if (Stackable)
                ToolTip.Text = Quantity + " " + Name;
            else
                ToolTip.Text = Name;
        }

        public GameItem(GameItem gameItem)
        {
            Image = gameItem.Image;
            SourceRectangle = gameItem.SourceRectangle;
            Type = gameItem.Type;
            Name = gameItem.Name;
            Stackable = gameItem.Stackable;
            Description = gameItem.Description;
            TexturePath = gameItem.TexturePath;
            Price = gameItem.Price;
            Weight = gameItem.Weight;

            ToolTip = new()
            {
                Visible = false,
                SpriteFont = gameItem.ToolTip.SpriteFont,
                TextColor = gameItem.ToolTip.TextColor,
                Text = gameItem.ToolTip.Text,
                Position = gameItem.ToolTip.Position
            };
            Quantity = gameItem.Quantity;
            Position = gameItem.Position;
        }

        public virtual GameItem Clone()
        {
            return new GameItem(this);
        }

        public virtual void Update()
        {
            ItemRectangle = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

            ToolTip.Position = Position + new Vector2(2, 0);

            if (Stackable)
            {
                ToolTip.Text = Quantity + " " + Name;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Image, Position, SourceRectangle, Color.White);
            spriteBatch.DrawRectangle(ItemRectangle, Color.WhiteSmoke, 1, 0);

            if (ToolTip.Visible)
                ToolTip.Draw(spriteBatch);
        }

        public override string ToString()
        {
            return GetData().ToString();
        }

        public void SetQuantity(int quantity)
        {
            if (Stackable is false)
                return; // Only allow setting quantity for stackable items

            Quantity = quantity;
        }

        public void AddQuantity(int quantity)
        {
            SetQuantity(Quantity + quantity);
        }

        public void RemoveQuantity(int quantity)
        {
            if (Stackable is false)
                return; // Only allow removing quantity for stackable items

            SetQuantity(Quantity - quantity);
        }

        public virtual ItemData GetData()
        {
            return new()
            {
                Name = Name,
                Type = Type,
                Description = Description,
                Price = Price,
                Weight = Weight,
                Stackable = Stackable,
                Position = Position,
                Quantity = Quantity,
                TexturePath = TexturePath,
            };
        }
    }
}
