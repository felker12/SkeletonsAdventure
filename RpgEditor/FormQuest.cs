using RpgLibrary.QuestClasses;

namespace RpgEditor;

public partial class FormQuest : Form
{
    public FormQuest()
    {
        InitializeComponent();
        AssetList.Bind(this, lbDetails, btnAdd, btnEdit, btnDelete, entry => entry.Data is QuestData, EditAsset);
    }

    private object? EditAsset(EditorProject project, object? data)
    {
        using var dialog = new FormQuestDetails();
        if (data is QuestData quest)
            dialog.LoadQuest(quest);
        dialog.ConfigureProject(project);
        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.Quest : null;
    }
}
