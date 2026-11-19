using System.ComponentModel;
using RpgLibrary.ItemClasses;

namespace RpgEditor;

public partial class FormWeaponDetails : Form
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public WeaponData? Weapon { get; set; }

    private readonly ErrorProvider errors;
    private EditorProject? project;

    public FormWeaponDetails()
    {
        InitializeComponent();
        foreach (var input in new[] { mtbPrice, mtbPositionX, mtbPositionY, mtbQuantity, mtbX, mtbY, mtbWidth, mtbHeight, mtbAttackValue })
            input.Mask = "";
        errors = new ErrorProvider(this) { BlinkStyle = ErrorBlinkStyle.NeverBlink };
        Disposed += (_, _) => errors.Dispose();
        cboHands.DataSource = Enum.GetValues<Hands>();
        Load += (_, _) => LoadItem(Weapon ?? new WeaponData
        {
            Type = "Weapon", Quantity = 1, TexturePath = tbPath.Text, SourceRectangle = new(0, 0, 32, 32)
        });
        AcceptButton = btnOK;
        CancelButton = btnCancel;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnOK.Click += BtnOK_Click;
    }

    internal void ConfigureTexturePicker(EditorProject current)
    {
        project = current;
        TexturePicker.Attach(this, current, tbPath, mtbX, mtbY, mtbWidth, mtbHeight);
    }

    private void LoadItem(WeaponData item)
    {
        tbName.Text = item.Name;
        tbType.Text = item.Type;
        tbDescription.Text = item.Description;
        mtbPrice.Text = item.Price.ToString();
        nudWeight.Minimum = Math.Min(nudWeight.Minimum, (decimal)item.Weight);
        nudWeight.Maximum = Math.Max(nudWeight.Maximum, (decimal)item.Weight);
        nudWeight.Value = (decimal)item.Weight;
        cbEquipped.Checked = item.Equipped;
        cbStackable.Checked = item.Stackable;
        cbConsumable.Checked = item.Consumable;
        mtbPositionX.Text = item.Position.X.ToString();
        mtbPositionY.Text = item.Position.Y.ToString();
        mtbQuantity.Text = item.Quantity.ToString();
        mtbX.Text = item.SourceRectangle.X.ToString();
        mtbY.Text = item.SourceRectangle.Y.ToString();
        mtbWidth.Text = item.SourceRectangle.Width.ToString();
        mtbHeight.Text = item.SourceRectangle.Height.ToString();
        tbPath.Text = item.TexturePath;
        cboHands.SelectedItem = item.NumberHands;
        mtbAttackValue.Text = item.AttackValue.ToString();
    }

    private void BtnOK_Click(object? sender, EventArgs e)
    {
        errors.Clear();
        if (string.IsNullOrWhiteSpace(tbName.Text))
        {
            ShowError(tbName, "Enter an item name.");
            return;
        }

        if (!ReadInteger(mtbPrice, 0, out int price) ||
            !ReadInteger(mtbQuantity, 1, out int quantity) ||
            !ReadInteger(mtbX, 0, out int sourceX) ||
            !ReadInteger(mtbY, 0, out int sourceY) ||
            !ReadInteger(mtbWidth, 1, out int width) ||
            !ReadInteger(mtbHeight, 1, out int height) ||
            !ReadInteger(mtbAttackValue, 0, out int stat))
            return;

        if (!float.TryParse(mtbPositionX.Text, out float positionX) || !float.IsFinite(positionX))
        {
            ShowError(mtbPositionX, "Enter a finite X coordinate.");
            return;
        }
        if (!float.TryParse(mtbPositionY.Text, out float positionY) || !float.IsFinite(positionY))
        {
            ShowError(mtbPositionY, "Enter a finite Y coordinate.");
            return;
        }
        if (cboHands.SelectedItem is not Hands selection)
        {
            ShowError(cboHands, "Select a value.");
            return;
        }
        if (!cbStackable.Checked && quantity != 1)
        {
            ShowError(mtbQuantity, "Non-stackable items have a quantity of one.");
            return;
        }

        var item = Weapon?.Clone() ?? new WeaponData();
        item.Name = tbName.Text.Trim();
        item.Type = tbType.Text;
        item.Description = tbDescription.Text;
        item.Price = price;
        item.Weight = (float)nudWeight.Value;
        item.Equipped = cbEquipped.Checked;
        item.Stackable = cbStackable.Checked;
        item.Consumable = cbConsumable.Checked;
        item.Position = new(positionX, positionY);
        item.Quantity = quantity;
        item.SourceRectangle = new(sourceX, sourceY, width, height);
        item.TexturePath = tbPath.Text.Trim();
        item.NumberHands = selection;
        item.AttackValue = stat;

        if (ItemInput.Validate(item, project) is { } error)
        {
            ShowError(tbPath, error);
            return;
        }
        Weapon = item;
        DialogResult = DialogResult.OK;
    }

    private bool ReadInteger(TextBoxBase input, int minimum, out int value)
    {
        if (int.TryParse(input.Text, out value) && value >= minimum)
            return true;
        ShowError(input, $"Enter a whole number of at least {minimum}.");
        return false;
    }

    private void ShowError(Control input, string message)
    {
        errors.SetError(input, message);
        input.Focus();
    }
}
