using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameMenu
{
    internal class EvolutionMenu : BaseMenu
    {
        public Player Player { get; set; } = World.Player;
        private readonly Texture2D buttonTexture = GameManager.ButtonTexture;

        Button EvolveBtn { get; set; }

        public EvolutionMenu() : base()
        {
            ControlManager = new(GameManager.Arial14);
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
            EvolveBtn = new(buttonTexture)
            {
                Text = "Evolve",
                Width = 80,
            };

            EvolveBtn.Click += EvolveBtn_Click;

            ControlManager.Add(EvolveBtn);
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
