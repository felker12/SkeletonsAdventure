namespace RpgLibrary.QuestClasses
{
    public class QuestData
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public bool Active { get; set; } = false;
        public LevelRequirementData RequirementData { get; set; } = new();
        public QuestRewardData RewardData { get; set; } = new();
        public List<string> RequiredQuestNameData { get; set; } = new();
        public List<BaseTaskData> BaseTasksData { get; set; } = new();

        public QuestData() { }

        public QuestData(QuestData data)
        {
            Name = data.Name;
            Description = data.Description;
            IsCompleted = data.IsCompleted;
            RequirementData = data.RequirementData;
            RewardData = data.RewardData;
            RequiredQuestNameData = data.RequiredQuestNameData;
            BaseTasksData = data.BaseTasksData;
        }

        public QuestData Clone()
        {
            return new QuestData(this);
        }

        public override string ToString()
        {
            return $"Name: {Name}, " +
                $"Description: {Description}, " +
                $"IsCompleted: {IsCompleted}, " +
                $"Requirements: {RequirementData}, " +
                $"Rewards: {RewardData}, " + 
                $"Required Quests: {RequiredQuestsToString()}," +
                $"Tasks: {TasksToString()}"; 
        }

        public string TasksToString()
        {
            if (BaseTasksData == null || BaseTasksData.Count == 0)
                return "No tasks.";

            return string.Join(", ", BaseTasksData.Select(t => t.ToString()));
        }

        public string RequiredQuestsToString()
        {
            string requiredQuests = string.Empty;

            if (RequiredQuestNameData == null || RequiredQuestNameData.Count == 0)
                return "No required quests.";

            return string.Join(", ", RequiredQuestNameData.Select(q => q));
        }
    }
}
