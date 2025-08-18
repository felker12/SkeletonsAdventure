
namespace RpgLibrary.ItemClasses
{
    public class ConsumableData : ItemData
    {
        public ConsumableEffect Effect { get; set; }
        public int EffectDuration { get; set; }
        public int EffectBonus { get; set; }

        public ConsumableData() : base()
        {
        }

        public ConsumableData(ItemData itemData) : base(itemData)
        {
        }

        public ConsumableData(ConsumableData consumableData) : base(consumableData)
        {
            Effect = consumableData.Effect;
            EffectDuration = consumableData.EffectDuration;
            EffectBonus = consumableData.EffectBonus;
        }

        public override ConsumableData Clone()
        {
            return new(this);
        }

        public override string ToString()
        {
            string toString = base.ToString() + ", ";
            toString += Effect.ToString() + ", ";
            toString += EffectDuration.ToString() + ", ";
            toString += EffectBonus.ToString();
            return toString;
        }
    }
}
