using RpgLibrary.QuestClasses;

namespace RpgLibrary.EntityClasses
{
    public class NPCData
    {
        public List<QuestData> QuestDatas { get; set; } = new();

        public NPCData() { }

        public NPCData(NPCData npcData)
        {
            QuestDatas = npcData.QuestDatas;
        }

        public NPCData Clone()
        {
            return new NPCData(this);
        }

        public override string ToString()
        {
            string ToString = string.Empty;

            foreach (var quest in QuestDatas)
                ToString += quest.ToString() + Environment.NewLine;

            return ToString;
        }
    }
}
