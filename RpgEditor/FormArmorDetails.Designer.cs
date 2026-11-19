using System.Xml.Linq;

namespace RpgEditor
{
    partial class FormArmorDetails
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
            btnCancel = new Button();
            btnOK = new Button();
            mtbPrice = new MaskedTextBox();
            label3 = new Label();
            tbType = new TextBox();
            label2 = new Label();
            tbName = new TextBox();
            label1 = new Label();
            label5 = new Label();
            cboArmorLocation = new ComboBox();
            label6 = new Label();
            mtbDefenseValue = new MaskedTextBox();
            tbDescription = new TextBox();
            label8 = new Label();
            mtbPositionX = new MaskedTextBox();
            lblLocation = new Label();
            mtbPositionY = new MaskedTextBox();
            mtbX = new MaskedTextBox();
            label13 = new Label();
            mtbY = new MaskedTextBox();
            mtbWidth = new MaskedTextBox();
            mtbHeight = new MaskedTextBox();
            tbPath = new TextBox();
            label14 = new Label();
            SuspendLayout();
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(206, 349);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(88, 27);
            btnCancel.TabIndex = 30;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(72, 349);
            btnOK.Margin = new Padding(4, 3, 4, 3);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(88, 27);
            btnOK.TabIndex = 31;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // mtbPrice
            // 
            mtbPrice.Location = new Point(126, 93);
            mtbPrice.Margin = new Padding(4, 3, 4, 3);
            mtbPrice.Mask = "000000";
            mtbPrice.Name = "mtbPrice";
            mtbPrice.Size = new Size(116, 23);
            mtbPrice.TabIndex = 21;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(79, 96);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(36, 15);
            label3.TabIndex = 20;
            label3.Text = "Price:";
            // 
            // tbType
            // 
            tbType.Location = new Point(126, 35);
            tbType.Margin = new Padding(4, 3, 4, 3);
            tbType.Name = "tbType";
            tbType.Size = new Size(116, 23);
            tbType.ReadOnly = true;
            tbType.TabStop = false;
            tbType.TabIndex = 19;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(79, 38);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(35, 15);
            label2.TabIndex = 18;
            label2.Text = "Type:";
            // 
            // tbName
            // 
            tbName.Location = new Point(126, 5);
            tbName.Margin = new Padding(4, 3, 4, 3);
            tbName.Name = "tbName";
            tbName.Size = new Size(116, 23);
            tbName.TabIndex = 17;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(75, 8);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 16;
            label1.Text = "Name:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(23, 240);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(93, 15);
            label5.TabIndex = 32;
            label5.Text = "Armor Location:";
            // 
            // cboArmorLocation
            // 
            cboArmorLocation.DropDownStyle = ComboBoxStyle.DropDownList;
            cboArmorLocation.FormattingEnabled = true;
            cboArmorLocation.Location = new Point(126, 237);
            cboArmorLocation.Margin = new Padding(4, 3, 4, 3);
            cboArmorLocation.Name = "cboArmorLocation";
            cboArmorLocation.Size = new Size(115, 23);
            cboArmorLocation.TabIndex = 33;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(25, 271);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(83, 15);
            label6.TabIndex = 34;
            label6.Text = "Defense Value:";
            // 
            // mtbDefenseValue
            // 
            mtbDefenseValue.Location = new Point(126, 268);
            mtbDefenseValue.Margin = new Padding(4, 3, 4, 3);
            mtbDefenseValue.Mask = "000";
            mtbDefenseValue.Name = "mtbDefenseValue";
            mtbDefenseValue.Size = new Size(116, 23);
            mtbDefenseValue.TabIndex = 35;
            // 
            // tbDescription
            // 
            tbDescription.Location = new Point(126, 64);
            tbDescription.Margin = new Padding(4, 3, 4, 3);
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new Size(116, 23);
            tbDescription.TabIndex = 37;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(44, 67);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(70, 15);
            label8.TabIndex = 36;
            label8.Text = "Description:";
            // 
            // mtbPositionX
            // 
            mtbPositionX.Location = new Point(125, 124);
            mtbPositionX.Margin = new Padding(4, 3, 4, 3);
            mtbPositionX.Mask = "00000";
            mtbPositionX.Name = "mtbPositionX";
            mtbPositionX.Size = new Size(55, 23);
            mtbPositionX.TabIndex = 45;
            mtbPositionX.Text = "0";
            // 
            // lblLocation
            // 
            lblLocation.AutoSize = true;
            lblLocation.Location = new Point(58, 127);
            lblLocation.Margin = new Padding(4, 0, 4, 0);
            lblLocation.Name = "lblLocation";
            lblLocation.Size = new Size(56, 15);
            lblLocation.TabIndex = 44;
            lblLocation.Text = "Location:";
            // 
            // mtbPositionY
            // 
            mtbPositionY.Location = new Point(188, 124);
            mtbPositionY.Margin = new Padding(4, 3, 4, 3);
            mtbPositionY.Mask = "000000";
            mtbPositionY.Name = "mtbPositionY";
            mtbPositionY.Size = new Size(55, 23);
            mtbPositionY.TabIndex = 46;
            mtbPositionY.Text = "0";
            // 
            // mtbX
            // 
            mtbX.Location = new Point(123, 162);
            mtbX.Margin = new Padding(4, 3, 4, 3);
            mtbX.Mask = "00000";
            mtbX.Name = "mtbX";
            mtbX.Size = new Size(45, 23);
            mtbX.TabIndex = 50;
            mtbX.Text = "0";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(1, 155);
            label13.Margin = new Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new Size(115, 30);
            label13.TabIndex = 49;
            label13.Text = "Source Position:\r\n(X, Y, Width, Height)\r\n";
            label13.TextAlign = ContentAlignment.TopRight;
            // 
            // mtbY
            // 
            mtbY.Location = new Point(176, 162);
            mtbY.Margin = new Padding(4, 3, 4, 3);
            mtbY.Mask = "00000";
            mtbY.Name = "mtbY";
            mtbY.Size = new Size(45, 23);
            mtbY.TabIndex = 51;
            mtbY.Text = "0";
            // 
            // mtbWidth
            // 
            mtbWidth.Location = new Point(229, 162);
            mtbWidth.Margin = new Padding(4, 3, 4, 3);
            mtbWidth.Mask = "00000";
            mtbWidth.Name = "mtbWidth";
            mtbWidth.Size = new Size(45, 23);
            mtbWidth.TabIndex = 52;
            mtbWidth.Text = "0";
            // 
            // mtbHeight
            // 
            mtbHeight.Location = new Point(282, 162);
            mtbHeight.Margin = new Padding(4, 3, 4, 3);
            mtbHeight.Mask = "00000";
            mtbHeight.Name = "mtbHeight";
            mtbHeight.Size = new Size(45, 23);
            mtbHeight.TabIndex = 53;
            mtbHeight.Text = "0";
            // 
            // tbPath
            // 
            tbPath.Location = new Point(126, 195);
            tbPath.Margin = new Padding(4, 3, 4, 3);
            tbPath.Name = "tbPath";
            tbPath.Size = new Size(201, 23);
            tbPath.TabIndex = 55;
            tbPath.Text = "TileSets/ProjectUtumno_full";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(44, 198);
            label14.Margin = new Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new Size(70, 15);
            label14.TabIndex = 54;
            label14.Text = "Source Path";
            // 
            // FormArmorDetails
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(380, 396);
            Controls.Add(tbPath);
            Controls.Add(label14);
            Controls.Add(mtbHeight);
            Controls.Add(mtbWidth);
            Controls.Add(mtbY);
            Controls.Add(mtbX);
            Controls.Add(label13);
            Controls.Add(mtbPositionY);
            Controls.Add(mtbPositionX);
            Controls.Add(lblLocation);
            Controls.Add(tbDescription);
            Controls.Add(label8);
            Controls.Add(mtbDefenseValue);
            Controls.Add(label6);
            Controls.Add(cboArmorLocation);
            Controls.Add(label5);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(mtbPrice);
            Controls.Add(label3);
            Controls.Add(tbType);
            Controls.Add(label2);
            Controls.Add(tbName);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 3, 4, 3);
            Name = "FormArmorDetails";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Armor Details";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.MaskedTextBox mtbPrice;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboArmorLocation;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.MaskedTextBox mtbDefenseValue;
        private TextBox tbDescription;
        private Label label8;
        private MaskedTextBox mtbPositionX;
        private Label lblLocation;
        private MaskedTextBox mtbPositionY;
        private MaskedTextBox mtbX;
        private Label label13;
        private MaskedTextBox mtbY;
        private MaskedTextBox mtbWidth;
        private MaskedTextBox mtbHeight;
        private TextBox tbPath;
        private Label label14;
    }
}
