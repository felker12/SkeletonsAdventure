using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpgEditor
{
    public partial class FormQuest : Form
    {
        public FormQuest()
        {
            InitializeComponent();

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        void BtnAdd_Click(object? sender, EventArgs e)
        {
            using (FormQuestDetails frmQuestDetails = new())
            {
                frmQuestDetails.ShowDialog();
                if (frmQuestDetails.Quest != null)
                {
                    AddQuest(frmQuestDetails.Quest);
                }
            }
        }

        void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (lbDetails.SelectedItem is QuestData quest)
            {
                using (FormQuestDetails frmQuestDetails = new())
                {
                    frmQuestDetails.Quest = quest;
                    frmQuestDetails.LoadQuest(quest);
                    frmQuestDetails.ShowDialog();
                    if (frmQuestDetails.Quest == null)
                        return;

                    int idx = lbDetails.SelectedIndex;
                    lbDetails.Items[idx] = frmQuestDetails.Quest;
                }
            }
        }

        void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lbDetails.SelectedItem is QuestData quest)
            {
                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to delete {quest.Name}?",
                    "Delete",
                    MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    lbDetails.Items.RemoveAt(lbDetails.SelectedIndex);
                }
            }
        }

        public void FillListBox()
        {
            lbDetails.Items.Clear();
            foreach(string s in FormDetails.ItemDataManager.QuestData.Keys)
            {
                lbDetails.Items.Add(FormDetails.ItemDataManager.QuestData[s]);
            }
        }

        private void AddQuest(QuestData questData)
        {
            if (FormDetails.ItemDataManager.QuestData.ContainsKey(questData.Name))
            {
                DialogResult result = MessageBox.Show(
                    questData.Name + " already exists. Overwrite it?",
                    "Existing quest",
                    MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                    return;
                FormDetails.ItemDataManager.QuestData[questData.Name] = questData;
                FillListBox();
                return;
            }
            FormDetails.ItemDataManager.QuestData.Add(questData.Name, questData);
            lbDetails.Items.Add(questData);
        }
    }
}
