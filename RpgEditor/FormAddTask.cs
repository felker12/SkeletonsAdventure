using RpgLibrary.QuestClasses;

namespace RpgEditor;

public partial class FormAddTask : Form
{
    private readonly ComboBox enemies = new() { DropDownStyle = ComboBoxStyle.DropDown };
    private readonly ErrorProvider errors;
    private bool requireKnownEnemy;

    public FormAddTask()
    {
        InitializeComponent();
        errors = new ErrorProvider(this) { BlinkStyle = ErrorBlinkStyle.NeverBlink };
        Disposed += (_, _) => errors.Dispose();
        enemies.Bounds = tbMonster.Bounds;
        enemies.TabIndex = tbMonster.TabIndex;
        enemies.AccessibleName = "Enemy type";
        tbMonster.Visible = false;
        Controls.Add(enemies);
        cbType.SelectedIndex = 0;
        nudRequired.Minimum = 1;
        nudRequired.Maximum = nudCompleted.Maximum = int.MaxValue;
        btnOK.DialogResult = DialogResult.None;
        btnOK.Click += SaveTask;
        cbType.SelectedIndexChanged += (_, _) => UpdateTaskType();
        UpdateTaskType();
    }

    internal BaseTaskData? TaskData { get; private set; }

    internal void Configure(IEnumerable<string> types, bool contentProject, BaseTaskData? task)
    {
        requireKnownEnemy = contentProject;
        enemies.Items.AddRange(types.Distinct().Order().ToArray());
        if (task is null)
            return;

        cbType.SelectedIndex = task is SlayTaskData ? 1 : 0;
        tbDescription.Text = task.TaskToComplete;
        nudRequired.Value = Math.Max(1, task.RequiredAmount);
        nudCompleted.Value = Math.Max(0, task.CompletedAmount);
        if (task is SlayTaskData slay)
            enemies.Text = slay.MonsterToSlay;
    }

    private void UpdateTaskType() => lblMonster.Visible = enemies.Visible = cbType.SelectedIndex == 1;

    private void SaveTask(object? sender, EventArgs e)
    {
        errors.Clear();
        if (string.IsNullOrWhiteSpace(tbDescription.Text))
        {
            errors.SetError(tbDescription, "Enter a task description.");
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
