using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Attacks;
using System.Linq;

namespace SkeletonsAdventure.GameUI
{
    internal class AttackIcon
    {
        private Rectangle? _iconRectangle = null;

        public AttackIcon() { }

        public AttackIcon(Rectangle? iconRectangle)
        {
            _iconRectangle = iconRectangle;
        }

        public AttackIcon(BasicAttack attack)
        {
            _iconRectangle = GetIconRectangle(attack);
        }

        public virtual Rectangle GetIconRectangle(BasicAttack attack)
        {
            if (_iconRectangle.HasValue)
                return _iconRectangle.Value;
            if (attack.Name == "BasicAttack")
                return new(20, 80, 32, 60);
            if (attack.animations.TryGetValue(AnimationKey.Right, out SpriteAnimation value))
                return value.Frames[0];

            _iconRectangle = attack.animations[attack.animations.Keys.First()].Frames[0];
            return _iconRectangle.Value;
        }

        public virtual void DrawIcon(BasicAttack attack, SpriteBatch spriteBatch, Vector2 position, int size = 32, Color tint = default)
        {
            if (tint == default)
                tint = Color.White;

            Rectangle src = GetIconRectangle(attack);
            float scale = GetUniformScaleForTarget(src, size);

            // Draw at `position` as a `size x size` box. We use a centered origin so the icon is centered in that box.
            Vector2 origin = new(src.Width / 2f, src.Height / 2f);
            Vector2 drawPos = position + new Vector2(size / 2f, size / 2f);

            spriteBatch.Draw(attack.Texture, drawPos, src, tint, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        //helper method to compute scale from a source frame to a desired target size.
        public static float GetUniformScaleForTarget(Rectangle frame, int targetSize = 32)
        {
            if (frame.Width <= 0 || frame.Height <= 0)
                return 1f;

            float scaleX = (float)targetSize / frame.Width;
            float scaleY = (float)targetSize / frame.Height;

            // Preserve aspect ratio and fit the frame inside target square.
            return Math.Min(scaleX, scaleY);
        }
    }
}