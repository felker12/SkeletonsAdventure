using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.Content;

namespace RpgEditor;

internal sealed class EnemyTemplateDialog : Form
{
    private readonly ComboBox enemyClass = new() { Name = "enemyClass", DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
    private readonly ComboBox texture = new() { Name = "enemyTexture", DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
    private readonly ComboBox dropTable = new() { Name = "enemyDropTable", DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
    private readonly NumericUpDown health = Number("baseEnemyHealth", 1);
    private readonly NumericUpDown attack = Number("baseEnemyAttack");
    private readonly NumericUpDown defence = Number("baseEnemyDefence");
    private readonly NumericUpDown xp = Number("baseEnemyXp");
    private readonly ListView drops = Details("guaranteedDrops", ("Item", 180), ("Quantity", 75), ("Source", 105));
    private readonly ListView spawnList = Details("enemySpawns", ("Map", 270), ("Object ID", 90), ("Position", 150), ("Tiled Level", 110));
    private readonly Label spawnStatus = new() { AutoSize = true };
    private readonly Label classStatus = new() { AutoSize = true };
    private readonly Button save = new() { Name = "acceptEnemyTemplate", Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
    private readonly EnemyData original;
    private readonly TiledEnemySpawns spawns;
    private readonly EditorProject project;
    private List<ItemBaseData> guaranteed;

    public EnemyTemplateDialog(EditorProject project, EnemyData? data)
    {
        this.project = project;
        original = data ?? new EnemyData { BaseHealth = 1 };
        guaranteed = original.GuaranteedItems.Select(item => new ItemBaseData(item.Name, item.Quantity)).ToList();
        spawns = TiledEnemySpawns.Read(project.Root);

        Text = data is null ? "New Enemy Template" : "Enemy Template";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(760, 560);
        MinimumSize = new Size(700, 520);
        ShowInTaskbar = false;
        Controls.Add(BuildLayout());

        var used = project.Assets.Select(entry => entry.Data).OfType<EnemyData>()
            .Where(enemy => !ReferenceEquals(enemy, data)).Select(enemy => enemy.Type).ToHashSet(StringComparer.Ordinal);
        enemyClass.DisplayMember = nameof(Choice.Label);
        enemyClass.Items.AddRange(EnemyClassCatalog.Entries.Where(entry => !used.Contains(entry.Type))
            .Select(entry => new Choice(entry.Type, EnemyClassCatalog.SpawnName(entry.Type))).OrderBy(choice => choice.Label).ToArray());
        texture.Items.AddRange(ContentReferenceValidator.ReadTextureCatalog(project.Root).Keys.Order(StringComparer.Ordinal).ToArray());
        texture.SelectedItem = original.TextureName;
        DropTableChoice[] tables = [new("", "None"), new(DataAssetCatalog.BasicDropTable, "Basic Drop Table")];
        dropTable.DisplayMember = nameof(DropTableChoice.Label);
        dropTable.Items.AddRange(tables);
        dropTable.SelectedItem = tables.FirstOrDefault(table => table.Id == original.DropTableName);
        enemyClass.SelectedItem = enemyClass.Items.Cast<Choice>().FirstOrDefault(choice => choice.Type == original.Type);

        health.Value = Math.Max(1, original.BaseHealth);
        attack.Value = Math.Max(0, original.BaseAttack);
        defence.Value = Math.Max(0, original.BaseDefence);
        xp.Value = Math.Max(0, original.BaseXP);
        enemyClass.SelectedIndexChanged += (_, _) => RefreshClass();
        texture.SelectedIndexChanged += (_, _) => UpdateSaveState();
        dropTable.SelectedIndexChanged += (_, _) => UpdateSaveState();
        RefreshClass();
    }

    public EnemyData ReadTemplate()
    {
        var selected = (Choice)enemyClass.SelectedItem!;
        return new EnemyData(original)
        {
            Type = selected.Type,
            TextureName = (string)texture.SelectedItem!,
            BaseHealth = (int)health.Value,
            CurrentHealth = (int)health.Value,
            BaseAttack = (int)attack.Value,
            BaseDefence = (int)defence.Value,
            BaseXP = (int)xp.Value,
            IsDead = false,
            LastDeathTime = TimeSpan.Zero,
            DropTableName = ((DropTableChoice)dropTable.SelectedItem!).Id,
            GuaranteedItems = guaranteed
        };
    }

    private Control BuildLayout()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(12) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 285));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var stats = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 2 };
        AddRow(stats, "Enemy class", enemyClass);
        AddRow(stats, "Texture", texture);
        AddRow(stats, "Base health", health);
        AddRow(stats, "Base attack", attack);
        AddRow(stats, "Base defence", defence);
        AddRow(stats, "Base XP", xp);
        AddRow(stats, "Random drops", dropTable);
        int statusRow = stats.RowCount++;
        classStatus.MaximumSize = new Size(260, 0);
        stats.Controls.Add(classStatus, 0, statusRow);
        stats.SetColumnSpan(classStatus, 2);
        layout.Controls.Add(stats, 0, 0);

        var loot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Height = 225 };
        loot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        loot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        loot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        loot.Controls.Add(new Label { Text = "Guaranteed Drops", AutoSize = true }, 0, 0);
        loot.Controls.Add(drops, 0, 1);
        var editDrops = new Button { Text = "Edit Drops…", AutoSize = true };
        editDrops.Click += (_, _) =>
        {
            if (ReferenceDialogs.EditItems(this, project, "Guaranteed Enemy Drops", guaranteed) is { } items)
            {
                guaranteed = items;
                RefreshDrops();
            }
        };
        loot.Controls.Add(editDrops, 0, 2);
        layout.Controls.Add(loot, 1, 0);

