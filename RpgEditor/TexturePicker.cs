using RpgLibrary.ItemClasses;
using System.Drawing.Drawing2D;

namespace RpgEditor;

internal sealed class TexturePicker : Form
{
    private readonly TextBox search = new() { Name = "textureSearch", PlaceholderText = "Filter textures", Dock = DockStyle.Top, AccessibleName = "Filter textures" };
    private readonly ListBox textures = new() { Name = "textures", Dock = DockStyle.Fill, AccessibleName = "Textures", HorizontalScrollbar = true };
    private readonly ListBox usages = new() { Dock = DockStyle.Fill, AccessibleName = "Items using this sheet", HorizontalScrollbar = true };
    private readonly SpriteSheetView sheet = new() { Name = "spriteSheet", Dock = DockStyle.Fill };
    private readonly NumericUpDown x = Coordinate();
    private readonly NumericUpDown y = Coordinate();
    private readonly NumericUpDown width = Coordinate(1);
    private readonly NumericUpDown height = Coordinate(1);
    private readonly NumericUpDown tileWidth = Coordinate(1, 32);
    private readonly NumericUpDown tileHeight = Coordinate(1, 32);
    private readonly NumericUpDown margin = Coordinate();
    private readonly NumericUpDown spacing = Coordinate();
    private readonly ComboBox mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 105, AccessibleName = "Selection mode" };
    private readonly Panel preview = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(37, 39, 43), AccessibleName = "Selected sprite preview" };
    private readonly Label status = new() { Dock = DockStyle.Fill, AutoEllipsis = true };
    private readonly Label zoom = new() { AutoSize = true, Margin = new Padding(8, 7, 3, 3) };
    private readonly Button apply = new() { Name = "applySprite", Text = "Use Sprite", AutoSize = true, DialogResult = DialogResult.OK };
    private readonly Dictionary<string, string> sources;
    private readonly ItemData[] items;
    private readonly SpritePickerPreferences? preferences;
    private Bitmap? atlas;
    private bool refreshingTextures;
    private bool updatingInputs;

    public string AssetName { get; private set; } = string.Empty;
    public Rectangle Sprite => new((int)x.Value, (int)y.Value, (int)width.Value, (int)height.Value);

    public TexturePicker(EditorProject project, string texture, Rectangle sprite) : this(
        ContentReferenceValidator.TextureSources(project.Root), texture, sprite,
        project.Assets.Select(entry => entry.Data).OfType<ItemData>(), SpritePickerPreferences.ForProject(project.Root))
    {
    }

    internal TexturePicker(Dictionary<string, string> sources, string texture, Rectangle sprite,
        IEnumerable<ItemData>? items = null, SpritePickerPreferences? preferences = null)
    {
        this.sources = sources;
        this.items = items?.ToArray() ?? [];
        this.preferences = preferences;
        Text = "Choose Texture and Sprite";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1100, 680);
        MinimumSize = new Size(850, 530);
        AutoScaleMode = AutoScaleMode.Dpi;

        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        AcceptButton = apply;
        CancelButton = cancel;
        Controls.Add(BuildLayout(cancel));

        mode.Items.AddRange(["Grid", "Free Select"]);
        mode.SelectedIndex = 0;
        mode.SelectedIndexChanged += (_, _) => UpdateGrid();
        foreach (var input in new[] { tileWidth, tileHeight, margin, spacing })
            input.ValueChanged += (_, _) => UpdateGrid();
        foreach (var input in new[] { x, y, width, height })
            input.ValueChanged += (_, _) =>
            {
                if (!updatingInputs)
                    UpdateSelection();
            };

        preview.Paint += DrawPreview;
        preview.Resize += (_, _) => preview.Invalidate();
        sheet.SpriteSelected += SetRectangle;
        sheet.ViewChanged += () => zoom.Text = $"{sheet.Zoom:P0}";
        search.TextChanged += (_, _) => RefreshTextureList();
        textures.SelectedIndexChanged += (_, _) =>
        {
            if (!refreshingTextures && textures.SelectedItem is string selected && selected != AssetName)
                LoadTexture(selected);
        };
        usages.SelectedIndexChanged += (_, _) =>
        {
            if (usages.SelectedItem is ItemData item)
            {
                var source = item.SourceRectangle;
                var rectangle = new Rectangle(source.X, source.Y, source.Width, source.Height);
                if (IsValid(rectangle))
                {
                    SetRectangle(rectangle);
                    sheet.Center(rectangle);
                }
            }
        };

        string current = texture.Replace('\\', '/');
        string? existing = sources.Keys.FirstOrDefault(key => key.Equals(current, StringComparison.OrdinalIgnoreCase));
        LoadTexture(existing ?? sources.Keys.Order(StringComparer.OrdinalIgnoreCase).FirstOrDefault(), existing is null ? null : sprite);
        RefreshTextureList();
        Shown += (_, _) =>
        {
            if (existing is not null && IsValid(sprite))
                sheet.Center(sprite);
            else if (preferences?.Find(AssetName) is { } saved)
                sheet.RestorePosition(saved.Position);
            if (atlas is null)
                search.Focus();
            else
                sheet.Focus();
        };
        FormClosed += (_, _) =>
        {
            RememberCurrentSheet();
            preferences?.Save();
        };
        Disposed += (_, _) => atlas?.Dispose();
    }

    private Control BuildLayout(Button cancel)
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3, Padding = new Padding(10) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 205));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        toolbar.Controls.AddRange([mode, Field("Tile W", tileWidth), Field("Tile H", tileHeight),
            Field("Margin", margin), Field("Spacing", spacing), ActionButton("Fit", sheet.Fit),
            ActionButton("100%", sheet.ActualSize), ActionButton("Center Selection", () => sheet.Center(sheet.Selection)), zoom]);
        layout.Controls.Add(toolbar, 0, 0);
        layout.SetColumnSpan(toolbar, 3);

        var browser = new Panel { Dock = DockStyle.Fill };
        browser.Controls.Add(textures);
        browser.Controls.Add(search);
        layout.Controls.Add(browser, 0, 1);
        layout.Controls.Add(sheet, 1, 1);

        var details = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        details.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        details.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        details.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        details.Controls.Add(new Label { Text = "Selected Sprite", AutoSize = true }, 0, 0);
        details.Controls.Add(preview, 0, 1);
        var coordinates = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        coordinates.Controls.AddRange([Field("X", x), Field("Y", y), Field("W", width), Field("H", height)]);
        details.Controls.Add(coordinates, 0, 2);
        details.Controls.Add(new Label { Text = "Used by Items", AutoSize = true }, 0, 3);
        usages.DisplayMember = nameof(ItemData.Name);
        details.Controls.Add(usages, 0, 4);
        layout.Controls.Add(details, 2, 1);

        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        status.TextAlign = ContentAlignment.MiddleLeft;
        footer.Controls.Add(status, 0, 0);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        actions.Controls.AddRange([apply, cancel]);
        footer.Controls.Add(actions, 1, 0);
        layout.Controls.Add(footer, 0, 2);
        layout.SetColumnSpan(footer, 3);
        return layout;
    }

    private static Control Field(string label, NumericUpDown input)
    {
        input.AccessibleName = label;
        var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        row.Controls.AddRange([new Label { Text = label, AutoSize = true, Margin = new Padding(3, 7, 0, 3) }, input]);
        return row;
    }

    private static Button ActionButton(string label, Action action)
    {
        var button = new Button { Text = label, AutoSize = true };
        button.Click += (_, _) => action();
        return button;
    }

    private static NumericUpDown Coordinate(int minimum = 0, int value = 0) =>
        new() { Minimum = minimum, Maximum = SpriteGrid.MaxInput, Value = Math.Max(minimum, value), Width = 63 };

    private void RefreshTextureList()
    {
        refreshingTextures = true;
        textures.BeginUpdate();
        try
        {
            textures.Items.Clear();
            textures.Items.AddRange(sources.Keys.Where(path => path.Contains(search.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                .Order(StringComparer.OrdinalIgnoreCase).ToArray());
            textures.SelectedIndex = textures.Items.IndexOf(AssetName);
        }
        finally
        {
            textures.EndUpdate();
            refreshingTextures = false;
        }
    }

    private void LoadTexture(string? asset, Rectangle? selection = null)
    {
        RememberCurrentSheet();
        AssetName = asset ?? string.Empty;
        sheet.SetImage(null);
        atlas?.Dispose();
        atlas = null;
        usages.Items.Clear();
        if (asset is null)
        {
            UpdateSelection();
            return;
        }

        try
        {
            using var image = Image.FromFile(sources[asset]);
            atlas = new Bitmap(image);
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or OutOfMemoryException or UnauthorizedAccessException)
        {
            UpdateSelection();
            status.Text = $"Could not open '{asset}'.";
            return;
        }

        var saved = preferences?.Find(asset);
        var grid = saved?.Grid ?? new SpriteGrid(Math.Min(32, atlas.Width), Math.Min(32, atlas.Height));
        updatingInputs = true;
        tileWidth.Value = grid.Width;
        tileHeight.Value = grid.Height;
        margin.Value = grid.Margin;
        spacing.Value = grid.Spacing;
        mode.SelectedIndex = saved?.GridMode == false ? 1 : 0;
        updatingInputs = false;
        sheet.SetImage(atlas);
        UpdateGrid();
        var first = new Point(grid.Margin, grid.Margin);
        var initial = grid.Select(first, first, atlas.Size);
        SetRectangle(selection ?? (initial.IsEmpty ? new Rectangle(Point.Empty, atlas.Size) : initial));
        if (saved is not null)
            sheet.RestorePosition(saved.Position);
        usages.Items.AddRange(items.Where(item => item.TexturePath.Replace('\\', '/').Equals(asset, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private void UpdateGrid()
    {
        if (updatingInputs)
            return;
        bool gridMode = mode.SelectedIndex == 0;
        sheet.SetGrid(gridMode ? CurrentGrid() : null);
        foreach (var input in new[] { tileWidth, tileHeight, margin, spacing })
            input.Enabled = gridMode;
        UpdateSelection();
    }

    private SpriteGrid CurrentGrid() => new((int)tileWidth.Value, (int)tileHeight.Value, (int)margin.Value, (int)spacing.Value);

    private void RememberCurrentSheet()
    {
        if (atlas is not null)
            preferences?.Remember(AssetName, new SpriteSheetPreference(CurrentGrid(), sheet.GetPosition(), mode.SelectedIndex == 0));
    }

    private void SetRectangle(Rectangle rectangle)
    {
        updatingInputs = true;
        x.Value = Math.Clamp(rectangle.X, x.Minimum, x.Maximum);
        y.Value = Math.Clamp(rectangle.Y, y.Minimum, y.Maximum);
        width.Value = Math.Clamp(rectangle.Width, width.Minimum, width.Maximum);
        height.Value = Math.Clamp(rectangle.Height, height.Minimum, height.Maximum);
        updatingInputs = false;
        UpdateSelection();
    }

    private bool IsValid(Rectangle rect) => atlas is not null && rect.Width > 0 && rect.Height > 0 &&
        new Rectangle(Point.Empty, atlas.Size).Contains(rect);

    private void UpdateSelection()
    {
        var selection = Sprite;
        apply.Enabled = IsValid(selection);
        sheet.SetSelection(apply.Enabled ? selection : Rectangle.Empty);
        preview.Invalidate();
        status.Text = atlas is null ? "Select a texture." :
            !apply.Enabled ? "The source rectangle must fit inside the texture." :
            $"{selection.Width} × {selection.Height} at ({selection.X}, {selection.Y})  ·  Wheel to zoom, middle-drag to pan";
        if (atlas is not null && sheet.Grid is { } grid && (grid.Columns(atlas.Size) == 0 || grid.Rows(atlas.Size) == 0))
            status.Text = "No complete cells fit this grid. Change the grid settings or use Free Select.";
    }

    private void DrawPreview(object? sender, PaintEventArgs e)
    {
        if (!apply.Enabled || atlas is null)
            return;
        var source = Sprite;
        float scale = Math.Min(preview.ClientSize.Width / (float)source.Width, preview.ClientSize.Height / (float)source.Height);
        if (scale >= 1)
            scale = MathF.Floor(scale);
        if (scale <= 0)
            return;
        float scaledWidth = source.Width * scale;
        float scaledHeight = source.Height * scale;
        var destination = new RectangleF((preview.Width - scaledWidth) / 2, (preview.Height - scaledHeight) / 2, scaledWidth, scaledHeight);
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.DrawImage(atlas, destination, source, GraphicsUnit.Pixel);
    }

    internal static void Attach(Form owner, EditorProject project, TextBox path, params TextBoxBase[] coordinates)
    {
        if (ContentReferenceValidator.TextureSources(project.Root).Count == 0)
            return;
        var button = new Button { Text = "…", Width = 35, Height = path.Height, Location = new Point(path.Right + 5, path.Top),
            AccessibleName = "Choose texture and preview sprite" };
        button.Click += (_, _) =>
        {
            int Read(int index) => int.TryParse(coordinates[index].Text, out int value) ? value : 0;
            using var dialog = new TexturePicker(project, path.Text, new Rectangle(Read(0), Read(1), Read(2), Read(3)));
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
