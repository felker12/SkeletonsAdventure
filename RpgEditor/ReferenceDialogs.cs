using RpgLibrary.ItemClasses;

namespace RpgEditor;

internal static class ReferenceDialogs
{
    public static string? PickName(Form owner, string title, IEnumerable<string> names)
    {
        var choices = names.Distinct().Order().ToArray();
        var select = new ComboBox { Width = 340, DropDownStyle = ComboBoxStyle.DropDownList, DataSource = choices };
        var apply = new Button { Text = "Add", DialogResult = DialogResult.OK, Enabled = choices.Length > 0 };
        using var dialog = CreateDialog(title, apply, select);
        return dialog.ShowDialog(owner) == DialogResult.OK ? (string?)select.SelectedItem : null;
    }

    public static List<ItemBaseData>? EditItems(Form owner, EditorProject project, string title, IEnumerable<ItemBaseData> original)
    {
        var templates = project.Assets.Select(entry => entry.Data).OfType<ItemData>().OrderBy(item => item.Name).ToArray();
        var select = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = nameof(ItemData.Name),
            DataSource = templates };
        var quantity = new NumericUpDown { Minimum = 1, Maximum = int.MaxValue, Width = 90 };
        var list = new ListBox { Width = 470, Height = 160 };
        foreach (var item in original)
            list.Items.Add(new ItemBaseData(item.Name, item.Quantity));
        var add = new Button { Text = "Add / Set", AutoSize = true, Enabled = templates.Length > 0 };
        var remove = new Button { Text = "Remove", AutoSize = true };
        var apply = new Button { Text = "OK", DialogResult = DialogResult.OK };
        var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        row.Controls.AddRange([select, new Label { Text = "Quantity", AutoSize = true }, quantity, add]);
        using var dialog = CreateDialog(title, apply, row, list, remove);
        void UpdateQuantity()
        {
            quantity.Enabled = select.SelectedItem is ItemData { Stackable: true };
            if (!quantity.Enabled)
                quantity.Value = 1;
        }
        select.SelectedIndexChanged += (_, _) => UpdateQuantity();
        UpdateQuantity();
        list.SelectedIndexChanged += (_, _) =>
        {
            if (list.SelectedItem is not ItemBaseData item)
                return;
            select.SelectedItem = templates.FirstOrDefault(template => template.Name == item.Name);
            if (quantity.Enabled)
                quantity.Value = Math.Max(1, item.Quantity);
        };
        add.Click += (_, _) =>
        {
            if (select.SelectedItem is ItemData item)
            {
                var entry = new ItemBaseData(item.Name, (int)quantity.Value);
                for (int i = 0; i < list.Items.Count; i++)
                {
                    if (((ItemBaseData)list.Items[i]).Name != item.Name)
                        continue;
                    list.Items[i] = entry;
                    return;
                }
                list.Items.Add(entry);
            }
        };
        remove.Click += (_, _) =>
        {
            if (list.SelectedIndex >= 0)
                list.Items.RemoveAt(list.SelectedIndex);
        };
        return dialog.ShowDialog(owner) == DialogResult.OK ? list.Items.Cast<ItemBaseData>().ToList() : null;
    }

    private static Form CreateDialog(string title, Button apply, params Control[] fields)
    {
        var dialog = new Form { Text = title, StartPosition = FormStartPosition.CenterParent, AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimizeBox = false, MaximizeBox = false };
        var stack = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new(12) };
        var actions = new FlowLayoutPanel { AutoSize = true };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
        actions.Controls.AddRange([apply, cancel]);
        stack.Controls.AddRange(fields);
        stack.Controls.Add(actions);
        dialog.Controls.Add(stack);
        dialog.AcceptButton = apply;
        dialog.CancelButton = cancel;
        return dialog;
    }
}
