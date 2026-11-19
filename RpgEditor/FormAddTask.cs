using RpgLibrary.QuestClasses;

namespace RpgEditor;

public partial class FormAddTask : Form
{
    private const string Interact = "Interact with object";
    private const string Reach = "Reach area";
    private readonly ComboBox enemies = new() { DropDownStyle = ComboBoxStyle.DropDown };
    private readonly ComboBox locations = new() { Name = "questLocation", DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label locationHint = new() { AutoSize = true };
    private readonly ErrorProvider errors;
    private bool requireKnownEnemy;
    private IReadOnlyList<ContentReferenceValidator.QuestTarget> targets = [];
    private LocationTaskData? previousLocation;

    public FormAddTask()
    {
        InitializeComponent();
        errors = new ErrorProvider(this) { BlinkStyle = ErrorBlinkStyle.NeverBlink };
        Disposed += (_, _) => errors.Dispose();
        ClientSize = new Size(510, ClientSize.Height);
        tbDescription.Width = 390;
        btnCancel.Left = ClientSize.Width - 95;
        btnOK.Left = btnCancel.Left - 85;
        enemies.Bounds = tbMonster.Bounds;
        enemies.TabIndex = tbMonster.TabIndex;
        enemies.AccessibleName = "Enemy type";
        tbMonster.Visible = false;
        locations.SetBounds(tbMonster.Left, nudRequired.Top, 390, tbMonster.Height);
        locations.TabIndex = tbMonster.TabIndex;
        locations.AccessibleName = "Tiled target";
        locations.DropDownWidth = 470;
        locationHint.Location = new Point(locations.Left, locations.Bottom + 8);
        Controls.AddRange([enemies, locations, locationHint]);
        cbType.SelectedIndex = 0;
        nudRequired.Minimum = 1;
        nudRequired.Maximum = nudCompleted.Maximum = int.MaxValue;
        btnOK.DialogResult = DialogResult.None;
        btnOK.Click += SaveTask;
        cbType.SelectedIndexChanged += (_, _) => UpdateTaskType();
        UpdateTaskType();
    }

    internal BaseTaskData? TaskData { get; private set; }

    internal void Configure(IEnumerable<string> types, bool contentProject, BaseTaskData? task,
        IReadOnlyList<ContentReferenceValidator.QuestTarget>? mapTargets = null)
    {
        requireKnownEnemy = contentProject;
        enemies.Items.AddRange(types.Distinct().Order().ToArray());
        targets = mapTargets ?? [];
        if (contentProject || task is LocationTaskData)
            cbType.Items.AddRange([Interact, Reach]);
        previousLocation = task as LocationTaskData;
        cbType.SelectedItem = task switch
        {
            LocationTaskData { Action: QuestLocationAction.Interact } => Interact,
            LocationTaskData => Reach,
            SlayTaskData or null => "Slay",
            _ => "Base"
        };
        if (task is null)
            return;

        tbDescription.Text = task.TaskToComplete;
        nudRequired.Value = Math.Max(1, task.RequiredAmount);
        nudCompleted.Value = Math.Max(0, task.CompletedAmount);
        if (task is SlayTaskData slay)
            enemies.Text = slay.MonsterToSlay;
    }

    private QuestLocationAction? SelectedAction => cbType.SelectedItem switch
    {
        Interact => QuestLocationAction.Interact,
        Reach => QuestLocationAction.ReachArea,
        _ => null
    };

    private void UpdateTaskType()
    {
        var action = SelectedAction;
        bool slay = Equals(cbType.SelectedItem, "Slay");
        lblMonster.Visible = slay || action.HasValue;
        lblMonster.Text = slay ? "Monster:" : "Tiled target:";
        lblMonster.Top = action.HasValue ? locations.Top + 3 : enemies.Top + 3;
        enemies.Visible = slay;
        locations.Visible = locationHint.Visible = action.HasValue;
        lblRequired.Visible = nudRequired.Visible = lblCompleted.Visible = nudCompleted.Visible = !action.HasValue;
        if (action is not { } selected)
            return;

        locations.Items.Clear();
        locations.Items.AddRange(targets.Where(target => target.Action == selected).ToArray());
        locations.SelectedItem = locations.Items.Cast<ContentReferenceValidator.QuestTarget>().FirstOrDefault(target =>
            previousLocation is not null && target.Map == previousLocation.MapName && target.ObjectId == previousLocation.ObjectId &&
            target.Action == previousLocation.Action);
        if (selected == QuestLocationAction.Interact)
            locationHint.Text = locations.Items.Count > 0 ? "Completes when the player uses this object." : "Add an interactable object in Tiled first.";
        else
            locationHint.Text = locations.Items.Count > 0 ? "Completes when the player enters this area." :
                "Add a rectangle to the QuestAreas object layer in Tiled.";
    }

    private void SaveTask(object? sender, EventArgs e)
    {
        errors.Clear();
        if (string.IsNullOrWhiteSpace(tbDescription.Text))
        {
            errors.SetError(tbDescription, "Enter a task description.");
            return;
        }
        if (SelectedAction is { } action)
        {
            if (locations.SelectedItem is not ContentReferenceValidator.QuestTarget target)
            {
                errors.SetError(locations, "Select a Tiled target.");
                return;
            }
            TaskData = new LocationTaskData
            {
                Action = action,
                MapName = target.Map,
                ObjectId = target.ObjectId,
                TaskToComplete = tbDescription.Text.Trim(),
                CompletedAmount = previousLocation is { CompletedAmount: 1 } && previousLocation.Action == action &&
                    previousLocation.MapName == target.Map && previousLocation.ObjectId == target.ObjectId ? 1 : 0
            };
            DialogResult = DialogResult.OK;
            return;
        }

        if (nudCompleted.Value > nudRequired.Value)
        {
            errors.SetError(nudCompleted, "Completed count cannot exceed the required count.");
            return;
        }

        BaseTaskData task = new();
        if (cbType.SelectedIndex == 1)
        {
            string enemy = enemies.Text.Trim();
            if (enemy.Length == 0 || (requireKnownEnemy && !enemies.Items.Contains(enemy)))
            {
                errors.SetError(enemies, "Select an enemy from the project.");
                return;
            }
            task = new SlayTaskData { MonsterToSlay = enemy };
        }
        task.TaskToComplete = tbDescription.Text;
        task.RequiredAmount = (int)nudRequired.Value;
        task.CompletedAmount = (int)nudCompleted.Value;
        TaskData = task;
        DialogResult = DialogResult.OK;
    }
}
