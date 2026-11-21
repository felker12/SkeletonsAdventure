using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.GameWorld;
using System.Linq;

namespace SkeletonsAdventure.GameMenu
{
    internal sealed class PlayerInfoMenu : BaseMenu
    {
        private const int LayoutWidth = 1080;
        private const int LayoutHeight = 426;
        private const int RowTop = 154;
        private const int RowStep = 51;
        private static readonly string[] StatNames = ["Attack", "Defence", "Max health", "Max mana"];
        private static readonly string[] StatColumns = ["Base", "Level", "Points", "Gear", "Evolution", "Current", "After apply"];
        private static readonly int[] ColumnCenters = [182, 263, 344, 425, 510, 615, 736];
        private static readonly int[] PointChanges = [1, 5, -1];
        private readonly int[] _pending = new int[StatNames.Length];
        private readonly MenuButton[,] _pointButtons = new MenuButton[StatNames.Length, PointChanges.Length];
        private readonly MenuButton _apply, _cancel;
        private readonly SpriteFont _font, _headingFont;
        private readonly StatusBar _experience;
        private Player _player;
        private Vector2 _origin;
        private float _scale;
        private int PendingPoints => _pending.Sum();
        private int AvailablePoints => _player.AttributePoints - PendingPoints;

        public PlayerInfoMenu(Texture2D pixel, SpriteFont font, SpriteFont headingFont) : base(pixel, MenuStyle.Panel)
        {
            Title = "Player";
            Visible = true;
            _font = font;
            _headingFont = headingFont;
            _experience = new(pixel, font)
            {
                Width = 580, Height = 22, Transparency = 1f,
                BorderColor = MenuStyle.Border, BackgroundColor = MenuStyle.Canvas, BarColor = MenuStyle.Selection
            };

            for (int row = 0; row < StatNames.Length; row++)
            {
                int stat = row;
                for (int column = 0; column < PointChanges.Length; column++)
                {
                    int change = PointChanges[column];
                    var button = new MenuButton(pixel, font, change > 0 ? $"+{change}" : change.ToString(),
                        () => AdjustPoints(stat, change)) { HasFocus = row == 0 && column == 0 };
                    _pointButtons[row, column] = button;
                    ControlManager.Add(button);
                }
            }

            _apply = new(pixel, font, "Apply points", ApplyPendingPoints, primary: true);
            _cancel = new(pixel, font, "Cancel", CancelPendingPoints);
            ControlManager.AddRange([_apply, _cancel]);
        }

        public override void MenuOpened()
        {
            _player = World.Player;
            CancelPendingPoints();
        }

        public override void Arrange(Rectangle bounds)
        {
            base.Arrange(bounds);
            const int padding = 16;
            _scale = Math.Clamp(Math.Min((bounds.Width - padding * 2f) / LayoutWidth,
                (bounds.Height - padding * 2f) / LayoutHeight), 0.01f, 1f);
            _origin = new(bounds.X + (bounds.Width - LayoutWidth * _scale) / 2, bounds.Y + padding);

            for (int row = 0; row < StatNames.Length; row++)
                for (int column = 0; column < PointChanges.Length; column++)
                    ArrangeButton(_pointButtons[row, column], 842 + column * 74, RowTop + row * RowStep + 7, 66, 32);

            ArrangeButton(_apply, 790, 370, 158, 40);
            ArrangeButton(_cancel, 958, 370, 122, 40);
        }

        private void ArrangeButton(MenuButton button, int x, int y, int width, int height)
        {
            var position = _origin + new Vector2(x, y) * _scale;
            button.Arrange(new((int)position.X, (int)position.Y,
                Math.Max(3, (int)(width * _scale)), Math.Max(3, (int)(height * _scale))), _scale);
        }

        internal void AdjustPoints(int stat, int change)
        {
            if (_pending[stat] + change < 0 || change > AvailablePoints)
                return;
            _pending[stat] += change;
            UpdateButtons();
        }

        internal void ApplyPendingPoints()
        {
            if (PendingPoints == 0 || AvailablePoints < 0)
                return;
            _player.ApplyAttributePoints(_pending[0], _pending[1], _pending[2], _pending[3]);
            CancelPendingPoints();
        }

