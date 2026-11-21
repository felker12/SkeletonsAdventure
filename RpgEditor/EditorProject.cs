using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;
using SkeletonsAdventure.Content;

namespace RpgEditor;

internal sealed record EditorAsset(string RelativePath, object Data)
{
    public string Name => Data switch
    {
        ItemData item => item.Name,
        EntityData entity => entity.Type,
        QuestData quest => quest.Name,
        _ => throw new InvalidOperationException("Unsupported editor asset.")
    };
}

internal sealed class EditorProject
{
    private const string TextureCatalogFile = TextureCatalog.AssetName + ".xml";
    private readonly Dictionary<string, EditorAsset> assets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> revisions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, byte[]> importedTextures = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string>? pendingTextureCatalog;
    private Dictionary<string, EditorAsset> saved = new(StringComparer.OrdinalIgnoreCase);
    private bool newProject;

    public string Root { get; }
    public bool IsGameContent { get; }
    public RPG? Metadata { get; }
    public string Title => Metadata?.Name ?? "Skeletons Adventure Content";
    public IEnumerable<EditorAsset> Assets => assets.Values;
    public bool IsDirty => newProject || pendingTextureCatalog is not null || assets.Count != saved.Count ||
        assets.Any(pair => !saved.TryGetValue(pair.Key, out var original) || !ReferenceEquals(original, pair.Value));

    private EditorProject(string root, bool isGameContent, RPG? metadata)
    {
        Root = root;
        IsGameContent = isGameContent;
        Metadata = metadata;
    }

    public static EditorProject Create(string parent, RPG metadata)
    {
        string root = Path.GetFullPath(Path.Combine(parent, "Game"));
        if (Directory.Exists(root) || File.Exists(root))
            throw new IOException($"A project already exists at '{root}'.");
        var project = new EditorProject(root, false, metadata) { newProject = true };
        project.Save();
        return project;
    }

    public static EditorProject Open(string selectedPath)
    {
        string selected = Path.GetFullPath(selectedPath);
        foreach (string candidate in new[] { selected, Path.Combine(selected, "Game"), Path.Combine(selected, "Assets"),
            Path.Combine(selected, "SkeletonsAdventure.Content", "Assets") })
        {
            string metadataPath = Path.Combine(candidate, "Game.xml");
            bool legacy = File.Exists(metadataPath);
            bool content = Directory.Exists(Path.Combine(candidate, "EntityData")) &&
                Directory.Exists(Path.Combine(candidate, "Items"));
            if (!legacy && !content)
                continue;

            var project = new EditorProject(candidate, !legacy, legacy ? XnaSerializer.Deserialize<RPG>(metadataPath) : null);
            project.Load<ItemData>("Items");
            project.Load<EntityData>(legacy ? "Entities" : "EntityData");
            project.Load<QuestData>("Quests");
            project.MarkSaved();
            return project;
        }

        throw new DirectoryNotFoundException("Select the game content project, its Assets folder, or a legacy Game project.");
    }

