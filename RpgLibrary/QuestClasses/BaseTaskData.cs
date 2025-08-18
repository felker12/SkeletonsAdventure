using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgLibrary.QuestClasses
{
    public class BaseTaskData
    {
        public int RequiredAmount { get; set; } = 1;
        public int CompletedAmount { get; set; } = 0;
        public string TaskToComplete { get; set; } = string.Empty;


        public BaseTaskData() { }

        public BaseTaskData(BaseTaskData data)
        {
            RequiredAmount = data.RequiredAmount;
            CompletedAmount = data.CompletedAmount;
            TaskToComplete = data.TaskToComplete;
        }

        public virtual BaseTaskData Clone()
        {
            return new BaseTaskData(this);
        }

        public override string ToString()
        {
            return $"{CompletedAmount} out of {RequiredAmount} completed for task: {TaskToComplete}";
        }
    }
}
