using SkeletonsAdventure.Animations;

namespace SkeletonsAdventure.Entities
{
    internal class AnimatedSprite : Sprite
    {
        protected Dictionary<AnimationKey, SpriteAnimation> _animations;

        //Get/Set
        protected bool IsAnimating { get; set; }
        public AnimationKey CurrentAnimation { get; set; }

        //Constructors
        public AnimatedSprite() : base()
        {
            SetFrames(3, 32, 54, 0, 10); //This is the default which is used by the skeleton spritesheet
        }

        //Methods
        public override void Update(GameTime gameTime)
        {
            if (IsAnimating)
                _animations[CurrentAnimation].Update(gameTime);

            UpdateFrame();
            UpdateCurrentAnimation();

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        /// <summary>
        /// Initializes the animations for the sprite by defining frame layout, size, 
        /// offsets, and optional padding.
        /// </summary>
        /// <param name="frameCount">
        /// The number of frames in each row of the sprite sheet for this animation.
        /// </param>
        /// <param name="frameWidth">
        /// The full width (in pixels) of a single frame cell in the sprite sheet, 
        /// including any empty space/padding within that cell.
        /// </param>
        /// <param name="frameHeight">
        /// The full height (in pixels) of a single frame cell in the sprite sheet, 
        /// including any empty space/padding within that cell.
        /// </param>
        /// <param name="xOffset">
        /// Horizontal offset (in pixels) from the left edge of the sprite sheet 
        /// where the first frame begins.
        /// </param>
        /// <param name="yOffset">
        /// Vertical offset (in pixels) from the top edge of the sprite sheet 
        /// where the first animation row begins.
        /// </param>
        /// <param name="paddingX">
        /// Extra horizontal space (in pixels) inside the frame cell that should be 
        /// trimmed off the actual sprite’s visual width.
        /// </param>
        /// <param name="paddingY">
        /// Extra vertical space (in pixels) inside the frame cell that should be 
        /// trimmed off the actual sprite’s visual height.
        /// </param>
        public void SetFrames(int frameCount, int frameWidth, int frameHeight, int xOffset = 0, int yOffset = 0, int paddingX = 0, int paddingY = 0)
        {
            _animations = [];
            CloneAnimations(CreateAnimations(frameCount, frameWidth, frameHeight, xOffset, yOffset, paddingX, paddingY));
            UpdateFrame();
        }

        protected void UpdateFrame()
        {
            Frame = _animations[CurrentAnimation].CurrentFrameRect;
        }

        protected void CloneAnimations(Dictionary<AnimationKey, SpriteAnimation> animation)
        {
            foreach (AnimationKey key in animation.Keys)
                _animations.Add(key, (SpriteAnimation)animation[key].Clone());
        }

        protected void UpdateCurrentAnimation()
        {
            Vector2 motion = CalculateReducedMotion(Motion);

            //TODO Add frames for angles
            if (motion != Vector2.Zero)
            {
                SetCurrentAnimationBasedOffMotion(motion);
                IsAnimating = true;
            }
            else
                IsAnimating = false;
        }

        /// <summary>
        /// Creates the directional sprite animations (Up, Down, Left, Right) 
        /// based on frame layout, offsets, and optional padding.
        /// </summary>
        /// <param name="frameCount">
        /// The number of frames in each row of the sprite sheet for this animation.
        /// </param>
        /// <param name="frameWidth">
        /// The full width (in pixels) of a single frame cell in the sprite sheet, 
        /// including any empty space/padding within that cell.
        /// </param>
        /// <param name="frameHeight">
        /// The full height (in pixels) of a single frame cell in the sprite sheet, 
        /// including any empty space/padding within that cell.
        /// </param>
        /// <param name="xOffset">
        /// Horizontal offset (in pixels) from the left edge of the sprite sheet 
        /// where the first frame begins.
        /// </param>
        /// <param name="yOffset">
        /// Vertical offset (in pixels) from the top edge of the sprite sheet 
        /// where the first animation row begins.
        /// </param>
        /// <param name="paddingX">
        /// Extra horizontal space (in pixels) inside the frame cell that should be 
        /// trimmed off the actual sprite’s visual width.
        /// </param>
        /// <param name="paddingY">
        /// Extra vertical space (in pixels) inside the frame cell that should be 
        /// trimmed off the actual sprite’s visual height.
        /// </param>
        /// <returns>
        /// A dictionary mapping each <see cref="AnimationKey"/> (direction) 
        /// to its corresponding <see cref="SpriteAnimation"/> sequence.
        /// </returns>
        private Dictionary<AnimationKey, SpriteAnimation> CreateAnimations
            (int frameCount, int frameWidth, int frameHeight, int xOffset = 0, int yOffset = 0, int paddingX = 0, int paddingY = 0)
        {
            Dictionary<AnimationKey, SpriteAnimation> _Animations = [];
            Width = frameWidth - paddingX;
            Height = frameHeight - paddingY;

            _Animations.Add(AnimationKey.Down, 
                new SpriteAnimation(frameCount, frameWidth, frameHeight, xOffset, 0));

            _Animations.Add(AnimationKey.Left, 
                new SpriteAnimation(frameCount, frameWidth, frameHeight, xOffset, frameHeight + yOffset));

            _Animations.Add(AnimationKey.Right, 
                new SpriteAnimation(frameCount, frameWidth, frameHeight, xOffset, (frameHeight + yOffset) * 2));

            _Animations.Add(AnimationKey.Up, 
                new SpriteAnimation(frameCount, frameWidth, frameHeight, xOffset, (frameHeight + yOffset) * 3));

            return _Animations;
        }

        protected void SetCurrentAnimationBasedOffMotion(Vector2 motion)
        {
            //TODO   
            //Left movement
            if (motion.X == -1 && motion.Y >= -.5 && motion.Y <= .5)
                CurrentAnimation = AnimationKey.Left;
            //Right movement
            else if (motion.X == 1 && motion.Y >= -.5 && motion.Y <= .5)
                CurrentAnimation = AnimationKey.Right;
            //Down movement
            else if (motion.Y == 1 && motion.X >= -.5 && motion.X <= .5)
                CurrentAnimation = AnimationKey.Down;
            //Up movement
            else if (motion.Y == -1 && motion.X >= -.5 && motion.X <= .5)
                CurrentAnimation = AnimationKey.Up;
            //=================Diagonal=================== //TODO change to diagonal movements if diagonals are added
            //Top left
            else if (motion.X < -.5 && motion.Y < -.5)
                CurrentAnimation = AnimationKey.Left;
            //Bottom left
            else if (motion.X < -.5 && motion.Y > .5)
                CurrentAnimation = AnimationKey.Left;
            //Top Right
            else if (motion.X > .5 && motion.Y < -.5)
                CurrentAnimation = AnimationKey.Right;
            //Bottom right
            else if (motion.X > .5 && motion.Y > .5)
                CurrentAnimation = AnimationKey.Right;
        }

        public static Vector2 CalculateReducedMotion(Vector2 Motion)
        {
            Vector2 motion = Vector2.Zero;

            if (Motion != Vector2.Zero)
            {
                if (Motion.X != 0 && Motion.Y != 0)
                {
                    if (Math.Abs(Motion.X) > Math.Abs(Motion.Y))
                    {
                        if (Math.Abs(Motion.X) > 1)
                            motion = new(Motion.X / Math.Abs(Motion.X), Motion.Y / Math.Abs(Motion.X));
                        else
                            motion = Motion;
                    }
                    else
                    {
                        if (Math.Abs(Motion.Y) > 1)
                            motion = new(Motion.X / Math.Abs(Motion.Y), Motion.Y / Math.Abs(Motion.Y));
                        else
                            motion = Motion;
                    }
                }
                else if (Motion.X != 0 && Motion.Y == 0)
                {
                    if (Math.Abs(Motion.X) > 1)
                        motion = new(Motion.X / Math.Abs(Motion.X), Motion.Y);
                    else
                        motion = Motion;
                }
                else if (Motion.X == 0 && Motion.Y != 0)
                {
                    if (Math.Abs(Motion.Y) > 1)
                        motion = new(Motion.X, Motion.Y / Math.Abs(Motion.Y));
                    else motion = Motion;
                }
            }

            return motion;
        }
    }
}

