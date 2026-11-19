using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;

namespace RpgEditor;

internal sealed class ProjectFileChange(string destination, string? staged)
{
    private string? backup;
    private bool installed;

    public static ProjectFileChange Stage(string path, object? data)
    {
        if (data is null)
            return new ProjectFileChange(path, null);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string staged = path + $".{Guid.NewGuid():N}.tmp";
        try
        {
            switch (data)
            {
                case ItemData item: XnaSerializer.Serialize(staged, item); break;
                case EntityData entity: XnaSerializer.Serialize(staged, entity); break;
                case QuestData quest: XnaSerializer.Serialize(staged, quest); break;
                case RPG metadata: XnaSerializer.Serialize(staged, metadata); break;
                default: throw new InvalidOperationException("Unsupported editor asset.");
            }
            return new ProjectFileChange(path, staged);
        }
        catch
        {
            File.Delete(staged);
            throw;
        }
    }

    public void Apply()
    {
        if (File.Exists(destination))
        {
            string path = destination + $".{Guid.NewGuid():N}.bak";
            if (staged is null)
                File.Move(destination, path);
            else
            {
                File.Replace(staged, destination, path);
                installed = true;
            }
            backup = path;
            return;
        }
        if (staged is not null)
        {
            File.Move(staged, destination);
            installed = true;
        }
    }

    public void RollBack()
    {
        if (installed)
            File.Delete(destination);
        if (backup is not null)
            File.Move(backup, destination);
        if (staged is not null)
            File.Delete(staged);
    }

    public void DiscardBackup()
    {
        if (backup is not null)
        {
            try { File.Delete(backup); }
            catch (IOException ex) { System.Diagnostics.Trace.WriteLine(ex); }
            catch (UnauthorizedAccessException ex) { System.Diagnostics.Trace.WriteLine(ex); }
        }
    }
}
