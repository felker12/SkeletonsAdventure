using RpgLibrary.EntityClasses;

namespace RpgLibrary.WorldClasses
{
    public class WorldData
    {
        public TimeSpan TotalTimeInWorld { get; set; } = new();
        public string CurrentLevel { get; set; } = string.Empty;
        public PlayerData PlayerData { get; set; } = new();
        public Dictionary<string, LevelData> Levels { get; set; } = new();
        public WorldData() { }
    }
}
