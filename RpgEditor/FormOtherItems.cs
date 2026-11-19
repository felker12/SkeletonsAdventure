using RpgLibrary.ItemClasses;

namespace RpgEditor;

internal sealed class FormOtherItems<T> : FormDetails where T : ItemData, new()
{
    public FormOtherItems(string title)
    {
        Text = title;
        ConfigureAssets(entry => entry.Data.GetType() == typeof(T), Edit);
    }

    private ItemData? Edit(EditorProject project, object? data)
    {
        var item = data is ItemData original ? original.Clone() : new T
        {
            Quantity = 1,
            SourceRectangle = new(0, 0, 32, 32)
        };
        if (data is null)
        {
            item.Type = item switch
            {
                ShieldData => "Shield",
                ConsumableData => "Consumable",
                _ => string.Empty
            };
            item.Consumable = item is ConsumableData;
        }
        using var dialog = new Form { Text = $"Edit {Text}", StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(510, 530), MinimizeBox = false, MaximizeBox = false };
        var grid = new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = item, ToolbarVisible = false };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, Padding = new(5) };
        var ok = new Button { Text = "OK" };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
        var selectTexture = new Button { Text = "Choose Sprite…", AutoSize = true };
        selectTexture.Click += (_, _) =>
        {
            var rectangle = item.SourceRectangle;
            using var picker = new TexturePicker(ContentReferenceValidator.TextureSources(project.Root),
                item.TexturePath, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
            if (picker.ShowDialog(dialog) == DialogResult.OK)
            {
                item.TexturePath = picker.AssetName;
                item.SourceRectangle = new(picker.Sprite.X, picker.Sprite.Y, picker.Sprite.Width, picker.Sprite.Height);
                grid.Refresh();
            }
        };
        ok.Click += (_, _) =>
        {
            string? error = ItemInput.Validate(item, project);
            if (error is null)
                dialog.DialogResult = DialogResult.OK;
            else
                MessageBox.Show(dialog, error, "Invalid Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
        actions.Controls.AddRange([ok, cancel, selectTexture]);
        dialog.Controls.Add(grid);
        dialog.Controls.Add(actions);
        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;
        return dialog.ShowDialog(this) == DialogResult.OK ? item : null;
    }
}
