using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgLibrary.MenuClasses
{
    public class TabbedMenuData : MenuData
    {
        public string ActiveMenu { get; set; } = string.Empty;
        public List<MenuData> MenuDatas { get; set; } = new();

        public TabbedMenuData() : base()
        {
        }

        public override string ToString()
        {
            string toString = base.ToString();
            toString += $"Active: {ActiveMenu}, ";

            return toString;
        }
    }
}
