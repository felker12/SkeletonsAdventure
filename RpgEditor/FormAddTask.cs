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
    public partial class FormAddTask : Form
    {
        public string SelectedTaskType => cbType.SelectedItem?.ToString() ?? "Base";
        public string TaskDescription => tbDescription.Text;
        public int RequiredAmount => (int)nudRequired.Value;
        public int CompletedAmount => (int)nudCompleted.Value;
        public string MonsterToSlay => tbMonster.Text;

        public FormAddTask()
        {
            InitializeComponent();
            cbType.SelectedIndex = 0;
            cbType.SelectedIndexChanged += cbType_SelectedIndexChanged;
            UpdateTaskTypeFields();
        }

        private void cbType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateTaskTypeFields();
        }

        private void UpdateTaskTypeFields()
        {
            bool isSlay = cbType.SelectedItem?.ToString() == "Slay";
            lblMonster.Visible = tbMonster.Visible = isSlay;
        }
    }
}
