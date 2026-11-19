namespace RpgEditor;

public partial class FormDetails : Form
{
    public FormDetails() => InitializeComponent();

    internal void ConfigureAssets(Func<EditorAsset, bool> matches, Func<EditorProject, object?, object?> edit, bool itemList = false) =>
        AssetList.Bind(this, lbDetails, btnAdd, btnEdit, btnDelete, matches, edit, itemList);
}
