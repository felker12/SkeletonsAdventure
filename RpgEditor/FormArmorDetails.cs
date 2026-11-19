using System.ComponentModel;
using RpgLibrary.DataClasses;
using RpgLibrary.ItemClasses;

namespace RpgEditor;

public partial class FormArmorDetails : Form
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ArmorData? Armor { get; set; }

    private readonly ErrorProvider errors;
    private EditorProject? project;
    private LevelRequirementData requirements = new();

    public FormArmorDetails()
    {
        InitializeComponent();
        foreach (var input in new[] { mtbPrice, mtbPositionX, mtbPositionY, mtbX, mtbY, mtbWidth, mtbHeight, mtbDefenseValue })
            input.Mask = "";
        errors = new ErrorProvider(this) { BlinkStyle = ErrorBlinkStyle.NeverBlink };
        Disposed += (_, _) => errors.Dispose();
        cboArmorLocation.DataSource = Enum.GetValues<ArmorLocation>();
        Load += (_, _) => LoadItem(Armor ?? new ArmorData
        {
            Type = "Armor", Quantity = 1, TexturePath = tbPath.Text, SourceRectangle = new(0, 0, 32, 32)
        });
        AcceptButton = btnOK;
        CancelButton = btnCancel;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnOK.Click += BtnOK_Click;
        var requirementSummary = EquipmentRequirementsDialog.CreateSummary(this, () => requirements, data => requirements = data);
        requirementSummary.Location = new Point(14, mtbDefenseValue.Bottom + 10);
        Controls.Add(requirementSummary);
    }

    internal void ConfigureTexturePicker(EditorProject current)
    {
        project = current;
        TexturePicker.Attach(this, current, tbPath, mtbX, mtbY, mtbWidth, mtbHeight);
    }

    private void LoadItem(ArmorData item)
    {
        tbName.Text = item.Name;
        tbType.Text = string.IsNullOrWhiteSpace(item.Type) ? ItemInput.DefaultEquipmentType(item) : item.Type;
        requirements = item.LevelRequirementData?.Clone() ?? new();
        tbDescription.Text = item.Description;
        mtbPrice.Text = item.Price.ToString();
        nudWeight.Minimum = Math.Min(nudWeight.Minimum, (decimal)item.Weight);
        nudWeight.Maximum = Math.Max(nudWeight.Maximum, (decimal)item.Weight);
        nudWeight.Value = (decimal)item.Weight;
        mtbPositionX.Text = item.Position.X.ToString();
        mtbPositionY.Text = item.Position.Y.ToString();
        mtbX.Text = item.SourceRectangle.X.ToString();
        mtbY.Text = item.SourceRectangle.Y.ToString();
        mtbWidth.Text = item.SourceRectangle.Width.ToString();
        mtbHeight.Text = item.SourceRectangle.Height.ToString();
        tbPath.Text = item.TexturePath;
        cboArmorLocation.SelectedItem = item.ArmorLocation;
        mtbDefenseValue.Text = item.DefenseValue.ToString();
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
            !ReadInteger(mtbX, 0, out int sourceX) ||
            !ReadInteger(mtbY, 0, out int sourceY) ||
            !ReadInteger(mtbWidth, 1, out int width) ||
            !ReadInteger(mtbHeight, 1, out int height) ||
            !ReadInteger(mtbDefenseValue, 0, out int stat))
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
        if (cboArmorLocation.SelectedItem is not ArmorLocation selection)
        {
            ShowError(cboArmorLocation, "Select a value.");
            return;
        }
        var item = Armor?.Clone() ?? new ArmorData();
        ItemInput.ApplyEquipmentTemplateDefaults(item);
        item.Name = tbName.Text.Trim();
        item.Description = tbDescription.Text;
        item.Price = price;
        item.Weight = (float)nudWeight.Value;
        item.Position = new(positionX, positionY);
        item.LevelRequirementData = requirements.Clone();
        item.SourceRectangle = new(sourceX, sourceY, width, height);
        item.TexturePath = tbPath.Text.Trim();
        item.ArmorLocation = selection;
        item.DefenseValue = stat;

        if (ItemInput.Validate(item, project) is { } error)
        {
            ShowError(tbPath, error);
            return;
        }
        Armor = item;
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
