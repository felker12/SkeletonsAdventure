using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;
using System.Linq;

namespace SkeletonsAdventure.GameMenu
{
    internal sealed class LearnSkillsMenu : BaseMenu
    {
        private const int LayoutWidth = 1080;
        private const int LayoutHeight = 442;
        private const int DetailsLeft = 800;
        private const int TierTop = 85;
        private const int TierStep = 88;
        private const int LevelsPerBand = 10;
        private const int NodeHeight = 70;
        private const int TreeWidth = 780;
        private static readonly Rectangle TreeArea = new(0, 48, TreeWidth, LayoutHeight - 48);

        private sealed record SkillNode(BasicAttack Attack, string Title, Rectangle Bounds, MenuButton Button);
        private enum PanGesture { None, Left, Middle }

        private readonly Dictionary<string, SkillNode> _nodes = [];
        private readonly List<(int Level, int Top, int Height)> _bands = [];
        private readonly MagicElement[] _elements;
        private readonly MenuButton _magicTab, _meleeTab, _fitTree, _learn;
        private readonly SpriteFont _font, _headingFont;
        private readonly Rectangle _treeContent;
        private readonly RasterizerState _treeRasterizer = new() { CullMode = CullMode.None, ScissorTestEnable = true };
        private Player _player;
        private string _selected, _hovered;
        private bool _showMelee;
        private Vector2 _origin;
        private float _scale;
        private Rectangle _treeViewport;
        private Vector2 _treeOffset;
        private float _treeZoom;
        private MouseState _lastMouse;
        private PanGesture _panGesture;
        private bool _blockNodeClick;
        private float FitZoom => Math.Min(TreeArea.Width / (float)_treeContent.Width, TreeArea.Height / (float)_treeContent.Height);
        private Matrix LayoutTransform => Matrix.CreateScale(_scale, _scale, 1f) * Matrix.CreateTranslation(_origin.X, _origin.Y, 0);
        private Matrix TreeTransform
        {
            get
            {
                var transform = Matrix.CreateScale(_treeZoom, _treeZoom, 1f) *
                    Matrix.CreateTranslation(_treeOffset.X, _treeOffset.Y, 0) * LayoutTransform;
                // Point-sampled text can lose glyph rows when the fitted tree lands between pixels.
                transform.M41 = MathF.Round(transform.M41);
                transform.M42 = MathF.Round(transform.M42);
                return transform;
            }
        }

        public LearnSkillsMenu(Texture2D pixel, SpriteFont font, SpriteFont headingFont,
            IReadOnlyDictionary<string, BasicAttack> attackTemplates) : base(pixel, MenuStyle.Panel)
        {
            ArgumentNullException.ThrowIfNull(attackTemplates);
            Title = "Skills";
            Visible = true;
            _font = font;
            _headingFont = headingFont;

            _magicTab = new(pixel, font, "Magic", () => ShowMelee(false)) { HasFocus = true };
            _meleeTab = new(pixel, font, "Melee", () => ShowMelee(true));
            _fitTree = new(pixel, font, "Fit Tree", FitTree);
            _learn = new(pixel, font, "Learn", () => LearnSelectedSkill(), primary: true);
            ControlManager.AddRange([_magicTab, _meleeTab]);

            var magicAttacks = attackTemplates
                .Where(entry => entry.Value is IMagicAttack)
                .Select(entry => (entry.Key, Attack: entry.Value, Magic: (IMagicAttack)entry.Value))
                .ToArray();
            _elements = magicAttacks.Select(skill => skill.Magic.Element).Distinct().Order().ToArray();
            var skills = magicAttacks
                .GroupBy(skill => Math.Max(0, skill.Attack.LevelRequirements.Level) / LevelsPerBand * LevelsPerBand)
                .OrderBy(band => band.Key);
            int top = TierTop;
            int contentWidth = Math.Max(TreeWidth, BranchCenter(_elements.Length - 1) + 107);
            foreach (var band in skills)
            {
                int rows = 1;
                foreach (var branch in band.GroupBy(skill => skill.Magic.Element).OrderBy(branch => branch.Key))
                {
                    var entries = branch.OrderBy(skill => skill.Attack.LevelRequirements.Level)
                        .ThenBy(skill => skill.Magic.DisplayName, StringComparer.Ordinal).ToArray();
                    int branchCenter = BranchCenter(Array.IndexOf(_elements, branch.Key));
                    rows = Math.Max(rows, (entries.Length + 1) / 2);
                    for (int i = 0; i < entries.Length; i++)
                    {
                        var (key, attack, magic) = entries[i];
                        bool paired = (i / 2) * 2 + 1 < entries.Length;
                        int width = paired ? 104 : 172;
                        int splitOffset = i % 2 == 0 ? -56 : 56;
                        int center = branchCenter + (paired ? splitOffset : 0);
                        var bounds = new Rectangle(center - width / 2, top + i / 2 * TierStep, width, NodeHeight);
                        var button = new MenuButton(pixel, font, magic.DisplayName, () =>
                        {
                            if (!_blockNodeClick)
                                SelectSkill(key);
                        }) { Name = key };
                        _nodes.Add(key, new SkillNode(attack, magic.DisplayName, bounds, button));
                        _selected ??= key;
                        contentWidth = Math.Max(contentWidth, bounds.Right);
                        ControlManager.Add(button);
                    }
                }

                _bands.Add((band.Key, top, (rows - 1) * TierStep + NodeHeight));
                top += rows * TierStep;
            }
            int bottom = _bands.Count > 0 ? _bands[^1].Top + _bands[^1].Height : TierTop;
            _treeContent = new Rectangle(0, TreeArea.Top, contentWidth, bottom - TreeArea.Top);

            ControlManager.AddRange([_fitTree, _learn]);
            ControlManager.FocusChanged += (control, _) =>
            {
                if (control is MenuButton { Name: { } name } && _nodes.TryGetValue(name, out var node))
                {
                    _hovered = null;
                    RevealSkill(node.Bounds);
                    SelectSkill(name);
                }
            };

            FitTree();
        }

