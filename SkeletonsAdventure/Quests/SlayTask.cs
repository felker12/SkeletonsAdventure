using RpgLibrary.QuestClasses;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Quests
{
    internal class SlayTask : BaseTask
    {
        public string MonsterToSlay { get; set; } = string.Empty;
        public Entity EntityToSlay { get; set; } = null!;

        public SlayTask() { }

        public SlayTask(SlayTask task) : base(task)
        {
            MonsterToSlay = task.MonsterToSlay;
            SetMonsterToSlay();
        }

        public SlayTask(SlayTaskData taskata) : base(taskata)
        {
            MonsterToSlay = taskata.MonsterToSlay; 
            SetMonsterToSlay();
        }

        public override SlayTaskData GetBaseTaskData()
        {
            return new()
            {
                RequiredAmount = RequiredAmount,
                CompletedAmount = CompletedAmount,
                TaskToComplete = TaskToComplete,
                MonsterToSlay = MonsterToSlay,
            };
        }

        public override SlayTask Clone()
        {
            return new SlayTask(this);
        }

        public Entity GetEntityToSlay()
        {
            if(EntityToSlay is null)
            {
                if(MonsterToSlay is null)
                    throw new InvalidOperationException("MonsterToSlay must be set before calling GetEntityToSlay.");
                else
                    SetMonsterToSlay();
            }
            
            return EntityToSlay;
        }

        public void SetMonsterToSlay()
        {
            if (string.IsNullOrEmpty(MonsterToSlay) is false)
            {
                EntityToSlay = GameManager.EnemiesClone[MonsterToSlay];
            }
            else
            {
                throw new InvalidOperationException("MonsterToSlay must be set before calling SetMonsterToSlay.");
            }
        }
    }
}
