using RpgLibrary.ItemClasses;

namespace SkeletonsAdventure.ItemClasses
{
    internal class Armor : EquipableItem
    {
        public ArmorLocation ArmorLocation { get; set; }
        public int DefenseValue { get; set; }

        public Armor(Armor armor) : base(armor)
        {
            ArmorLocation = armor.ArmorLocation;
            DefenseValue = armor.DefenseValue;
        }

        public Armor(ArmorData data) : base(data)
        {
            ArmorLocation = data.ArmorLocation;
            DefenseValue = data.DefenseValue;
        }

        public override Armor Clone()
        {
            return new Armor(this);
        }

        public override ArmorData ToData()
        {
            return new ArmorData
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
                ArmorLocation = ArmorLocation,
                DefenseValue = DefenseValue
            };
        }
    }
}
