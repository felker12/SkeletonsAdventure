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
            SetFrames(3, 32, 64, paddingY: 10); //This is the default which is used by the skeleton spritesheet
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
        /// <summary>
        /// Defines the sprite sheet layout and initializes animations.
        /// </summary>
        /// <param name="frameCount">Number of frames per animation row.</param>
        /// <param name="frameWidth">Width of each frame in pixels.</param>
        /// <param name="frameHeight">Height of each frame in pixels.</param>
        /// <param name="xOffset">Horizontal offset before the first frame begins.</param>
        /// <param name="yOffset">Vertical offset between rows of animations.</param>
        /// <param name="paddingX">Horizontal padding to subtract from the visible frame width.</param>
        /// <param name="paddingY">Vertical padding to subtract from the visible frame height.</param>
        /// <param name="order">
        /// Optional custom order of <see cref="AnimationKey"/> values.  
        /// Defaults to Down, Left, Right, Up if not provided.
        /// </param>
        public void SetFrames(
            int frameCount,
            int frameWidth,
            int frameHeight,
            int xOffset = 0,
            int yOffset = 0,
            int paddingX = 0,
            int paddingY = 0,
            AnimationKey[] order = null)
        {
            _animations = [];

            // Default order if not supplied
            order ??= [AnimationKey.Down, AnimationKey.Left, AnimationKey.Right, AnimationKey.Up];

            CloneAnimations(CreateAnimations(order, frameCount, frameWidth, frameHeight, xOffset, yOffset, paddingX, paddingY));
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
        /// <param name="order">
        /// The order of <see cref="AnimationKey"/> values. 
        /// Each key corresponds to a row in the sprite sheet.
        /// </param>
        /// <returns>
        /// A dictionary mapping each <see cref="AnimationKey"/> (direction) 
        /// to its corresponding <see cref="SpriteAnimation"/> sequence.
        /// </returns>
        private Dictionary<AnimationKey, SpriteAnimation> CreateAnimations(
            AnimationKey[] order,
            int frameCount,
            int frameWidth,
            int frameHeight,
            int xOffset = 0,
            int yOffset = 0,
            int paddingX = 0,
            int paddingY = 0)
        {
            Dictionary<AnimationKey, SpriteAnimation> _Animations = [];
            Width = frameWidth - paddingX;
            Height = frameHeight - paddingY;

            for (int i = 0; i < order.Length; i++)
            {
                int rowY = yOffset + frameHeight * i;
                _Animations.Add(order[i],
                    new SpriteAnimation(frameCount, frameWidth, frameHeight, xOffset, rowY));
            }

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

