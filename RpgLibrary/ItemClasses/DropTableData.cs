
namespace RpgLibrary.ItemClasses
{
    public class DropTableData
    {
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
