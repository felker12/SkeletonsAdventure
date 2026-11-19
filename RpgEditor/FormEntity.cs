using RpgLibrary.EntityClasses;

namespace RpgEditor;

public partial class FormEntity : FormDetails
{
    public FormEntity()
    {
        InitializeComponent();
        ConfigureAssets(entry => entry.Data is EntityData, EditAsset);
    }

    private object? EditAsset(EditorProject project, object? data)
    {
        if (project.IsGameContent)
        {
            using var template = new EnemyTemplateDialog(project, data as EnemyData);
            return template.ShowDialog(this) == DialogResult.OK ? template.ReadTemplate() : null;
        }

        using var dialog = new FormEntityDetails();
        if (data is EntityData entity)
            dialog.LoadEntity(entity);
        dialog.ConfigureProject(project);
        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.Entity : null;
    }
}
