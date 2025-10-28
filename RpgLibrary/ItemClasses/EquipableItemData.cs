
namespace RpgLibrary.ItemClasses
{
    public class EquipableItemData : ItemData
    {

        public EquipableItemData() : base() { }

        public EquipableItemData(EquipableItemData data) : base(data) 
        { 

        }

        public override EquipableItemData Clone()
        {
            return new EquipableItemData(this);
        }

        public override string ToString()
        {
            string toString = base.ToString();

            return toString;
        }
    }
}
