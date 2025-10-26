
namespace RpgLibrary.ItemClasses
{
    public class ShieldData : ItemData
    {
        public Hands NumberHands { get; set; }
        public int DefenceValue { get; set; }

        public ShieldData() : base()
        {
        }

        public ShieldData(ItemData itemData) : base(itemData)
        {
        }

        protected ShieldData(ShieldData shieldData) : base(shieldData)
        {
            NumberHands = shieldData.NumberHands;
            DefenceValue = shieldData.DefenceValue;
        }

        public override ShieldData Clone()
        {
            return new(this);
        }

        public override string ToString()
        {
            string toString = base.ToString() + ", ";
            toString += NumberHands.ToString() + ", ";
            toString += DefenceValue.ToString();
            return toString;
        }
    }
}
