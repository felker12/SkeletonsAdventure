using RpgLibrary.QuestClasses;
using SkeletonsAdventure.Entities;
using System.Linq;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Quests
{
    internal class Quest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public bool Active { get; set; } = false;
        public LevelRequirements Requirements { get; set; } = new();
        public QuestReward Reward { get; set; } = new();
        public List<string> RequiredQuestNames { get; set; } = [];
        public List<BaseTask> Tasks { get; set; } = [];
        public BaseTask ActiveTask => GetActiveTask();
        public string QuestInfo => GetQuestInfo();
        public string QuestDetails => GetQuestDetails();

        public Quest() { }

        public Quest(Quest quest)
        {
            Name = quest.Name;
            Description = quest.Description;
            IsCompleted = quest.IsCompleted;
            Active = quest.Active;
            Requirements = quest.Requirements;
            Reward = quest.Reward.Clone();
            RequiredQuestNames = quest.RequiredQuestNames;
            Tasks = quest.Tasks;
        }

        public Quest(QuestData data)
        {
            Name = data.Name;
            Description = data.Description;
            IsCompleted = data.IsCompleted;
            Active = data.Active;
            Requirements = new LevelRequirements(data.RequirementData);
            Reward = new QuestReward(data.RewardData);

            RequiredQuestNames = [.. data.RequiredQuestNameData.Select(q => q)];

            //TODO  type check the base task to see what type of task it is
            foreach (var taskData in data.BaseTasksData)
            {
                if(taskData is SlayTaskData slayTaskData)
                {
                    Tasks.Add(new SlayTask(slayTaskData));
                }
                //TODO: Uncomment and implement other task types as needed
                //else if (taskData is CollectTaskData collectTaskData)
                //{
                //    Tasks.Add(new CollectTask(collectTaskData));
                //}
                //else if (taskData is TalkToTaskData talkToTaskData)
                //{
                //    Tasks.Add(new TalkToTask(talkToTaskData));
                //}
                else if (taskData is not null)
                {
                    Tasks.Add(new BaseTask(taskData));
                }
            }
        }

        public Quest Clone()
        {
            return new Quest(this);
        }

        public void CheckTasksCompleted()
        {
            foreach (var task in Tasks)
            {
                if (!task.IsCompleted)
                    return; // If any task is not completed, the quest is not completed
            }
            CompleteQuest();
        }

        public void ProgressTask()//Can only progress the active task
        {
            BaseTask task = ActiveTask;
            task.ProgressTask();
            World.AddMessage(task.Progress);

            if (task.IsCompleted)
            {
                // If the task is completed, check if all tasks are completed
                CheckTasksCompleted();
                World.AddMessage($"Task: '{task.TaskToComplete}' completed for quest '{Name}'.");
            }
        }

        public bool PlayerRequirementsMet(Player player)
        {
            if (Requirements != null)
                return Requirements.CheckRequirements(player);
            else
                return true; // No requirements means the quest is available
        }

        public void StartQuest()
        {
            Active = true;
        }

        public void CompleteQuest()
        {
            IsCompleted = true;
            Active = false;
            World.AddMessage($"Quest {Name} Completed!");
            World.AddMessage($"Rewards gained: {Reward}");
        }

        public bool CheckCompletedQuest(Player player)
        {
            foreach (Quest completedQuest in player.CompletedQuests)
            {
                if (completedQuest.Name == Name)
                    return true;
            }
            return false;
        }

        public bool CheckActiveQuest(Player player)
        {
            foreach (Quest activeQuest in player.ActiveQuests)
            {
                if (activeQuest.Name == Name)
                    return true;
            }
            return false;
        }

        public QuestData GetQuestData()
        {
            return new QuestData
            {
                Name = Name,
                Description = Description,
                IsCompleted = IsCompleted,
                Active = Active,
                RequirementData = Requirements.ToData(),
                RewardData = Reward.GetQuestRewardData(),
                RequiredQuestNameData = RequiredQuestNames,
                BaseTasksData = GetBaseTaskDatas(),
            };
        }

        public BaseTask GetActiveTask()
        {
            if (Active == false)
                return null; //only have an active task if the quest is active

            return Tasks.FirstOrDefault(t => !t.IsCompleted);
        }

        public override string ToString()
        {
            return $"Name: {Name}, " +
                   $"\nDescription: {Description}, " +
                   $"\nIsCompleted: {IsCompleted}, " +
                   $"\nActive: {Active}, " +
                   $"\nRequirements: {Requirements}, " +
                   $"\nRewards: {Reward}, " +
                   $"\nRequired Quests: {RequiredQuestsToString()}" +
                   $"\nTasks: {TasksToString()}";
        }

        private string GetQuestInfo()
        {
            string info = $"Name: {Name}, " +
                   $"\nDescription: {Description}, " +
                   $"\nIsCompleted: {IsCompleted}, " +
                   $"\nActive: {Active}, " +
                   $"\nRequirements: {Requirements}, " +
                   $"\nRewards: {Reward}, " +
                   $"\nRequired Quests: {RequiredQuestsToString()}";

            if (ActiveTask != null)
                info += $"\nActive Task: {ActiveTask}";

            return info;
        }

        private string GetQuestDetails()
        {
            string info = $"Name: {Name} ";

            if (ActiveTask != null)
                info += $"\n{ActiveTask}";

            return info;
        }

        public List<BaseTaskData> GetBaseTaskDatas()
        {
            List<BaseTaskData> taskDataList = [];

            foreach (var task in Tasks)
            {
                taskDataList.Add(task.GetBaseTaskData());
            }

            return taskDataList;
        }

        public string RequiredQuestsToString()
        {
            string requiredQuests = string.Empty;

            if (RequiredQuestNames == null || RequiredQuestNames.Count == 0)
                return "No required quests.";

            return string.Join(", ", RequiredQuestNames.Select(q => q));
        }

        public string TasksToString()
        {
            if (Tasks == null || Tasks.Count == 0)
                return "No tasks.";

            return string.Join(",\n", Tasks.Select(t => t.ToString()));
        }
    }
}
