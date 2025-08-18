using RpgLibrary.ItemClasses;

namespace RpgLibrary.QuestClasses
{
    public class QuestRewardData
    {
        public int XP { get; set; } = 0;
        public int Coins { get; set; } = 0;
        public List<ItemData> Items { get; set; } = new();

        public QuestRewardData() { }

        public QuestRewardData(QuestRewardData data)
        {
            XP = data.XP;
            Coins = data.Coins;
            Items = data.Items;
        }

        public QuestRewardData Clone()
        {
            return new QuestRewardData(this);
        }

        public override string ToString()
        {
            return
                $"XP: {XP}, " +
                $"Gold: {Coins}, " +
                $"Items: {string.Join(", ", Items)}";
        }
    }
}
