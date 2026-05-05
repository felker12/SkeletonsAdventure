using RpgLibrary.ItemClasses;

namespace SkeletonsAdventure.ItemClasses
{
    internal class Shield : EquipableItem
    {
        public Hands NumberHands { get; set; }
        public int DefenceValue { get; set; }

        public Shield(Shield item) : base(item)
        {
            NumberHands = item.NumberHands;
            DefenceValue = item.DefenceValue;
        }

        public Shield(ShieldData data, Texture2D texture) : base(data, texture)
        {
            NumberHands = data.NumberHands;
            DefenceValue = data.DefenceValue;
        }

        public override Shield Clone()
        {
            return new Shield(this);
        }

        public override ShieldData ToData()
        {
            return new(base.ToData())
            {
                NumberHands = NumberHands,
                DefenceValue = DefenceValue
            };
        }
    }
}
