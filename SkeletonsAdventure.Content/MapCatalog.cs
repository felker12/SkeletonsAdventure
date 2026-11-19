namespace SkeletonsAdventure.Content;

internal static class MapCatalog
{
    public static IReadOnlyList<string> LevelNames { get; } = ["Level0", "Level0_Test", "Level1", "TestLevel"];

    public static string AssetName(string levelName) => $"TiledFiles/{levelName}";
}

internal static class BuiltInQuests
{
    public const string TestQuest = "Test Quest";
    public const string Test2 = "Test2";
    public const string Test3 = "Test3";
    public const string SlaySkeletons = "SlaySkeletons";

    public static IReadOnlyList<string> Names { get; } = [TestQuest, Test2, Test3, SlaySkeletons];
}

internal static class DataAssetCatalog
{
    public const string FileName = "DataAssets.txt";
    public const string BasicDropTable = "BasicDropTable";
}
