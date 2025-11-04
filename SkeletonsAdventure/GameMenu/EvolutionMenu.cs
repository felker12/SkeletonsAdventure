using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities;
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

        public EvolutionMenu(Player player) : this()
        {
            Player = player; 
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
            // Evolve the player and update the world player reference

            Debug.WriteLine("Evolve button clicked.");
            Debug.WriteLine($"Current Player Level: {Player.Level}");
            Debug.WriteLine($"Player base stats before evolution: Health={Player.BaseHealth}, Attack={Player.BaseAttack}, Defense={Player.BaseDefence}");
            Debug.WriteLine($"Evolution type: {Player.EvolutionType}");

            World.Player.Evolve();
            Player = World.Player;

            Debug.WriteLine($"Player base stats after evolution: Health={Player.BaseHealth}, Attack={Player.BaseAttack}, Defense={Player.BaseDefence}");
        }
    }
}
