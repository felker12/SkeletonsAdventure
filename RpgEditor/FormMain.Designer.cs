namespace RpgEditor
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            gameToolStripMenuItem = new ToolStripMenuItem();
            newGameToolStripMenuItem = new ToolStripMenuItem();
            openGameToolStripMenuItem = new ToolStripMenuItem();
            saveGameToolStripMenuItem = new ToolStripMenuItem();
            exitGameToolStripMenuItem = new ToolStripMenuItem();
            itemsToolStripMenuItem = new ToolStripMenuItem();
            weaponsToolStripMenuItem = new ToolStripMenuItem();
            armorToolStripMenuItem = new ToolStripMenuItem();
            questsToolStripMenuItem = new ToolStripMenuItem();
            questToolStripMenuItem = new ToolStripMenuItem();
            entitiesToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { gameToolStripMenuItem, itemsToolStripMenuItem, questsToolStripMenuItem, entitiesToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 3, 0, 3);
            menuStrip1.Size = new Size(1416, 30);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // gameToolStripMenuItem
            // 
            gameToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newGameToolStripMenuItem, openGameToolStripMenuItem, saveGameToolStripMenuItem, exitGameToolStripMenuItem });
            gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            gameToolStripMenuItem.Size = new Size(62, 24);
            gameToolStripMenuItem.Text = "&Game";
            // 
            // newGameToolStripMenuItem
            // 
            newGameToolStripMenuItem.Name = "newGameToolStripMenuItem";
            newGameToolStripMenuItem.Size = new Size(171, 26);
            newGameToolStripMenuItem.Text = "New Game";
            newGameToolStripMenuItem.Click += NewGameToolStripMenuItem_Click;
            // 
            // openGameToolStripMenuItem
            // 
            openGameToolStripMenuItem.Name = "openGameToolStripMenuItem";
            openGameToolStripMenuItem.Size = new Size(171, 26);
            openGameToolStripMenuItem.Text = "Open Game";
            openGameToolStripMenuItem.Click += OpenGameToolStripMenuItem_Click;
            // 
            // saveGameToolStripMenuItem
            // 
            saveGameToolStripMenuItem.Name = "saveGameToolStripMenuItem";
            saveGameToolStripMenuItem.Size = new Size(171, 26);
            saveGameToolStripMenuItem.Text = "Save Game";
            saveGameToolStripMenuItem.Click += SaveGameToolStripMenuItem_Click;
            // 
            // exitGameToolStripMenuItem
            // 
            exitGameToolStripMenuItem.Name = "exitGameToolStripMenuItem";
            exitGameToolStripMenuItem.Size = new Size(171, 26);
            exitGameToolStripMenuItem.Text = "Exit Game";
            exitGameToolStripMenuItem.Click += ExitGameToolStripMenuItem_Click;
            // 
            // itemsToolStripMenuItem
            // 
            itemsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { weaponsToolStripMenuItem, armorToolStripMenuItem });
            itemsToolStripMenuItem.Enabled = false;
            itemsToolStripMenuItem.Name = "itemsToolStripMenuItem";
            itemsToolStripMenuItem.Size = new Size(59, 24);
            itemsToolStripMenuItem.Text = "&Items";
            // 
            // weaponsToolStripMenuItem
            // 
            weaponsToolStripMenuItem.Name = "weaponsToolStripMenuItem";
            weaponsToolStripMenuItem.Size = new Size(153, 26);
            weaponsToolStripMenuItem.Text = "Weapons";
            weaponsToolStripMenuItem.Click += WeaponsToolStripMenuItem_Click;
            // 
            // armorToolStripMenuItem
            // 
            armorToolStripMenuItem.Name = "armorToolStripMenuItem";
            armorToolStripMenuItem.Size = new Size(153, 26);
            armorToolStripMenuItem.Text = "Armor";
            armorToolStripMenuItem.Click += ArmorToolStripMenuItem_Click;
            // 
            // questsToolStripMenuItem
            // 
            questsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { questToolStripMenuItem });
            questsToolStripMenuItem.Enabled = false;
            questsToolStripMenuItem.Name = "questsToolStripMenuItem";
            questsToolStripMenuItem.Size = new Size(67, 24);
            questsToolStripMenuItem.Text = "Quests";
            // 
            // questToolStripMenuItem
            // 
            questToolStripMenuItem.Name = "questToolStripMenuItem";
            questToolStripMenuItem.Size = new Size(130, 26);
            questToolStripMenuItem.Text = "Quest";
            questToolStripMenuItem.Click += QuestToolStripMenuItem_Click;
            // 
            // entitiesToolStripMenuItem
            // 
            entitiesToolStripMenuItem.Enabled = false;
            entitiesToolStripMenuItem.Name = "entitiesToolStripMenuItem";
            entitiesToolStripMenuItem.Size = new Size(71, 24);
            entitiesToolStripMenuItem.Text = "Entities";
            entitiesToolStripMenuItem.Click += EntityToolStripMenuItem_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1416, 977);
            Controls.Add(menuStrip1);
            IsMdiContainer = true;
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormMain";
            Text = "FormMain";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem gameToolStripMenuItem;
        private ToolStripMenuItem newGameToolStripMenuItem;
        private ToolStripMenuItem openGameToolStripMenuItem;
        private ToolStripMenuItem saveGameToolStripMenuItem;
        private ToolStripMenuItem exitGameToolStripMenuItem;
        private ToolStripMenuItem itemsToolStripMenuItem;
        private ToolStripMenuItem weaponsToolStripMenuItem;
        private ToolStripMenuItem armorToolStripMenuItem;
        private ToolStripMenuItem questsToolStripMenuItem;
        private ToolStripMenuItem questToolStripMenuItem;
        private ToolStripMenuItem entitiesToolStripMenuItem;
    }
}