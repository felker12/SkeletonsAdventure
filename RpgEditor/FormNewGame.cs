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
    public partial class FormNewGame : Form
    {
        public RPG RPG { get; set; } = new();

        public FormNewGame()
        {
            InitializeComponent();
            btnOK.Click += new EventHandler(BtnOK_Click);
        }

        void BtnOK_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text) ||
            string.IsNullOrEmpty(txtDescription.Text))
            {
                MessageBox.Show("You must enter a name and a description.", "Error");
                return;
            }
            RPG = new RPG(txtName.Text, txtDescription.Text);
            this.Close();
        }
    }
}
