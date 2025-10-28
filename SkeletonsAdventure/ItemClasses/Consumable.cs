using RpgLibrary.ItemClasses;
using System.Security.Cryptography.X509Certificates;

namespace SkeletonsAdventure.ItemClasses
{
    internal class Consumable : StackableItem
    {
        public bool IsConsumable { get; set; } = false;
        public ConsumableEffect Effect { get; set; }
        public int EffectDuration { get; set; }
        public int EffectBonus { get; set; }


        public Consumable(Consumable item) : base(item)
        {

            IsConsumable = item.IsConsumable;
            Effect = item.Effect;
            EffectBonus = item.EffectBonus;
            EffectDuration = item.EffectDuration;
        }


        public Consumable(ConsumableData data) : base(data)
        {
            IsConsumable = data.Consumable;
            Effect = data.Effect;
            EffectBonus = data.EffectBonus;
            EffectDuration = data.EffectDuration;

        }

        public override Consumable Clone()
        {
            return new(this);
        }

        public override ConsumableData ToData()
        {
            return new ConsumableData
            {
                Name = Name,
                Type = Type,
                Description = Description,
                Price = Price,
                Weight = Weight,
                Stackable = Stackable,
                Consumable = IsConsumable,
                Position = Position,
                Quantity = Quantity,
                SourceRectangle = SourceRectangle,
                TexturePath = TexturePath,
                Effect = Effect,
                EffectDuration = EffectDuration,
                EffectBonus = EffectBonus
            };
        }
    }
}
