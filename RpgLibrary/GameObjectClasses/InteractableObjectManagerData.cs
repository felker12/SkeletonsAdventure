using System.Text;

namespace RpgLibrary.GameObjectClasses
{
    public class InteractableObjectManagerData
    {
        public List<InteractableObjectData> InteractableObjectsData { get; set; } = new();  

        public InteractableObjectManagerData() { }

        public InteractableObjectManagerData(InteractableObjectManagerData interactableObjectManagerData)
        {
            foreach (var obj in interactableObjectManagerData.InteractableObjectsData)
            {
                InteractableObjectsData.Add(obj.Clone());
            }
        }

        public InteractableObjectManagerData Clone()
        {
            return new InteractableObjectManagerData(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine("InteractableObjectManagerData: ");
            foreach (var obj in InteractableObjectsData)
            {
                sb.AppendLine(obj.ToString() + ", ");
            }
            return sb.ToString();
        }
    }
}
