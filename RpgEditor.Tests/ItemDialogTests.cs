using RpgLibrary.ItemClasses;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.QuestClasses;

namespace RpgEditor.Tests;

public sealed class ItemDialogTests
{
    [Theory]
    [InlineData("Interact with object", QuestLocationAction.Interact, 14)]
    [InlineData("Reach area", QuestLocationAction.ReachArea, 20)]
    public async Task QuestTaskPickerOffersOnlyTargetsForTheSelectedAction(string action, QuestLocationAction kind, int id)
    {
        await OnUiThread(() =>
        {
            using var dialog = new FormAddTask();
            dialog.Configure([], true, null, [
                new(QuestLocationAction.Interact, "Level0", 14, "Shrine"),
                new(QuestLocationAction.ReachArea, "Level1", 20, "Catacombs")]);
            Submit(dialog, () =>
            {
                Field<ComboBox>(dialog, "cbType").SelectedItem = action;
                Field<TextBox>(dialog, "tbDescription").Text = "Find the objective";
                var target = Field<ComboBox>(dialog, "questLocation");
                Assert.Equal(id, Assert.IsType<ContentReferenceValidator.QuestTarget>(Assert.Single(target.Items)).ObjectId);
                target.SelectedIndex = 0;
                Field<Button>(dialog, "btnOK").PerformClick();
            });
            var result = Assert.IsType<LocationTaskData>(dialog.TaskData);
            Assert.Equal(kind, result.Action);
            Assert.Equal(id, result.ObjectId);
            Assert.Equal(1, result.RequiredAmount);
        });
    }

