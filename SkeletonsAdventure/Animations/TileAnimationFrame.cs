
using MonoGame.Extended.Tiled;

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

        public TileAnimationFrame(TiledMapTilesetTileAnimationFrame animationFrame)
        {
            LocalTileId = animationFrame.LocalTileIdentifier;
            Duration = (float)animationFrame.Duration.TotalMilliseconds;
        }

        public override string ToString()
        {
            return $"TileId: {LocalTileId}, Duration: {Duration}";
        }
    }
}
