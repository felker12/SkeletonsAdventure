namespace RpgEditor;

internal static class AssetList
{
    public static void Bind(Form owner, ListBox list, Button add, Button edit, Button delete,
        Func<EditorAsset, bool> matches, Func<EditorProject, object?, object?> showEditor)
    {
        FormMain Main() => (FormMain)owner.MdiParent!;
        EditorProject Project() => Main().Project!;

        list.DisplayMember = nameof(EditorAsset.Name);
        edit.Enabled = delete.Enabled = false;
        void Refresh(string? selectedPath = null)
        {
            selectedPath ??= (list.SelectedItem as EditorAsset)?.RelativePath;
            var entries = Project().Assets.Where(matches).OrderBy(entry => entry.Name).ToArray();
            list.DataSource = entries;
            list.SelectedIndex = Array.FindIndex(entries, entry => entry.RelativePath == selectedPath);
        }

        void Change(EditorAsset? original)
        {
            var replacement = showEditor(Project(), original?.Data);
            if (replacement is null)
                return;
            try
            {
                Project().Change(original, replacement);
                Main().RefreshProjectStatus();
                Refresh(original?.RelativePath);
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show(owner, ex.Message, "Invalid Asset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        owner.Activated += (_, _) => Refresh();
        list.SelectedIndexChanged += (_, _) => edit.Enabled = delete.Enabled = list.SelectedItem is EditorAsset;
        add.Click += (_, _) => Change(null);
        edit.Click += (_, _) =>
        {
            if (list.SelectedItem is EditorAsset entry)
                Change(entry);
        };
        list.DoubleClick += (_, _) => edit.PerformClick();
        delete.Click += (_, _) =>
        {
            if (list.SelectedItem is EditorAsset entry &&
                MessageBox.Show(owner, $"Remove '{entry.Name}' on the next save?", "Remove Asset",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Project().Remove(entry);
                Main().RefreshProjectStatus();
                Refresh();
            }
        };
    }
}
