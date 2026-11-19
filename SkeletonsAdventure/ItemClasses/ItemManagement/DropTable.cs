using RpgLibrary.DataClasses;
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.HelperClasses;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SkeletonsAdventure.ItemClasses.ItemManagement
{
    public class DropTable : ICloneableGameClass<DropTable>
    {
        public IReadOnlyDictionary<string, DropTableItem> DropTableDictionary { get; }
        public int Count => DropTableDictionary.Count;
        public int NoDropWeight { get; }
        private readonly long _itemWeight;
        private readonly Random _random;

        public DropTable() : this([], null, Random.Shared) { }
        public DropTable(DropTable table) : this(table.DropTableDictionary.Values, table.NoDropWeight, table._random) { }
        public DropTable(List<DropTableItem> items) : this(items, null, Random.Shared) { }
        public DropTable(Dictionary<string, DropTableItem> items) : this(items.Values, null, Random.Shared) { }
        public DropTable(DropTableData data) : this(data.DropTableList.Select(item => new DropTableItem(item)), data.NoDropWeight, Random.Shared) { }
        public DropTable(IEnumerable<DropTableItem> items, int noDropWeight) : this(items, noDropWeight, Random.Shared) { }

        // Missing no-drop weights retain the original percentage-based data format.
        internal DropTable(IEnumerable<DropTableItem> items, int? noDropWeight, Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            Dictionary<string, DropTableItem> entries = [];
            foreach (var entry in items)
            {
                if (entry is null || string.IsNullOrWhiteSpace(entry.ItemName) || entry.DropChance < 0 ||
                    entry.MinQuantity < 1 || entry.MaxQuantity < entry.MinQuantity)
                    throw new InvalidDataException("Invalid loot entry or quantity range.");
                var template = GameManager.GetItemByName(entry.ItemName)
                    ?? throw new InvalidDataException($"Unknown loot item: {entry.ItemName}");
                if (!template.Stackable && (entry.MinQuantity != 1 || entry.MaxQuantity != 1))
                    throw new InvalidDataException($"Non-stackable loot must have quantity one: {entry.ItemName}");
                if (!entries.TryAdd(entry.ItemName, entry.Clone()))
                    throw new InvalidDataException($"Duplicate loot item: {entry.ItemName}");
                _itemWeight += entry.DropChance;
            }
            if (noDropWeight is null && _itemWeight > 100)
                throw new InvalidDataException("Legacy loot percentages exceed 100. Specify an explicit no-drop weight for weighted tables.");
            NoDropWeight = noDropWeight ?? 100 - (int)_itemWeight;
            if (NoDropWeight < 0)
                throw new InvalidDataException("The no-drop weight cannot be negative.");
            DropTableDictionary = new ReadOnlyDictionary<string, DropTableItem>(entries);
        }

        public DropTableData ToData() => new()
        {
            NoDropWeight = NoDropWeight,
            DropTableList = DropTableDictionary.Values.Select(entry => entry.ToData()).ToList()
        };
        public DropTable Clone() => new(this);

        // Each enemy roll makes one selection, including the explicit no-drop outcome.
        public GameItem GetDrop()
        {
            long total = _itemWeight + NoDropWeight;
            if (total == 0)
                return null;
            long roll = _random.NextInt64(total);
            foreach (var entry in DropTableDictionary.Values)
            {
                if (roll < entry.DropChance)
                    return CreateItem(entry);
                roll -= entry.DropChance;
            }
            return null;
        }

        public List<GameItem> GetDropsList(int amount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(amount);
            List<GameItem> items = [];
            for (int i = 0; i < amount; i++)
            {
                var item = GetDrop();
                if (item is not null)
                    items.Add(item);
            }
            return items;
        }
        public GameItem[] GetDrops(int amount) => GetDropsList(amount).ToArray();

        // Chests ignore the no-drop outcome and sample without replacement.
        public List<GameItem> GetUniqueDropsList(int amount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(amount);
            var available = DropTableDictionary.Values.Where(entry => entry.DropChance > 0).ToList();
            if (amount > 0 && available.Count == 0)
                throw new InvalidOperationException("A guaranteed loot table needs at least one positive-weight item.");
            int count = Math.Min(amount, available.Count);
            List<GameItem> items = [];
            long remainingWeight = _itemWeight;
            for (int i = 0; i < count; i++)
            {
                long roll = _random.NextInt64(remainingWeight);
                for (int index = 0; index < available.Count; index++)
                {
                    var entry = available[index];
                    if (roll < entry.DropChance)
                    {
                        items.Add(CreateItem(entry));
                        remainingWeight -= entry.DropChance;
                        available.RemoveAt(index);
                        break;
                    }
                    roll -= entry.DropChance;
                }
            }
            return items;
        }

        private GameItem CreateItem(DropTableItem entry)
        {
            var item = GameManager.GetItemByName(entry.ItemName)
                ?? throw new InvalidDataException($"Unknown loot item: {entry.ItemName}");
            if (item.Stackable)
                item.SetQuantity(RandomInclusive(entry.MinQuantity, entry.MaxQuantity));
            if (item is EquipableItem equipment)
                equipment.SetEquipped(false);
            return item;
        }

        private int RandomInclusive(int min, int max)
        {
            if (min < 0 || max < min)
                throw new ArgumentOutOfRangeException(nameof(min));
            if (min == max)
                return min;
            return (int)(min + _random.NextInt64((long)max - min + 1));
        }
        public List<GameItem> GetRandomAmountOfDrops(int min, int max) => GetDropsList(RandomInclusive(min, max));
        public List<GameItem> GetRandomAmountOfUniqueDrops(int min, int max) => GetUniqueDropsList(RandomInclusive(min, max));
        public List<GameItem> GetRandomAmountOfDrops(MinMaxPair range) => GetRandomAmountOfDrops(range.Min, range.Max);
        public List<GameItem> GetRandomAmountOfUniqueDrops(MinMaxPair range) => GetRandomAmountOfUniqueDrops(range.Min, range.Max);
        public override string ToString() => $"{string.Join(", ", DropTableDictionary.Values)}; No drop weight: {NoDropWeight}";
    }
}
