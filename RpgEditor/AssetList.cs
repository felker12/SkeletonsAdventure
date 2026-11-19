namespace RpgEditor;

internal static class AssetList
{
    public static void Bind(Form owner, ListBox list, Button add, Button edit, Button delete,
        Func<EditorAsset, bool> matches, Func<EditorProject, object?, object?> showEditor)
    {
        FormMain Main() => (FormMain)owner.MdiParent!;
        EditorProject Project() => Main().Project!;

        list.DisplayMember = nameof(EditorAsset.Name);
        var whereUsed = new Button { Name = "whereUsed", Text = "Where Used…", Width = Math.Max(96, delete.Width),
            Height = delete.Height, Anchor = delete.Anchor, Enabled = false };
        whereUsed.Location = delete.Right + whereUsed.Width + 12 <= owner.ClientSize.Width
            ? new Point(delete.Right + 8, delete.Top)
            : new Point(Math.Min(delete.Left, owner.ClientSize.Width - whereUsed.Width - 8), delete.Bottom + 6);
        owner.Controls.Add(whereUsed);
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
                bool propagate = false;
                if (original is not null)
                {
                    string newName = new EditorAsset(original.RelativePath, replacement).Name;
                    if (original.Name != newName &&
                        !WhereUsedDialog.ConfirmRename(owner, original, newName, Project().FindUsages(original), out propagate))
                        return;
                }
                Project().Change(original, replacement, propagate);
                Main().RefreshProjectStatus();
                Refresh(original?.RelativePath);
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show(owner, ex.Message, "Invalid Asset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        owner.Activated += (_, _) => Refresh();
        list.SelectedIndexChanged += (_, _) => whereUsed.Enabled = edit.Enabled = delete.Enabled = list.SelectedItem is EditorAsset;
        whereUsed.Click += (_, _) =>
        {
            if (list.SelectedItem is EditorAsset entry)
                WhereUsedDialog.Inspect(owner, entry, Project().FindUsages(entry));
        };
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
                WhereUsedDialog.ConfirmRemove(owner, Project(), entry, Project().FindUsages(entry), out var replacement))
            {
                try
                {
                    Project().Remove(entry, replacement);
                    Main().RefreshProjectStatus();
                    Refresh();
                }
                catch (InvalidDataException ex)
                {
                    MessageBox.Show(owner, ex.Message, "Invalid Asset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        };
    }
}
