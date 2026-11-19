namespace RpgEditor;

internal enum ProjectConflictChoice
{
    Cancel,
    Reload,
    Overwrite
}

internal static class ProjectConflictDialog
{
    public static ProjectConflictChoice Show(Form owner, ProjectSaveConflictException conflict)
    {
        using var dialog = new Form { Text = "File Changed on Disk", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false };
        var content = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            Padding = new Padding(12) };
        var message = new Label { AutoSize = true, MaximumSize = new Size(520, 0),
            Text = conflict.Revision is null ? "This asset file was removed outside the editor." : "This asset file changed outside the editor." };
        var file = new TextBox { Text = conflict.FilePath, Width = 520, ReadOnly = true };
        var guidance = new Label { AutoSize = true, MaximumSize = new Size(520, 0),
            Text = "Reload discards the pending change to this file. Overwrite applies the editor's pending edit or deletion." };
        var actions = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        var reload = new Button { Text = "Reload from Disk", AutoSize = true, DialogResult = DialogResult.Retry };
        var overwrite = new Button { Text = "Overwrite", AutoSize = true, DialogResult = DialogResult.Yes };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        actions.Controls.AddRange([reload, overwrite, cancel]);
        content.Controls.AddRange([message, file, guidance, actions]);
        dialog.Controls.Add(content);
        dialog.CancelButton = cancel;
        dialog.Shown += (_, _) => cancel.Focus();
        return dialog.ShowDialog(owner) switch
        {
            DialogResult.Retry => ProjectConflictChoice.Reload,
            DialogResult.Yes => ProjectConflictChoice.Overwrite,
            _ => ProjectConflictChoice.Cancel
        };
    }
}
