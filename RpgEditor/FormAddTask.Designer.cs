namespace RpgEditor
{
    partial class FormAddTask
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
            this.cbType = new System.Windows.Forms.ComboBox();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.nudRequired = new System.Windows.Forms.NumericUpDown();
            this.nudCompleted = new System.Windows.Forms.NumericUpDown();
            this.tbMonster = new System.Windows.Forms.TextBox();
            this.lblType = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblRequired = new System.Windows.Forms.Label();
            this.lblCompleted = new System.Windows.Forms.Label();
            this.lblMonster = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudRequired)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCompleted)).BeginInit();
            this.SuspendLayout();
            // 
            // cbType
            // 
            this.cbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbType.FormattingEnabled = true;
            this.cbType.Items.AddRange(new object[] { "Base", "Slay" });
            this.cbType.Location = new System.Drawing.Point(100, 12);
            this.cbType.Name = "cbType";
            this.cbType.Size = new System.Drawing.Size(200, 23);
            this.cbType.TabIndex = 0;
            // 
            // tbDescription
            // 
            this.tbDescription.Location = new System.Drawing.Point(100, 47);
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(200, 23);
            this.tbDescription.TabIndex = 1;
            // 
            // nudRequired
            // 
            this.nudRequired.Location = new System.Drawing.Point(100, 82);
            this.nudRequired.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudRequired.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.nudRequired.Name = "nudRequired";
            this.nudRequired.Size = new System.Drawing.Size(80, 23);
            this.nudRequired.TabIndex = 2;
            this.nudRequired.Value = 1;
            // 
            // nudCompleted
            // 
            this.nudCompleted.Location = new System.Drawing.Point(100, 117);
            this.nudCompleted.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.nudCompleted.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.nudCompleted.Name = "nudCompleted";
            this.nudCompleted.Size = new System.Drawing.Size(80, 23);
            this.nudCompleted.TabIndex = 3;
            this.nudCompleted.Value = 0;
            // 
            // tbMonster
            // 
            this.tbMonster.Location = new System.Drawing.Point(100, 152);
            this.tbMonster.Name = "tbMonster";
            this.tbMonster.Size = new System.Drawing.Size(200, 23);
            this.tbMonster.TabIndex = 4;
            this.tbMonster.Visible = false;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(10, 15);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(61, 15);
            this.lblType.TabIndex = 5;
            this.lblType.Text = "Task Type:";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(10, 50);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(69, 15);
            this.lblDescription.TabIndex = 6;
            this.lblDescription.Text = "Description:";
            // 
            // lblRequired
            // 
            this.lblRequired.AutoSize = true;
            this.lblRequired.Location = new System.Drawing.Point(10, 85);
            this.lblRequired.Name = "lblRequired";
            this.lblRequired.Size = new System.Drawing.Size(56, 15);
            this.lblRequired.TabIndex = 7;
            this.lblRequired.Text = "Required:";
            // 
            // lblCompleted
            // 
            this.lblCompleted.AutoSize = true;
            this.lblCompleted.Location = new System.Drawing.Point(10, 120);
            this.lblCompleted.Name = "lblCompleted";
            this.lblCompleted.Size = new System.Drawing.Size(69, 15);
            this.lblCompleted.TabIndex = 8;
            this.lblCompleted.Text = "Completed:";
            // 
            // lblMonster
            // 
            this.lblMonster.AutoSize = true;
            this.lblMonster.Location = new System.Drawing.Point(10, 155);
            this.lblMonster.Name = "lblMonster";
            this.lblMonster.Size = new System.Drawing.Size(54, 15);
            this.lblMonster.TabIndex = 9;
            this.lblMonster.Text = "Monster:";
            this.lblMonster.Visible = false;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(140, 200);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 30);
            this.btnOK.TabIndex = 10;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(225, 200);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            // 
            // FormAddTask
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 250);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblMonster);
            this.Controls.Add(this.lblCompleted);
            this.Controls.Add(this.lblRequired);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.tbMonster);
            this.Controls.Add(this.nudCompleted);
            this.Controls.Add(this.nudRequired);
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.cbType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormAddTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Task";
            ((System.ComponentModel.ISupportInitialize)(this.nudRequired)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCompleted)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox cbType;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.NumericUpDown nudRequired;
        private System.Windows.Forms.NumericUpDown nudCompleted;
        private System.Windows.Forms.TextBox tbMonster;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblRequired;
        private System.Windows.Forms.Label lblCompleted;
        private System.Windows.Forms.Label lblMonster;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}