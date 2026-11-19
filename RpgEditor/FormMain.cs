namespace RpgEditor;

public partial class FormMain : Form
{
    private readonly Dictionary<string, Form> assetWindows = [];
    private readonly ToolStripMenuItem validateContent = new("Validate Content");
    internal EditorProject? Project { get; private set; }

    public FormMain()
    {
        InitializeComponent();
        FormClosing += (_, e) => e.Cancel = !CanSwitchProject();
        saveGameToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
        openGameToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
        validateContent.Click += (_, _) => ValidateContent();
        gameToolStripMenuItem.DropDownItems.Insert(gameToolStripMenuItem.DropDownItems.IndexOf(exitGameToolStripMenuItem), validateContent);
        itemsToolStripMenuItem.DropDownItems.Add("Shields", null, (_, _) =>
            ShowAssets("Shields", () => new FormOtherItems<RpgLibrary.ItemClasses.ShieldData>("Shields")));
        itemsToolStripMenuItem.DropDownItems.Add("Consumables", null, (_, _) =>
            ShowAssets("Consumables", () => new FormOtherItems<RpgLibrary.ItemClasses.ConsumableData>("Consumables")));
        itemsToolStripMenuItem.DropDownItems.Add("Other Items", null, (_, _) =>
            ShowAssets("Other Items", () => new FormOtherItems<RpgLibrary.ItemClasses.ItemData>("Other Items")));
        RefreshProjectStatus();
    }

    internal void RefreshProjectStatus()
    {
        Text = Project is null ? "RPG Editor" : $"{Project.Title}{(Project.IsDirty ? " *" : "")} - RPG Editor";
        itemsToolStripMenuItem.Enabled = questsToolStripMenuItem.Enabled = entitiesToolStripMenuItem.Enabled = Project is not null;
        saveGameToolStripMenuItem.Enabled = Project?.IsDirty == true;
        validateContent.Enabled = Project?.IsGameContent == true;
    }

    private bool CanSwitchProject()
    {
        if (Project?.IsDirty != true)
            return true;

        return MessageBox.Show(this, "Save changes to the current project?", "Unsaved Changes",
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) switch
        {
            DialogResult.Yes => SaveProject(),
            DialogResult.No => true,
            _ => false
        };
    }

    private void NewGameToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var dialog = new FormNewGame();
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        using var folders = new FolderBrowserDialog { Description = "Select the parent folder for the new Game project." };
        if (folders.ShowDialog(this) != DialogResult.OK || !CanSwitchProject())
            return;
        try { ActivateProject(EditorProject.Create(folders.SelectedPath, dialog.RPG)); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Could Not Create Project", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void OpenGameToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the game content project, its Assets folder, or a legacy Game project.",
            SelectedPath = Project?.Root ?? string.Empty,
            ShowNewFolderButton = false
        };
        if (dialog.ShowDialog(this) != DialogResult.OK || !CanSwitchProject())
            return;

        try { ActivateProject(EditorProject.Open(dialog.SelectedPath)); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Could Not Open Project", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ActivateProject(EditorProject project)
    {
        foreach (var window in assetWindows.Values)
            window.Dispose();
        assetWindows.Clear();
        Project = project;
        RefreshProjectStatus();
    }

    private bool SaveProject()
    {
        if (Project is null)
            return false;
        try
        {
            Project.Save();
            RefreshProjectStatus();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Could Not Save Project", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void ValidateContent()
    {
        if (Project?.IsGameContent != true)
            return;
        if (Project.IsDirty &&
            MessageBox.Show(this, "Save pending edits and validate the content?", "Validate Content",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            return;

        try
        {
            if (Project.IsDirty)
            {
                if (!SaveProject())
                    return;
            }
            else
                ContentReferenceValidator.Validate(Project.Root);
            MessageBox.Show(this, "Content validation passed.", "Validate Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Content Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveGameToolStripMenuItem_Click(object? sender, EventArgs e) => SaveProject();
    private void ExitGameToolStripMenuItem_Click(object? sender, EventArgs e) => Close();
    private void WeaponsToolStripMenuItem_Click(object? sender, EventArgs e) => ShowAssets("Weapons", () => new FormWeapon());
    private void ArmorToolStripMenuItem_Click(object? sender, EventArgs e) => ShowAssets("Armor", () => new FormArmor());
    private void QuestToolStripMenuItem_Click(object? sender, EventArgs e) => ShowAssets("Quests", () => new FormQuest());
    private void EntityToolStripMenuItem_Click(object? sender, EventArgs e) => ShowAssets("Entities", () => new FormEntity());

    private void ShowAssets(string name, Func<Form> create)
    {
        if (!assetWindows.TryGetValue(name, out var window) || window.IsDisposed)
        {
            window = create();
            window.MdiParent = this;
            window.Text = name;
            assetWindows[name] = window;
        }
        window.Show();
        window.BringToFront();
    }
}