    [Fact]
    public async Task EnemyTemplateShowsEffectiveDropsAndTiledPlacementsWithRegisteredChoices()
    {
        string root = Directory.CreateTempSubdirectory("EnemyTemplateTests-").FullName;
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "EntityData"));
            Directory.CreateDirectory(Path.Combine(root, "Items"));
            Directory.CreateDirectory(Path.Combine(root, "TiledFiles"));
            const string skeleton = "SkeletonsAdventure.Entities.Skeleton";
            XnaSerializer.Serialize(Path.Combine(root, "EntityData/Skeleton.xml"), new EnemyData
            {
                Type = skeleton, TextureName = "SkeletonBody", BaseHealth = 20, IsDead = true, CurrentHealth = 0,
                GuaranteedItems = [new("Coins", 2)]
            });
            XnaSerializer.Serialize(Path.Combine(root, "EntityData/Spider.xml"),
                new EnemyData { Type = "SkeletonsAdventure.Entities.Spider", TextureName = "SpiderBody", BaseHealth = 5 });
            XnaSerializer.Serialize(Path.Combine(root, "TextureCatalog.xml"), new Dictionary<string, string>
            {
                ["SkeletonBody"] = "Player/SkeletonSpriteSheet",
                ["AlternateSkeletonBody"] = "Player/SkeletonSpriteSheet",
                ["SpiderBody"] = "EntitySprites/spider"
            });
            XnaSerializer.Serialize(Path.Combine(root, "Items/Bones.xml"), new ItemData { Name = "Bones" });
            foreach (string file in new[] { "Level0", "Level0_Test", "Level1", "TestLevel" })
                File.WriteAllText(Path.Combine(root, $"TiledFiles/{file}.tmx"), "<map />");
            File.WriteAllText(Path.Combine(root, "TiledFiles/Level0.tmx"), """
                <map><objectgroup name="SpawnerLayer">
                  <object id="15" name="Skeleton" x="32.5" y="12">
                    <properties><property name="lvl" value="3" /></properties>
                  </object>
                  <object id="16" name="Skeleton" x="64" y="96" />
                  <object id="17" name="Spider" x="5" y="7" />
                </objectgroup></map>
                """);
            var project = EditorProject.Open(root);
            var asset = Assert.IsType<EnemyData>(project.Assets.Single(entry => entry.Name == skeleton).Data);
            await OnUiThread(() =>
            {
                using var dialog = new EnemyTemplateDialog(project, asset);
                Submit(dialog, () =>
                {
                    var classes = Field<ComboBox>(dialog, "enemyClass");
                    Assert.Equal(ComboBoxStyle.DropDownList, classes.DropDownStyle);
                    Assert.Equal("Skeleton", classes.GetItemText(classes.SelectedItem));
                    Assert.Contains(classes.Items.Cast<object>(), choice => classes.GetItemText(choice) == "Spider (has template)");
                    var textures = Field<ComboBox>(dialog, "enemyTexture");
                    Assert.Equal(ComboBoxStyle.DropDownList, textures.DropDownStyle);
                    Assert.Equal("SkeletonBody", textures.SelectedItem);
                    textures.SelectedItem = "AlternateSkeletonBody";
                    var listedDrops = Field<ListView>(dialog, "guaranteedDrops");
                    Assert.Equal("Coins", listedDrops.Items[0].Text);
                    Assert.Equal("Game default", listedDrops.Items[1].SubItems[2].Text);
                    var spawns = Field<ListView>(dialog, "enemySpawns");
                    Assert.Equal(2, spawns.Items.Count);
                    Assert.Equal("15", spawns.Items[0].SubItems[1].Text);
                    Assert.Equal("32.5, 12", spawns.Items[0].SubItems[2].Text);
                    Assert.Equal("3", spawns.Items[0].SubItems[3].Text);
                    Assert.Equal("Map default", spawns.Items[1].SubItems[3].Text);
                    Field<NumericUpDown>(dialog, "baseEnemyHealth").Value = 25;
                    Field<Button>(dialog, "acceptEnemyTemplate").PerformClick();
                });
                var result = dialog.ReadTemplate();
                Assert.False(result.IsDead);
                Assert.Equal(25, result.BaseHealth);
                Assert.Equal(25, result.CurrentHealth);
                Assert.Equal("AlternateSkeletonBody", result.TextureName);
                Assert.Single(result.GuaranteedItems);
            });
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task NewEnemyTemplateListsOccupiedAndAvailableClassesWithoutAllowingDuplicates()
    {
        using var directory = new SpriteTestDirectory();
        string root = Path.GetDirectoryName(directory.WriteImage("Sheet.png", new Size(16, 16)))!;
        Directory.CreateDirectory(Path.Combine(root, "EntityData"));
        Directory.CreateDirectory(Path.Combine(root, "Items"));
        const string skeleton = "SkeletonsAdventure.Entities.Skeleton";
        XnaSerializer.Serialize(Path.Combine(root, "EntityData/Skeleton.xml"),
            new EnemyData { Type = skeleton, TextureName = "SkeletonBody" });
        XnaSerializer.Serialize(Path.Combine(root, "TextureCatalog.xml"),
            new Dictionary<string, string> { ["SkeletonBody"] = "Sheet" });
        var project = EditorProject.Open(root);
        project.AddEntityTexture("NewSprite", "Sheet");

        await OnUiThread(() =>
        {
            using var dialog = new EnemyTemplateDialog(project, null);
            var classes = Field<ComboBox>(dialog, "enemyClass");
            var textures = Field<ComboBox>(dialog, "enemyTexture");
            var save = Field<Button>(dialog, "acceptEnemyTemplate");
            textures.SelectedItem = "NewSprite";
            classes.SelectedItem = classes.Items.Cast<object>().Single(choice => classes.GetItemText(choice) == "Skeleton (has template)");
            Assert.False(save.Enabled);
            classes.SelectedItem = classes.Items.Cast<object>().Single(choice => classes.GetItemText(choice) == "Goblin");
            Assert.True(save.Enabled);
            Assert.Equal("NewSprite", dialog.ReadTemplate().TextureName);
        });
    }

    [Fact]
    public async Task SpritePickerKeepsAnExistingRectangleAndCanChooseAStandaloneTexture()
    {
        using var directory = new SpriteTestDirectory();
        string sheetFile = directory.WriteImage("Atlas.png", new Size(2048, 3040));
        string bonesFile = directory.WriteImage("Bones.png", new Size(32, 32));
        await OnUiThread(() =>
        {
            var sources = new Dictionary<string, string> { ["Tiles/Atlas"] = sheetFile, ["Items/Bones"] = bonesFile };
            var sword = new Rectangle(1952, 1472, 32, 32);
            using var picker = new TexturePicker(sources, "Tiles/Atlas", sword);
            Submit(picker, () =>
            {
                var sheet = Field<SpriteSheetView>(picker, "spriteSheet");
                Assert.Equal(sword, picker.Sprite);
                Assert.Equal(sword, sheet.Selection);
                Assert.True(sheet.Zoom > 1);
                Field<TextBox>(picker, "textureSearch").Text = "bones";
                Assert.Equal(sword, picker.Sprite);
                var textures = Field<ListBox>(picker, "textures");
                Assert.Single(textures.Items);
                textures.SelectedIndex = 0;
                Field<Button>(picker, "applySprite").PerformClick();
            });
            Assert.Equal("Items/Bones", picker.AssetName);
            Assert.Equal(new Rectangle(0, 0, 32, 32), picker.Sprite);
        });
    }

    [Fact]
    public async Task WeaponDialogValidatesAttackAndPreservesSpriteAndRequirements()
    {
        var original = new WeaponData
        {
            Name = "Test Sword", Type = "Slashing", Quantity = 7, Position = new(-1.5f, 9.25f),
            Equipped = true, Consumable = true, Stackable = true,
            SourceRectangle = new(128, 256, 32, 32), AttackValue = 10,
            LevelRequirementData = new() { Level = 8, Mana = 3 }
        };
        await OnUiThread(() =>
        {
            using var dialog = new FormWeaponDetails { Weapon = original };
            Submit(dialog, () =>
            {
                Assert.True(Field<TextBox>(dialog, "tbType").ReadOnly);
                var values = Field<Label>(dialog, "requirementValues");
                Assert.Contains("Level 8", values.Text);
                Assert.Contains("Mana 3", values.Text);
                Assert.True(values.Parent!.Bottom <= Field<Button>(dialog, "btnOK").Top);
                Assert.Empty(dialog.Controls.Find("cbConsumable", searchAllChildren: true));
                Assert.Empty(dialog.Controls.Find("cbStackable", searchAllChildren: true));
                Assert.Empty(dialog.Controls.Find("mtbQuantity", searchAllChildren: true));
                var attack = Field<MaskedTextBox>(dialog, "mtbAttackValue");
                attack.Text = "";
                Field<Button>(dialog, "btnOK").PerformClick();
                Assert.Equal(DialogResult.None, dialog.DialogResult);
                attack.Text = "12";
                Field<Button>(dialog, "btnOK").PerformClick();
            });
            var saved = Assert.IsType<WeaponData>(dialog.Weapon);
            Assert.NotSame(original, saved);
            Assert.Equal(12, saved.AttackValue);
            Assert.Equal("Slashing", saved.Type);
            Assert.Equal(1, saved.Quantity);
            Assert.False(saved.Equipped);
            Assert.False(saved.Consumable);
            Assert.False(saved.Stackable);
            Assert.Equal(original.Position, saved.Position);
            Assert.Equal(original.SourceRectangle, saved.SourceRectangle);
            Assert.Equal(8, saved.LevelRequirementData.Level);
            Assert.Equal(3, saved.LevelRequirementData.Mana);
        });
    }

    [Fact]
    public async Task RequirementsDialogCollectsAllFourValuesWithoutChangingTheOriginal()
    {
        await OnUiThread(() =>
        {
            var original = new LevelRequirementData { Level = 3, Attack = 2, Defence = 1, Mana = 4 };
            using var dialog = new EquipmentRequirementsDialog(original);
            Submit(dialog, () =>
            {
                Assert.Equal(3m, Field<NumericUpDown>(dialog, "Level").Value);
                Field<NumericUpDown>(dialog, "Level").Value = 10;
                Field<NumericUpDown>(dialog, "Attack").Value = 5;
                Field<NumericUpDown>(dialog, "Defence").Value = 7;
                Field<NumericUpDown>(dialog, "Mana").Value = 9;
                Field<Button>(dialog, "acceptRequirements").PerformClick();
            });
            var edited = dialog.ReadRequirements();
            Assert.Equal((10, 5, 7, 9), (edited.Level, edited.Attack, edited.Defence, edited.Mana));
            Assert.Equal((3, 2, 1, 4), (original.Level, original.Attack, original.Defence, original.Mana));
        });
    }

    [Fact]
    public void ShieldFieldsOnlyAllowOffHandAndSaveAsOneEquipmentItem()
    {
        var shield = new ShieldData { NumberHands = Hands.Both, Stackable = true, Consumable = true, Quantity = 4 };
        var fields = new ShieldAuthoringFields(shield);
        var properties = System.ComponentModel.TypeDescriptor.GetProperties(fields);
        Assert.True(properties[nameof(fields.Slot)]!.IsReadOnly);
        Assert.True(properties[nameof(fields.Type)]!.IsReadOnly);
        Assert.Null(properties[nameof(shield.NumberHands)]);
        Assert.Null(properties[nameof(shield.Stackable)]);
        Assert.Null(properties[nameof(shield.Consumable)]);
        Assert.Null(properties[nameof(shield.Quantity)]);

        ItemInput.ApplyEquipmentTemplateDefaults(shield);
        Assert.Equal(Hands.Off, shield.NumberHands);
        Assert.Equal(1, shield.Quantity);
        Assert.Equal("Shield", shield.Type);
        Assert.False(shield.Stackable);
        Assert.False(shield.Consumable);
    }

    [Fact]
    public async Task ArmorDialogUsesSpriteFieldsAndPreservesEquipmentRequirements()
    {
        await OnUiThread(() =>
        {
            var original = new ArmorData
            {
                Name = "Test Robe", Quantity = 1, Position = new(5, 6),
                SourceRectangle = new(128, 256, 32, 32),
                LevelRequirementData = new() { Level = 7 }
            };
            using var dialog = new FormArmorDetails { Armor = original };
            Submit(dialog, () =>
            {
                Assert.Contains("Level 7", Field<Label>(dialog, "requirementValues").Text);
                Field<MaskedTextBox>(dialog, "mtbX").Text = "256";
                Field<Button>(dialog, "btnOK").PerformClick();
            });
            var saved = Assert.IsType<ArmorData>(dialog.Armor);
            Assert.Equal(original.Position, saved.Position);
            Assert.Equal(256, saved.SourceRectangle.X);
            Assert.Equal(original.SourceRectangle.Y, saved.SourceRectangle.Y);
            Assert.Equal(7, saved.LevelRequirementData.Level);
        });
    }

    private static T Field<T>(Form dialog, string name) where T : Control =>
        Assert.IsType<T>(Assert.Single(dialog.Controls.Find(name, searchAllChildren: true)));

    private static void Submit(Form dialog, Action edit)
    {
        Exception? failure = null;
        dialog.Shown += (_, _) =>
        {
            try { edit(); }
            catch (Exception error) { failure = error; }
            finally
            {
                if (dialog.DialogResult == DialogResult.None)
                    dialog.Close();
            }
        };
        var result = dialog.ShowDialog();
        if (failure is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw(failure);
        Assert.Equal(DialogResult.OK, result);
    }

    private static Task OnUiThread(Action action)
    {
        var result = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try { action(); result.SetResult(); }
            catch (Exception ex) { result.SetException(ex); }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return result.Task;
    }

    private sealed class SpriteTestDirectory : IDisposable
    {
        private readonly string path = Directory.CreateTempSubdirectory("SpritePickerTests-").FullName;

        public string WriteImage(string name, Size size)
        {
            string file = Path.Combine(path, name);
            using var bitmap = new Bitmap(size.Width, size.Height);
            bitmap.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            return file;
        }

        public void Dispose() => Directory.Delete(path, recursive: true);
    }
}
