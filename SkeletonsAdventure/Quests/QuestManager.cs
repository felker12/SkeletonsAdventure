using RpgLibrary.QuestClasses;
using SkeletonsAdventure.GameWorld;
using System.Linq;

namespace SkeletonsAdventure.Quests
{
    internal class QuestManager
    {
        private static Dictionary<string, Quest> Quests = GameManager.QuestsClone;

        internal QuestManager() { }

        public static void StartQuest(Quest quest)
        {
            quest.StartQuest();
        }

        public static void CompleteQuest(Quest quest)
        { 
            quest.CompleteQuest();
        }

        public static QuestManagerData GetQuestManagerData()
        {
            return new QuestManagerData
            {
                QuestsData = [.. Quests.Values.Select(q => q.GetQuestData())]
            };
        }

        public static void LoadQuestManagerData(QuestManagerData data)
        {
            Quests = data.QuestsData.ToDictionary(q => q.Name, q => new Quest(q));
        }

        public override string ToString()
        {
            return $"Quests: {string.Join(", ", Quests.Select(q => q.Value.Name))}";
        }
    }
}
