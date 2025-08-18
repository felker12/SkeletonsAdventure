using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgLibrary.GameObjectClasses
{
    public class QuestNodeData : InteractableObjectData
    {

        public QuestNodeData() : base() { }

        public QuestNodeData(QuestNodeData data) : base(data)
        {

        }

        public override QuestNodeData Clone()
        {
            return new(this);
        }
    }
}
