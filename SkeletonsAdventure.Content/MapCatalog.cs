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
    public const string Coins = "Coins";
    public const string Bones = "Bones";
    public const string SkeletonType = "SkeletonsAdventure.Entities.Skeleton";
    public const string EliteSkeletonName = "EliteSkeleton";
    public const string EliteSkeletonType = "SkeletonsAdventure.Entities." + EliteSkeletonName;

    public static IReadOnlyList<(string Name, int Weight, int Minimum, int Maximum)> BasicDrops { get; } =
    [
        (Coins, 20, 1, 12), ("Robes", 5, 1, 1), (Bones, 20, 1, 2), ("Sword", 5, 1, 1),
        ("Shoes", 5, 1, 1), ("Hood", 5, 1, 1), ("Round Shield", 5, 1, 1)
    ];
}
