
namespace RpgLibrary.ItemClasses
{
    public class DropTableData
    {
        [Microsoft.Xna.Framework.Content.ContentSerializer(Optional = true)]
        public int? NoDropWeight { get; set; }
        public List<DropTableItemData> DropTableList { get; set; } = new List<DropTableItemData>();

        public DropTableData() { }

        public DropTableData(List<DropTableItemData> data)
        {
            DropTableList = data;
        }

        public override string ToString()
        {
            return string.Join(", ", DropTableList.Select(item => item.ToString()));
        }
    }
}
