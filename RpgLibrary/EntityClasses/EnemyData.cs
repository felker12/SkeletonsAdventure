using RpgLibrary.ItemClasses;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgLibrary.EntityClasses
{
    public class EnemyData : EntityData
    {
        public EnemyData()
        {

        }

        public EnemyData(EnemyData data) : base(data)
        {
            GuaranteedItems = data.GuaranteedItems;
            DropTableName = data.DropTableName;
        }

        public EnemyData(EntityData data) : base(data)
        {
        }

        public override string ToString()
        {
            return base.ToString() + 
                $"{DropTableName}, " +
                $"";
        }
    }
}
