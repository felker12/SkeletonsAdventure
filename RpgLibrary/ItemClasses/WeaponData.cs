
namespace RpgLibrary.ItemClasses
{
    public class WeaponData : ItemData
    {
        public Hands NumberHands {  get; set; }
        public int AttackValue {  get; set; }

        public WeaponData() : base()
        {
        }

        public WeaponData(ItemData itemData) : base(itemData)
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