        internal void CancelPendingPoints()
        {
            Array.Clear(_pending);
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            int available = AvailablePoints;
            for (int row = 0; row < StatNames.Length; row++)
                for (int column = 0; column < PointChanges.Length; column++)
                {
                    int change = PointChanges[column];
                    _pointButtons[row, column].Enabled = change > 0 ? available >= change : _pending[row] > 0;
                }

            _apply.Enabled = _cancel.Enabled = PendingPoints > 0;
        }

        public override void Draw(SpriteBatch batch)
        {
            if (!Visible || _player is null)
                return;

            batch.Begin();
            batch.Draw(Texture, Rectangle, TintColor);
            batch.End();

            var transform = Matrix.CreateScale(_scale, _scale, 1f) * Matrix.CreateTranslation(_origin.X, _origin.Y, 0);
            batch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transform);
            batch.Draw(Texture, new Rectangle(0, 0, 620, 104), MenuStyle.Surface);
            batch.Draw(Texture, new Rectangle(634, 0, 446, 104), MenuStyle.Surface);
            string form = _player.EvolutionType == PlayerEvolutionType.ArmoredSkeleton ? "Armored Skeleton" : "Skeleton";
            batch.DrawString(_headingFont, $"Level {_player.Level}   {form}", new Vector2(18, 17), MenuStyle.Ivory);
            var xp = GameManager.XPTable.GetProgress(_player.TotalXP);
            _experience.Update(xp.Current, xp.Required, new Vector2(18, 60),
                xp.AtMaximumLevel ? "Max level" : $"{xp.Current:N0} / {xp.Required:N0} XP to next level");
            _experience.Draw(batch);
            batch.DrawString(_headingFont, $"{AvailablePoints:N0} points available", new Vector2(652, 17), MenuStyle.Brass);
            batch.DrawString(_font, $"{PendingPoints:N0} pending", new Vector2(652, 62), MenuStyle.MutedText);

            batch.DrawString(_font, "Attribute", new Vector2(18, 123), MenuStyle.MutedText);
            for (int column = 0; column < StatColumns.Length; column++)
                DrawCentered(batch, StatColumns[column], ColumnCenters[column], 123, MenuStyle.MutedText);
            DrawCentered(batch, "Allocate points", 949, 123, MenuStyle.MutedText);

            int[][] values =
            [
                [_player.BaseAttack, _player.bonusAttackFromLevel, _player.bonusAttackFromAttributePoints,
                    _player.EquippedItems.EquippedItemsAttackBonus(), _player.bonusAttackFromEvolution, _player.Attack],
                [_player.BaseDefence, _player.bonusDefenceFromLevel, _player.bonusDefenceFromAttributePoints,
                    _player.EquippedItems.EquippedItemsDefenceBonus(), _player.bonusDefenceFromEvolution, _player.Defence],
                [_player.BaseHealth, _player.bonusHealthFromLevel, _player.bonusHealthFromAttributePoints,
                    0, _player.bonusHealthFromEvolution, _player.MaxHealth],
                [_player.BaseMana, _player.bonusManaFromLevel, _player.bonusManaFromAttributePoints,
                    0, _player.bonusManaFromEvolution, _player.MaxMana]
            ];
            for (int row = 0; row < StatNames.Length; row++)
            {
                int y = RowTop + row * RowStep;
                batch.Draw(Texture, new Rectangle(0, y, LayoutWidth, 46), MenuStyle.Surface);
                batch.DrawString(_font, StatNames[row], new Vector2(18, y + 14), MenuStyle.Ivory);
                for (int column = 0; column < values[row].Length; column++)
                    DrawCentered(batch, values[row][column].ToString(), ColumnCenters[column], y + 14,
                        column == values[row].Length - 1 ? MenuStyle.Ivory : MenuStyle.MutedText);

                string preview = _pending[row] == 0 ? "-" : $"{(long)values[row][^1] + _pending[row]} (+{_pending[row]})";
                DrawCentered(batch, preview, ColumnCenters[^1], y + 14, _pending[row] > 0 ? MenuStyle.Brass : MenuStyle.MutedText);
            }
            batch.End();

            batch.Begin();
            ControlManager.Draw(batch);
            batch.End();
        }

        private void DrawCentered(SpriteBatch batch, string text, int x, int y, Color color) =>
            batch.DrawString(_font, text, new Vector2(x - _font.MeasureString(text).X / 2, y), color);
    }
}
