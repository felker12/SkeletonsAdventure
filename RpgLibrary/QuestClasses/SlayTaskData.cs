using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgLibrary.QuestClasses
{
    public class SlayTaskData : BaseTaskData
    {
        public string MonsterToSlay { get; set; } = string.Empty;

        public SlayTaskData() { }

        public SlayTaskData(SlayTaskData data) : base(data)
        {
            MonsterToSlay = data.MonsterToSlay;
        }

        public override BaseTaskData Clone()
        {
            return new SlayTaskData(this);
        }

        public override string ToString()
        {
            return $"{base.ToString()} for monster: {MonsterToSlay}";
        }
    }
}
