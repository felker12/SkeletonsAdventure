namespace SkeletonsAdventure.Content;

internal static class MapCatalog
{
    public static IReadOnlyList<string> LevelNames { get; } = ["Level0", "Level0_Test", "Level1", "TestLevel"];

    public static string AssetName(string levelName) => $"TiledFiles/{levelName}";
}