        private static int BranchCenter(int branch) => 210 + branch * 230;

        public override void Arrange(Rectangle bounds)
        {
            base.Arrange(bounds);
            const int padding = 16;
            _scale = Math.Clamp(Math.Min((bounds.Width - padding * 2f) / LayoutWidth,
                (bounds.Height - padding * 2f) / LayoutHeight), 0.01f, 1f);
            _origin = new(bounds.X + (bounds.Width - LayoutWidth * _scale) / 2, bounds.Y + padding);

            _magicTab.Arrange(Place(new Rectangle(0, 0, 110, 38)), _scale);
            _meleeTab.Arrange(Place(new Rectangle(118, 0, 110, 38)), _scale);
            _fitTree.Arrange(Place(new Rectangle(TreeWidth - 116, 0, 116, 38)), _scale);
            _learn.Arrange(Place(new Rectangle(DetailsLeft + 16, 386, 156, 38)), _scale);
            _treeViewport = Place(TreeArea);
            ArrangeTreeButtons();
        }

        private Rectangle Place(Rectangle bounds) => Place(bounds, LayoutTransform);

        private static Rectangle Place(Rectangle bounds, Matrix transform)
        {
            var start = Vector2.Transform(bounds.Location.ToVector2(), transform);
            var end = Vector2.Transform(new Vector2(bounds.Right, bounds.Bottom), transform);
            int x = (int)MathF.Round(start.X);
            int y = (int)MathF.Round(start.Y);
            return new Rectangle(x, y, (int)MathF.Round(end.X) - x, (int)MathF.Round(end.Y) - y);
        }

        private void ArrangeTreeButtons()
        {
            var transform = TreeTransform;
            foreach (var node in _nodes.Values)
                node.Button.Arrange(Rectangle.Intersect(Place(node.Bounds, transform), _treeViewport), _scale * _treeZoom);
        }

        internal void FitTree()
        {
            _treeZoom = FitZoom;
            _hovered = null;
            SetTreeOffset(TreeArea.Center.ToVector2() - _treeContent.Center.ToVector2() * _treeZoom);
            RefreshLearnButton();
        }

        private void SetTreeOffset(Vector2 offset)
        {
            _treeOffset = new(
                ClampOffset(offset.X, _treeContent.Left, _treeContent.Right, TreeArea.Left, TreeArea.Right),
                ClampOffset(offset.Y, _treeContent.Top, _treeContent.Bottom, TreeArea.Top, TreeArea.Bottom));
            ArrangeTreeButtons();
        }

        private float ClampOffset(float offset, int contentStart, int contentEnd, int viewStart, int viewEnd)
        {
            float min = viewEnd - contentEnd * _treeZoom;
            float max = viewStart - contentStart * _treeZoom;
            return min <= max ? Math.Clamp(offset, min, max) : (min + max) / 2;
        }

