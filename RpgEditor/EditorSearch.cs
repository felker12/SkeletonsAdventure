namespace RpgEditor;

internal static class EditorSearch
{
    public static bool Matches(string query, string label) => query
        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .All(term => label.Contains(term, StringComparison.OrdinalIgnoreCase));
}
