using Microsoft.Xna.Framework;
using RpgLibrary.ItemClasses;

namespace RpgLibrary.EntityClasses
{
    public class EntityData
    {
        public string Type { get; set; } = string.Empty;
        public int ID { get; set; } = 0;
        public int BaseHealth { get; set; }
        public int BaseDefence { get; set; }
        public int BaseAttack { get; set; }
        public int EntityLevel { get; set; }
        public int BaseXP { get; set; }
        public int CurrentHealth { get; set; }
        public Vector2? Position { get; set; }
        public Vector2? RespawnPosition { get; set; }
        public bool IsDead { get; set; } = false;
        public TimeSpan? LastDeathTime { get; set; } = new();
        public string DropTableName { get; set; } = string.Empty;
        public List<ItemData> GuaranteedItems { get; set; } = new();

        public EntityData() { }

        public EntityData(EntityData entityData)
        {
            ID = entityData.ID;
            Type = entityData.Type;
            BaseHealth = entityData.BaseHealth;
            BaseDefence = entityData.BaseDefence;
            BaseAttack = entityData.BaseAttack;
            EntityLevel = entityData.EntityLevel;
            BaseXP = entityData.BaseXP;
            Position = entityData.Position;
            RespawnPosition = entityData.RespawnPosition;
            CurrentHealth = entityData.CurrentHealth;
            IsDead = entityData.IsDead;
            DropTableName = entityData.DropTableName;
            LastDeathTime = entityData.LastDeathTime;
        }

        public virtual EntityData Clone()
        {
            return new(this);
        }

        public override string ToString()
        {
            string toString = string.Empty;
            toString += ID + ", ";
            toString += Type + ", ";
            toString += BaseHealth + ", ";
            toString += BaseDefence + ", ";
            toString += BaseAttack + ", ";
            toString += EntityLevel + ", ";
            toString += BaseXP + ", ";
            toString += Position + ", ";
            toString += RespawnPosition + ", ";
            toString += CurrentHealth + ", ";
            toString += IsDead + ", ";
            toString += LastDeathTime + ", ";
            toString += DropTableName + ", ";
            toString += string.Join(";", GuaranteedItems.Select(item => item.ToString()));

            return toString;
        }
    }
}