        private void RevealSkill(Rectangle bounds)
        {
            var start = bounds.Location.ToVector2() * _treeZoom + _treeOffset;
            var end = start + bounds.Size.ToVector2() * _treeZoom;
            SetTreeOffset(_treeOffset + new Vector2(
                RevealAxis(start.X, end.X, TreeArea.Left, TreeArea.Right),
                RevealAxis(start.Y, end.Y, TreeArea.Top, TreeArea.Bottom)));
        }

        private static float RevealAxis(float start, float end, int viewStart, int viewEnd)
        {
            if (end - start > viewEnd - viewStart)
                return (viewStart + viewEnd - start - end) / 2;
            return Math.Max(0, viewStart - start) - Math.Max(0, end - viewEnd);
        }

        public override void MenuOpened()
        {
            _player = World.Player;
            _hovered = null;
            _lastMouse = Mouse.GetState();
            _panGesture = PanGesture.None;
            _blockNodeClick = false;
            RefreshLearnButton();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateTreeInput(Mouse.GetState());
            base.Update(gameTime);
        }

        internal void UpdateTreeInput(MouseState mouse)
        {
            var previous = _lastMouse;
            _lastMouse = mouse;
            _blockNodeClick = false;
            if (_showMelee)
                return;

            bool wasPanning = _panGesture != PanGesture.None;
            if (!wasPanning && _treeViewport.Contains(mouse.Position))
            {
                if (mouse.MiddleButton == ButtonState.Pressed && previous.MiddleButton == ButtonState.Released)
                    _panGesture = PanGesture.Middle;
                else if (mouse.LeftButton == ButtonState.Pressed && previous.LeftButton == ButtonState.Released &&
                    GetSkillAt(mouse.Position) is null)
                    _panGesture = PanGesture.Left;
            }

            bool panning = _panGesture != PanGesture.None;
            _blockNodeClick = panning;
            if (panning)
            {
                if (wasPanning && mouse.Position != previous.Position)
                    SetTreeOffset(_treeOffset + (mouse.Position - previous.Position).ToVector2() / _scale);
                var held = _panGesture == PanGesture.Middle ? mouse.MiddleButton : mouse.LeftButton;
                if (held == ButtonState.Released)
                    _panGesture = PanGesture.None;
            }

            int wheelDelta = mouse.ScrollWheelValue - previous.ScrollWheelValue;
            if (wheelDelta != 0 && _treeViewport.Contains(mouse.Position))
            {
                var pointer = (mouse.Position.ToVector2() - _origin) / _scale;
                var treePoint = (pointer - _treeOffset) / _treeZoom;
                _treeZoom = Math.Clamp(_treeZoom * MathF.Pow(1.15f, wheelDelta / 120f), FitZoom * 0.65f, 2.5f);
                SetTreeOffset(pointer - treePoint * _treeZoom);
            }

            if (panning)
                _hovered = _panGesture == PanGesture.None ? GetSkillAt(mouse.Position) : null;
            else if (mouse.Position != previous.Position || wheelDelta != 0)
                _hovered = GetSkillAt(mouse.Position);

            RefreshLearnButton();
        }

        internal string GetSkillAt(Point pointer)
        {
            if (!_showMelee && _treeViewport.Contains(pointer))
                foreach (var (key, node) in _nodes)
                    if (node.Button.Rectangle.Contains(pointer))
                        return key;

            return null;
        }

        internal void ShowMelee(bool show)
        {
            _showMelee = show;
            _hovered = null;
            _panGesture = PanGesture.None;
            _blockNodeClick = false;
            _fitTree.Visible = _fitTree.Enabled = !show;
            foreach (var node in _nodes.Values)
                node.Button.Visible = node.Button.Enabled = !show;
            RefreshLearnButton();
        }

        internal void SelectSkill(string key)
        {
            if (!_showMelee && _nodes.ContainsKey(key))
            {
                _selected = key;
                RefreshLearnButton();
            }
        }

        private void RefreshLearnButton()
        {
            _learn.Visible = !_showMelee && _selected is not null;
            _learn.Enabled = false;
            if (!_learn.Visible || _player is null)
                return;

            var manager = _player.LearnedAttackManager;
            string preview = _hovered ?? _selected;
            bool learned = manager.Contains(preview);
            _learn.Text = learned ? "Learned" : "Learn";
            _learn.Enabled = !learned && preview == _selected &&
                manager.CheckRequirements(_nodes[_selected].Attack);
        }

