using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RpgEditor;

internal sealed record SpriteSheetPreference(SpriteGrid Grid, SpriteSheetPosition Position, bool GridMode);

internal sealed class SpritePickerPreferences
{
    private readonly string filePath;
    private readonly Dictionary<string, SpriteSheetPreference> sheets = new(StringComparer.OrdinalIgnoreCase);
    private bool changed;

    public static SpritePickerPreferences ForProject(string root)
    {
        string projectKey = Path.TrimEndingDirectorySeparator(root).ToUpperInvariant();
        string key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(projectKey)));
        return new SpritePickerPreferences(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RpgEditor", "SpriteSheets", key + ".json"));
    }

    internal SpritePickerPreferences(string filePath)
    {
        this.filePath = filePath;
        try
        {
            using var stream = File.OpenRead(filePath);
            var saved = JsonSerializer.Deserialize<Dictionary<string, SpriteSheetPreference>>(stream);
            if (saved is not null)
                foreach (var (asset, preference) in saved)
                    if (preference?.Grid is { Width: > 0 and <= SpriteGrid.MaxInput, Height: > 0 and <= SpriteGrid.MaxInput,
                        Margin: >= 0 and <= SpriteGrid.MaxInput, Spacing: >= 0 and <= SpriteGrid.MaxInput })
                        sheets[asset] = preference;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException) { }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Trace.WriteLine($"Could not read sprite picker preferences. {ex.Message}");
        }
    }

    public SpriteSheetPreference? Find(string asset) => sheets.GetValueOrDefault(asset);

    public void Remember(string asset, SpriteSheetPreference preference)
    {
        if (Find(asset) == preference)
            return;
        sheets[asset] = preference;
        changed = true;
    }

    public void Save()
    {
        if (!changed)
            return;
        string temporary = filePath + $".{Guid.NewGuid():N}.tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            try
            {
                File.WriteAllText(temporary, JsonSerializer.Serialize(sheets));
                File.Move(temporary, filePath, overwrite: true);
                changed = false;
            }
            finally { File.Delete(temporary); }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Trace.WriteLine($"Could not save sprite picker preferences. {ex.Message}");
        }
    }
}
