using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SkeletonsAdventure.Controls
{
    public class PictureBox : Control
    {
        #region Property Region
        public Texture2D Image { get; set; }
        public Rectangle SourceRectangle { get; set; }
        public Rectangle DestinationRectangle { get; set; }
        #endregion
        #region Constructors
        public PictureBox(Texture2D image, Rectangle destination)
        {
            Image = image;
            DestinationRectangle = destination;
            SourceRectangle = new Rectangle(0, 0, image.Width, image.Height);
            TextColor = Color.White;
        }
        public PictureBox(Texture2D image, Rectangle destination, Rectangle source)
        {
            Image = image;
            DestinationRectangle = destination;
            SourceRectangle = source;

            TextColor = Color.White;
        }
        #endregion
        #region Abstract Method Region
        public override void Update(GameTime gameTime)
        {
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Image, DestinationRectangle, SourceRectangle, TextColor);
        }
        public override void HandleInput(PlayerIndex playerIndex)
        {
        }
        #endregion
        #region Picture Box Methods
        public void SetPosition(Vector2 newPosition)
        {
            DestinationRectangle = new Rectangle(
            (int)newPosition.X,
            (int)newPosition.Y,
            SourceRectangle.Width,
            SourceRectangle.Height);
        }
        #endregion
    }
}
