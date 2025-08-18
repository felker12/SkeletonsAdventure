using MonoGame.Extended;
using RpgLibrary.ItemClasses;

namespace SkeletonsAdventure.ItemClasses
{
    internal class EquipableItem : GameItem
    {
        public bool Equipped { get; set; } = false;

        public EquipableItem(EquipableItem item) : base(item)
        {
            Equipped = item.Equipped;
        }

        public EquipableItem(ItemData data) : base(data)//TODO use equipable item data
        {
            Equipped = data.Equipped;
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

        public override ItemData GetData()
        {
            return base.GetData();
        }

    }
}
