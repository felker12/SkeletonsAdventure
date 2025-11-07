
namespace RpgLibrary.AttackData
{
    public class MultiShotAttackData : AttackData
    {
        public string ShotName { get; set; } = string.Empty;
        public int ShotCount { get; set; } = 3;
        public TimeSpan ShotInterval { get; set; } = TimeSpan.FromMilliseconds(500);
        public int ShotsFired { get; set; } = 0;

        public MultiShotAttackData() { }

        public override string ToString()
        {
            return base.ToString() +
                $", Shot Name: {ShotName}, " +
                $"Shot Count: {ShotCount}, " +
                $"Shot Interval: {ShotInterval}, " +
                $"Shots Fired: {ShotsFired}";
        }
    }
}
