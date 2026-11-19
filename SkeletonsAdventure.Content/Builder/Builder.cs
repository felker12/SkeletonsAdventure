using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGame.Extended.Content.Pipeline.Tilemaps;
using MonoGame.Extended.Content.Pipeline.Tilemaps.Tiled;
using MonoGame.Framework.Content.Pipeline.Builder;
using SkeletonsAdventure.Content;

// The XML importer resolves the RPG data types from loaded assemblies.
_ = typeof(RpgLibrary.AttackData.AttackData).Assembly;

if (args.Length is 2 or 3 && args[0] == "validate")
{
    try
    {
        ContentReferenceValidator.Validate(args[1]);
        if (args.Length == 3)
            ContentReferenceValidator.WriteAssetList(args[1], args[2]);
        return 0;
    }
    catch (InvalidDataException ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 1;
    }
}

var builder = new GameContentBuilder();
builder.Run(args);
return builder.FailedToBuild > 0 ? 1 : 0;

internal sealed class GameContentBuilder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        var content = new ContentCollection();

        var textureImporter = new TextureImporter();
        var textureProcessor = new TextureProcessor
        {
            ColorKeyColor = Color.Magenta,
            ColorKeyEnabled = true,
            GenerateMipmaps = false,
            PremultiplyAlpha = true,
            ResizeToPowerOfTwo = false,
            MakeSquare = false,
            TextureFormat = TextureProcessorOutputFormat.Color
        };
        content.Include<WildcardRule>("**/*.png", textureImporter, textureProcessor);
        content.Include<WildcardRule>("**/*.PNG", textureImporter, textureProcessor);
        foreach (string excluded in ContentReferenceValidator.ExcludedTextures)
            content.Exclude<WildcardRule>(excluded);

        content.Include<WildcardRule>("Fonts/*.spritefont", new FontDescriptionImporter(), new FontDescriptionProcessor
        {
            PremultiplyAlpha = true,
            TextureFormat = TextureProcessorOutputFormat.Compressed
        });

        var xmlImporter = new XmlImporter();
        var xmlProcessor = new PassThroughProcessor();
        content.Include<WildcardRule>("AttackData/*.xml", xmlImporter, xmlProcessor);
        content.Include<WildcardRule>("AttackData/*.Xml", xmlImporter, xmlProcessor);
        content.Include<WildcardRule>("EntityData/*.xml", xmlImporter, xmlProcessor);
        content.Include<WildcardRule>("EntityData/**/*.xml", xmlImporter, xmlProcessor);
        content.Include<WildcardRule>("Items/*.xml", xmlImporter, xmlProcessor);
        content.Include<WildcardRule>("Items/**/*.xml", xmlImporter, xmlProcessor);
        content.Include<WildcardRule>("Quests/*.xml", xmlImporter, xmlProcessor);
        content.Include<WildcardRule>("Quests/**/*.xml", xmlImporter, xmlProcessor);

        foreach (string levelName in MapCatalog.LevelNames)
            content.Include<WildcardRule>(MapCatalog.AssetName(levelName) + ".tmx", new TiledTilemapImporter(), new TilemapProcessor());
        content.Include<WildcardRule>(ContentReferenceValidator.LeverTileset, new TilemapTilesetImporter(), new TilemapTilesetProcessor());

        return content;
    }
}
