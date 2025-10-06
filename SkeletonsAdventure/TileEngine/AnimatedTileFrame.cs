
namespace SkeletonsAdventure.TileEngine
{
    public class AnimatedTileFrame
    {
        public int TileId { get; set; }
        public int Duration { get; set; } // milliseconds

        public AnimatedTileFrame(int tileId, int duration)
        {
            TileId = tileId;
            Duration = duration;
        }

        public AnimatedTileFrame()
        {
        }
    }
}
