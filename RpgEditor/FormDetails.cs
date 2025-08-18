using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;

namespace RpgEditor
{
    public partial class FormDetails : Form
    {
        public static ItemDataManager ItemDataManager { get; private set; } = new();

        public FormDetails()
        {
            InitializeComponent();

            FormDetails.ItemDataManager ??= new();

            this.FormClosing += new(FormDetails_FormClosing);
        }

        void FormDetails_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            if (e.CloseReason == CloseReason.MdiFormClosing)
            {
                e.Cancel = false;
                this.Close();
            }
        }

        public static void WriteItemData()
        {
            foreach (string s in ItemDataManager.ArmorData.Keys)
            {
                XnaSerializer.Serialize<ArmorData>(
                FormMain.ItemPath + @"\Armor\" + s + ".xml",
                ItemDataManager.ArmorData[s]);
            }

            foreach (string s in ItemDataManager.WeaponData.Keys)
            {
                XnaSerializer.Serialize<WeaponData>(
                FormMain.ItemPath + @"\Weapon\" + s + ".xml",
                ItemDataManager.WeaponData[s]);
            }

            //TODO do the same for consumables
        }

        public static void WriteQuestData()
        {
            foreach (string s in ItemDataManager.QuestData.Keys)
            {
                XnaSerializer.Serialize<QuestData>(
                    FormMain.QuestPath + @"\" + s + ".xml",
                    ItemDataManager.QuestData[s]);
            }
        }

        public static void WriteEntityData()
        {
            //TODO: Implement entity data writing logic here
            foreach (var s in ItemDataManager.EntityData.Keys)
            {
                XnaSerializer.Serialize<EntityData>(
                    FormMain.EntityPath + @"\" + s + ".xml",
                    ItemDataManager.EntityData[s]);
            }
        }

        public static void ReadItemData()
        {
            ItemDataManager = new ItemDataManager();

            string[] fileNames = Directory.GetFiles(Path.Combine(FormMain.ItemPath, "Armor"), "*.xml");
            foreach (string s in fileNames)
            {
                ArmorData armorData = XnaSerializer.Deserialize<ArmorData>(s);
                ItemDataManager.ArmorData.Add(armorData.Name, armorData);
            }

            fileNames = Directory.GetFiles(Path.Combine(FormMain.ItemPath, "Weapon"), "*.xml");
            foreach (string s in fileNames)
            {
                WeaponData weaponData = XnaSerializer.Deserialize<WeaponData>(s);
                ItemDataManager.WeaponData.Add(weaponData.Name, weaponData);
            }

            //TODO do the same for consumables

        }

        public static void ReadQuestData()
        {
            string[] fileNames = Directory.GetFiles(Path.Combine(FormMain.QuestPath), "*.xml");
            foreach (string s in fileNames)
            {
                QuestData questData = XnaSerializer.Deserialize<QuestData>(s);
                ItemDataManager.QuestData.Add(questData.Name, questData);
            }
        }

        public static void ReadEntityData()
        {
            string[] fileNames = Directory.GetFiles(Path.Combine(FormMain.EntityPath), "*.xml");
            foreach (string s in fileNames)
            {
                EntityData entityData = XnaSerializer.Deserialize<EntityData>(s);
                ItemDataManager.EntityData.Add(entityData.GetType().Name, entityData);
            }
        }
    }
}