        internal bool LearnSelectedSkill()
        {
            if (!_learn.Enabled)
                return false;

            _player.LearnAttack(_nodes[_selected].Attack.Clone());
            RefreshLearnButton();
            return _player.LearnedAttackManager.Contains(_selected);
        }

        public override void Draw(SpriteBatch batch)
        {
            if (!Visible)
                return;

            batch.Begin(samplerState: SamplerState.PointClamp);
            batch.Draw(Texture, Rectangle, TintColor);
            _magicTab.Draw(batch);
            _meleeTab.Draw(batch);
            var activeTab = _showMelee ? _meleeTab : _magicTab;
            batch.DrawRectangle(activeTab.Rectangle, MenuStyle.Brass, 2, 0);
            if (_fitTree.Visible)
            {
                _fitTree.Draw(batch);
                batch.DrawString(_font, "Wheel to zoom  |  Drag to pan",
                    _origin + new Vector2(248, 10) * _scale, MenuStyle.MutedText,
                    0, Vector2.Zero, _scale, SpriteEffects.None, 0);
            }
            batch.End();
            if (_showMelee || _player is null)
                return;

            DrawTreeViewport(batch);

            batch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: LayoutTransform);
            string preview = _hovered ?? _selected;
            if (preview is not null)
                DrawDetails(batch, _nodes[preview], preview == _selected);
            batch.End();

            if (_learn.Visible)
            {
                batch.Begin(samplerState: SamplerState.PointClamp);
                _learn.Draw(batch);
                batch.End();
            }
        }

