using MonoGame.Extended;
using SkeletonsAdventure.GameWorld;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace SkeletonsAdventure.GameUI
{
    internal class MessageBox
    {
        private readonly int BorderWidth = 2;
        private readonly int MessageBoxPadding = 8;

        public List<string> Messages { get; private set; } = [];
        public bool IsVisible { get; set; } = true;
        public int MaxLines { get; set; } = 10;
        public Color BackgroundColor { get; set; } = Color.LightGray * 0.4f;
        public Color TextColor { get; set; } = Color.White;
        public SpriteFont Font { get; set; } = GameManager.Arial12;
        public Vector2 Position { get; set; } = new Vector2(0, 0);
        public int Width { get; set; } = 500;
        public int Height => (int)Font.MeasureString($"1").Y * MaxLines + BorderWidth * 2 + MessageBoxPadding;
        public Rectangle Rectangle => new((int)Position.X, (int)Position.Y, Width, Height);

        // Scrollbar properties
        private int _scrollOffset = 0;
        private bool _isDragging = false;
        private readonly int ScrollBarWidth = 15;
        private readonly Color ScrollBarColor = Color.Gray * 0.6f;
        private readonly Color ScrollHandleColor = Color.DarkGray * 0.6f;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private Rectangle _scrollBarBounds;
        private Rectangle _handleBounds;
        private int scrollWheelDelta;

        public MessageBox()
        {
        }

        public MessageBox(Color backgroundColor, Color textColor) : this()
        {
            BackgroundColor = backgroundColor;
            TextColor = textColor;
        }

        public MessageBox(Vector2 position, int width, int maxLines, Color backgroundColor, Color textColor)
            : this(backgroundColor, textColor)
        {
            Position = position;
            Width = width;
            MaxLines = maxLines;
        }

        public MessageBox(Vector2 position, int width, int maxLines, Color backgroundColor, Color textColor, SpriteFont font)
            : this(position, width, maxLines, backgroundColor, textColor)
        {
            Font = font;
        }

        public void Update()
        {
            if (!IsVisible) return;

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            _scrollBarBounds = GetScrollBarBounds();
            _handleBounds = GetScrollHandleBounds();
            scrollWheelDelta = _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
        }

        public void HandleInput()
        {
            // Handle mouse input for scrollbar
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                Point mousePos = new(_currentMouseState.X, _currentMouseState.Y);

                if (_previousMouseState.LeftButton == ButtonState.Released)
                    if (_handleBounds.Contains(mousePos))
                        _isDragging = true;

                if (_isDragging)
                {
                    float scrollPercent = (mousePos.Y - _scrollBarBounds.Y) / (float)_scrollBarBounds.Height;
                    scrollPercent = Math.Clamp(scrollPercent, 0, 1);
                    _scrollOffset = (int)((Messages.Count - MaxLines) * scrollPercent);
                    _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, Messages.Count - MaxLines));
                }
            }
            else
                _isDragging = false;

            // Handle mouse wheel
            if (scrollWheelDelta != 0 && Rectangle.Contains(_currentMouseState.Position))
            {
                _scrollOffset -= scrollWheelDelta / 120; // 120 is a standard mouse wheel step
                _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, Messages.Count - MaxLines));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;

            // Draw background
            spriteBatch.FillRectangle(Rectangle, BackgroundColor);
            spriteBatch.DrawRectangle(Rectangle, Color.Black, BorderWidth, 1);

            // Draw messages
            Vector2 textPosition = new(Position.X + MessageBoxPadding, Position.Y + MessageBoxPadding);

            // Draw visible messages
            for (int i = _scrollOffset; i < Messages.Count && i < _scrollOffset + MaxLines; i++)
            {
                spriteBatch.DrawString(Font, Messages[i], textPosition, TextColor);
                textPosition.Y += Font.LineSpacing;
            }

            // Draw scrollbar if needed
            if (Messages.Count > MaxLines)
            {
                // Draw scrollbar background
                Rectangle scrollBarBounds = GetScrollBarBounds();
                spriteBatch.FillRectangle(scrollBarBounds, ScrollBarColor);

                // Draw scroll handle
                Rectangle handleBounds = GetScrollHandleBounds();
                spriteBatch.FillRectangle(handleBounds, ScrollHandleColor);
            }
        }

        private Rectangle GetScrollBarBounds()
        {
            return new Rectangle(
                (int)Position.X + Width - ScrollBarWidth - BorderWidth,
                (int)Position.Y + BorderWidth,
                ScrollBarWidth - BorderWidth,
                Height - BorderWidth * 2);
        }

        private Rectangle GetScrollHandleBounds()
        {
            Rectangle scrollBarBounds = GetScrollBarBounds();
            if (Messages.Count <= MaxLines)
            {
                // No scrolling needed, handle fills the bar
                return new Rectangle(
                    scrollBarBounds.X,
                    scrollBarBounds.Y,
                    ScrollBarWidth,
                    scrollBarBounds.Height
                );
            }

            float handleHeight = Math.Max(20, (MaxLines / (float)Messages.Count) * scrollBarBounds.Height);
            handleHeight = Math.Min(handleHeight, scrollBarBounds.Height); // Never exceed bar height

            float scrollPercent = _scrollOffset / (float)(Messages.Count - MaxLines);
            float maxHandleY = scrollBarBounds.Y + scrollBarBounds.Height - handleHeight;
            float handleY = scrollBarBounds.Y + (scrollBarBounds.Height - handleHeight) * scrollPercent;

            // Clamp handleY to stay within the scrollbar
            handleY = Math.Clamp(handleY, scrollBarBounds.Y, maxHandleY);

            return new Rectangle(
                scrollBarBounds.X,
                (int)Math.Round(handleY),
                ScrollBarWidth,
                (int)Math.Round(handleHeight)
            );
        }

        public void ToggleVisibility()
        {
            IsVisible = !IsVisible;
        }

        public void AddMessage(string message)
        {
            float maxLineWidth = Width - MessageBoxPadding * 2;
            string[] words = message.Split(' '); // Split the message into words
            StringBuilder currentLine = new();

            // If the message is empty, do nothing
            // if not empty, check if it fits in the current line
            foreach (string word in words)
            {
                string testLine = currentLine.Length == 0 ? word : currentLine + word + " ";
                float testLineWidth = Font.MeasureString(testLine).X;

                // If the test line exceeds the max width, we need to break it
                if (testLineWidth > maxLineWidth)
                {
                    // Only add the current line if it fits
                    if (currentLine.Length > 0)
                    {
                        Messages.Add(currentLine.ToString().TrimEnd());
                        currentLine.Clear();
                    }

                    // If the word itself is too long, split it
                    if (Font.MeasureString(word).X > maxLineWidth)
                    {
                        // Split the word into smaller parts that fit within the max width
                        string splitWord = word;
                        while (Font.MeasureString(splitWord).X > maxLineWidth)
                        {
                            int charCount = 1;
                            while (charCount < splitWord.Length &&
                                   Font.MeasureString(splitWord[..(charCount + 1)]).X <= maxLineWidth)
                            {
                                charCount++;
                            }
                            Messages.Add(splitWord[..charCount]);
                            splitWord = splitWord[charCount..];
                        }
                        if (splitWord.Length > 0)
                            currentLine.Append(splitWord + " ");
                    }
                    else // If the word fits, add it to the current line
                        currentLine.Append(word + " ");
                }
                else // If the test line fits, just add the word to the current line
                    currentLine.Append(word + " ");
            }

            if (currentLine.Length > 0 && Font.MeasureString(currentLine.ToString()).X <= maxLineWidth)
                Messages.Add(currentLine.ToString().TrimEnd());

            // Auto-scroll to bottom when new message is added
            if (!_isDragging && Messages.Count > MaxLines)
                _scrollOffset = Messages.Count - MaxLines;
        }

        public void AddMessages(List<string> messages)
        {
            foreach (string s in messages)
                AddMessage(s);
        }

        public void ClearMessages()
        {
            Messages.Clear();
            _scrollOffset = 0;
        }

        public void RemoveMessage(string message)
        {
            Messages.Remove(message);

            // Adjust scroll offset if needed
            if (_scrollOffset > Messages.Count - MaxLines)
                _scrollOffset = Math.Max(0, Messages.Count - MaxLines);
        }
    }
}
