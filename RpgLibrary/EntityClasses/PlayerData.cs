using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;

namespace RpgLibrary.EntityClasses
{
    public class PlayerData : EntityData
    {
        public int totalXP = 0, 
            baseMana = 0,
            mana = 0,
            maxMana = 0,
            attributePoints = 0,
            bonusAttackFromAttributePoints = 0, 
            bonusDefenceFromAttributePoints = 0, 
            bonusHealthFromAttributePoints = 0,
            bonusManaFromAttributePoints;
        public List<ItemData> backpack = new();
        public List<QuestData> activeQuests = new();
        public List<QuestData> completedQuests = new();
        public string displayQuestName = string.Empty;

        public PlayerData() { }

        public PlayerData(PlayerData entityData) : base(entityData)
        {
            totalXP = entityData.totalXP;
            baseMana = entityData.baseMana;
            mana = entityData.mana;
            maxMana = entityData.maxMana;
            attributePoints = entityData.attributePoints;
            bonusAttackFromAttributePoints = entityData.bonusAttackFromAttributePoints;
            bonusDefenceFromAttributePoints = entityData.bonusDefenceFromAttributePoints;
            bonusHealthFromAttributePoints = entityData.bonusHealthFromAttributePoints;
            bonusManaFromAttributePoints = entityData.bonusManaFromAttributePoints;
            backpack = entityData.backpack;
            activeQuests = entityData.activeQuests;
            completedQuests = entityData.completedQuests;
            displayQuestName = entityData.displayQuestName;
        }

        public PlayerData(EntityData entityData) : base(entityData)
        {
        }

        public override string ToString()
        {
            string toString = base.ToString() + ", ";
            toString += totalXP + ", ";
            toString += baseMana + ", ";
            toString += mana + ", ";
            toString += maxMana + ", ";
            toString += attributePoints + ", ";
            toString += bonusAttackFromAttributePoints + ", ";
            toString += bonusDefenceFromAttributePoints + ", ";
            toString += bonusHealthFromAttributePoints + ", ";
            toString += bonusManaFromAttributePoints + ", ";
            //toString += string.Join(";", backpack.Select(item => item.ToString())) + ", ";
            //toString += string.Join(";", activeQuests.Select(quest => quest.ToString())) + ", ";
            //toString += string.Join(";", completedQuests.Select(quest => quest.ToString())) + ", ";
            toString += displayQuestName;
            return toString;
        }
    }
}
