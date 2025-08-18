using RpgLibrary.ItemClasses;

namespace SkeletonsAdventure.ItemClasses
{
    internal class StackableItem : GameItem
    {


        public StackableItem(GameItem gameItem) : base(gameItem)
        {
        }

        public StackableItem(ItemData data) : base(data) //TODO use stackable item data
        {


        }




    }
}
