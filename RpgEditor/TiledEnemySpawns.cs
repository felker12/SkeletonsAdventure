using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using SkeletonsAdventure.Content;

namespace RpgEditor;

internal sealed record TiledEnemySpawn(string File, string ObjectId, string Enemy, string Position, string Level);

internal sealed class TiledEnemySpawns
{
    public List<TiledEnemySpawn> Spawns { get; } = [];
    public List<(string File, string Error)> UnreadableMaps { get; } = [];

    public static TiledEnemySpawns Read(string root)
    {
        var result = new TiledEnemySpawns();
        foreach (string level in MapCatalog.LevelNames)
        {
            string file = MapCatalog.AssetName(level) + ".tmx";
            try
            {
                var map = XDocument.Load(Path.Combine(root, file));
                foreach (var layer in map.Descendants("objectgroup").Where(node => (string?)node.Attribute("name") == "SpawnerLayer"))
                {
                    foreach (var obj in layer.Elements("object"))
                    {
                        string? enemy = (string?)obj.Attribute("name");
                        if (string.IsNullOrWhiteSpace(enemy))
                            continue;
                        var properties = obj.Element("properties")?.Elements("property").ToArray() ?? [];
                        var property = properties.FirstOrDefault(node => (string?)node.Attribute("name") == "level") ??
                            properties.FirstOrDefault(node => (string?)node.Attribute("name") == "lvl");
                        string? value = (string?)property?.Attribute("value") ?? property?.Value;
                        string? propertyType = (string?)property?.Attribute("type");
                        string levelText = propertyType is null or "int" or "string" &&
                            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int authoredLevel) &&
                            authoredLevel != 0 ? authoredLevel.ToString(CultureInfo.InvariantCulture) : "Map default";
                        string position = $"{(string?)obj.Attribute("x") ?? "0"}, {(string?)obj.Attribute("y") ?? "0"}";
                        result.Spawns.Add(new(file, (string?)obj.Attribute("id") ?? "—", enemy, position, levelText));
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or XmlException)
            {
                result.UnreadableMaps.Add((file, ex.Message));
            }
        }
        return result;
    }

    public IEnumerable<TiledEnemySpawn> ForClass(string type) =>
        Spawns.Where(spawn => spawn.Enemy == EnemyClassCatalog.SpawnName(type));
}
