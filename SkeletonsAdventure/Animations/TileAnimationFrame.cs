
namespace SkeletonsAdventure.Animations
{
    internal class TileAnimationFrame
    {
        public int TileId { get; set; } = 0;
        public float Duration { get; set; } = 0;
        public TileAnimationFrame(int tileId, int duration)
        {
            TileId = tileId;
            Duration = duration;
        }

        public TileAnimationFrame() { }

        public override string ToString()
        {
            return $"TileId: {TileId}, Duration: {Duration}";
        }
    }
}
