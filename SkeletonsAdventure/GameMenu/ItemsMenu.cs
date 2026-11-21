using MonoGame.Extended;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using System.Linq;

namespace SkeletonsAdventure.GameMenu
{
    internal sealed class ItemsMenu : BaseMenu
    {
        private const int Padding = 24;
        private const int RowHeight = 38;
        private const int RowStep = 43;
        private const int MaxRows = 8;
        private readonly MenuButton[] _itemButtons = new MenuButton[MaxRows * 2];
        private readonly MenuButton _equip, _previous, _next;
        private readonly Label _pageInfo, _itemName, _itemDetails, _stats, _equipped;
        private readonly SpriteFont _font, _headingFont;
        private Player _player;
        private GameItem _selected;
        private int _page, _rows = MaxRows, _detailsWidth;

        private List<GameItem> Items => _player.Backpack.Items;
        private int PageSize => _rows * 2;
        private int LastPage => Math.Max(0, (Items.Count - 1) / PageSize);

        public ItemsMenu(Texture2D pixel, SpriteFont font, SpriteFont headingFont) : base(pixel, MenuStyle.Panel)
        {
            Title = "Items";
            Visible = true;
            _font = font;
            _headingFont = headingFont;
            for (int i = 0; i < _itemButtons.Length; i++)
            {
                int slot = i;
                _itemButtons[i] = new MenuButton(pixel, font, "", () => SelectItem(Items[_page * PageSize + slot]))
                    { Enabled = false, Visible = false, HasFocus = i == 0 };
                ControlManager.Add(_itemButtons[i]);
            }

            _equip = new(pixel, font, "Equip", () => ToggleSelectedEquipment(), primary: true);
            _previous = new(pixel, font, "Previous", () => ChangePage(-1));
            _next = new(pixel, font, "Next", () => ChangePage(1));
            _pageInfo = new(font) { TextColor = MenuStyle.MutedText };
            _itemName = new(headingFont) { TextColor = MenuStyle.Ivory };
            _itemDetails = new(font) { TextColor = MenuStyle.MutedText };
            _stats = new(font) { TextColor = MenuStyle.Ivory };
            _equipped = new(font) { TextColor = MenuStyle.Ivory };
            ControlManager.AddRange([_equip, _previous, _next, _pageInfo, _itemName, _itemDetails, _stats, _equipped]);
        }

        public override void MenuOpened()
        {
            _player = World.Player;
            _page = Math.Min(_page, LastPage);
            if (!Items.Contains(_selected))
                _selected = Items.ElementAtOrDefault(_page * PageSize);
            Refresh();
        }

        public override void Arrange(Rectangle bounds)
        {
            base.Arrange(bounds);
            _rows = Math.Clamp((bounds.Height - 106) / RowStep, 1, MaxRows);
            int x = bounds.X + Padding;
            int y = bounds.Y + Padding;
            int columnWidth = Math.Max(44, (bounds.Width - Padding * 3) / 4);
            for (int slot = 0; slot < _itemButtons.Length; slot++)
            {
                int column = slot / _rows;
                int row = slot % _rows;
                _itemButtons[slot].Arrange(new(x + column * columnWidth, y + 32 + row * RowStep, columnWidth - 6, RowHeight));
            }

            int pagerY = y + 32 + _rows * RowStep + 6;
            _previous.Arrange(new(x, pagerY, 104, 32));
            _next.Arrange(new(x + 112, pagerY, 82, 32));
            _pageInfo.Position = new(x + 208, pagerY + 7);

            int detailsX = x + columnWidth * 2 + Padding;
            _detailsWidth = Math.Max(1, bounds.Right - Padding - detailsX);
            _itemName.Position = new(detailsX, y + 32);
            _itemDetails.Position = new(detailsX, y + 64);
            _equip.Arrange(new(detailsX, y + 176, 124, 36));
            _stats.Position = new(detailsX, y + 266);
            _equipped.Position = new(detailsX + _detailsWidth / 2, y + 266);
            if (_player is not null)
            {
                _page = _selected is null ? Math.Min(_page, LastPage) : Math.Max(0, Items.IndexOf(_selected)) / PageSize;
                Refresh();
            }
        }

        internal void SelectItem(GameItem item)
        {
            if (!Items.Contains(item))
                return;
            _selected = item;
            Refresh();
        }

