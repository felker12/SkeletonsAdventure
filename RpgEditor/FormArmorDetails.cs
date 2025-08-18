using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RpgLibrary.ItemClasses;
using Microsoft.Xna.Framework;

namespace RpgEditor
{
    public partial class FormArmorDetails : Form
    {
        public ArmorData? Armor { get; set; } = null;
        public FormArmorDetails()
        {
            InitializeComponent();

            this.Load += FormArmorDetails_Load;
            this.FormClosing += new FormClosingEventHandler(FormArmorDetails_FormClosing);
            btnOK.Click += new EventHandler(BtnOK_Click);
            btnCancel.Click += new EventHandler(BtnCancel_Click);
        }

        void FormArmorDetails_Load(object? sender, EventArgs e)
        {
            foreach (ArmorLocation location in Enum.GetValues(typeof(ArmorLocation)))
                cboArmorLocation.Items.Add(location);

            cboArmorLocation.SelectedIndex = 0;

            if (Armor != null)
            {
                tbName.Text = Armor.Name;
                tbType.Text = Armor.Type;
                tbDescription.Text = Armor.Description;
                mtbPrice.Text = Armor.Price.ToString();
                nudWeight.Value = (decimal)Armor.Weight;
                cbEquipped.Checked = Armor.Equipped;
                cbStackable.Checked = Armor.Stackable;
                cbConsumable.Checked = Armor.Consumable;
                mtbPositionX.Text = Armor.Position.X.ToString();
                mtbPositionY.Text = Armor.Position.Y.ToString();
                mtbQuantity.Text = Armor.Quantity.ToString();
                mtbX.Text = Armor.SourceRectangle.X.ToString();
                mtbY.Text = Armor.SourceRectangle.Y.ToString();
                mtbWidth.Text = Armor.SourceRectangle.Width.ToString();
                mtbHeight.Text = Armor.SourceRectangle.Height.ToString();
                tbPath.Text = Armor.TexturePath;
                cboArmorLocation.SelectedIndex = (int)Armor.ArmorLocation;
                mtbDefenseValue.Text = Armor.DefenseValue.ToString();
            }
        }
        void FormArmorDetails_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }
        void BtnOK_Click(object? sender, EventArgs e)
        {
            float weight = 0f;
            if (string.IsNullOrEmpty(tbName.Text))
            {

                MessageBox.Show("You must enter a name for the item.");
                return;
            }
            if (!int.TryParse(mtbPrice.Text, out int price))
            {
                MessageBox.Show("Price must be an integer value.");
                return;
            }

            weight = (float)nudWeight.Value;

            if (!int.TryParse(mtbDefenseValue.Text, out int defVal))
            {
                MessageBox.Show("Defense value must be an interger value.");
                return;
            }

            if(!float.TryParse(mtbPositionX.Text, out float posX))
            {
                MessageBox.Show("Position X value must be a float value.");
                return;
            }

            if (!float.TryParse(mtbPositionY.Text, out float posY))
            {
                MessageBox.Show("Position Y value must be a float value.");
                return;
            }

            if(!int.TryParse(mtbQuantity.Text, out int quantity))
            {
                MessageBox.Show("Quantity value must be a float value.");
                return;
            }

            if(!int.TryParse(mtbPositionX.Text, out int sourcePosX))
            {
                MessageBox.Show("Position X value must be a float value.");
                return;
            }
            if (!int.TryParse(mtbPositionY.Text, out int sourcePosY))
            {
                MessageBox.Show("Position Y value must be a float value.");
                return;
            }
            if(!int.TryParse(mtbHeight.Text, out int height))
            {
                MessageBox.Show("Height value must be a float value.");
                return;
            }
            if (!int.TryParse(mtbWidth.Text, out int width))
            {
                MessageBox.Show("Width value must be a float value.");
                return;
            }


            Armor = new()
            {
                Name = tbName.Text,
                Type = tbType.Text,
                Description = tbDescription.Text,
                Price = price,
                Weight = weight,
                Equipped = cbEquipped.Checked,
                Stackable = cbStackable.Checked,
                Consumable = cbConsumable.Checked,
                Position = new(posX, posY),
                Quantity = quantity,
                SourceRectangle = new Microsoft.Xna.Framework.Rectangle(sourcePosX, sourcePosY, width, height),
                TexturePath = tbPath.Text,
                ArmorLocation = (ArmorLocation)cboArmorLocation.SelectedIndex,
                DefenseValue = defVal
            };

            this.FormClosing -= FormArmorDetails_FormClosing;
            this.Close();
        }
        void BtnCancel_Click(object? sender, EventArgs e)
        {
            Armor = null;
            this.FormClosing -= FormArmorDetails_FormClosing;
            this.Close();
        }

        private void FormArmorDetails_Load_1(object sender, EventArgs e)
        {

        }

    }
}