        private void DrawTreeViewport(SpriteBatch batch)
        {
            var graphics = batch.GraphicsDevice;
            var clip = Rectangle.Intersect(_treeViewport, graphics.Viewport.Bounds);
            if (clip.Width <= 0 || clip.Height <= 0)
                return;

            var previousScissor = graphics.ScissorRectangle;
            var previousRasterizer = graphics.RasterizerState;
            graphics.ScissorRectangle = clip;
            try
            {
                batch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: _treeRasterizer, transformMatrix: TreeTransform);
                DrawTree(batch);
                batch.End();
            }
            finally
            {
                graphics.ScissorRectangle = previousScissor;
                graphics.RasterizerState = previousRasterizer;
            }
        }

        private void DrawTree(SpriteBatch batch)
        {
            for (int branch = 0; branch < _elements.Length; branch++)
            {
                var element = _elements[branch];
                string title = element == MagicElement.NonElemental ? "Non-elemental" : element.ToString();
                DrawCentered(batch, title, _headingFont, BranchCenter(branch), 51, 214, MenuStyle.Brass);
            }

            foreach (var (level, y, height) in _bands)
            {
                batch.Draw(Texture, new Rectangle(_treeContent.Left, y, _treeContent.Width, height), MenuStyle.Canvas);
                DrawCentered(batch, level == 0 ? "Starter" : $"Level {level}", _font, 60, y + 17, 104, MenuStyle.MutedText);
                DrawCentered(batch, $"{level}-{(long)level + LevelsPerBand - 1}", _font, 60, y + 38, 104, MenuStyle.MutedText);
            }

            var manager = _player.LearnedAttackManager;
            foreach (var node in _nodes.Values)
                foreach (string requirement in node.Attack.SkillRequirementsNames)
                    if (_nodes.TryGetValue(requirement, out var parent))
                        DrawConnection(batch, parent.Bounds, node.Bounds, manager.Contains(requirement) ? MenuStyle.Brass : MenuStyle.Border);

            foreach (var (key, node) in _nodes)
            {
                var bounds = node.Bounds;
                bool selected = key == _selected;
                bool highlighted = selected || key == _hovered;
                batch.Draw(Texture, bounds, highlighted ? MenuStyle.Selection : MenuStyle.Surface);
                batch.DrawRectangle(bounds, highlighted || node.Button.HasFocus ? MenuStyle.Brass : MenuStyle.Border,
                    selected ? 2 : 1, 0);
                node.Attack.DrawIcon(batch, new Vector2(bounds.X + 8, bounds.Y + 5), 26);
                var (status, color) = GetStatus(node.Attack);
                batch.DrawString(_font, status, new Vector2(bounds.X + 39, bounds.Y + 9), color);
                int requiredLevel = node.Attack.LevelRequirements.Level;
                DrawCentered(batch, node.Title, _font, bounds.Center.X, bounds.Y + (requiredLevel > 0 ? 29 : 39),
                    bounds.Width - 12, MenuStyle.Ivory);
                if (requiredLevel > 0)
                    DrawCentered(batch, $"Requires level {requiredLevel}", _font, bounds.Center.X, bounds.Y + 49,
                        bounds.Width - 12, MenuStyle.MutedText);
            }
        }

        private void DrawConnection(SpriteBatch batch, Rectangle parent, Rectangle child, Color color)
        {
            if (parent.Top == child.Top)
            {
                var (left, right) = parent.Left < child.Left ? (parent, child) : (child, parent);
                batch.Draw(Texture, new Rectangle(left.Right, left.Center.Y, right.Left - left.Right, 2), color);
                return;
            }

            if (parent.Top > child.Top)
                (parent, child) = (child, parent);
            int from = parent.Center.X;
            int to = child.Center.X;
            int bend = child.Top - (TierStep - NodeHeight) / 2;
            batch.Draw(Texture, new Rectangle(from, parent.Bottom, 2, bend - parent.Bottom), color);
            batch.Draw(Texture, new Rectangle(Math.Min(from, to), bend, Math.Abs(to - from) + 2, 2), color);
            batch.Draw(Texture, new Rectangle(to, bend, 2, child.Top - bend), color);
        }

        private (string Text, Color Color) GetStatus(BasicAttack attack)
        {
            var manager = _player.LearnedAttackManager;
            if (manager.Contains(attack.Name))
                return ("Learned", MenuStyle.Brass);
            return manager.CheckRequirements(attack) ? ("Ready", MenuStyle.Ivory) : ("Locked", MenuStyle.MutedText);
        }

        private void DrawDetails(SpriteBatch batch, SkillNode node, bool selected)
        {
            batch.Draw(Texture, new Rectangle(DetailsLeft, 51, LayoutWidth - DetailsLeft, 391), MenuStyle.Surface);
            int x = DetailsLeft + 16;
            var (status, color) = GetStatus(node.Attack);
            DrawCentered(batch, node.Title, _headingFont, DetailsLeft + (LayoutWidth - DetailsLeft) / 2, 68, 248, MenuStyle.Ivory);
            batch.DrawString(_font, status, new Vector2(x, 104), color);
            batch.DrawString(_font, $"Casting cost  {node.Attack.ManaCost} mana", new Vector2(x, 135), MenuStyle.MutedText);
            batch.DrawString(_headingFont, "Learning requirements", new Vector2(x, 174), MenuStyle.Brass);
            int y = 211;
            var requirements = node.Attack.LevelRequirements;
            if (requirements is not null)
            {
                DrawStat("Level", _player.Level, requirements.Level);
                DrawStat("Attack stat", _player.AttackExcludingEquipment, requirements.Attack);
                DrawStat("Defence stat", _player.DefenceExcludingEquipment, requirements.Defence);
                DrawStat("Mana stat", _player.ManaExcludingEquipment, requirements.Mana);
            }

            foreach (string key in node.Attack.SkillRequirementsNames)
            {
                string title = _nodes.TryGetValue(key, out var prerequisite) ? prerequisite.Title : key;
                DrawRequirement($"Learn {title}", _player.LearnedAttackManager.Contains(key));
            }

            if (y == 211)
                batch.DrawString(_font, "No requirements", new Vector2(x, y), MenuStyle.MutedText);
            if (!selected)
                batch.DrawString(_font, "Select to keep this preview", new Vector2(x, 355), MenuStyle.MutedText);

            void DrawStat(string name, int current, int required)
            {
                if (required > 0)
                    DrawRequirement($"{name}  {current}/{required}", current >= required);
            }

            void DrawRequirement(string text, bool met)
            {
                batch.DrawString(_font, text, new Vector2(x, y), MenuStyle.Ivory);
                batch.DrawString(_font, met ? "Met" : "Needed", new Vector2(1000, y), met ? MenuStyle.Brass : MenuStyle.MutedText);
                y += 24;
            }
        }

        private static void DrawCentered(SpriteBatch batch, string text, SpriteFont font, int x, int y, int width, Color color)
        {
            float textWidth = font.MeasureString(text).X;
            float scale = Math.Min(1f, width / Math.Max(1, textWidth));
            batch.DrawString(font, text, new Vector2(x - textWidth * scale / 2, y), color,
                0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }
}
