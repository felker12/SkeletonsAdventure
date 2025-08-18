using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RpgLibrary.QuestClasses;
using RpgLibrary.WorldClasses;

namespace RpgEditor
{
    public partial class FormQuestDetails : Form
    {
        public QuestData? Quest { get; set; }

        public FormQuestDetails()
        {
            InitializeComponent();
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            btnAddTask.Click += BtnAddTask_Click;
        }

        void BtnOK_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("You must enter a quest name.");
                return;
            }

            Quest = new QuestData
            {
                Name = tbName.Text,
                Description = tbDescription.Text,
                IsCompleted = cbIsCompleted.Checked,
                Active = cbActive.Checked,
                RequirementData = new RequirementData
                {
                    Level = (int)nudLevel.Value,
                    Defence = (int)nudDefence.Value,
                    Attack = (int)nudAttack.Value
                },
                RewardData = new QuestRewardData
                {
                    XP = (int)nudXP.Value,
                    Coins = (int)nudGold.Value,
                    // Items can be added with a more advanced UI if needed
                },
                RequiredQuestNameData = new List<string>(lbRequiredQuests.Items.Cast<string>()),
                BaseTasksData = new List<BaseTaskData>(lbTasks.Items.Cast<BaseTaskData>())
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        void BtnCancel_Click(object? sender, EventArgs e)
        {
            Quest = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void BtnAddTask_Click(object? sender, EventArgs e)
        {
            using (var dialog = new FormAddTask())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    BaseTaskData taskData;
                    if (dialog.SelectedTaskType == "Slay")
                    {
                        taskData = new SlayTaskData
                        {
                            TaskToComplete = dialog.TaskDescription,
                            RequiredAmount = dialog.RequiredAmount,
                            CompletedAmount = dialog.CompletedAmount,
                            MonsterToSlay = dialog.MonsterToSlay
                        };
                    }
                    else
                    {
                        taskData = new BaseTaskData
                        {
                            TaskToComplete = dialog.TaskDescription,
                            RequiredAmount = dialog.RequiredAmount,
                            CompletedAmount = dialog.CompletedAmount
                        };
                    }
                    lbTasks.Items.Add(taskData);
                }
            }
        }

        public void LoadQuest(QuestData quest)
        {
            tbName.Text = quest.Name;
            tbDescription.Text = quest.Description;
            cbIsCompleted.Checked = quest.IsCompleted;
            cbActive.Checked = quest.Active;
            nudLevel.Value = quest.RequirementData.Level;
            nudDefence.Value = quest.RequirementData.Defence;
            nudAttack.Value = quest.RequirementData.Attack;
            nudXP.Value = quest.RewardData.XP;
            nudGold.Value = quest.RewardData.Coins;

            lbRequiredQuests.Items.Clear();
            foreach (var rq in quest.RequiredQuestNameData)
                lbRequiredQuests.Items.Add(rq);

            lbTasks.Items.Clear();
            foreach (var t in quest.BaseTasksData)
                lbTasks.Items.Add(t);
        }
    }
}
