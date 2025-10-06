
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
