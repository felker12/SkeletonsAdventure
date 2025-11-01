using RpgLibrary.QuestClasses;

namespace RpgLibrary.ItemClasses
{
    public class EquipableItemData : ItemData
    {
        public LevelRequirementData LevelRequirementData { get; set; } = new();

        public EquipableItemData() : base() { }

        public EquipableItemData(EquipableItemData data) : base(data) 
        { 
            LevelRequirementData = data.LevelRequirementData;
        }

        public EquipableItemData(ItemData data) : base(data) 
        {
        }

        public override EquipableItemData Clone()
        {
            return new EquipableItemData(this);
        }

        public override string ToString()
        {
            string toString = base.ToString();
            toString += LevelRequirementData?.ToString();

            return toString;
        }
    }
}
