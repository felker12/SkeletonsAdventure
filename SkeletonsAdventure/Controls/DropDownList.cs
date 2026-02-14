using Microsoft.Xna.Framework.Input;

namespace SkeletonsAdventure.Controls
{
    public class DropdownList : Control
    {
        #region Fields
        private List<string> _items;
        private int _selectedIndex = -1;
        private bool _isExpanded = false;
        private int _hoveredIndex = -1;
        private int _itemHeight = 30;
        private int _maxVisibleItems = 5;
        private int _scrollOffset = 0;
        private Rectangle _headerRectangle;
        private Rectangle _dropdownRectangle;
        private Texture2D _pixel;
        #endregion

        #region Properties
        public List<string> Items
        {
            get => _items;
            set
            {
                _items = value ?? [];
                if (_selectedIndex >= _items.Count)
                    _selectedIndex = -1;
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= -1 && value < _items.Count)
                {
                    _selectedIndex = value;
                    OnSelected(EventArgs.Empty);
                }
            }
        }

        public string SelectedItem
        {
            get => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;
        }

        public int ItemHeight
        {
            get => _itemHeight;
            set => _itemHeight = Math.Max(20, value);
        }

        public int MaxVisibleItems
        {
            get => _maxVisibleItems;
            set => _maxVisibleItems = Math.Max(1, value);
        }

        //public Color DropdownBackgroundColor { get; set; } = new Color(40, 40, 40);
        public Color DropdownBackgroundColor { get; set; }
        public Color HoverColor { get; set; } 
        public Color BorderColor { get; set; }
        public Color SelectedItemColor { get; set; }
        #endregion

        #region Constructor
        public DropdownList()
        {
            Initialize();
        }

        public DropdownList(SpriteFont font) : base(font)
        {
            Initialize();
        }

        public DropdownList(string text) : base(text)
        {
            Initialize();
        }

        private void Initialize()
        {
            _items = [];
            Height = 35;

            TextColor = Color.Black;
            BackgroundColor = Color.White;
            //DropdownBackgroundColor = new Color(40, 40, 40); //Alternative darker background color for dropdown, use white text if needed
            DropdownBackgroundColor = Color.White;
            SelectedItemColor = new Color(50, 120, 200);
            HoverColor = new Color(70, 70, 70);
            BorderColor = Color.Gray;
        }
        #endregion

