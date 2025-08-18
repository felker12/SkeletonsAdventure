
using RpgLibrary.QuestClasses;
using RpgLibrary.EntityClasses;

namespace RpgLibrary.ItemClasses
{
    public class ItemDataManager
    {
        public Dictionary<string, ArmorData> ArmorData { get; set; } = new();
        public Dictionary<string, WeaponData> WeaponData { get; set; } = new();
        public Dictionary<string, ConsumableData> ConsumableData { get; set; } = new();
        public Dictionary<string, ItemData> ItemData { get; set; } = new();
        public Dictionary<string, QuestData> QuestData { get; set; } = new();
        public Dictionary<string, EntityData> EntityData { get; set; } = new();
    }
}
