using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameMenu
{
    internal sealed class LearnSkillsMenu : BaseMenu
    {
        private const int LayoutWidth = 1080;
        private const int LayoutHeight = 442;
        private const int DetailsLeft = 800;
        private const int TierTop = 85;
        private const int TierStep = 88;
        private const int NodeHeight = 64;

        private static readonly string[] Branches = ["Fire", "Water / Ice", "Non-elemental"];
        private static readonly string[] Tiers = ["Starter", "Intermediate", "Advanced", "Mastery"];
        private static readonly (string Key, string Title, int Branch, int Tier, int Split)[] MagicLayout =
        [
            (nameof(FireBall), "Fireball", 0, 0, 0),
            (nameof(FireWave), "Fire Wave", 0, 2, 0),
            (nameof(BlueFireWave), "Blue Fire Wave", 0, 3, 0),
            (nameof(WaterBall), "Water Ball", 1, 0, 0),
            (nameof(IceBullet), "Ice Bullet", 1, 1, 0),
            (nameof(IceBullets), "Ice Bullets", 1, 2, -1),
            (nameof(IcePillar), "Ice Pillar", 1, 2, 1),
            (nameof(TriangleAttack), "Triangle Attack", 2, 0, 0),
            (nameof(SpinningTriangleAttack), "Spinning Triangle Attack", 2, 2, 0)
        ];

        private sealed record SkillNode(BasicAttack Attack, string Title, Rectangle Bounds, MenuButton Button);

        private readonly Dictionary<string, SkillNode> _nodes = [];
        private readonly MenuButton _magicTab, _meleeTab, _learn;
        private readonly SpriteFont _font, _headingFont;
        private Player _player;
        private string _selected, _hovered;
        private bool _showMelee;
        private Vector2 _origin;
        private float _scale;

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
            _learn = new(pixel, font, "Learn", () => LearnSelectedSkill(), primary: true);
            ControlManager.AddRange([_magicTab, _meleeTab]);

            foreach (var (key, title, branch, tier, split) in MagicLayout)
            {
                if (!attackTemplates.TryGetValue(key, out var attack))
                    continue;

                int width = split == 0 ? 172 : 104;
                int center = BranchCenter(branch) + split * 56;
                var button = new MenuButton(pixel, font, title, () => SelectSkill(key)) { Name = key };
                _nodes.Add(key, new SkillNode(attack, title, new(center - width / 2, TierTop + tier * TierStep, width, NodeHeight), button));
                _selected ??= key;
                ControlManager.Add(button);
            }

            ControlManager.Add(_learn);
            ControlManager.FocusChanged += (control, _) =>
            {
                if (control is MenuButton { Name: { } name } && _nodes.ContainsKey(name))
                    SelectSkill(name);
            };

            RefreshLearnButton();
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
            _learn.Arrange(Place(new Rectangle(DetailsLeft + 16, 386, 156, 38)), _scale);
            foreach (var node in _nodes.Values)
                node.Button.Arrange(Place(node.Bounds), _scale);
        }

        private Rectangle Place(Rectangle bounds) => new(
            (int)MathF.Round(_origin.X + bounds.X * _scale), (int)MathF.Round(_origin.Y + bounds.Y * _scale),
            Math.Max(3, (int)MathF.Round(bounds.Width * _scale)), Math.Max(3, (int)MathF.Round(bounds.Height * _scale)));

        public override void MenuOpened()
        {
            _player = World.Player;
            _hovered = null;
            RefreshLearnButton();
        }

        public override void Update(GameTime gameTime)
        {
            _hovered = GetSkillAt(Mouse.GetState().Position);
            RefreshLearnButton();
            base.Update(gameTime);
        }

        internal string GetSkillAt(Point pointer)
        {
            if (!_showMelee)
                foreach (var (key, node) in _nodes)
                    if (node.Button.Rectangle.Contains(pointer))
                        return key;

            return null;
        }

        internal void ShowMelee(bool show)
        {
            _showMelee = show;
            _hovered = null;
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
            batch.End();
            if (_showMelee || _player is null)
                return;

            var transform = Matrix.CreateScale(_scale, _scale, 1f) * Matrix.CreateTranslation(_origin.X, _origin.Y, 0);
            batch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transform);
            DrawTree(batch);
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

        private void DrawTree(SpriteBatch batch)
        {
            for (int branch = 0; branch < Branches.Length; branch++)
                DrawCentered(batch, Branches[branch], _headingFont, BranchCenter(branch), 51, 214, MenuStyle.Brass);

            for (int tier = 0; tier < Tiers.Length; tier++)
            {
                int y = TierTop + tier * TierStep;
                batch.Draw(Texture, new Rectangle(0, y, 780, NodeHeight), MenuStyle.Canvas);
                batch.DrawString(_font, Tiers[tier], new Vector2(12, y + 23), MenuStyle.MutedText);
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
                DrawCentered(batch, node.Title, _font, bounds.Center.X, bounds.Y + 39, bounds.Width - 12, MenuStyle.Ivory);
            }
        }

        private void DrawConnection(SpriteBatch batch, Rectangle parent, Rectangle child, Color color)
        {
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