        #region Helper Methods
        private void CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            if (_pixel == null)
            {
                _pixel = new Texture2D(graphicsDevice, 1, 1);
                _pixel.SetData([Color.White]);
            }
        }

        private void UpdateRectangles()
        {
            _headerRectangle = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

            if (_isExpanded)
            {
                int visibleItems = Math.Min(_maxVisibleItems, _items.Count);
                int dropdownHeight = visibleItems * _itemHeight + 4; // +4 for borders
                _dropdownRectangle = new Rectangle(
                    (int)Position.X,
                    (int)Position.Y + Height,
                    Width,
                    dropdownHeight
                );
            }
        }

        private int GetItemIndexAtPosition(int mouseY)
        {
            if (!_isExpanded || _items.Count == 0)
                return -1;

            int relativeY = mouseY - _dropdownRectangle.Y - 2; // -2 for border
            if (relativeY < 0 || relativeY >= _dropdownRectangle.Height - 4)
                return -1;

            int index = (relativeY / _itemHeight) + _scrollOffset;
            return index >= 0 && index < _items.Count ? index : -1;
        }

        private void HandleScroll()
        {
            if (_items.Count <= _maxVisibleItems)
                return;

            MouseState mouseState = Mouse.GetState();
            int scrollDelta = mouseState.ScrollWheelValue - _previousMouse.ScrollWheelValue;

            if (scrollDelta > 0)
                _scrollOffset = Math.Max(0, _scrollOffset - 1);
            else if (scrollDelta < 0)
                _scrollOffset = Math.Min(_items.Count - _maxVisibleItems, _scrollOffset + 1);
        }
        #endregion

        #region Override Methods
        public override void Update(GameTime gameTime)
        {
            if (!Enabled || !Visible)
                return;

            UpdateRectangles();
            IsMouseHovering();

            if (_isExpanded && _dropdownRectangle.Contains(_currentMouse.Position))
            {
                _hoveredIndex = GetItemIndexAtPosition(_currentMouse.Y);
                HandleScroll();
            }
            else if (!_headerRectangle.Contains(_currentMouse.Position))
            {
                _hoveredIndex = -1;
            }
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if (!Enabled || !Visible)
                return;

            // Check for left click
            if (_currentMouse.LeftButton == ButtonState.Pressed &&
                _previousMouse.LeftButton == ButtonState.Released)
            {
                // Click on header - toggle dropdown
                if (_headerRectangle.Contains(_currentMouse.Position))
                {
                    _isExpanded = !_isExpanded;
                    if (!_isExpanded)
                        _scrollOffset = 0;
                }
                // Click on dropdown item - select it
                else if (_isExpanded && _dropdownRectangle.Contains(_currentMouse.Position))
                {
                    int clickedIndex = GetItemIndexAtPosition(_currentMouse.Y);
                    if (clickedIndex >= 0)
                    {
                        SelectedIndex = clickedIndex;
                        _isExpanded = false;
                        _scrollOffset = 0;
                    }
                }
                // Click outside - close dropdown
                else if (_isExpanded)
                {
                    _isExpanded = false;
                    _scrollOffset = 0;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;

            CreatePixelTexture(spriteBatch.GraphicsDevice);

            // Draw header background
            spriteBatch.Draw(_pixel, _headerRectangle, BackgroundColor);

            // Draw header border
            DrawBorder(spriteBatch, _headerRectangle, BorderColor, 2);

            // Draw selected item text or placeholder
            string displayText = SelectedItem ?? (Text ?? "Select...");
            Vector2 textSize = SpriteFont.MeasureString(displayText);
            Vector2 textPosition = new(
                Position.X + 10,
                Position.Y + (Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(SpriteFont, displayText, textPosition, TextColor);

            // Draw arrow indicator
            //string arrow = _isExpanded ? "▲" : "▼";
            string arrow = _isExpanded ? "^" : "v";
            Vector2 arrowSize = SpriteFont.MeasureString(arrow);
            Vector2 arrowPosition = new(
                Position.X + Width - arrowSize.X - 10,
                Position.Y + (Height - arrowSize.Y) / 2
            );
            spriteBatch.DrawString(SpriteFont, arrow, arrowPosition, TextColor);

            // Draw dropdown if expanded
            if (_isExpanded)
            {
                DrawDropdown(spriteBatch);
            }
        }

        private void DrawDropdown(SpriteBatch spriteBatch)
        {
            // Draw dropdown background
            spriteBatch.Draw(_pixel, _dropdownRectangle, DropdownBackgroundColor);

            // Draw dropdown border
            DrawBorder(spriteBatch, _dropdownRectangle, BorderColor, 2);

            // Draw items
            int visibleItems = Math.Min(_maxVisibleItems, _items.Count - _scrollOffset);
            for (int i = 0; i < visibleItems; i++)
            {
                int itemIndex = i + _scrollOffset;
                Rectangle itemRectangle = new(
                    _dropdownRectangle.X + 2,
                    _dropdownRectangle.Y + 2 + (i * _itemHeight),
                    _dropdownRectangle.Width - 4,
                    _itemHeight
                );

                // Draw item background
                Color itemColor = DropdownBackgroundColor;
                if (itemIndex == _selectedIndex)
                    itemColor = SelectedItemColor;
                else if (itemIndex == _hoveredIndex)
                    itemColor = HoverColor;

                spriteBatch.Draw(_pixel, itemRectangle, itemColor);

                // Draw item text
                string itemText = _items[itemIndex];
                Vector2 itemTextSize = SpriteFont.MeasureString(itemText);
                Vector2 itemTextPosition = new(
                    itemRectangle.X + 10,
                    itemRectangle.Y + (itemRectangle.Height - itemTextSize.Y) / 2
                );

                // Clip text if too long
                if (itemTextSize.X > itemRectangle.Width - 20)
                {
                    int maxChars = (int)((itemRectangle.Width - 30) / (itemTextSize.X / itemText.Length));
                    itemText = string.Concat(itemText.AsSpan(0, Math.Max(0, maxChars)), "...");
                }

                spriteBatch.DrawString(SpriteFont, itemText, itemTextPosition, TextColor);
            }

            // Draw scroll indicator if needed
            if (_items.Count > _maxVisibleItems)
            {
                string scrollIndicator = $"{_scrollOffset + 1}-{_scrollOffset + visibleItems} of {_items.Count}";
                Vector2 scrollSize = SpriteFont.MeasureString(scrollIndicator);
                Vector2 scrollPosition = new(
                    _dropdownRectangle.X + _dropdownRectangle.Width - scrollSize.X - 5,
                    _dropdownRectangle.Y + _dropdownRectangle.Height - scrollSize.Y - 2
                );
                spriteBatch.DrawString(SpriteFont, scrollIndicator, scrollPosition, Color.LightGray);
            }
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
        {
            // Top
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            // Left
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }
        #endregion

        #region Public Methods
        public void AddItem(string item)
        {
            if (!string.IsNullOrEmpty(item))
                _items.Add(item);
        }

        public void RemoveItem(string item)
        {
            int index = _items.IndexOf(item);
            if (index >= 0)
            {
                _items.RemoveAt(index);
                if (_selectedIndex == index)
                    _selectedIndex = -1;
                else if (_selectedIndex > index)
                    _selectedIndex--;
            }
        }

        public void AddItems(List<string> items)
        {
            foreach (string item in items)
                AddItem(item);
        }

        public void Clear()
        {
            _items.Clear();
            _selectedIndex = -1;
            _scrollOffset = 0;
        }

        public void Close()
        {
            _isExpanded = false;
            _scrollOffset = 0;
        }
        #endregion
    }
}