using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;

namespace RpgEditor;

internal sealed class ProjectFileChange(string destination, string? staged, string? revision)
{
    private string? backup;
    private bool installed;
    public string? Revision { get; } = revision;

    public static ProjectFileChange Stage(string path, object? data)
    {
        if (data is null)
            return new ProjectFileChange(path, null, null);

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
            return new ProjectFileChange(path, staged, ProjectFileRevision.Read(staged));
        }
        catch
        {
            File.Delete(staged);
            throw;
        }
    }

    public void Apply(string? expectedRevision)
    {
        string? actual = ProjectFileRevision.Read(destination);
        if (actual != expectedRevision)
            throw new ProjectSaveConflictException(destination, actual);

        if (expectedRevision is not null)
        {
            string path = destination + $".{Guid.NewGuid():N}.bak";
            try
            {
                if (staged is null)
                    File.Move(destination, path);
                else
                {
                    File.Replace(staged, destination, path);
                    installed = true;
                }
            }
            catch (FileNotFoundException) when (ProjectFileRevision.Read(destination) is null)
            {
                throw new ProjectSaveConflictException(destination, null);
            }
            backup = path;
            actual = ProjectFileRevision.Read(backup);
            if (actual != expectedRevision)
                throw new ProjectSaveConflictException(destination, actual);
            return;
        }
        if (staged is not null)
        {
            try { File.Move(staged, destination); }
            catch (IOException) when (File.Exists(destination))
            {
                throw new ProjectSaveConflictException(destination, ProjectFileRevision.Read(destination));
            }
            installed = true;
        }
    }

    public void RollBack()
    {
        if (installed)
        {
            if (ProjectFileRevision.Read(destination) != Revision)
                throw new IOException($"'{destination}' changed during rollback. Its recovery file was retained.");
            File.Delete(destination);
        }
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
