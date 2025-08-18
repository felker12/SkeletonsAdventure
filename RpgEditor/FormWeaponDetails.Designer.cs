namespace RpgEditor
{
    partial class FormWeaponDetails
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
            tbPath = new TextBox();
            label14 = new Label();
            mtbHeight = new MaskedTextBox();
            mtbWidth = new MaskedTextBox();
            mtbY = new MaskedTextBox();
            mtbX = new MaskedTextBox();
            label13 = new Label();
            mtbQuantity = new MaskedTextBox();
            label11 = new Label();
            mtbPositionY = new MaskedTextBox();
            mtbPositionX = new MaskedTextBox();
            lblLocation = new Label();
            cbConsumable = new CheckBox();
            label12 = new Label();
            cbStackable = new CheckBox();
            label10 = new Label();
            cbEquipped = new CheckBox();
            label9 = new Label();
            tbDescription = new TextBox();
            label8 = new Label();
            nudWeight = new NumericUpDown();
            label4 = new Label();
            mtbPrice = new MaskedTextBox();
            label3 = new Label();
            tbType = new TextBox();
            label2 = new Label();
            tbName = new TextBox();
            label1 = new Label();
            cboHands = new ComboBox();
            label5 = new Label();
            mtbAttackValue = new MaskedTextBox();
            label6 = new Label();
            btnCancel = new Button();
            btnOK = new Button();
            ((System.ComponentModel.ISupportInitialize)nudWeight).BeginInit();
            SuspendLayout();
            // 
            // tbPath
            // 
            tbPath.Location = new Point(122, 336);
            tbPath.Margin = new Padding(4, 3, 4, 3);
            tbPath.Name = "tbPath";
            tbPath.Size = new Size(201, 23);
            tbPath.TabIndex = 83;
            tbPath.Text = "TileSets/ProjectUtumno_full";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(40, 339);
            label14.Margin = new Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new Size(70, 15);
            label14.TabIndex = 82;
            label14.Text = "Source Path";
            // 
            // mtbHeight
            // 
            mtbHeight.Location = new Point(278, 303);
            mtbHeight.Margin = new Padding(4, 3, 4, 3);
            mtbHeight.Mask = "00000";
            mtbHeight.Name = "mtbHeight";
            mtbHeight.Size = new Size(45, 23);
            mtbHeight.TabIndex = 81;
            mtbHeight.Text = "0";
            // 
            // mtbWidth
            // 
            mtbWidth.Location = new Point(225, 303);
            mtbWidth.Margin = new Padding(4, 3, 4, 3);
            mtbWidth.Mask = "00000";
            mtbWidth.Name = "mtbWidth";
            mtbWidth.Size = new Size(45, 23);
            mtbWidth.TabIndex = 80;
            mtbWidth.Text = "0";
            // 
            // mtbY
            // 
            mtbY.Location = new Point(172, 303);
            mtbY.Margin = new Padding(4, 3, 4, 3);
            mtbY.Mask = "00000";
            mtbY.Name = "mtbY";
            mtbY.Size = new Size(45, 23);
            mtbY.TabIndex = 79;
            mtbY.Text = "0";
            // 
            // mtbX
            // 
            mtbX.Location = new Point(119, 303);
            mtbX.Margin = new Padding(4, 3, 4, 3);
            mtbX.Mask = "00000";
            mtbX.Name = "mtbX";
            mtbX.Size = new Size(45, 23);
            mtbX.TabIndex = 78;
            mtbX.Text = "0";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(-3, 296);
            label13.Margin = new Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new Size(115, 30);
            label13.TabIndex = 77;
            label13.Text = "Source Position:\r\n(X, Y, Width, Height)\r\n";
            label13.TextAlign = ContentAlignment.TopRight;
            // 
            // mtbQuantity
            // 
            mtbQuantity.Location = new Point(121, 264);
            mtbQuantity.Margin = new Padding(4, 3, 4, 3);
            mtbQuantity.Mask = "0000000";
            mtbQuantity.Name = "mtbQuantity";
            mtbQuantity.Size = new Size(116, 23);
            mtbQuantity.TabIndex = 76;
            mtbQuantity.Text = "1";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(56, 267);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(56, 15);
            label11.TabIndex = 75;
            label11.Text = "Quantity:";
            // 
            // mtbPositionY
            // 
            mtbPositionY.Location = new Point(184, 235);
            mtbPositionY.Margin = new Padding(4, 3, 4, 3);
            mtbPositionY.Mask = "000000";
            mtbPositionY.Name = "mtbPositionY";
            mtbPositionY.Size = new Size(55, 23);
            mtbPositionY.TabIndex = 74;
            mtbPositionY.Text = "0";
            // 
            // mtbPositionX
            // 
            mtbPositionX.Location = new Point(121, 235);
            mtbPositionX.Margin = new Padding(4, 3, 4, 3);
            mtbPositionX.Mask = "00000";
            mtbPositionX.Name = "mtbPositionX";
            mtbPositionX.Size = new Size(55, 23);
            mtbPositionX.TabIndex = 73;
            mtbPositionX.Text = "0";
            // 
            // lblLocation
            // 
            lblLocation.AutoSize = true;
            lblLocation.Location = new Point(54, 238);
            lblLocation.Margin = new Padding(4, 0, 4, 0);
            lblLocation.Name = "lblLocation";
            lblLocation.Size = new Size(56, 15);
            lblLocation.TabIndex = 72;
            lblLocation.Text = "Location:";
            // 
            // cbConsumable
            // 
            cbConsumable.AutoSize = true;
            cbConsumable.Location = new Point(121, 210);
            cbConsumable.Name = "cbConsumable";
            cbConsumable.Size = new Size(47, 19);
            cbConsumable.TabIndex = 71;
            cbConsumable.Text = "true";
            cbConsumable.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(35, 210);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new Size(77, 15);
            label12.TabIndex = 70;
            label12.Text = "Consumable:";
            // 
            // cbStackable
            // 
            cbStackable.AutoSize = true;
            cbStackable.Location = new Point(122, 186);
            cbStackable.Name = "cbStackable";
            cbStackable.Size = new Size(47, 19);
            cbStackable.TabIndex = 69;
            cbStackable.Text = "true";
            cbStackable.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(53, 186);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(60, 15);
            label10.TabIndex = 68;
            label10.Text = "Stackable:";
            // 
            // cbEquipped
            // 
            cbEquipped.AutoSize = true;
            cbEquipped.Location = new Point(122, 161);
            cbEquipped.Name = "cbEquipped";
            cbEquipped.Size = new Size(47, 19);
            cbEquipped.TabIndex = 67;
            cbEquipped.Text = "true";
            cbEquipped.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(53, 161);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(60, 15);
            label9.TabIndex = 66;
            label9.Text = "Equipped:";
            // 
            // tbDescription
            // 
            tbDescription.Location = new Point(122, 71);
            tbDescription.Margin = new Padding(4, 3, 4, 3);
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new Size(116, 23);
            tbDescription.TabIndex = 65;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(40, 74);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(70, 15);
            label8.TabIndex = 64;
            label8.Text = "Description:";
            // 
            // nudWeight
            // 
            nudWeight.DecimalPlaces = 2;
            nudWeight.Location = new Point(122, 130);
            nudWeight.Margin = new Padding(4, 3, 4, 3);
            nudWeight.Name = "nudWeight";
            nudWeight.Size = new Size(117, 23);
            nudWeight.TabIndex = 63;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(64, 132);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(48, 15);
            label4.TabIndex = 62;
            label4.Text = "Weight:";
            // 
            // mtbPrice
            // 
            mtbPrice.Location = new Point(122, 100);
            mtbPrice.Margin = new Padding(4, 3, 4, 3);
            mtbPrice.Mask = "000000";
            mtbPrice.Name = "mtbPrice";
            mtbPrice.Size = new Size(116, 23);
            mtbPrice.TabIndex = 61;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(75, 103);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(36, 15);
            label3.TabIndex = 60;
            label3.Text = "Price:";
            // 
            // tbType
            // 
            tbType.Location = new Point(122, 42);
            tbType.Margin = new Padding(4, 3, 4, 3);
            tbType.Name = "tbType";
            tbType.Size = new Size(116, 23);
            tbType.TabIndex = 59;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(75, 45);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(35, 15);
            label2.TabIndex = 58;
            label2.Text = "Type:";
            // 
            // tbName
            // 
            tbName.Location = new Point(122, 12);
            tbName.Margin = new Padding(4, 3, 4, 3);
            tbName.Name = "tbName";
            tbName.Size = new Size(116, 23);
            tbName.TabIndex = 57;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(71, 15);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 56;
            label1.Text = "Name:";
            // 
            // cboHands
            // 
            cboHands.DropDownStyle = ComboBoxStyle.DropDownList;
            cboHands.FormattingEnabled = true;
            cboHands.Location = new Point(121, 372);
            cboHands.Margin = new Padding(4, 3, 4, 3);
            cboHands.Name = "cboHands";
            cboHands.Size = new Size(115, 23);
            cboHands.TabIndex = 85;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(64, 375);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(44, 15);
            label5.TabIndex = 84;
            label5.Text = "Hands:";
            // 
            // mtbAttackValue
            // 
            mtbAttackValue.Location = new Point(118, 401);
            mtbAttackValue.Margin = new Padding(4, 3, 4, 3);
            mtbAttackValue.Mask = "0000000";
            mtbAttackValue.Name = "mtbAttackValue";
            mtbAttackValue.Size = new Size(116, 23);
            mtbAttackValue.TabIndex = 87;
            mtbAttackValue.Text = "1";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(39, 404);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(72, 15);
            label6.TabIndex = 86;
            label6.Text = "Attack Value";
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(209, 447);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(88, 27);
            btnCancel.TabIndex = 88;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(75, 447);
            btnOK.Margin = new Padding(4, 3, 4, 3);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(88, 27);
            btnOK.TabIndex = 89;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // FormWeaponDetails
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(380, 506);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(mtbAttackValue);
            Controls.Add(label6);
            Controls.Add(cboHands);
            Controls.Add(label5);
            Controls.Add(tbPath);
            Controls.Add(label14);
            Controls.Add(mtbHeight);
            Controls.Add(mtbWidth);
            Controls.Add(mtbY);
            Controls.Add(mtbX);
            Controls.Add(label13);
            Controls.Add(mtbQuantity);
            Controls.Add(label11);
            Controls.Add(mtbPositionY);
            Controls.Add(mtbPositionX);
            Controls.Add(lblLocation);
            Controls.Add(cbConsumable);
            Controls.Add(label12);
            Controls.Add(cbStackable);
            Controls.Add(label10);
            Controls.Add(cbEquipped);
            Controls.Add(label9);
            Controls.Add(tbDescription);
            Controls.Add(label8);
            Controls.Add(nudWeight);
            Controls.Add(label4);
            Controls.Add(mtbPrice);
            Controls.Add(label3);
            Controls.Add(tbType);
            Controls.Add(label2);
            Controls.Add(tbName);
            Controls.Add(label1);
            Name = "FormWeaponDetails";
            Text = "FormWeaponDetails";
            ((System.ComponentModel.ISupportInitialize)nudWeight).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox tbPath;
        private Label label14;
        private MaskedTextBox mtbHeight;
        private MaskedTextBox mtbWidth;
        private MaskedTextBox mtbY;
        private MaskedTextBox mtbX;
        private Label label13;
        private MaskedTextBox mtbQuantity;
        private Label label11;
        private MaskedTextBox mtbPositionY;
        private MaskedTextBox mtbPositionX;
        private Label lblLocation;
        private CheckBox cbConsumable;
        private Label label12;
        private CheckBox cbStackable;
        private Label label10;
        private CheckBox cbEquipped;
        private Label label9;
        private TextBox tbDescription;
        private Label label8;
        private NumericUpDown nudWeight;
        private Label label4;
        private MaskedTextBox mtbPrice;
        private Label label3;
        private TextBox tbType;
        private Label label2;
        private TextBox tbName;
        private Label label1;
        private ComboBox cboHands;
        private Label label5;
        private MaskedTextBox mtbAttackValue;
        private Label label6;
        private Button btnCancel;
        private Button btnOK;
    }
}