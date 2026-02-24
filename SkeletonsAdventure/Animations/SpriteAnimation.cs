using Microsoft.Xna.Framework;
using System;

namespace SkeletonsAdventure.Animations
{
    public enum AnimationKey { Down, Left, Right, Up }

    public class SpriteAnimation
    {
        private readonly Rectangle[] frames;
        private int framesPerSecond;
        private TimeSpan frameLength;
        private TimeSpan frameTimer;
        private int currentFrame;
        private int frameWidth;
        private int frameHeight;

        public int FramesPerSecond
        {
            get { return framesPerSecond; }
            set
            {
                if (value < 1)
                    framesPerSecond = 1;
                else if (value > 60)
                    framesPerSecond = 60;
                else
                    framesPerSecond = value;
                frameLength = TimeSpan.FromSeconds(1 / (double)framesPerSecond);
            }
        }
        public Rectangle CurrentFrameRect
        {
            get { return frames[currentFrame]; }
        }

        public Rectangle[] Frames
        {
            get { return frames; }
        }

        //Constructors
        public SpriteAnimation(int frameCount, int frameWidth, int frameHeight, int xOffset, int yOffset)
        {
            frames = new Rectangle[frameCount];
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            for (int i = 0; i < frameCount; i++)
            {
                frames[i] = new Rectangle(
                xOffset + (frameWidth * i),
                yOffset,
                frameWidth,
                frameHeight);
            }
            FramesPerSecond = 5;
            Reset();
        }

        private SpriteAnimation(SpriteAnimation animation)
        {
            this.frames = animation.frames;
            FramesPerSecond = animation.FramesPerSecond;
        }

        //Methods
        public void Update(GameTime gameTime)
        {
            frameTimer += gameTime.ElapsedGameTime;
            if (frameTimer >= frameLength)
            {
                frameTimer = TimeSpan.Zero;
                currentFrame = (currentFrame + 1) % frames.Length;
            }
        }

        public void Reset()
        {
            currentFrame = 0;
            frameTimer = TimeSpan.Zero;
        }

        public object Clone()
        {
            SpriteAnimation animationClone = new(this)
            {
                frameWidth = this.frameWidth,
                frameHeight = this.frameHeight
            };
            animationClone.Reset();
            return animationClone;
        }
    }
}
