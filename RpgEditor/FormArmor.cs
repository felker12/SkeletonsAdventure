using RpgLibrary.ItemClasses;
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
    public partial class FormArmor : FormDetails
    {
        public FormArmor()
        {
            InitializeComponent();

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        void BtnAdd_Click(object? sender, EventArgs e)
        {
            using (FormArmorDetails frmArmorDetails = new())
            {
                frmArmorDetails.ShowDialog();
                if (frmArmorDetails.Armor != null)
                {
                    AddArmor(frmArmorDetails.Armor);
                }
            }
        }
        void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (lbDetails.SelectedItem != null)
            {
                string? detail = lbDetails.SelectedItem.ToString();

                if(detail != null)
                {
                    string[]? parts = detail.Split(',');
                    string entity = parts[0].Trim();
                    ArmorData? data = ItemDataManager.ArmorData[entity];
                    ArmorData? newData = null;
                    using (FormArmorDetails frmArmorData = new FormArmorDetails())
                    {
                        frmArmorData.Armor = data;
                        frmArmorData.ShowDialog();
                        if (frmArmorData.Armor == null)
                            return;
                        if (frmArmorData.Armor.Name == entity)
                        {
                            ItemDataManager.ArmorData[entity] = frmArmorData.Armor;
                            FillListBox();
                            return;
                        }
                        newData = frmArmorData.Armor;
                    }
                    DialogResult result = MessageBox.Show(
                    "Name has changed. Do you want to add a new entry?",

                    "New Entry",

                    MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                        return;
                    if (ItemDataManager.ArmorData.ContainsKey(newData.Name))
                    {
                        MessageBox.Show("Entry already exists. Use Edit to modify the entry.");
                        return;
                    }
                    lbDetails.Items.Add(newData);
                    ItemDataManager.ArmorData.Add(newData.Name, newData);
                }
            }
        }
        void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lbDetails.SelectedItem != null)
            {
                string? detail = lbDetails.SelectedItem.ToString();

                if (detail == null)
                    return;

                string[] parts = detail.Split(',');

                string entity = parts[0].Trim();
                DialogResult result = MessageBox.Show(
                "Are you sure you want to delete " + entity + "?",

                "Delete",

                MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    lbDetails.Items.RemoveAt(lbDetails.SelectedIndex);
                    ItemDataManager.ArmorData.Remove(entity);
                    if (File.Exists(FormMain.ItemPath + @"\Armor\" + entity + ".xml"))
                        File.Delete(FormMain.ItemPath + @"\Armor\" + entity + ".xml");
                }
            }
        }

        public void FillListBox()
        {
            lbDetails.Items.Clear();
            foreach (string s in FormDetails.ItemDataManager.ArmorData.Keys)
                lbDetails.Items.Add(FormDetails.ItemDataManager.ArmorData[s]);
        }
        private void AddArmor(ArmorData armorData)
        {
            if (FormDetails.ItemDataManager.ArmorData.ContainsKey(armorData.Name))
            {
                DialogResult result = MessageBox.Show(
                armorData.Name + " already exists. Overwrite it?",
                "Existing armor",
                MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                    return;
                ItemDataManager.ArmorData[armorData.Name] = armorData;
                FillListBox();
                return;
            }
            ItemDataManager.ArmorData.Add(armorData.Name, armorData);
            lbDetails.Items.Add(armorData);
        }
    }
}
