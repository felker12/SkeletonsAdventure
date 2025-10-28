
namespace RpgLibrary.ItemClasses
{
    public class WeaponData : EquipableItemData
    {
        public Hands NumberHands {  get; set; }
        public int AttackValue {  get; set; }

        public WeaponData() : base()
        {
        }

        protected WeaponData(WeaponData weaponData) : base(weaponData)
        {
            NumberHands = weaponData.NumberHands;
            AttackValue = weaponData.AttackValue;
        }

        public override WeaponData Clone()
        {
            return new(this);
        }

        public override string ToString()
        {
            string toString = base.ToString() + ", ";
            toString += NumberHands.ToString() + ", ";
            toString += AttackValue.ToString();
            return toString;
        }
    }
}
