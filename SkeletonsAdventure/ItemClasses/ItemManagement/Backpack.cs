namespace SkeletonsAdventure.ItemClasses.ItemManagement
{
    internal class Backpack() : ItemList
    {
        public int Capacity { get; } = 76;

        public override bool Add(GameItem item)
        {
            if (item is null) 
                return false;

            if (Count <= Capacity)
            {
                if (item.Stackable && ContainsItem(item)) //if the item is stackable and already exists in the backpack, increase the quantity of that item
                {
                    return AddItemToStack(item); //returns true if the item was added to an existing stack
                }
                else if (Count < Capacity) //dont add the item if it exceeds capacity
                {
                    Items.Add(item.Clone());
                    return true;
                }
            }

            return false;
        }
    }
}
