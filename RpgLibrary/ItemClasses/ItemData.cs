using Microsoft.Xna.Framework;

namespace RpgLibrary.ItemClasses
{
    public enum Hands { Main, Off, Both }
    public enum ArmorLocation { Body, Head, Hands, Feet }
    public enum ConsumableEffect { Heal, AttackIncrease, DefenceIncrease }

    public class ItemData
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public float Weight { get; set; }
        public bool Equipped { get; set; }
        public bool Stackable { get; set; }
        public bool Consumable { get; set; }
        public Vector2 Position { get; set; } = Vector2.Zero;
        public int Quantity { get; set; }
        public Rectangle SourceRectangle { get; set; }
        public string TexturePath { get; set; } = string.Empty;

        public ItemData()
        {
        }

        protected ItemData(ItemData itemData)
        {
            Name = itemData.Name;
            Type = itemData.Type;
            Description = itemData.Description;
            Price = itemData.Price;
            Weight = itemData.Weight;
            Equipped = itemData.Equipped;
            Stackable = itemData.Stackable;
            Consumable = itemData.Consumable;
            Position = itemData.Position;
            Quantity = itemData.Quantity;
            SourceRectangle = itemData.SourceRectangle;
            TexturePath = itemData.TexturePath;
        }

        public virtual ItemData Clone()
        {
            return new(this);
        }

        public override string ToString()
        {
            string toString = Name + ", ";
            toString += Type + ", ";
            toString += Description + ", ";
            toString += Price + ", ";
            toString += Weight + ", ";
            toString += Equipped + ", ";
            toString += Stackable + ", ";
            toString += Consumable + ", ";
            toString += Position.ToString() + ", ";
            toString += Quantity + ", ";
            toString += SourceRectangle.ToString() + ", ";
            toString += TexturePath;
            return toString;
        }
    }
}
