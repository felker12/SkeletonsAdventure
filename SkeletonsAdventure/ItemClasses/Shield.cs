using RpgLibrary.ItemClasses;

namespace SkeletonsAdventure.ItemClasses
{
    internal class Shield : EquipableItem
    {
        public Hands NumberHands { get; set; }
        public int DefenceValue { get; set; }


        public Shield(ShieldData data) : base(data)
        {
            NumberHands = data.NumberHands;
            DefenceValue = data.DefenceValue;
        }
        public Shield(Shield item) : base(item)
        {
            NumberHands = item.NumberHands;
            DefenceValue = item.DefenceValue;
        }

        public override Shield Clone()
        {
            return new Shield(this);
        }

        public override ShieldData ToData()
        {
            return new ShieldData
            {
                Name = Name,
                Type = Type,
                Description = Description,
                Price = Price,
                Weight = Weight,
                Equipped = Equipped,
                Stackable = Stackable,
                Position = Position,
                Quantity = Quantity,
                SourceRectangle = SourceRectangle,
                TexturePath = TexturePath,
                NumberHands = NumberHands,
                DefenceValue = DefenceValue
            };

        }
    }
}
