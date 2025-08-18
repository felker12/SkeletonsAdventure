namespace RpgEditor
{
    partial class FormQuestDetails
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
            tbName = new TextBox();
            tbDescription = new TextBox();
            cbIsCompleted = new CheckBox();
            cbActive = new CheckBox();
            nudLevel = new NumericUpDown();
            nudDefence = new NumericUpDown();
            nudAttack = new NumericUpDown();
            nudXP = new NumericUpDown();
            nudGold = new NumericUpDown();
            lbRequiredQuests = new ListBox();
            lbTasks = new ListBox();
            btnOK = new Button();
            btnCancel = new Button();
            btnAddTask = new Button();
            lblName = new Label();
            lblDescription = new Label();
            lblLevel = new Label();
            lblDefence = new Label();
            lblAttack = new Label();
            lblXP = new Label();
            lblGold = new Label();
            lblRequiredQuests = new Label();
            lblTasks = new Label();
            ((System.ComponentModel.ISupportInitialize)nudLevel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDefence).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudAttack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudXP).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudGold).BeginInit();
            SuspendLayout();
            // 
            // tbName
            // 
            tbName.Location = new Point(114, 16);
            tbName.Margin = new Padding(3, 4, 3, 4);
            tbName.Name = "tbName";
            tbName.Size = new Size(285, 27);
            tbName.TabIndex = 0;
            // 
            // tbDescription
            // 
            tbDescription.Location = new Point(114, 55);
            tbDescription.Margin = new Padding(3, 4, 3, 4);
            tbDescription.Multiline = true;
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new Size(285, 79);
            tbDescription.TabIndex = 1;
            // 
            // cbIsCompleted
            // 
            cbIsCompleted.AutoSize = true;
            cbIsCompleted.Enabled = false;
            cbIsCompleted.Location = new Point(423, 20);
            cbIsCompleted.Margin = new Padding(3, 4, 3, 4);
            cbIsCompleted.Name = "cbIsCompleted";
            cbIsCompleted.Size = new Size(119, 24);
            cbIsCompleted.TabIndex = 2;
            cbIsCompleted.Text = "Is Completed";
            cbIsCompleted.UseVisualStyleBackColor = true;
            // 
            // cbActive
            // 
            cbActive.AutoSize = true;
            cbActive.Enabled = false;
            cbActive.Location = new Point(548, 20);
            cbActive.Margin = new Padding(3, 4, 3, 4);
            cbActive.Name = "cbActive";
            cbActive.Size = new Size(72, 24);
            cbActive.TabIndex = 3;
            cbActive.Text = "Active";
            cbActive.UseVisualStyleBackColor = true;
            // 
            // nudLevel
            // 
            nudLevel.Location = new Point(114, 147);
            nudLevel.Margin = new Padding(3, 4, 3, 4);
            nudLevel.Name = "nudLevel";
            nudLevel.Size = new Size(69, 27);
            nudLevel.TabIndex = 4;
            // 
            // nudDefence
            // 
            nudDefence.Location = new Point(114, 185);
            nudDefence.Margin = new Padding(3, 4, 3, 4);
            nudDefence.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudDefence.Name = "nudDefence";
            nudDefence.Size = new Size(69, 27);
            nudDefence.TabIndex = 5;
            // 
            // nudAttack
            // 
            nudAttack.Location = new Point(114, 224);
            nudAttack.Margin = new Padding(3, 4, 3, 4);
            nudAttack.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudAttack.Name = "nudAttack";
            nudAttack.Size = new Size(69, 27);
            nudAttack.TabIndex = 6;
            // 
            // nudXP
            // 
            nudXP.Location = new Point(114, 263);
            nudXP.Margin = new Padding(3, 4, 3, 4);
            nudXP.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudXP.Name = "nudXP";
            nudXP.Size = new Size(91, 27);
            nudXP.TabIndex = 7;
            // 
            // nudGold
            // 
            nudGold.Location = new Point(114, 301);
            nudGold.Margin = new Padding(3, 4, 3, 4);
            nudGold.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudGold.Name = "nudGold";
            nudGold.Size = new Size(91, 27);
            nudGold.TabIndex = 8;
            // 
            // lbRequiredQuests
            // 
            lbRequiredQuests.FormattingEnabled = true;
            lbRequiredQuests.Location = new Point(423, 107);
            lbRequiredQuests.Margin = new Padding(3, 4, 3, 4);
            lbRequiredQuests.Name = "lbRequiredQuests";
            lbRequiredQuests.Size = new Size(228, 124);
            lbRequiredQuests.TabIndex = 9;
            // 
            // lbTasks
            // 
            lbTasks.FormattingEnabled = true;
            lbTasks.Location = new Point(423, 346);
            lbTasks.Margin = new Padding(3, 4, 3, 4);
            lbTasks.Name = "lbTasks";
            lbTasks.Size = new Size(228, 124);
            lbTasks.TabIndex = 10;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(114, 393);
            btnOK.Margin = new Padding(3, 4, 3, 4);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(103, 40);
            btnOK.TabIndex = 11;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(114, 464);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(103, 40);
            btnCancel.TabIndex = 12;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnAddTask
            // 
            btnAddTask.Location = new Point(491, 478);
            btnAddTask.Margin = new Padding(3, 4, 3, 4);
            btnAddTask.Name = "btnAddTask";
            btnAddTask.Size = new Size(103, 40);
            btnAddTask.TabIndex = 13;
            btnAddTask.Text = "Add Task";
            btnAddTask.UseVisualStyleBackColor = true;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(14, 20);
            lblName.Name = "lblName";
            lblName.Size = new Size(52, 20);
            lblName.TabIndex = 14;
            lblName.Text = "Name:";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(14, 55);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(88, 20);
            lblDescription.TabIndex = 15;
            lblDescription.Text = "Description:";
            // 
            // lblLevel
            // 
            lblLevel.AutoSize = true;
            lblLevel.Location = new Point(14, 149);
            lblLevel.Name = "lblLevel";
            lblLevel.Size = new Size(76, 20);
            lblLevel.TabIndex = 16;
            lblLevel.Text = "Level Req:";
            // 
            // lblDefence
            // 
            lblDefence.AutoSize = true;
            lblDefence.Location = new Point(14, 188);
            lblDefence.Name = "lblDefence";
            lblDefence.Size = new Size(97, 20);
            lblDefence.TabIndex = 17;
            lblDefence.Text = "Defence Req:";
            // 
            // lblAttack
            // 
            lblAttack.AutoSize = true;
            lblAttack.Location = new Point(14, 227);
            lblAttack.Name = "lblAttack";
            lblAttack.Size = new Size(84, 20);
            lblAttack.TabIndex = 18;
            lblAttack.Text = "Attack Req:";
            // 
            // lblXP
            // 
            lblXP.AutoSize = true;
            lblXP.Location = new Point(14, 265);
            lblXP.Name = "lblXP";
            lblXP.Size = new Size(83, 20);
            lblXP.TabIndex = 19;
            lblXP.Text = "XP Reward:";
            // 
            // lblGold
            // 
            lblGold.AutoSize = true;
            lblGold.Location = new Point(14, 304);
            lblGold.Name = "lblGold";
            lblGold.Size = new Size(98, 20);
            lblGold.TabIndex = 20;
            lblGold.Text = "Gold Reward:";
            // 
            // lblRequiredQuests
            // 
            lblRequiredQuests.AutoSize = true;
            lblRequiredQuests.Location = new Point(423, 83);
            lblRequiredQuests.Name = "lblRequiredQuests";
            lblRequiredQuests.Size = new Size(120, 20);
            lblRequiredQuests.TabIndex = 21;
            lblRequiredQuests.Text = "Required Quests:";
            // 
            // lblTasks
            // 
            lblTasks.AutoSize = true;
            lblTasks.Location = new Point(423, 322);
            lblTasks.Name = "lblTasks";
            lblTasks.Size = new Size(45, 20);
            lblTasks.TabIndex = 22;
            lblTasks.Text = "Tasks:";
            // 
            // FormQuestDetails
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(686, 560);
            Controls.Add(btnAddTask);
            Controls.Add(lblTasks);
            Controls.Add(lblRequiredQuests);
            Controls.Add(lblGold);
            Controls.Add(lblXP);
            Controls.Add(lblAttack);
            Controls.Add(lblDefence);
            Controls.Add(lblLevel);
            Controls.Add(lblDescription);
            Controls.Add(lblName);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(lbTasks);
            Controls.Add(lbRequiredQuests);
            Controls.Add(nudGold);
            Controls.Add(nudXP);
            Controls.Add(nudAttack);
            Controls.Add(nudDefence);
            Controls.Add(nudLevel);
            Controls.Add(cbActive);
            Controls.Add(cbIsCompleted);
            Controls.Add(tbDescription);
            Controls.Add(tbName);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormQuestDetails";
            Text = "Quest Details";
            ((System.ComponentModel.ISupportInitialize)nudLevel).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudDefence).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudAttack).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudXP).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudGold).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public System.Windows.Forms.TextBox tbName;
        public System.Windows.Forms.TextBox tbDescription;
        public System.Windows.Forms.CheckBox cbIsCompleted;
        public System.Windows.Forms.CheckBox cbActive;
        public System.Windows.Forms.NumericUpDown nudLevel;
        public System.Windows.Forms.NumericUpDown nudDefence;
        public System.Windows.Forms.NumericUpDown nudAttack;
        public System.Windows.Forms.NumericUpDown nudXP;
        public System.Windows.Forms.NumericUpDown nudGold;
        public System.Windows.Forms.ListBox lbRequiredQuests;
        public System.Windows.Forms.ListBox lbTasks;
        public System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.Button btnAddTask;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.Label lblDefence;
        private System.Windows.Forms.Label lblAttack;
        private System.Windows.Forms.Label lblXP;
        private System.Windows.Forms.Label lblGold;
        private System.Windows.Forms.Label lblRequiredQuests;
        private System.Windows.Forms.Label lblTasks;
    }
}