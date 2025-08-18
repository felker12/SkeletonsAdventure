using MonoGame.Extended;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Entities
{
    internal class Sprite
    {
        public Texture2D Texture { get; set; } = GameManager.SkeletonTexture;
        public Color DefaultColor { get; set; } = Color.White;
        public Color SpriteColor { get; set; } = Color.White;
        public Vector2 Position { get; set; } = new();
        public Vector2 Motion { get; set; } = new();
        public float Speed { get; set; } = 1.5f;
        public int Width { get; set; } = 32;
        public int Height { get; set; } = 32;
        public Rectangle Frame { get; set; } = new(0, 0, 32, 32);
        public float RotationAngle { get; set; } = 0.0f;
        public float Scale { get; set; } = 1.0f;
        //TODO make use of the CanMove property. For example when the entity is stunned or frozen or casting a spell
        public bool CanMove { get; set; } = true;
        public Label Info { get; set; } = new(string.Empty)
        {
            Visible = true,
        };
        public Rectangle Rectangle => new((int)Position.X, (int)Position.Y, Width, Height);
        public Vector2 FrameCenter => new(Width / 2, Height / 2);
        public Vector2 Center => Position + FrameCenter;

        public Sprite() { }

        public virtual void Update(GameTime gameTime)
        {
            Position = LockToMap(Position);
            Info.Text = string.Empty; //Set the text back to empty before adding something to it (like from a child class)
            Info.Position = LockToMap(Position - new Vector2(0, 100));
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Center, Frame, SpriteColor, RotationAngle, FrameCenter, Scale, SpriteEffects.None, 1);
            //spriteBatch.Draw(Texture, Position, Frame, SpriteColor);

            spriteBatch.DrawRectangle(Rectangle, SpriteColor, 1, 0); //TODO

            Info.Draw(spriteBatch);
        }

        public Vector2 LockToMap(Vector2 position)
        {
            return new(MathHelper.Clamp(position.X, 0, World.CurrentLevel.Width - Width),
                MathHelper.Clamp(position.Y, 0, World.CurrentLevel.Height - Height));
        }
    }
}