        internal bool ToggleSelectedEquipment()
        {
            if (_selected is not EquipableItem equipment || !Items.Contains(equipment))
                return false;

            bool wasEquipped = equipment.Equipped;
            if (wasEquipped)
                _player.EquippedItems.TryUnequipItem(equipment);
            else
                _player.EquippedItems.TryEquipItem(equipment);

            _player.RefreshEquipmentStats();
            Refresh();
            return equipment.Equipped != wasEquipped;
        }

        private void ChangePage(int change)
        {
            _page = Math.Clamp(_page + change, 0, LastPage);
            _selected = Items.ElementAtOrDefault(_page * PageSize);
            Refresh();
        }

        private void Refresh()
        {
            for (int slot = 0; slot < _itemButtons.Length; slot++)
            {
                var button = _itemButtons[slot];
                var item = slot < PageSize ? Items.ElementAtOrDefault(_page * PageSize + slot) : null;
                button.Enabled = button.Visible = item is not null;
                if (item is not null)
                    button.Text = Fit(item.Name, _font, button.Width - 58);
            }

            _previous.Enabled = _page > 0;
            _next.Enabled = _page < LastPage;
            _pageInfo.Text = $"Page {_page + 1} of {LastPage + 1}";
            _itemName.Text = _selected is null ? "Backpack is empty" : Fit(_selected.Name, _headingFont, _detailsWidth);

            var equipment = _selected as EquipableItem;
            _equip.Visible = equipment is not null;
            _equip.Enabled = equipment is not null && (equipment.Equipped || _player.EquippedItems.CheckRequirements(equipment));
            _equip.Text = equipment?.Equipped == true ? "Unequip" : "Equip";
            _itemDetails.Text = _selected switch
            {
                Weapon weapon => $"Weapon\nAttack +{weapon.AttackValue}",
                Shield shield => $"Shield\nDefence +{shield.DefenceValue}",
                Armor armor => $"{armor.ArmorLocation} armour\nDefence +{armor.DefenseValue}",
                Consumable item => $"Consumable\nQuantity  {item.Quantity}\nUse during gameplay",
                GameItem item => $"{item.Type}\nQuantity  {item.Quantity}",
                _ => ""
            };
            if (equipment is not null && !_equip.Enabled)
                _itemDetails.Text += $"\n{Fit(equipment.LevelRequirements.MissingRequirementsText(_player), _font, _detailsWidth)}";

            _stats.Text = $"Attack  {_player.Attack}\nDefence  {_player.Defence}\n" +
                $"Health  {_player.Health}/{_player.MaxHealth}\nMana  {_player.Mana}/{_player.MaxMana}";
            var names = Items.OfType<EquipableItem>().Where(item => item.Equipped)
                .Select(item => Fit(item.Name, _font, _detailsWidth / 2));
            _equipped.Text = string.Join("\n", names.DefaultIfEmpty("Nothing equipped"));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible || _player is null)
                return;

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(Texture, Rectangle, TintColor);
            spriteBatch.DrawString(_headingFont, $"Backpack  {Items.Count}/{_player.Backpack.Capacity}", Position + new Vector2(Padding), MenuStyle.Brass);
            spriteBatch.DrawString(_headingFont, "Selected item", _itemName.Position - new Vector2(0, 32), MenuStyle.Brass);
            spriteBatch.DrawString(_headingFont, "Current stats", _stats.Position - new Vector2(0, 34), MenuStyle.Brass);
            spriteBatch.DrawString(_headingFont, "Equipped", _equipped.Position - new Vector2(0, 34), MenuStyle.Brass);
            ControlManager.Draw(spriteBatch);

            for (int slot = 0; slot < PageSize && _page * PageSize + slot < Items.Count; slot++)
            {
                var item = Items[_page * PageSize + slot];
                var row = _itemButtons[slot].Rectangle;
                var icon = new Rectangle(row.Right - 34, row.Y + 5, 28, 28);
                spriteBatch.Draw(item.Image, icon, item.SourceRectangle, Color.White);
                if (item is EquipableItem { Equipped: true })
                    spriteBatch.DrawRectangle(icon, MenuStyle.Brass, 1, 0);
                if (item == _selected)
                    spriteBatch.DrawRectangle(row, MenuStyle.Brass, 2, 0);
            }
            spriteBatch.End();
        }

        private static string Fit(string text, SpriteFont font, int width)
        {
            if (font.MeasureString(text).X <= width)
                return text;
            const string ellipsis = "...";
            if (font.MeasureString(ellipsis).X > width)
                return "";
            int length = text.Length;
            while (length > 0 && font.MeasureString(text[..length] + ellipsis).X > width)
                length--;
            return text[..length] + ellipsis;
        }
    }
}
