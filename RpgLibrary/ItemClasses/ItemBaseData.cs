
namespace RpgLibrary.ItemClasses
{
    public class ItemBaseData
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;

        public ItemBaseData() { }

        public ItemBaseData(string name, int amount = 1)
        {
            Name = name;
            Quantity = amount;
        }

        public ItemBaseData(ItemData data)
        {
            Name = data.Name;
            Quantity = data.Quantity;
        }

        public override string ToString()
        {
            return $"Name: {Name}, " +
                $"Quantity: {Quantity}";
        }
    }
}
