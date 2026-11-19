using RpgLibrary.ItemClasses;
using SkeletonsAdventure.Entities.PlayerClasses;
using System.Linq;

namespace SkeletonsAdventure.ItemClasses.ItemManagement
{
    internal class DroppedLootManager
    {
        private readonly List<GameItem> _items = [];
        private readonly List<GameItem> _pending = [];
        public IReadOnlyList<GameItem> Items => _items;
        public GroundLootPlacement Placement { get; set; }

        public void Update()
        {
            PlacePendingDrops();

            foreach (GameItem item in Items)
                item.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (GameItem item in Items)
                item.Draw(spriteBatch);
        }

        public void Add(GameItem item, Vector2 pos)
        {
            if (item is null)
                return;
            Add([item], pos);
        }

        public void Add(List<GameItem> items, Vector2 pos)
        {
            var drops = items.Where(item => item is not null).Select(item => item.Clone()).ToList();
            if (drops.Count == 0)
                return;
            var positions = Placement?.Place(pos, drops.Count, _items.Select(item => item.Position));
            if (positions is { Count: 0 })
            {
                // Keep the origin for retries and save files until the drop can be reached.
                foreach (var drop in drops)
                {
                    drop.Position = pos;
                    _pending.Add(drop);
                }
                return;
            }

            for (int i = 0; i < drops.Count; i++)
                PlaceDrop(drops[i], positions is null ? pos : positions[i]);
        }

        public void RepairPlacement()
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                if (Placement is not null && !Placement.IsSafe(item.Position))
                {
                    _items.RemoveAt(i);
                    _pending.Add(item);
                    continue;
                }

                item.Update();
            }

            PlacePendingDrops();
        }

        private void PlacePendingDrops()
        {
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                var item = _pending[i];
                var positions = Placement?.Place(item.Position, 1, _items.Select(drop => drop.Position));
                if (positions is { Count: 0 })
                    return;

                PlaceDrop(item, positions is null ? item.Position : positions[0]);
                _pending.RemoveAt(i);
            }
        }

        private void PlaceDrop(GameItem item, Vector2 position)
        {
            item.Position = position;
            item.Update();
            _items.Add(item);
        }

        public void Add(ItemList lootList, Vector2 pos)
        {
            Add(lootList.Items, pos);
        }

        public void RestoreDrops(IEnumerable<GameItem> drops)
        {
            _items.Clear();
            _pending.Clear();
            _items.AddRange(drops);
        }

        public bool TryPickUp(GameItem item, Player player)
        {
            if (item is null || player is null || player.IsDead || player.Health <= 0)
                return false;

            int index = _items.IndexOf(item);
            if (index < 0 || !player.Backpack.Add(item))
                return false;

            _items.RemoveAt(index);
            return true;
        }

        public List<ItemData> GetDroppedItemData() =>
            _items.Concat(_pending).Select(item => item.ToData()).ToList();
    }
}
