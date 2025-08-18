using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.States
{
    internal class StartScreen : State
    {
        readonly Label title;
        readonly LinkLabel startLabel;

        public StartScreen(Game1 game)
          : base(game)
        {
            title = new()
            {
                Text = "The Adventures of The Skeleton"
            };
            title.Position = new Vector2(Game1.ScreenWidth / 2 - (title.SpriteFont.MeasureString(title.Text)).X / 2, 30);
            title.TextColor = Color.White;
            ControlManager.Add(title);

            startLabel = new LinkLabel()
            {
                Text = "Press ENTER to begin"
            };
            startLabel.Position = new Vector2(Game1.ScreenWidth / 2 - startLabel.SpriteFont.MeasureString(startLabel.Text).X / 2, 450);
            startLabel.TextColor = Color.White;
            startLabel.TabStop = true;
            startLabel.HasFocus = true;
            startLabel.Selected += new EventHandler(StartLabel_Selected);
            ControlManager.Add(startLabel);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.LightBlue);

            spriteBatch.Begin();

            ControlManager.Draw(spriteBatch);

            spriteBatch.End();
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            
        }

        public override void PostUpdate(GameTime gameTime)
        {
        }

        public override void Update(GameTime gameTime)
        {
            ControlManager.Update(gameTime, PlayerIndexInControl);
        }
        private void StartLabel_Selected(object sender, EventArgs e)
        {
            StateManager.ChangeState(new MenuScreen(GameManager.Game));
        }
        public override void StateChangeToHandler()
        {
        }

        public override void StateChangeFromHandler()
        {
        }
    }
}
