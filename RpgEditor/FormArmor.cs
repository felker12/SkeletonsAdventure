using RpgLibrary.ItemClasses;

namespace RpgEditor;

public partial class FormArmor : FormDetails
{
    public FormArmor()
    {
        InitializeComponent();
        ConfigureAssets(entry => entry.Data is ArmorData, EditAsset, itemList: true);
    }

    private object? EditAsset(EditorProject project, object? data)
    {
        var dialog = new FormArmorDetails { Armor = data as ArmorData };
        dialog.ConfigureTexturePicker(project);
        using (dialog)
            return dialog.ShowDialog(this) == DialogResult.OK ? dialog.Armor : null;
    }
}
