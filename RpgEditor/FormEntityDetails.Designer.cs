namespace RpgEditor
{
    partial class FormEntityDetails
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
            tbId = new TextBox();
            tbType = new TextBox();
            tbBaseHealth = new TextBox();
            tbBaseDefence = new TextBox();
            tbBaseAttack = new TextBox();
            tbEntityLevel = new TextBox();
            tbBaseXP = new TextBox();
            tbCurrentHealth = new TextBox();
            tbPositionX = new TextBox();
            tbPositionY = new TextBox();
            tbRespawnX = new TextBox();
            tbRespawnY = new TextBox();
            cbIsDead = new CheckBox();
            lblId = new Label();
            lblType = new Label();
            lblBaseHealth = new Label();
            lblBaseDefence = new Label();
            lblBaseAttack = new Label();
            lblEntityLevel = new Label();
            lblBaseXP = new Label();
            lblCurrentHealth = new Label();
            lblPosition = new Label();
            lblRespawn = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // tbId
            // 
            tbId.Location = new Point(137, 16);
            tbId.Margin = new Padding(3, 4, 3, 4);
            tbId.Name = "tbId";
            tbId.Size = new Size(114, 27);
            tbId.TabIndex = 0;
            // 
            // tbType
            // 
            tbType.Location = new Point(137, 55);
            tbType.Margin = new Padding(3, 4, 3, 4);
            tbType.Name = "tbType";
            tbType.Size = new Size(114, 27);
            tbType.TabIndex = 1;
            // 
            // tbBaseHealth
            // 
            tbBaseHealth.Location = new Point(137, 93);
            tbBaseHealth.Margin = new Padding(3, 4, 3, 4);
            tbBaseHealth.Name = "tbBaseHealth";
            tbBaseHealth.Size = new Size(114, 27);
            tbBaseHealth.TabIndex = 2;
            tbBaseHealth.TextChanged += tbBaseHealth_TextChanged;
            // 
            // tbBaseDefence
            // 
            tbBaseDefence.Location = new Point(137, 132);
            tbBaseDefence.Margin = new Padding(3, 4, 3, 4);
            tbBaseDefence.Name = "tbBaseDefence";
            tbBaseDefence.Size = new Size(114, 27);
            tbBaseDefence.TabIndex = 3;
            // 
            // tbBaseAttack
            // 
            tbBaseAttack.Location = new Point(137, 171);
            tbBaseAttack.Margin = new Padding(3, 4, 3, 4);
            tbBaseAttack.Name = "tbBaseAttack";
            tbBaseAttack.Size = new Size(114, 27);
            tbBaseAttack.TabIndex = 4;
            // 
            // tbEntityLevel
            // 
            tbEntityLevel.Location = new Point(137, 209);
            tbEntityLevel.Margin = new Padding(3, 4, 3, 4);
            tbEntityLevel.Name = "tbEntityLevel";
            tbEntityLevel.Size = new Size(114, 27);
            tbEntityLevel.TabIndex = 5;
            // 
            // tbBaseXP
            // 
            tbBaseXP.Location = new Point(137, 248);
            tbBaseXP.Margin = new Padding(3, 4, 3, 4);
            tbBaseXP.Name = "tbBaseXP";
            tbBaseXP.Size = new Size(114, 27);
            tbBaseXP.TabIndex = 6;
            // 
            // tbCurrentHealth
            // 
            tbCurrentHealth.Enabled = false;
            tbCurrentHealth.Location = new Point(137, 287);
            tbCurrentHealth.Margin = new Padding(3, 4, 3, 4);
            tbCurrentHealth.Name = "tbCurrentHealth";
            tbCurrentHealth.Size = new Size(114, 27);
            tbCurrentHealth.TabIndex = 7;
            // 
            // tbPositionX
            // 
            tbPositionX.Location = new Point(137, 325);
            tbPositionX.Margin = new Padding(3, 4, 3, 4);
            tbPositionX.Name = "tbPositionX";
            tbPositionX.Size = new Size(51, 27);
            tbPositionX.TabIndex = 8;
            tbPositionX.Text = "0";
            // 
            // tbPositionY
            // 
            tbPositionY.Location = new Point(200, 325);
            tbPositionY.Margin = new Padding(3, 4, 3, 4);
            tbPositionY.Name = "tbPositionY";
            tbPositionY.Size = new Size(51, 27);
            tbPositionY.TabIndex = 9;
            tbPositionY.Text = "0";
            // 
            // tbRespawnX
            // 
            tbRespawnX.Location = new Point(137, 364);
            tbRespawnX.Margin = new Padding(3, 4, 3, 4);
            tbRespawnX.Name = "tbRespawnX";
            tbRespawnX.Size = new Size(51, 27);
            tbRespawnX.TabIndex = 10;
            tbRespawnX.Text = "0";
            // 
            // tbRespawnY
            // 
            tbRespawnY.Location = new Point(200, 364);
            tbRespawnY.Margin = new Padding(3, 4, 3, 4);
            tbRespawnY.Name = "tbRespawnY";
            tbRespawnY.Size = new Size(51, 27);
            tbRespawnY.TabIndex = 11;
            tbRespawnY.Text = "0";
            // 
            // cbIsDead
            // 
            cbIsDead.AutoSize = true;
            cbIsDead.Enabled = false;
            cbIsDead.Location = new Point(137, 403);
            cbIsDead.Margin = new Padding(3, 4, 3, 4);
            cbIsDead.Name = "cbIsDead";
            cbIsDead.Size = new Size(81, 24);
            cbIsDead.TabIndex = 12;
            cbIsDead.Text = "Is Dead";
            cbIsDead.UseVisualStyleBackColor = true;
            // 
            // lblId
            // 
            lblId.AutoSize = true;
            lblId.Location = new Point(14, 20);
            lblId.Name = "lblId";
            lblId.Size = new Size(27, 20);
            lblId.TabIndex = 13;
            lblId.Text = "ID:";
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new Point(14, 59);
            lblType.Name = "lblType";
            lblType.Size = new Size(43, 20);
            lblType.TabIndex = 14;
            lblType.Text = "Type:";
            // 
            // lblBaseHealth
            // 
            lblBaseHealth.AutoSize = true;
            lblBaseHealth.Location = new Point(14, 97);
            lblBaseHealth.Name = "lblBaseHealth";
            lblBaseHealth.Size = new Size(91, 20);
            lblBaseHealth.TabIndex = 15;
            lblBaseHealth.Text = "Base Health:";
            // 
            // lblBaseDefence
            // 
            lblBaseDefence.AutoSize = true;
            lblBaseDefence.Location = new Point(14, 136);
            lblBaseDefence.Name = "lblBaseDefence";
            lblBaseDefence.Size = new Size(102, 20);
            lblBaseDefence.TabIndex = 16;
            lblBaseDefence.Text = "Base Defence:";
            // 
            // lblBaseAttack
            // 
            lblBaseAttack.AutoSize = true;
            lblBaseAttack.Location = new Point(14, 175);
            lblBaseAttack.Name = "lblBaseAttack";
            lblBaseAttack.Size = new Size(89, 20);
            lblBaseAttack.TabIndex = 17;
            lblBaseAttack.Text = "Base Attack:";
            // 
            // lblEntityLevel
            // 
            lblEntityLevel.AutoSize = true;
            lblEntityLevel.Location = new Point(14, 213);
            lblEntityLevel.Name = "lblEntityLevel";
            lblEntityLevel.Size = new Size(87, 20);
            lblEntityLevel.TabIndex = 18;
            lblEntityLevel.Text = "Entity Level:";
            // 
            // lblBaseXP
            // 
            lblBaseXP.AutoSize = true;
            lblBaseXP.Location = new Point(14, 252);
            lblBaseXP.Name = "lblBaseXP";
            lblBaseXP.Size = new Size(64, 20);
            lblBaseXP.TabIndex = 19;
            lblBaseXP.Text = "Base XP:";
            // 
            // lblCurrentHealth
            // 
            lblCurrentHealth.AutoSize = true;
            lblCurrentHealth.Location = new Point(14, 291);
            lblCurrentHealth.Name = "lblCurrentHealth";
            lblCurrentHealth.Size = new Size(108, 20);
            lblCurrentHealth.TabIndex = 20;
            lblCurrentHealth.Text = "Current Health:";
            // 
            // lblPosition
            // 
            lblPosition.AutoSize = true;
            lblPosition.Location = new Point(14, 329);
            lblPosition.Name = "lblPosition";
            lblPosition.Size = new Size(64, 20);
            lblPosition.TabIndex = 21;
            lblPosition.Text = "Position:";
            // 
            // lblRespawn
            // 
            lblRespawn.AutoSize = true;
            lblRespawn.Location = new Point(14, 368);
            lblRespawn.Name = "lblRespawn";
            lblRespawn.Size = new Size(71, 20);
            lblRespawn.TabIndex = 22;
            lblRespawn.Text = "Respawn:";
            // 
            // btnOK
            // 
            btnOK.Location = new Point(69, 453);
            btnOK.Margin = new Padding(3, 4, 3, 4);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(86, 40);
            btnOK.TabIndex = 23;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(166, 453);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(86, 40);
            btnCancel.TabIndex = 24;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // FormEntityDetails
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(286, 520);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(lblRespawn);
            Controls.Add(lblPosition);
            Controls.Add(lblCurrentHealth);
            Controls.Add(lblBaseXP);
            Controls.Add(lblEntityLevel);
            Controls.Add(lblBaseAttack);
            Controls.Add(lblBaseDefence);
            Controls.Add(lblBaseHealth);
            Controls.Add(lblType);
            Controls.Add(lblId);
            Controls.Add(cbIsDead);
            Controls.Add(tbRespawnY);
            Controls.Add(tbRespawnX);
            Controls.Add(tbPositionY);
            Controls.Add(tbPositionX);
            Controls.Add(tbCurrentHealth);
            Controls.Add(tbBaseXP);
            Controls.Add(tbEntityLevel);
            Controls.Add(tbBaseAttack);
            Controls.Add(tbBaseDefence);
            Controls.Add(tbBaseHealth);
            Controls.Add(tbType);
            Controls.Add(tbId);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormEntityDetails";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Entity Details";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox tbId;
        private System.Windows.Forms.TextBox tbType;
        private System.Windows.Forms.TextBox tbBaseHealth;
        private System.Windows.Forms.TextBox tbBaseDefence;
        private System.Windows.Forms.TextBox tbBaseAttack;
        private System.Windows.Forms.TextBox tbEntityLevel;
        private System.Windows.Forms.TextBox tbBaseXP;
        private System.Windows.Forms.TextBox tbCurrentHealth;
        private System.Windows.Forms.TextBox tbPositionX;
        private System.Windows.Forms.TextBox tbPositionY;
        private System.Windows.Forms.TextBox tbRespawnX;
        private System.Windows.Forms.TextBox tbRespawnY;
        private System.Windows.Forms.CheckBox cbIsDead;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblBaseHealth;
        private System.Windows.Forms.Label lblBaseDefence;
        private System.Windows.Forms.Label lblBaseAttack;
        private System.Windows.Forms.Label lblEntityLevel;
        private System.Windows.Forms.Label lblBaseXP;
        private System.Windows.Forms.Label lblCurrentHealth;
        private System.Windows.Forms.Label lblPosition;
        private System.Windows.Forms.Label lblRespawn;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}