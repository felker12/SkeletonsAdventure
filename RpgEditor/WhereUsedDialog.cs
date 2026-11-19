namespace RpgEditor;

internal sealed class WhereUsedDialog : Form
{
    private readonly FlowLayoutPanel options = new() { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
        WrapContents = false };
    private readonly Button confirm = new() { AutoSize = true, DialogResult = DialogResult.OK };

    private WhereUsedDialog(string caption, string message, IReadOnlyList<AssetUsage> usages, string? action = null)
    {
        Text = caption;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(850, 375);
        MinimumSize = new Size(560, 275);
        MinimizeBox = false;
        ShowInTaskbar = false;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = usages.Count == 0 ? $"{message} No references found." : message, AutoSize = true }, 0, 0);

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, RowHeadersVisible = false, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, AllowUserToResizeRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "File", FillWeight = 35 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Where Used", FillWeight = 49 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Update", FillWeight = 16 });
        foreach (var usage in usages)
        {
            int index = grid.Rows.Add(usage.File, usage.Location, usage.CanPropagate ? "Editor" : "Manual");
            grid.Rows[index].Cells[1].ToolTipText = usage.Location;
        }
        layout.Controls.Add(grid, 0, 1);
        layout.Controls.Add(options, 0, 2);
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var close = new Button { Text = action is null ? "Close" : "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        buttons.Controls.Add(close);
        CancelButton = close;
        if (action is not null)
        {
            if (usages.Any(usage => !usage.CanPropagate))
                options.Controls.Add(new Label { Text = "Review the manual references and update them in Tiled or game code where needed.", AutoSize = true });
            confirm.Text = action;
            buttons.Controls.Add(confirm);
        }
        layout.Controls.Add(buttons, 0, 3);
        Controls.Add(layout);
        Shown += (_, _) => close.Focus();
    }

    public static void Inspect(Form owner, EditorAsset asset, IReadOnlyList<AssetUsage> usages)
    {
        using var dialog = new WhereUsedDialog("Where Used", $"References to '{asset.Name}'.", usages);
        dialog.ShowDialog(owner);
    }

    public static bool ConfirmRename(Form owner, EditorAsset asset, string newName, IReadOnlyList<AssetUsage> usages,
        out bool propagate)
    {
        using var dialog = new WhereUsedDialog("Rename Asset", $"Rename '{asset.Name}' to '{newName}' on the next save.", usages, "Rename");
        var update = new CheckBox { Text = "Update references managed by the editor", AutoSize = true, Checked = true };
        if (usages.Any(usage => usage.CanPropagate))
            dialog.options.Controls.Add(update);
        bool accepted = dialog.ShowDialog(owner) == DialogResult.OK;
        propagate = update.Checked;
        return accepted;
    }

    public static bool ConfirmRemove(Form owner, EditorProject project, EditorAsset asset, IReadOnlyList<AssetUsage> usages,
        out EditorAsset? replacement)
    {
        using var dialog = new WhereUsedDialog("Remove Asset", $"Remove '{asset.Name}' on the next save.", usages, "Remove Asset");
        var select = new ComboBox { Width = 340, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = nameof(EditorAsset.Name),
            AccessibleName = "Replacement asset" };
        if (usages.Any(usage => usage.CanPropagate))
        {
            var candidates = project.Assets.Where(candidate => project.CanReplaceReferences(asset, candidate))
                .OrderBy(candidate => candidate.Name).ToArray();
            dialog.options.Controls.Add(new Label
            {
                Text = candidates.Length > 0 ? "Choose a replacement for the editor references." :
                    "Remove the editor references before deleting this asset.",
                AutoSize = true
            });
            if (candidates.Length > 0)
            {
                select.Items.AddRange(candidates);
                dialog.options.Controls.Add(select);
                select.SelectedIndexChanged += (_, _) => dialog.confirm.Enabled = select.SelectedItem is EditorAsset;
            }
            dialog.confirm.Enabled = false;
        }
        bool accepted = dialog.ShowDialog(owner) == DialogResult.OK;
        replacement = select.SelectedItem as EditorAsset;
        return accepted;
    }
}
