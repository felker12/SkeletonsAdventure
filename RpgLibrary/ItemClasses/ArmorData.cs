
namespace RpgLibrary.ItemClasses
{
    public class ArmorData : EquipableItemData
    {
        public ArmorLocation ArmorLocation {  get; set; }
        public int DefenseValue {  get; set; }

        public ArmorData() : base()
        {
        }

        public ArmorData(ArmorData armorData) : base(armorData)
        {
            ArmorLocation = armorData.ArmorLocation;
            DefenseValue = armorData.DefenseValue;
        }

        public ArmorData(EquipableItemData data) : base(data)
        {
        }

        public override ArmorData Clone()
        {
            return new(this);
        }

        public override string ToString()
        {
            string toString = base.ToString() + ", ";
            toString += ArmorLocation.ToString() + ", ";
            toString += DefenseValue.ToString();
            return toString;
        }
    }
}
