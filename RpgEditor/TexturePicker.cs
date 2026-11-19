namespace RpgEditor;

internal sealed class TexturePicker : Form
{
    private readonly ComboBox textures = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 370 };
    private readonly NumericUpDown x = Coordinate();
    private readonly NumericUpDown y = Coordinate();
    private readonly NumericUpDown width = Coordinate(1);
    private readonly NumericUpDown height = Coordinate(1);
    private readonly PictureBox preview = new() { Width = 280, Height = 165, SizeMode = PictureBoxSizeMode.Zoom };
    private readonly Label status = new() { AutoSize = true };
    private readonly Button apply = new() { Text = "Use Sprite", AutoSize = true, DialogResult = DialogResult.OK };
    private Bitmap? atlas;

    public string AssetName => (string)textures.SelectedItem!;
    public Rectangle Sprite => new((int)x.Value, (int)y.Value, (int)width.Value, (int)height.Value);

    public TexturePicker(Dictionary<string, string> sources, string texture, Rectangle sprite)
    {
        Text = "Choose Texture and Sprite";
        StartPosition = FormStartPosition.CenterParent;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        var layout = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new(12) };
        var coordinates = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        coordinates.Controls.AddRange([new Label { Text = "X", AutoSize = true }, x, new Label { Text = "Y", AutoSize = true }, y,
            new Label { Text = "Width", AutoSize = true }, width, new Label { Text = "Height", AutoSize = true }, height]);
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        var buttons = new FlowLayoutPanel { AutoSize = true };
        buttons.Controls.AddRange([apply, cancel]);
        layout.Controls.AddRange([textures, coordinates, preview, status, buttons]);
        Controls.Add(layout);
        AcceptButton = apply;
        CancelButton = cancel;

        TextureChanged();
        foreach (var input in new[] { x, y, width, height })
            input.ValueChanged += (_, _) => UpdatePreview();

        textures.SelectedIndexChanged += (_, _) => TextureChanged();
        textures.Items.AddRange(sources.Keys.Order().ToArray());
        x.Value = Math.Clamp(sprite.X, x.Minimum, x.Maximum);
        y.Value = Math.Clamp(sprite.Y, y.Minimum, y.Maximum);
        width.Value = Math.Clamp(sprite.Width, width.Minimum, width.Maximum);
        height.Value = Math.Clamp(sprite.Height, height.Minimum, height.Maximum);
        textures.SelectedIndex = textures.Items.IndexOf(texture.Replace('\\', '/'));
        if (textures.SelectedIndex == -1 && textures.Items.Count > 0)
            textures.SelectedIndex = 0;

        Disposed += (_, _) =>
        {
            preview.Image?.Dispose();
            atlas?.Dispose();
        };

        void TextureChanged()
        {
            atlas?.Dispose();
            atlas = null;
            if (textures.SelectedItem is string selected)
            {
                try
                {
                    using var image = Image.FromFile(sources[selected]);
                    atlas = new Bitmap(image);
                }
                catch (Exception ex) when (ex is IOException or ArgumentException or OutOfMemoryException)
                {
                    UpdatePreview();
                    status.Text = "Could not open the texture.";
                    return;
                }
            }
            UpdatePreview();
        }
    }

    private static NumericUpDown Coordinate(int minimum = 0) => new() { Minimum = minimum, Maximum = 32768, Width = 65 };

    private void UpdatePreview()
    {
        preview.Image?.Dispose();
        preview.Image = null;
        apply.Enabled = atlas is not null && new Rectangle(0, 0, atlas.Width, atlas.Height).Contains(Sprite);
        if (apply.Enabled)
            preview.Image = atlas!.Clone(Sprite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        status.Text = atlas is null ? "Select a texture." :
            !apply.Enabled ? "The source rectangle must fit inside the texture." : $"{atlas.Width} × {atlas.Height}";
    }

    internal static void Attach(Form owner, EditorProject project, TextBox path, params TextBoxBase[] coordinates)
    {
        var sources = ContentReferenceValidator.TextureSources(project.Root);
        if (sources.Count == 0)
            return;
        var button = new Button { Text = "…", Width = 35, Height = path.Height, Location = new Point(path.Right + 5, path.Top),
            AccessibleName = "Choose texture and preview sprite" };
        button.Click += (_, _) =>
        {
            int Read(int index) => int.TryParse(coordinates[index].Text, out int value) ? value : 0;
            using var dialog = new TexturePicker(sources, path.Text, new Rectangle(Read(0), Read(1), Read(2), Read(3)));
            if (dialog.ShowDialog(owner) == DialogResult.OK)
            {
                path.Text = dialog.AssetName;
                int[] values = [dialog.Sprite.X, dialog.Sprite.Y, dialog.Sprite.Width, dialog.Sprite.Height];
                for (int i = 0; i < coordinates.Length; i++)
                    coordinates[i].Text = values[i].ToString();
            }
        };
        owner.Controls.Add(button);
    }
}
