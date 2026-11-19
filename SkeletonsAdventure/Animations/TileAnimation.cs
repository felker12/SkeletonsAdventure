
using MonoGame.Extended.Tilemaps;
using System.Linq;

namespace SkeletonsAdventure.Animations
{
    internal class TileAnimation
    {
        public List<TileAnimationFrame> Frames { get; set; } = [];
        public int CurrentFrameIndex { get; set; } = 0;
        public float TotalDuration => Frames.Sum(frame => frame.Duration);

        public TileAnimation() { }

        public TileAnimation(List<TileAnimationFrame> frames)
        {
            Frames = frames;
        }

        public TileAnimation(TilemapTileAnimation animation)
        {
            foreach(var frame in animation.Frames)
            {
                Frames.Add(new(frame));
            }
        }

        public override string ToString()
        {
            return $"Frames: {string.Join(", ", Frames)}";
        }

        public TileAnimationFrame GetCurrentFrame()
        {
            if (Frames.Count == 0)
                return null;

            return Frames[CurrentFrameIndex];
        }

        public TileAnimation Clone()
        { 
            return new TileAnimation(Frames); 
        }
    }
}
