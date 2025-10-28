using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using System.Linq;

namespace SkeletonsAdventure.Quests
{
    internal class QuestReward
    {
        public int XP { get; set; } = 0;
        public int Coins { get; set; } = 0;
        public List<GameItem> Items { get; set; } = [];

        public QuestReward()
        {
        }

        public QuestReward(QuestReward questReward)
        {
            XP = questReward.XP;
            Coins = questReward.Coins;
            Items = [.. questReward.Items.Select(i => new GameItem(i))];
        }

        public QuestReward(QuestRewardData data)
        {
            XP = data.XP;
            Coins = data.Coins;

            foreach (ItemData itemData in data.Items)
            {
                GameItem item = GameManager.LoadGameItemFromItemData(itemData);
                Items.Add(item); 
            }
        }

        public QuestReward Clone()
        {
            return new QuestReward(this);
        }

        public QuestRewardData GetQuestRewardData()
        {
            return new QuestRewardData
            {
                XP = XP,
                Coins = Coins,
                Items = [.. Items.Select(i => i.ToData())]
            };
        }

        public override string ToString()
        {
            return 
                $"XP: {XP}, " +
                $"Gold: {Coins}, " +
                $"Items: {string.Join(", ", Items.Select(i => i.Name))}"; // Assuming GameItem has a Name property
        }
    }
}
