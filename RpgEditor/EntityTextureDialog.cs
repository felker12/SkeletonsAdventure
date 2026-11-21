namespace RpgEditor;

internal sealed class EntityTextureDialog : Form
{
    public EntityTextureDialog(EditorProject project)
    {
        Text = "Add Entity Texture";
        ClientSize = new Size(590, 140);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = ShowInTaskbar = false;

        var name = new TextBox { Dock = DockStyle.Fill, AccessibleName = "Texture name" };
        var texture = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, AccessibleName = "Project texture or imported PNG"
        };
        texture.Items.AddRange(ContentReferenceValidator.TextureSources(project.Root).Keys.Order(StringComparer.Ordinal).ToArray());
        texture.SelectedIndexChanged += (_, _) =>
        {
            if (name.Text.Length == 0 && texture.SelectedItem is string selected)
                name.Text = Path.GetFileName(Path.IsPathFullyQualified(selected) ? Path.ChangeExtension(selected, null) : selected);
        };

        var import = new Button { Text = "Import PNG…", AutoSize = true };
        import.Click += (_, _) =>
        {
            using var file = new OpenFileDialog { Title = "Choose an entity sprite sheet", Filter = "PNG images (*.png)|*.png" };
            if (file.ShowDialog(this) != DialogResult.OK)
                return;
            if (!texture.Items.Contains(file.FileName))
                texture.Items.Add(file.FileName);
            texture.SelectedItem = file.FileName;
        };

        var add = new Button { Text = "Add Texture", AutoSize = true };
        add.Click += (_, _) =>
        {
            try
            {
                if (texture.SelectedItem is not string selected)
                    throw new InvalidDataException("Choose a project texture or import a PNG.");
                project.AddEntityTexture(name.Text.Trim(), selected);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                MessageBox.Show(this, ex.Message, "Could Not Add Texture", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        AcceptButton = add;
        CancelButton = cancel;
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        actions.Controls.AddRange([cancel, add]);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 3, RowCount = 3 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = "Texture name", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
        layout.Controls.Add(name, 1, 0);
        layout.Controls.Add(new Label { Text = "Sprite sheet", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
        layout.Controls.Add(texture, 1, 1);
        layout.Controls.Add(import, 2, 1);
        layout.Controls.Add(actions, 0, 2);
        layout.SetColumnSpan(actions, 3);
        Controls.Add(layout);
    }
}
