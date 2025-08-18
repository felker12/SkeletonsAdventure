using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.GameObjectClasses;
using RpgLibrary.ItemClasses;

namespace RpgLibrary.WorldClasses
{
    public class LevelData
    {
        public MinMaxPair MinMaxPair { get; set; } = new();
        public EntityManagerData EntityManagerData { get; set; } = new();
        public List<ItemData> DroppedItemDatas { get; set; } = new();
        public List<ChestData> Chests { get; set; } = new();

        public LevelData() { }

        public override string ToString()
        {
            string toString = MinMaxPair.ToString() + "\n";
            toString += EntityManagerData.ToString();
            return toString;
        }
    }
}
