using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgEditor
{
    public class RPG
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public RPG() { }
        public RPG(string name, string description)
        {
            Name = name;
            Description = description;
        }

    }
}
