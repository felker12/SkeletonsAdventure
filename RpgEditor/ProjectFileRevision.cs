using System.Security.Cryptography;

namespace RpgEditor;

internal sealed class ProjectSaveConflictException(string file, string? revision)
    : IOException($"'{file}' changed on disk after it was loaded.")
{
    public string FilePath { get; } = file;
    public string? Revision { get; } = revision;
}

internal static class ProjectFileRevision
{
    public static byte[]? ReadBytes(string file)
    {
        try { return File.ReadAllBytes(file); }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    public static string? Read(string file) => Of(ReadBytes(file));

    public static string? Of(byte[]? bytes) => bytes is null ? null : Convert.ToHexString(SHA256.HashData(bytes));
}
