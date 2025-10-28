using MonoGame.Extended;
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.Quests;

namespace SkeletonsAdventure.ItemClasses
{
    internal class EquipableItem : GameItem
    {
        public bool Equipped { get; set; } = false;
        public LevelRequirements LevelRequirements { get; set; } = null;

        public EquipableItem(EquipableItem item) : base(item)
        {
            Equipped = item.Equipped;
            LevelRequirements = item.LevelRequirements;
        }

        public EquipableItem(EquipableItemData data) : base(data)
        {
            Equipped = data.Equipped;
            LevelRequirements = new(data.LevelRequirementData);
        }
        public void SetEquipped(bool equipped)
        {
            Equipped = equipped;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Image, Position, SourceRectangle, Color.White);

            //TODO
            if (Equipped == true)
                spriteBatch.DrawRectangle(ItemRectangle, Color.MediumVioletRed, 2, 0);
            else
                spriteBatch.DrawRectangle(ItemRectangle, Color.WhiteSmoke, 1, 0);

            if (ToolTip.Visible)
                ToolTip.Draw(spriteBatch);
        }

        public override EquipableItem Clone()
        {
            return new(this);
        }

        public override EquipableItemData ToData()
        {
            return new(base.ToData())
            {
                LevelRequirementData = LevelRequirements.ToData(),
            };
        }
    }
}
