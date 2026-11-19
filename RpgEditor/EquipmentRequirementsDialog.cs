using RpgLibrary.DataClasses;

namespace RpgEditor;

internal sealed class EquipmentRequirementsDialog : Form
{
    private readonly NumericUpDown level;
    private readonly NumericUpDown attack;
    private readonly NumericUpDown defence;
    private readonly NumericUpDown mana;

    public EquipmentRequirementsDialog(LevelRequirementData current)
    {
        Text = "Level Requirements";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        MinimizeBox = MaximizeBox = false;
        ShowInTaskbar = false;

        var fields = new TableLayoutPanel { AutoSize = true, Padding = new Padding(12), ColumnCount = 2 };
        level = AddField(fields, "Level", current.Level);
        attack = AddField(fields, "Attack", current.Attack);
        defence = AddField(fields, "Defence", current.Defence);
        mana = AddField(fields, "Mana", current.Mana);
        var ok = new Button { Name = "acceptRequirements", Text = "OK", DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
        var buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        buttons.Controls.AddRange([ok, cancel]);
        fields.Controls.Add(buttons, 0, 4);
        fields.SetColumnSpan(buttons, 2);
        Controls.Add(fields);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    public LevelRequirementData ReadRequirements() => new()
    {
        Level = (int)level.Value,
        Attack = (int)attack.Value,
        Defence = (int)defence.Value,
        Mana = (int)mana.Value
    };

    private static NumericUpDown AddField(TableLayoutPanel fields, string name, int current)
    {
        int row = fields.RowCount++;
        var input = new NumericUpDown
        {
            Name = name,
            AccessibleName = name,
            Maximum = int.MaxValue,
            Value = Math.Max(0, current),
            Width = 130
        };
        fields.Controls.Add(new Label { Text = name, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        fields.Controls.Add(input, 1, row);
        return input;
    }

    public static FlowLayoutPanel CreateSummary(Form owner, Func<LevelRequirementData> get, Action<LevelRequirementData> set)
    {
        var values = new Label { Name = "requirementValues", AutoSize = true, Margin = new Padding(3, 3, 12, 3) };
        var button = new Button { Name = "editRequirements", Text = "Requirements…", AutoSize = true };
        var panel = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false };
        panel.Controls.AddRange([values, button]);
        owner.Load += (_, _) => UpdateValues();
        button.Click += (_, _) =>
        {
            using var dialog = new EquipmentRequirementsDialog(get());
            if (dialog.ShowDialog(owner) == DialogResult.OK)
            {
                set(dialog.ReadRequirements());
                UpdateValues();
            }
        };
        return panel;

        void UpdateValues()
        {
            var requirements = get();
            values.Text = $"Level {requirements.Level}    Attack {requirements.Attack}{Environment.NewLine}" +
                $"Defence {requirements.Defence}    Mana {requirements.Mana}";
        }
    }
}
