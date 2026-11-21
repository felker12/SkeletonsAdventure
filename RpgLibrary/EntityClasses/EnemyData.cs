
using RpgLibrary.ItemClasses;

namespace RpgLibrary.EntityClasses
{
    public class EnemyData : EntityData
    {
        [Microsoft.Xna.Framework.Content.ContentSerializer(Optional = true)]
        public string? TextureName { get; set; }
        public string DropTableName { get; set; } = string.Empty;
        public List<ItemBaseData> GuaranteedItems { get; set; } = new();

        public EnemyData() { }

        public EnemyData(EnemyData data) : base(data)
        {
            TextureName = data.TextureName;
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
