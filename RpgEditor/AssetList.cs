using RpgLibrary.ItemClasses;

namespace RpgEditor;

internal static class AssetList
{
    public static void Bind(Form owner, ListBox list, Button add, Button edit, Button delete,
        Func<EditorAsset, bool> matches, Func<EditorProject, object?, object?> showEditor, bool itemList = false)
    {
        FormMain Main() => (FormMain)owner.MdiParent!;
        EditorProject Project() => Main().Project!;

        list.DisplayMember = nameof(EditorAsset.Name);
        var search = new TextBox { Name = "assetSearch", PlaceholderText = "Search assets", AccessibleName = "Search assets",
            Location = list.Location, Width = list.Width,
            Anchor = AnchorStyles.Top | (list.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) };
        list.Top += search.Height + 6;
        list.Height -= search.Height + 6;
        owner.Controls.Add(search);

        var whereUsed = new Button { Name = "whereUsed", Text = "Where Used…", Width = Math.Max(96, delete.Width),
            Height = delete.Height, Anchor = delete.Anchor, Enabled = false };
        whereUsed.Location = delete.Right + whereUsed.Width + 12 <= owner.ClientSize.Width
            ? new Point(delete.Right + 8, delete.Top)
            : new Point(Math.Min(delete.Left, owner.ClientSize.Width - whereUsed.Width - 8), delete.Bottom + 6);
        owner.Controls.Add(whereUsed);
        Button? duplicate = null;
        if (itemList)
        {
            duplicate = new Button { Name = "duplicateAsset", Text = "Duplicate", Width = whereUsed.Width, Height = whereUsed.Height,
                Location = new Point(whereUsed.Right + 8, whereUsed.Top), Anchor = whereUsed.Anchor, Enabled = false };
            if (duplicate.Right > owner.ClientSize.Width - 8)
                duplicate.Location = new Point(whereUsed.Left, whereUsed.Bottom + 6);
            owner.Controls.Add(duplicate);

            var previews = new AssetThumbnails(() => Project().Root);
            list.DrawMode = DrawMode.OwnerDrawFixed;
            list.ItemHeight = 40;
            list.DrawItem += (_, e) =>
            {
                if (e.Index >= 0 && e.Index < list.Items.Count && list.Items[e.Index] is EditorAsset { Data: ItemData item })
                    previews.Draw(e, item);
            };
            owner.Disposed += (_, _) => previews.Dispose();
        }
        edit.Enabled = delete.Enabled = false;
        void Refresh(string? selectedPath = null)
        {
            selectedPath ??= (list.SelectedItem as EditorAsset)?.RelativePath;
            var entries = Project().Assets.Where(entry => matches(entry) && EditorSearch.Matches(search.Text, entry.Name))
                .OrderBy(entry => entry.Name).ToArray();
            list.DataSource = entries;
            list.SelectedIndex = Array.FindIndex(entries, entry => entry.RelativePath == selectedPath);
        }

        void Change(EditorAsset? original, ItemData? draftCopy = null)
        {
            try
            {
                var replacement = showEditor(Project(), original?.Data ?? draftCopy);
                if (replacement is null)
                    return;

                bool propagate = false;
                if (original is not null)
                {
                    string newName = new EditorAsset(original.RelativePath, replacement).Name;
                    if (original.Name != newName &&
                        !WhereUsedDialog.ConfirmRename(owner, original, newName, Project().FindUsages(original), out propagate))
                        return;
                }
                var saved = Project().Change(original, replacement, propagate);
                Main().RefreshProjectStatus();
                if (!EditorSearch.Matches(search.Text, saved.Name))
                    search.Clear();
                Refresh(saved.RelativePath);
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show(owner, ex.Message, "Invalid Asset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        search.TextChanged += (_, _) => Refresh();
        owner.Activated += (_, _) => Refresh();
        owner.Load += (_, _) =>
        {
            var main = Main();
            void RefreshReloaded() => Refresh();
            main.ProjectAssetsReloaded += RefreshReloaded;
            owner.Disposed += (_, _) => main.ProjectAssetsReloaded -= RefreshReloaded;
        };
        list.SelectedIndexChanged += (_, _) =>
        {
            whereUsed.Enabled = edit.Enabled = delete.Enabled = list.SelectedItem is EditorAsset;
            if (duplicate is not null)
                duplicate.Enabled = list.SelectedItem is EditorAsset { Data: ItemData };
        };
        if (duplicate is not null)
            duplicate.Click += (_, _) =>
            {
                if (list.SelectedItem is EditorAsset { Data: ItemData item })
                    Change(null, CreateDraftCopy(Project(), item));
            };
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

    internal static ItemData CreateDraftCopy(EditorProject project, ItemData item)
    {
        var existing = project.Assets.Where(entry => entry.Data is ItemData).Select(entry => entry.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var draft = item.Clone();
        string stem = item.Name + " Copy";
        draft.Name = stem;
        for (int version = 2; existing.Contains(draft.Name); version++)
            draft.Name = $"{stem} {version}";
        return draft;
    }
}