        layout.Controls.Add(spawnStatus, 0, 1);
        layout.SetColumnSpan(spawnStatus, 2);
        layout.Controls.Add(spawnList, 0, 2);
        layout.SetColumnSpan(spawnList, 2);
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        actions.Controls.AddRange([cancel, save]);
        layout.Controls.Add(actions, 0, 3);
        layout.SetColumnSpan(actions, 2);
        AcceptButton = save;
        CancelButton = cancel;
        return layout;
    }

    private void RefreshClass()
    {
        string? selected = (enemyClass.SelectedItem as Choice)?.Type;
        classStatus.Text = enemyClass.Items.Count == 0 ? "All registered enemy classes already have templates." :
            selected is null && original.Type.Length > 0 ? $"Choose a registered class to replace '{original.Type}'." : string.Empty;
        var entries = selected is null ? [] : spawns.ForClass(selected).ToArray();
        spawnList.BeginUpdate();
        spawnList.Items.Clear();
        foreach (var spawn in entries)
            spawnList.Items.Add(new ListViewItem([spawn.File, spawn.ObjectId, spawn.Position, spawn.Level]));
        foreach (var issue in spawns.UnreadableMaps)
            spawnList.Items.Add(new ListViewItem([issue.File, "—", "Could not read map", "—"]) { ToolTipText = issue.Error });
        spawnList.EndUpdate();
        spawnStatus.Text = $"Tiled Spawns ({entries.Length}) · Edit positions and levels in Tiled";
        RefreshDrops();
        UpdateSaveState();
    }

    private void RefreshDrops()
    {
        drops.Items.Clear();
        foreach (var drop in guaranteed)
            drops.Items.Add(new ListViewItem([drop.Name, drop.Quantity.ToString(), "Template"]));
        if (enemyClass.SelectedItem is Choice choice &&
            EnemyClassCatalog.Entries.Any(entry => entry.Type == choice.Type && entry.DefaultBones) &&
            guaranteed.All(item => item.Name != DataAssetCatalog.Bones) &&
            project.Assets.Select(entry => entry.Data).OfType<ItemData>().FirstOrDefault(item => item.Name == DataAssetCatalog.Bones) is { } bones)
            drops.Items.Add(new ListViewItem([bones.Name, bones.Quantity.ToString(), "Game default"]));
    }

    private void UpdateSaveState() =>
        save.Enabled = enemyClass.SelectedItem is Choice && texture.SelectedItem is string && dropTable.SelectedItem is DropTableChoice;

    private static NumericUpDown Number(string name, int minimum = 0) =>
        new() { Name = name, Minimum = minimum, Maximum = int.MaxValue, Width = 100 };

    private static void AddRow(TableLayoutPanel layout, string label, Control field)
    {
        int row = layout.RowCount++;
        layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        field.AccessibleName = label;
        layout.Controls.Add(field, 1, row);
    }

    private static ListView Details(string name, params (string Label, int Width)[] columns)
    {
        var list = new ListView { Name = name, View = View.Details, Dock = DockStyle.Fill, FullRowSelect = true, ShowItemToolTips = true };
        foreach (var (label, width) in columns)
            list.Columns.Add(label, width);
        return list;
    }

    private sealed record Choice(string Type, string Label);
    private sealed record DropTableChoice(string Id, string Label);
}
