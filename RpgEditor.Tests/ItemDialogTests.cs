using RpgLibrary.ItemClasses;

namespace RpgEditor.Tests;

public sealed class ItemDialogTests
{
    [Fact]
    public async Task WeaponDialogValidatesAttackAndPreservesSpriteAndRequirements()
    {
        var original = new WeaponData
        {
            Name = "Test Sword", Quantity = 1, Position = new(-1.5f, 9.25f),
            SourceRectangle = new(128, 256, 32, 32), AttackValue = 10,
            LevelRequirementData = new() { Level = 8, Mana = 3 }
        };
        await OnUiThread(() =>
        {
            using var dialog = new FormWeaponDetails { Weapon = original };
            Submit(dialog, () =>
            {
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
            Assert.Equal(original.Position, saved.Position);
            Assert.Equal(original.SourceRectangle, saved.SourceRectangle);
            Assert.Equal(8, saved.LevelRequirementData.Level);
            Assert.Equal(3, saved.LevelRequirementData.Mana);
        });
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
}
