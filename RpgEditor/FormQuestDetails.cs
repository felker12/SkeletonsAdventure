using System.ComponentModel;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;
using SkeletonsAdventure.Content;

namespace RpgEditor;

public partial class FormQuestDetails : Form
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public QuestData? Quest { get; private set; }

    private readonly ErrorProvider errors;
    private EditorProject? project;
    private List<ItemData> rewards = [];

    public FormQuestDetails()
    {
        InitializeComponent();
        errors = new ErrorProvider(this) { BlinkStyle = ErrorBlinkStyle.NeverBlink };
        Disposed += (_, _) => errors.Dispose();
        AcceptButton = btnOK;
        CancelButton = btnCancel;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnOK.Click += SaveQuest;
        btnAddTask.Click += (_, _) => EditTask(-1);
        AddAction("Edit Task", lbTasks.Left, btnAddTask.Bottom + 6, () => EditTask(lbTasks.SelectedIndex, true));
        AddAction("Remove Task", lbTasks.Left + 94, btnAddTask.Bottom + 6, () => RemoveSelected(lbTasks));
        AddAction("Add Prerequisite", lbRequiredQuests.Left, lbRequiredQuests.Bottom + 7, AddPrerequisite);
        AddAction("Remove", lbRequiredQuests.Left + 138, lbRequiredQuests.Bottom + 7, () => RemoveSelected(lbRequiredQuests));
        AddAction("Reward Items…", lbRequiredQuests.Left, lbRequiredQuests.Bottom + 42, EditRewards);

        foreach (var number in new[] { nudLevel, nudDefence, nudAttack, nudXP, nudGold })
            number.Maximum = int.MaxValue;
    }

    internal void ConfigureProject(EditorProject current) => project = current;

    private void AddAction(string label, int x, int y, Action action)
    {
        var button = new Button { Text = label, AutoSize = true, Location = new Point(x, y) };
        button.Click += (_, _) => action();
        Controls.Add(button);
    }

    private static void RemoveSelected(ListBox list)
    {
        if (list.SelectedIndex >= 0)
            list.Items.RemoveAt(list.SelectedIndex);
    }

    private void AddPrerequisite()
    {
        if (project is null)
            return;
        var names = project.Assets.Where(entry => entry.Data is QuestData).Select(entry => entry.Name);
        if (project.IsGameContent)
            names = names.Concat(BuiltInQuests.Names);
        var available = names.Where(name => name != Quest?.Name && name != tbName.Text && !lbRequiredQuests.Items.Contains(name));
        if (ReferenceDialogs.PickName(this, "Choose a Quest Prerequisite", available) is { } name)
            lbRequiredQuests.Items.Add(name);
    }

    private void EditRewards()
    {
        if (project is not null &&
            ReferenceDialogs.EditItems(this, project, "Quest Reward Items", rewards.Select(item => new ItemBaseData(item))) is { } items)
        {
            rewards = items.Select(entry =>
            {
                var item = rewards.FirstOrDefault(current => current.Name == entry.Name)?.Clone() ?? new ItemData { Name = entry.Name };
                item.Quantity = entry.Quantity;
                return item;
            }).ToList();
        }
    }

    private void EditTask(int index, bool requireSelection = false)
    {
        if (requireSelection && index < 0)
            return;
        using var dialog = new FormAddTask();
        var enemies = project?.Assets.Where(entry => entry.Data is EnemyData).Select(entry => entry.Name) ?? [];
        dialog.Configure(enemies, project?.IsGameContent == true, index < 0 ? null : (BaseTaskData)lbTasks.Items[index]);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.TaskData is not { } task)
            return;
        if (index < 0)
            lbTasks.Items.Add(task);
        else
            lbTasks.Items[index] = task;
    }

    private void SaveQuest(object? sender, EventArgs e)
    {
        errors.Clear();
        if (string.IsNullOrWhiteSpace(tbName.Text) || tbName.Text.Contains(','))
        {
            errors.SetError(tbName, "Enter a quest name without commas.");
            tbName.Focus();
            return;
        }

        var quest = Quest?.Clone() ?? new QuestData();
        quest.Name = tbName.Text.Trim();
        quest.Description = tbDescription.Text;
        quest.IsCompleted = cbIsCompleted.Checked;
        quest.Active = cbActive.Checked;
        quest.RequirementData.Level = (int)nudLevel.Value;
        quest.RequirementData.Defence = (int)nudDefence.Value;
        quest.RequirementData.Attack = (int)nudAttack.Value;
        quest.RewardData.XP = (int)nudXP.Value;
        quest.RewardData.Coins = (int)nudGold.Value;
        quest.RewardData.Items = rewards.Select(item => item.Clone()).ToList();
        quest.RequiredQuestNameData = lbRequiredQuests.Items.Cast<string>().ToList();
        quest.BaseTasksData = lbTasks.Items.Cast<BaseTaskData>().Select(task => task.Clone()).ToList();
        if (quest.RequiredQuestNameData.Contains(quest.Name))
        {
            errors.SetError(lbRequiredQuests, "A quest cannot require itself.");
            return;
        }
        Quest = quest;
        DialogResult = DialogResult.OK;
    }

    public void LoadQuest(QuestData quest)
    {
        Quest = quest;
        tbName.Text = quest.Name;
        tbDescription.Text = quest.Description;
        cbIsCompleted.Checked = quest.IsCompleted;
        cbActive.Checked = quest.Active;
        nudLevel.Value = quest.RequirementData.Level;
        nudDefence.Value = quest.RequirementData.Defence;
        nudAttack.Value = quest.RequirementData.Attack;
        nudXP.Value = quest.RewardData.XP;
        nudGold.Value = quest.RewardData.Coins;
        rewards = quest.RewardData.Items.Select(item => item.Clone()).ToList();
        lbRequiredQuests.Items.Clear();
        lbRequiredQuests.Items.AddRange(quest.RequiredQuestNameData.ToArray());
        lbTasks.Items.Clear();
        lbTasks.Items.AddRange(quest.BaseTasksData.ToArray());
    }
}
