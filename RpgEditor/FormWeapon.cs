using RpgLibrary.ItemClasses;

namespace RpgEditor;

public partial class FormWeapon : FormDetails
{
    public FormWeapon()
    {
        InitializeComponent();
        ConfigureAssets(entry => entry.Data is WeaponData, EditAsset);
    }

    private object? EditAsset(EditorProject project, object? data)
    {
        var dialog = new FormWeaponDetails { Weapon = data as WeaponData };
        dialog.ConfigureTexturePicker(project);
        using (dialog)
            return dialog.ShowDialog(this) == DialogResult.OK ? dialog.Weapon : null;
    }
}
