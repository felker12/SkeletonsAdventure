using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameMenu
{
    internal class EvolutionMenu : BaseMenu
    {
        public Player Player { get; set; } = World.Player;
        private readonly Texture2D _buttonTexture;
        private readonly SpriteFont _buttonFont;

        Button EvolveBtn { get; set; }

        public EvolutionMenu(Texture2D texture, Texture2D buttonTexture, SpriteFont buttonFont, Color color)
            : base(texture, color)
        {
            _buttonTexture = buttonTexture ?? throw new ArgumentNullException(nameof(buttonTexture));
            _buttonFont = buttonFont ?? throw new ArgumentNullException(nameof(buttonFont));
            CreateControls();
            PositionControls();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if(Player.CanEvolve)
                EvolveBtn.Enabled = true;
            else
                EvolveBtn.Enabled = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        private void CreateControls()
        {
            EvolveBtn = new(_buttonTexture, _buttonFont)
            {
                Text = "Evolve",
                Width = 136,
                Height = 42,
            };

            EvolveBtn.Click += EvolveBtn_Click;

            ControlManager.Add(EvolveBtn);
        }

        public override void Arrange(Rectangle bounds)
        {
            base.Arrange(bounds);
            PositionControls();
        }

        private void PositionControls()
        {
            EvolveBtn.Position = new Vector2(
                Rectangle.Center.X - (EvolveBtn.Width / 2),
                Rectangle.Center.Y - (EvolveBtn.Height / 2));
        }

        private void EvolveBtn_Click(object sender, EventArgs e)
        {
            World.Player.Evolve();
            Player = World.Player;
        }
    }
}
