using RpgLibrary.ItemClasses;

namespace SkeletonsAdventure.ItemClasses
{
    internal class Weapon : EquipableItem
    {
        public Hands NumberHands { get; set; }
        public int AttackValue { get; set; }

        public Weapon(Weapon item) : base(item)
        {
            NumberHands = item.NumberHands;
            AttackValue = item.AttackValue;
        }

        public Weapon(WeaponData data) : base(data)
        {
            NumberHands = data.NumberHands;
            AttackValue = data.AttackValue;
        }

        public override Weapon Clone()
        {
            return new Weapon(this);
        }

        public override WeaponData ToData()
        {
            return new(base.ToData())
            {
                NumberHands = NumberHands,
                AttackValue = AttackValue
            };
        }
    }
}
