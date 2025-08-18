using RpgLibrary.ItemClasses;
using Microsoft.Xna.Framework;

namespace RpgLibrary.GameObjectClasses
{
    public enum ChestType { Starter, Basic, MidTier, Advanced, Special } //TODO this might not be needed
    public class ChestData
    {
        public int ID { get; set; } = -1;
        public ChestType ChestType { get; set; }
        public Vector2 Position { get; set; } = new();
        public List<ItemData> ItemDatas { get; set; } = new();

        public ChestData() { }

        public ChestData(ChestData data)
        {
            ItemDatas = data.ItemDatas;
            ChestType = data.ChestType;
            Position = data.Position;
            ID = data.ID;
        }

        public ChestData Clone()
        {
            return new(this)
            {
                ID = ID,
                ChestType = ChestType,
                Position = Position,
                ItemDatas = ItemDatas
            };
        }

        public override string ToString()
        {
            string toString = ID + ", ";
            toString += ChestType + "\n";

            foreach(ItemData data in ItemDatas)
            {
                toString += "\n" + data.ToString();
            }

            return toString;
        }
    }
}
