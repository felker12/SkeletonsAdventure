using RpgLibrary.EntityClasses;

namespace RpgEditor;

public partial class FormEntity : FormDetails
{
    public FormEntity()
    {
        InitializeComponent();
        ConfigureAssets(entry => entry.Data is EntityData, EditAsset);
        var addTexture = new Button
        {
            Text = "Add Texture…", Name = "addEntityTexture", AutoSize = true,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left, Location = new Point(lbDetails.Left, btnAdd.Top)
        };
        Controls.Add(addTexture);
        Load += (_, _) =>
        {
            var main = (FormMain)MdiParent!;
            addTexture.Visible = main.Project?.IsGameContent == true;
            addTexture.Click += (_, _) =>
            {
                try
                {
                    using var dialog = new EntityTextureDialog(main.Project!);
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                        main.RefreshProjectStatus();
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException)
                {
                    MessageBox.Show(this, ex.Message, "Could Not Open Textures", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
        };
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
