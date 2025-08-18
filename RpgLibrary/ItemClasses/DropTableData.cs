using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgLibrary.ItemClasses
{
    public class DropTableData
    {
        public List<DropTableItemData> DropTableList { get; set; } = new List<DropTableItemData>();

        public DropTableData() { }

        public override string ToString()
        {
            return string.Join(", ", DropTableList.Select(item => item.ToString()));
        }
    }
}
