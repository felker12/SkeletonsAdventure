
using MonoGame.Extended.Tilemaps;

namespace SkeletonsAdventure.Animations
{
    internal class TileAnimationFrame
    {
        public int LocalTileId { get; set; } = 0;
        public float Duration { get; set; } = 0;
        public TileAnimationFrame(int tileId, int duration)
        {
            LocalTileId = tileId;
            Duration = duration;
        }

        public TileAnimationFrame() { }

        public TileAnimationFrame(TilemapTileAnimationFrame animationFrame)
        {
            LocalTileId = animationFrame.TileId;
            Duration = animationFrame.Duration * 1000f;
        }

        public TileAnimationFrame(TileAnimationFrame frame)
        {
            LocalTileId = frame.LocalTileId;
            Duration = frame.Duration;
        }

        public override string ToString()
        {
            return $"TileId: {LocalTileId}, Duration: {Duration}";
        }

        public TileAnimationFrame Clone()
        {
            return new(this);
        }
    }
}
