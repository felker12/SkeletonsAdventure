using RpgLibrary.EntityClasses;
using RpgLibrary.GameObjectClasses;
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
    public partial class FormEntity : FormDetails
    {
        public FormEntity()
        {
            InitializeComponent();

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        void BtnAdd_Click(object? sender, EventArgs e)
        {
            using (FormEntityDetails formEntityDetails = new())
            {
                formEntityDetails.ShowDialog();
                if (formEntityDetails.Entity != null)
                {
                    AddEntity(formEntityDetails.Entity);
                }
            }
        }

        void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (lbDetails.SelectedItem is EntityData data)
            {
                using (FormEntityDetails formEntityDetails = new())
                {
                    formEntityDetails.Entity = data;
                    formEntityDetails.LoadEntity(data);
                    formEntityDetails.ShowDialog();
                    if (formEntityDetails.Entity == null)
                        return;

                    int idx = lbDetails.SelectedIndex;
                    lbDetails.Items[idx] = formEntityDetails.Entity;
                }
            }
        }

        void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lbDetails.SelectedItem is EntityData data)
            {
                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to delete {data.Type}?",
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
            foreach (string s in FormDetails.ItemDataManager.EntityData.Keys)
            {
                lbDetails.Items.Add(FormDetails.ItemDataManager.EntityData[s]);
            }
        }

        private void AddEntity(EntityData entityData)
        {
            if (FormDetails.ItemDataManager.EntityData.ContainsKey(entityData.Type))
            {
                DialogResult result = MessageBox.Show(
                    entityData.Type + " already exists. Overwrite it?",
                    "Existing quest",
                    MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                    return;
                FormDetails.ItemDataManager.EntityData[entityData.Type] = entityData;
                FillListBox();
                return;
            }
            FormDetails.ItemDataManager.EntityData.Add(entityData.Type, entityData);
            lbDetails.Items.Add(entityData);
        }
    }
}
