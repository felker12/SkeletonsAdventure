using System.Diagnostics;
using System.Text.Json;

namespace RpgEditor;

internal sealed record RecentProject(string Path, string Title);

internal sealed class RecentProjects
{
    private const int Limit = 10;
    private readonly string filePath;
    private List<RecentProject> entries;

    public RecentProjects(string filePath)
    {
        this.filePath = filePath;
        entries = Read();
    }

    public IReadOnlyList<RecentProject> Entries => entries;

    public void Remember(EditorProject project)
    {
        var entry = new RecentProject(Path.TrimEndingDirectorySeparator(project.Root), project.Title);
        entries = new[] { entry }.Concat(Read()).Concat(entries)
            .DistinctBy(candidate => candidate.Path, StringComparer.OrdinalIgnoreCase)
            .Take(Limit).ToList();

        string temporary = filePath + $".{Guid.NewGuid():N}.tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            try
            {
                File.WriteAllText(temporary, JsonSerializer.Serialize(entries));
                File.Move(temporary, filePath, overwrite: true);
            }
            finally
            {
                File.Delete(temporary);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Trace.WriteLine($"Could not save recent projects. {ex.Message}");
        }
    }

    private List<RecentProject> Read()
    {
        try
        {
            using var file = File.OpenRead(filePath);
            return (JsonSerializer.Deserialize<List<RecentProject?>>(file) ?? [])
                .OfType<RecentProject>()
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Path) && Path.IsPathFullyQualified(entry.Path) &&
                    !string.IsNullOrWhiteSpace(entry.Title))
                .DistinctBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)
                .Take(Limit).ToList();
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            return [];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Trace.WriteLine($"Could not read recent projects. {ex.Message}");
            return [];
        }
    }
}
