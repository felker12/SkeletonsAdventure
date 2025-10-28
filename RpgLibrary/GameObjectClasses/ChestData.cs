using Microsoft.Xna.Framework;
using RpgLibrary.DataClasses;
using RpgLibrary.ItemClasses;

namespace RpgLibrary.GameObjectClasses
{
    public enum ChestType { Starter, Basic, MidTier, Advanced, Special } //TODO this might not be needed
    public class ChestData
    {
        public int ID { get; set; } = -1;
        public ChestType ChestType { get; set; }
        public Vector2 Position { get; set; } = new();
        public string DropTableName { get; set; } = string.Empty;
        public bool ChestEmptied { get; set; } = false;
        public MinMaxPair LootAmountRange { get; set; } = new(2, 4);
        public List<ItemData> ItemDatas { get; set; } = new();

        public ChestData() { }

        public ChestData(ChestData data)
        {
            DropTableName = data.DropTableName;
            ChestType = data.ChestType;
            Position = data.Position;
            ID = data.ID;
            ItemDatas = new List<ItemData>(data.ItemDatas);
            ChestEmptied = data.ChestEmptied;
            LootAmountRange = new MinMaxPair(data.LootAmountRange.Min, data.LootAmountRange.Max);
        }

        public ChestData Clone()
        {
            return new(this);
        }

        public override string ToString()
        {
            string toString = ID + ", ";
            toString += ChestType + "\n";
            toString += "Position: " + Position + "\n";
            toString += "Drop Table: " + DropTableName + "\n";
            toString += "Loot Amount Range: " + LootAmountRange + "\n";

            foreach (ItemData itemData in ItemDatas)
                toString += itemData.ToString() + "\n";

            return toString;
        }
    }
}
