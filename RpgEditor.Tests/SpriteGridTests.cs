namespace RpgEditor.Tests;

public sealed class SpriteGridTests
{
    [Fact]
    public void SheetPreferencesPersistGridAndViewPerTexture()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.Delete(path);
            var preferences = new SpritePickerPreferences(path);
            var atlas = new SpriteSheetPreference(new SpriteGrid(24, 16, 2, 1),
                new SpriteSheetPosition(2, 150.5f, 75, false), GridMode: true);
            var irregular = new SpriteSheetPreference(new SpriteGrid(32, 32),
                new SpriteSheetPosition(1, 16, 16, true), GridMode: false);
            preferences.Remember("TileSets/Sheet", atlas);
            preferences.Remember("Items/Other", irregular);
            preferences.Save();

            var reopened = new SpritePickerPreferences(path);
            Assert.Equal(atlas, reopened.Find("tilesets/sheet"));
            Assert.Equal(irregular, reopened.Find("Items/Other"));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ExistingItemAtlasSnapsToTheSwordCell()
    {
        var grid = new SpriteGrid(32, 32);
        var sheet = new Size(2048, 3040);
        var click = new Point(1959, 1481);
        Assert.Equal(new Rectangle(1952, 1472, 32, 32), grid.Select(click, click, sheet));
    }

    [Fact]
    public void SpacedGridSnapsBothDragDirectionsAndClipsToWholeCells()
    {
        var grid = new SpriteGrid(12, 8, Margin: 2, Spacing: 3);
        var sheet = new Size(49, 39);
        var expected = new Rectangle(2, 2, 42, 30);
        Assert.Equal(expected, grid.Select(new Point(3, 4), new Point(65, 55), sheet));
        Assert.Equal(expected, grid.Select(new Point(39, 29), new Point(0, 0), sheet));
        Assert.True(grid.Select(new Point(14, 4), new Point(28, 20), sheet).IsEmpty);
        Assert.True(grid.Select(Point.Empty, new Point(8, 8), sheet).IsEmpty);
        Assert.True(new SpriteGrid(128, 128).Select(new Point(5, 5), new Point(6, 6), sheet).IsEmpty);
    }

    [Fact]
    public void FreeSelectionIncludesBothEndpointsAndClampsAtTheSheetEdges()
    {
        var sheet = new Size(32, 20);
        Assert.Equal(new Rectangle(7, 3, 1, 1), SpriteGrid.SelectPixels(new Point(7, 3), new Point(7, 3), sheet));
        Assert.Equal(new Rectangle(0, 12, 11, 8),
            SpriteGrid.SelectPixels(new Point(10, 12), new Point(-5, 22), sheet));
    }
}
