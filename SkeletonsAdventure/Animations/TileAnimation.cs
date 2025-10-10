
using MonoGame.Extended.Tiled;
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

        public TileAnimation(TiledMapTilesetAnimatedTile tiledMapTilesetAnimatedTile)
        {
            foreach(var frame in tiledMapTilesetAnimatedTile.AnimationFrames)
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
    }
}