    private void Load<T>(string directory) where T : class
    {
        string path = Path.Combine(Root, directory);
        if (!Directory.Exists(path))
            return;
        foreach (string file in Directory.EnumerateFiles(path, "*.xml", SearchOption.AllDirectories))
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(file);
                using var stream = new MemoryStream(bytes);
                var entry = new EditorAsset(Path.GetRelativePath(Root, file), XnaSerializer.Deserialize<T>(stream));
                CheckName(entry.Data, entry.Name, null);
                assets.Add(entry.RelativePath, entry);
                revisions.Add(entry.RelativePath, ProjectFileRevision.Of(bytes)!);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Could not open '{file}'. {ex.Message}", ex);
            }
        }
    }

    public IReadOnlyList<AssetUsage> FindUsages(EditorAsset asset) => AssetReferences.Find(this, asset);

    public IReadOnlyDictionary<string, string> EntityTextures =>
        pendingTextureCatalog ?? ContentReferenceValidator.ReadTextureCatalog(Root);

    public void AddEntityTexture(string name, string sourcePath)
    {
        if (!IsGameContent)
            throw new InvalidDataException("Open the game content project to register entity textures.");
        if (string.IsNullOrWhiteSpace(name) || name != name.Trim())
            throw new InvalidDataException("Enter a texture name.");

        var catalog = pendingTextureCatalog;
        string? revision = null;
        if (catalog is null)
        {
            string file = Path.Combine(Root, TextureCatalogFile);
            byte[] bytes = File.ReadAllBytes(file);
            using var source = new MemoryStream(bytes);
            catalog = ContentReferenceValidator.ReadTextureCatalog(Root, source);
            revision = ProjectFileRevision.Of(bytes);
        }
        if (catalog.Keys.Contains(name, StringComparer.OrdinalIgnoreCase))
            throw new InvalidDataException($"A texture named '{name}' is already registered.");

        string asset = sourcePath;
        if (Path.IsPathFullyQualified(sourcePath))
        {
            if (!Path.GetExtension(sourcePath).Equals(".png", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Choose a PNG sprite sheet.");
            string filename = Path.GetFileNameWithoutExtension(sourcePath);
            if (string.IsNullOrWhiteSpace(filename) || filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidDataException("Choose a PNG with a valid filename.");
            string relative = Path.Combine("EntitySprites", filename + ".png");
            if (File.Exists(Path.Combine(Root, relative)) || importedTextures.ContainsKey(relative))
                throw new InvalidDataException($"'{relative}' already exists. Choose it from the project textures or rename the PNG before importing.");

            byte[] image = File.ReadAllBytes(sourcePath);
            using var stream = new MemoryStream(image);
            try
            {
                using var png = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: true);
                if (png.RawFormat.Guid != System.Drawing.Imaging.ImageFormat.Png.Guid)
                    throw new InvalidDataException("The selected image is not a PNG.");
            }
            catch (Exception ex) when (ex is ArgumentException or OutOfMemoryException)
            {
                throw new InvalidDataException("The selected PNG could not be opened.", ex);
            }
            asset = Path.ChangeExtension(relative, null).Replace('\\', '/');
            importedTextures.Add(relative, image);
        }
        else if (!ContentReferenceValidator.TextureSources(Root).ContainsKey(asset))
        {
            throw new InvalidDataException($"'{asset}' is not a texture included in the content build.");
        }

        catalog.Add(name, asset);
        if (pendingTextureCatalog is null)
            revisions[TextureCatalogFile] = revision!;
        pendingTextureCatalog = catalog;
    }

    public EditorAsset Change(EditorAsset? original, object replacement, bool propagateRename = false)
    {
        if (IsGameContent && replacement is EnemyData enemy && !EnemyClassCatalog.Contains(enemy.Type))
            throw new InvalidDataException($"Unknown enemy class '{enemy.Type}'. Choose a registered enemy class.");
        var entry = new EditorAsset(original?.RelativePath ?? string.Empty, replacement);
        CheckName(replacement, entry.Name, original);

        if (original is null)
        {
            string folder = replacement switch
            {
                ArmorData => Path.Combine("Items", "Armor"),
                WeaponData => Path.Combine("Items", IsGameContent ? "Weapons" : "Weapon"),
                ShieldData => Path.Combine("Items", "Shields"),
                ConsumableData => Path.Combine("Items", "Consumables"),
                ItemData => Path.Combine("Items", "Misc"),
                EntityData => IsGameContent ? "EntityData" : "Entities",
                QuestData => "Quests",
                _ => throw new InvalidOperationException("Unsupported editor asset.")
            };
            string path = Path.Combine(folder, entry.Name + ".xml");
            if (assets.ContainsKey(path) || (File.Exists(Path.Combine(Root, path)) && !saved.ContainsKey(path)))
                throw new InvalidDataException($"An asset file already exists at '{path}'.");
            entry = entry with { RelativePath = path };
        }

        if (propagateRename && original is not null && original.Name != entry.Name)
        {
            entry = AssetReferences.Rewrite(entry, original, entry.Name);
            PropagateReferences(original, entry.Name);
        }
        assets[entry.RelativePath] = entry;
        return entry;
    }

    public void Remove(EditorAsset entry, EditorAsset? replacement = null)
    {
        if (replacement is not null)
        {
            if (!assets.TryGetValue(replacement.RelativePath, out var current) || !ReferenceEquals(current, replacement) ||
                !CanReplaceReferences(entry, replacement))
                throw new InvalidDataException("Choose a compatible replacement asset.");
            PropagateReferences(entry, replacement.Name);
        }
        assets.Remove(entry.RelativePath);
    }

    public bool CanReplaceReferences(EditorAsset target, EditorAsset candidate)
    {
        if (!AssetReferences.CanReplace(target, candidate))
            return false;
        if (candidate.Data is not QuestData)
            return true;

        var quests = assets.Values.Select(asset => asset.Data).OfType<QuestData>().ToDictionary(quest => quest.Name);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        return !RequiresTarget(candidate.Name);

        bool RequiresTarget(string name)
        {
            if (name == target.Name)
                return true;
            if (!visited.Add(name))
                return false;
            if (quests.TryGetValue(name, out var quest))
                return quest.RequiredQuestNameData.Any(RequiresTarget);

            return IsGameContent && name == SkeletonsAdventure.Content.BuiltInQuests.Test2 &&
                RequiresTarget(SkeletonsAdventure.Content.BuiltInQuests.TestQuest);
        }
    }

    private void PropagateReferences(EditorAsset target, string replacementName)
    {
        var rewrites = assets.Values.Where(source => source.RelativePath != target.RelativePath)
            .Select(source => AssetReferences.Rewrite(source, target, replacementName))
            .Where(source => !ReferenceEquals(assets[source.RelativePath], source))
            .ToArray();
        foreach (var source in rewrites)
            assets[source.RelativePath] = source;
    }

    private void CheckName(object data, string name, EditorAsset? original)
    {
        if (string.IsNullOrWhiteSpace(name) || name != name.Trim() || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidDataException("Enter a name that can be used in a project file.");
        if (assets.Values.Any(entry => !ReferenceEquals(entry, original) &&
            string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase) &&
            (data, entry.Data) is ((ItemData, ItemData) or (EntityData, EntityData) or (QuestData, QuestData))))
            throw new InvalidDataException($"An asset named '{name}' already exists in this category.");
    }

    public void ReloadAsset(string file)
    {
        string relative = Path.GetRelativePath(Root, file);
        if (relative.Equals(TextureCatalogFile, StringComparison.OrdinalIgnoreCase))
        {
            pendingTextureCatalog = null;
            importedTextures.Clear();
            return;
        }
        if (File.Exists(file) && importedTextures.Remove(relative))
            return;

        var current = assets.GetValueOrDefault(relative) ?? saved.GetValueOrDefault(relative);
        if (current is null)
            throw new InvalidDataException($"'{relative}' is not an editor asset.");

        byte[]? bytes = ProjectFileRevision.ReadBytes(file);
        if (bytes is null)
        {
            assets.Remove(relative);
            saved.Remove(relative);
            revisions.Remove(relative);
            return;
        }

        using var stream = new MemoryStream(bytes);
        object data = current.Data switch
        {
            ItemData => XnaSerializer.Deserialize<ItemData>(stream),
            EntityData => XnaSerializer.Deserialize<EntityData>(stream),
            QuestData => XnaSerializer.Deserialize<QuestData>(stream),
            _ => throw new InvalidDataException($"Unsupported asset '{relative}'.")
        };
        var reloaded = new EditorAsset(relative, data);
        CheckName(data, reloaded.Name, current);
        assets[relative] = saved[relative] = reloaded;
        revisions[relative] = ProjectFileRevision.Of(bytes)!;
    }

    public void Save(IReadOnlyDictionary<string, string?>? approvedOverwrites = null)
    {
        var pending = new List<(string Path, object? Data)>();
        if (newProject)
            pending.Add(("Game.xml", Metadata));
        foreach (var (path, image) in importedTextures)
            pending.Add((path, image));
        if (pendingTextureCatalog is not null)
            pending.Add((TextureCatalogFile, pendingTextureCatalog));
        foreach (var entry in assets.Values)
            if (!saved.TryGetValue(entry.RelativePath, out var original) || !ReferenceEquals(original, entry))
                pending.Add((entry.RelativePath, entry.Data));
        foreach (var entry in saved.Values)
            if (!assets.ContainsKey(entry.RelativePath))
                pending.Add((entry.RelativePath, null));

        var changes = new List<(string Path, ProjectFileChange Change, string? ExpectedRevision)>();
        try
        {
            foreach (var (relative, data) in pending)
            {
                string file = Path.Combine(Root, relative);
                string? actual = ProjectFileRevision.Read(file);
                if (actual != revisions.GetValueOrDefault(relative) &&
                    (approvedOverwrites is null || !approvedOverwrites.TryGetValue(file, out var approved) || actual != approved))
                    throw new ProjectSaveConflictException(file, actual);
                changes.Add((relative, ProjectFileChange.Stage(file, data), actual));
            }

            foreach (var (_, change, expected) in changes)
                change.Apply(expected);
            if (IsGameContent)
                ContentReferenceValidator.Validate(Root);
        }
        catch (Exception failure)
        {
            var errors = new List<Exception> { failure };
            foreach (var (_, change, _) in changes.AsEnumerable().Reverse())
            {
                try { change.RollBack(); }
                catch (Exception error) { errors.Add(error); }
            }
            if (errors.Count > 1)
                throw new IOException("The save failed and some files could not be restored. Recovery files were retained.",
                    new AggregateException(errors));
            throw;
        }

        MarkSaved();
        pendingTextureCatalog = null;
        importedTextures.Clear();
        foreach (var (relative, change, _) in changes)
        {
            if (change.Revision is { } revision)
                revisions[relative] = revision;
            else
                revisions.Remove(relative);
            change.DiscardBackup();
        }
    }

    private void MarkSaved()
    {
        newProject = false;
        saved = new(assets, StringComparer.OrdinalIgnoreCase);
    }
}
