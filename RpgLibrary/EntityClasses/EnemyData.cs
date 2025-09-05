
using RpgLibrary.ItemClasses;

namespace RpgLibrary.EntityClasses
{
    public class EnemyData : EntityData
    {
        public string DropTableName { get; set; } = string.Empty;
        public List<ItemData> GuaranteedItems { get; set; } = new();

        public EnemyData() { }

        public EnemyData(EnemyData data) : base(data)
        {
            GuaranteedItems = data.GuaranteedItems;
            DropTableName = data.DropTableName;
        }

        public EnemyData(EntityData data) : base(data) { }

        public override string ToString()
        {
            return base.ToString()+ ", " + DropTableName + ", " + string.Join(";", GuaranteedItems.Select(item => item.ToString()));
        }
    }
}
