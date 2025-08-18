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
    public partial class FormWeapon : FormDetails
    {
        public FormWeapon()
        {
            InitializeComponent();

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
        }
        void BtnAdd_Click(object? sender, EventArgs e)
        {
            using (FormWeaponDetails frmWeaponDetails = new())
            {
                frmWeaponDetails.ShowDialog();
                if (frmWeaponDetails.Weapon != null)
                {
                    AddWeapon(frmWeaponDetails.Weapon);
                }
            }

        }
        void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (lbDetails.SelectedItem != null)
            {
                string? detail = lbDetails.SelectedItem.ToString();

                if (detail != null)
                {
                    string[] parts = detail.Split(',');
                    string entity = parts[0].Trim();
                    WeaponData? data = ItemDataManager.WeaponData[entity];
                    WeaponData? newData = null;
                    using (FormWeaponDetails frmWeaponData = new())
                    {
                        frmWeaponData.Weapon = data;
                        frmWeaponData.ShowDialog();
                        if (frmWeaponData.Weapon == null)
                            return;
                        if (frmWeaponData.Weapon.Name == entity)
                        {
                            ItemDataManager.WeaponData[entity] = frmWeaponData.Weapon;
                            FillListBox();
                            return;
                        }
                        newData = frmWeaponData.Weapon;
                    }
                    DialogResult result = MessageBox.Show(
                    "Name has changed. Do you want to add a new entry?",

                    "New Entry",

                    MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                        return;
                    if (ItemDataManager.WeaponData.ContainsKey(newData.Name))
                    {
                        MessageBox.Show("Entry already exists. Use Edit to modify the entry.");
                        return;
                    }
                    lbDetails.Items.Add(newData);
                    ItemDataManager.WeaponData.Add(newData.Name, newData);
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
                    ItemDataManager.WeaponData.Remove(entity);
                    if (File.Exists(FormMain.ItemPath + @"\Weapon\" + entity + ".xml"))
                        File.Delete(FormMain.ItemPath + @"\Weapon\" + entity + ".xml");
                }
            }
        }
        public void FillListBox()
        {
            lbDetails.Items.Clear();
            foreach (string s in FormDetails.ItemDataManager.WeaponData.Keys)
                lbDetails.Items.Add(FormDetails.ItemDataManager.WeaponData[s]);
        }
        private void AddWeapon(WeaponData weaponData)
        {
            if (FormDetails.ItemDataManager.WeaponData.ContainsKey(weaponData.Name))
            {
                DialogResult result = MessageBox.Show(
                weaponData.Name + " already exists. Overwrite it?",
                "Existing weapon",
                MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                    return;
                ItemDataManager.WeaponData[weaponData.Name] = weaponData;
                FillListBox();
                return;
            }
            ItemDataManager.WeaponData.Add(weaponData.Name, weaponData);
            lbDetails.Items.Add(weaponData);
        }
    }
}
