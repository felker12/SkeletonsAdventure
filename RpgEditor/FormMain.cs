using Microsoft.Xna.Framework.Input;
using RpgEditor;
using RpgLibrary.DataClasses;
using RpgLibrary.GameObjectClasses;
using RpgLibrary.ItemClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpgEditor
{
    public partial class FormMain : Form
    {
        RPG RPG { get; set; } = new();
        FormArmor? frmArmor;
        FormWeapon? frmWeapon;
        FormQuest? frmQuest;
        FormEntity? frmEntity;

        public static string ItemPath { get; set; } = string.Empty;
        public static string GamePath { get; set; } = string.Empty;
        public static string QuestPath { get; set; } = string.Empty;
        public static string EntityPath { get; set; } = string.Empty;

        public FormMain()
        {
            InitializeComponent();

            this.FormClosing += FormMain_FormClosing;

            // Add this if you have a menu strip or toolbar for entities
            // Example for a menu item:
            // entityToolStripMenuItem.Click += EntityToolStripMenuItem_Click;
        }

        void FormMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
            "Unsaved changes will be lost. Are you sure you want to exit?",
            "Exit?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
            if (result == DialogResult.No)
                e.Cancel = true;
        }

        private void NewGameToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using (FormNewGame frmNewGame = new())
            {
                DialogResult result = frmNewGame.ShowDialog();
                if (result == DialogResult.OK && frmNewGame.RPG != null)
                {
                    FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                    folderDialog.Description = "Select folder to create game in.";
                    folderDialog.SelectedPath = Application.StartupPath;
                    DialogResult folderResult = folderDialog.ShowDialog();
                    if (folderResult == DialogResult.OK)
                    {
                        try
                        {
                            GamePath = Path.Combine(folderDialog.SelectedPath, "Game");
                            ItemPath = Path.Combine(GamePath, "Items");
                            QuestPath = Path.Combine(GamePath, "Quests");
                            EntityPath = Path.Combine(GamePath, "Entities");

                            if (Directory.Exists(GamePath))
                                throw new Exception("Selected directory already exists.");

                            Directory.CreateDirectory(GamePath);
                            Directory.CreateDirectory(ItemPath + @"\Armor");
                            Directory.CreateDirectory(ItemPath + @"\Weapon");
                            Directory.CreateDirectory(QuestPath);
                            Directory.CreateDirectory(EntityPath);
                            RPG = frmNewGame.RPG;
                            XnaSerializer.Serialize<RPG>(GamePath + @"\Game.xml", RPG);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                            return;
                        }
                        itemsToolStripMenuItem.Enabled = true;
                        questsToolStripMenuItem.Enabled = true;
                    }
                }
            }
        }

        private void OpenGameToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new()
            {
                Description = "Select Game folder",
                SelectedPath = Application.StartupPath
            };
            bool tryAgain = false;
            do
            {
                DialogResult folderResult = folderDialog.ShowDialog();
                DialogResult msgBoxResult;
                if (folderResult == DialogResult.OK)
                {
                    if (File.Exists(folderDialog.SelectedPath + @"\Game\Game.xml"))
                    {
                        try
                        {
                            OpenGame(folderDialog.SelectedPath);
                            tryAgain = false;
                        }
                        catch (Exception ex)
                        {
                            msgBoxResult = MessageBox.Show(
                            ex.ToString(),
                            "Error opening game.",
                            MessageBoxButtons.RetryCancel);
                            if (msgBoxResult == DialogResult.Cancel)
                                tryAgain = false;
                            else if (msgBoxResult == DialogResult.Retry)
                                tryAgain = true;
                        }
                    }

                    else
                    {
                        msgBoxResult = MessageBox.Show(
                        "Game not found, try again?",
                        "Game does not exist",
                        MessageBoxButtons.RetryCancel);
                        if (msgBoxResult == DialogResult.Cancel)
                            tryAgain = false;
                        else if (msgBoxResult == DialogResult.Retry)
                            tryAgain = true;
                    }
                }
            } while (tryAgain);
        }

        private void SaveGameToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (RPG != null)
            {
                try
                {
                    XnaSerializer.Serialize<RPG>(GamePath + @"\Game.xml", RPG);
                    FormDetails.WriteItemData();
                    FormDetails.WriteQuestData();
                    FormDetails.WriteEntityData();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.ToString(), "Error saving game.");
                }
            }
        }

        private void OpenGame(string path)
        {
            GamePath = Path.Combine(path, "Game");
            ItemPath = Path.Combine(GamePath, "Items");
            QuestPath = Path.Combine(GamePath, "Quests");
            EntityPath = Path.Combine(GamePath, "Entities");

            System.Diagnostics.Debug.WriteLine("GamePath: " + GamePath);
            System.Diagnostics.Debug.WriteLine("ItemPath: " + ItemPath);
            System.Diagnostics.Debug.WriteLine("QuestPath: " + QuestPath);
            System.Diagnostics.Debug.WriteLine("EntityPath: " + EntityPath);


            RPG = XnaSerializer.Deserialize<RPG>(GamePath + @"\Game.xml");

            FormDetails.ReadItemData();
            FormDetails.ReadQuestData();
            FormDetails.ReadEntityData();

            PrepareForms();
        }

        private void PrepareForms()
        {
            itemsToolStripMenuItem.Enabled = true;
            questsToolStripMenuItem.Enabled = true;
            entitiesToolStripMenuItem.Enabled = true;
        }

        private void ExitGameToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void WeaponsToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            frmWeapon ??= new()
            {
                MdiParent = this
            };
            frmWeapon.FillListBox();
            frmWeapon.Show();
            frmWeapon.BringToFront();
        }

        private void ArmorToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            frmArmor ??= new()
            {
                MdiParent = this
            };
            frmArmor.FillListBox();
            frmArmor.Show();
            frmArmor.BringToFront();
        }

        private void QuestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmQuest ??= new()
            {
                MdiParent = this
            };
            frmQuest.FillListBox();
            frmQuest.Show();
            frmQuest.BringToFront();
        }

        private void EntityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmEntity ??= new FormEntity
            {
                MdiParent = this
            };
            frmEntity.FillListBox();
            frmEntity.Show();
            frmEntity.BringToFront();
        }
    }
}
