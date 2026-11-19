using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGame.Extended.Content.Pipeline.Tilemaps;
using MonoGame.Extended.Content.Pipeline.Tilemaps.Tiled;
using MonoGame.Framework.Content.Pipeline.Builder;
using SkeletonsAdventure.Content;

// The XML importer resolves the RPG data types from loaded assemblies.
_ = typeof(RpgLibrary.AttackData.AttackData).Assembly;

if (args is ["validate", var sourceDirectory])
{
    try
    {
        ContentReferenceValidator.Validate(sourceDirectory);
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
    internal static IReadOnlyList<string> ExcludedTextures { get; } =
    [
        "AttackSprites/FireBallSpriteSheet_OG.png",
        "Player/SkeletonSpriteSheet_LeftHalf.png",
        "Player/SkeletonSpriteSheet_Stylized.png",
        // TSX assets build their own textures and would collide with these output paths.
        "TiledFiles/CrossedOffBlock.png",
        "TiledFiles/doors_lever_chest_animation.png",
        "TiledFiles/fire_animation.png",
        "TiledFiles/Objects.png",
        "TiledFiles/ProjectUtumno_full.png",
        "TiledFiles/walls_floor.png"
    ];
    internal const string LeverTileset = "TiledFiles/doors_lever_chest_animation.tsx";

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
        foreach (string excluded in ExcludedTextures)
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
        content.Include<WildcardRule>("Items/**/*.xml", xmlImporter, xmlProcessor);

        foreach (string levelName in MapCatalog.LevelNames)
            content.Include<WildcardRule>(MapCatalog.AssetName(levelName) + ".tmx", new TiledTilemapImporter(), new TilemapProcessor());
        content.Include<WildcardRule>(LeverTileset, new TilemapTilesetImporter(), new TilemapTilesetProcessor());

        return content;
    }
}
