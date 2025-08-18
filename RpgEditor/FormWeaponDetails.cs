using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RpgLibrary.ItemClasses;

namespace RpgEditor
{
    public partial class FormWeaponDetails : Form
    {
        public WeaponData? Weapon { get; set; }

        public FormWeaponDetails()
        {
            InitializeComponent();
            this.Load += FormWeaponDetails_Load;
            this.FormClosing += new FormClosingEventHandler(FormWeaponDetails_FormClosing);
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        void FormWeaponDetails_Load(object? sender, EventArgs e)
        {
            foreach (Hands location in Enum.GetValues(typeof(Hands)))
                cboHands.Items.Add(location);
            cboHands.SelectedIndex = 0;

            if (Weapon != null)
            {
                tbName.Text = Weapon.Name;
                tbType.Text = Weapon.Type;
                mtbPrice.Text = Weapon.Price.ToString();
                tbDescription.Text = Weapon.Description;
                nudWeight.Value = (decimal)Weapon.Weight;
                cbEquipped.Checked = Weapon.Equipped;
                cbStackable.Checked = Weapon.Stackable;
                cbConsumable.Checked = Weapon.Consumable;
                mtbPositionX.Text = Weapon.Position.X.ToString();
                mtbPositionY.Text = Weapon.Position.Y.ToString();
                mtbQuantity.Text = Weapon.Quantity.ToString();
                mtbX.Text = Weapon.SourceRectangle.X.ToString();
                mtbY.Text = Weapon.SourceRectangle.Y.ToString();
                mtbWidth.Text = Weapon.SourceRectangle.Width.ToString();
                mtbHeight.Text = Weapon.SourceRectangle.Height.ToString();
                tbPath.Text = Weapon.TexturePath;
                cboHands.SelectedIndex = (int)Weapon.NumberHands;
                mtbAttackValue.Text = Weapon.AttackValue.ToString();
            }
        }
        void FormWeaponDetails_FormClosing(object? sender, FormClosingEventArgs e)
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

            if (!float.TryParse(mtbPositionX.Text, out float posX))
            {
                MessageBox.Show("Position X value must be a float value.");
                return;
            }

            if (!float.TryParse(mtbPositionY.Text, out float posY))
            {
                MessageBox.Show("Position Y value must be a float value.");
                return;
            }

            if (!int.TryParse(mtbQuantity.Text, out int quantity))
            {
                MessageBox.Show("Quantity value must be a float value.");
                return;
            }

            if (!int.TryParse(mtbPositionX.Text, out int sourcePosX))
            {
                MessageBox.Show("Position X value must be a float value.");
                return;
            }
            if (!int.TryParse(mtbPositionY.Text, out int sourcePosY))
            {
                MessageBox.Show("Position Y value must be a float value.");
                return;
            }
            if (!int.TryParse(mtbHeight.Text, out int height))
            {
                MessageBox.Show("Height value must be a float value.");
                return;
            }
            if (!int.TryParse(mtbWidth.Text, out int width))
            {
                MessageBox.Show("Width value must be a float value.");
                return;
            }

            if(cboHands.SelectedItem is null)
            {
                MessageBox.Show("You must select a hand type.");
                return;
            }

            Weapon = new WeaponData
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
                NumberHands = (Hands)cboHands.SelectedItem,
                AttackValue = int.Parse(mtbAttackValue.Text)
            };

            this.FormClosing -= FormWeaponDetails_FormClosing;
            this.Close();
        }
        void BtnCancel_Click(object? sender, EventArgs e)
        {
            Weapon = null;
            this.FormClosing -= FormWeaponDetails_FormClosing;
            this.Close();
        }
    }
}
