using Microsoft.Xna.Framework.Graphics;
using RpgLibrary.DataClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;

namespace RpgLibrary.EntityClasses
{
    // Defines the different evolution stages a player can have
    public enum PlayerEvolutionType
    {
        Skeleton, //Base form
        ArmoredSkeleton, //2nd evolution
    }

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
        public PlayerEvolutionType EvolutionType = PlayerEvolutionType.Skeleton;
        public string TextureName = string.Empty;
        public List<ItemData> backpack = new();
        public List<QuestData> activeQuests = new();
        public List<QuestData> completedQuests = new();
        public string displayQuestName = string.Empty;
        public KillCounterData killCounter = new();

        public PlayerData() { }

        public PlayerData(PlayerData playerData) : base(playerData)
        {
            totalXP = playerData.totalXP;
            baseMana = playerData.baseMana;
            mana = playerData.mana;
            maxMana = playerData.maxMana;
            attributePoints = playerData.attributePoints;
            bonusAttackFromAttributePoints = playerData.bonusAttackFromAttributePoints;
            bonusDefenceFromAttributePoints = playerData.bonusDefenceFromAttributePoints;
            bonusHealthFromAttributePoints = playerData.bonusHealthFromAttributePoints;
            bonusManaFromAttributePoints = playerData.bonusManaFromAttributePoints;
            backpack = playerData.backpack;
            activeQuests = playerData.activeQuests;
            completedQuests = playerData.completedQuests;
            displayQuestName = playerData.displayQuestName;
            killCounter = playerData.killCounter;
            EvolutionType = playerData.EvolutionType;
            TextureName = playerData.TextureName;
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
            toString += EvolutionType.ToString() + ", ";
            //toString += string.Join(";", backpack.Select(item => item.ToString())) + ", ";
            //toString += string.Join(";", activeQuests.Select(quest => quest.ToString())) + ", ";
            //toString += string.Join(";", completedQuests.Select(quest => quest.ToString())) + ", ";
            toString += displayQuestName;
            return toString;
        }
    }
}
