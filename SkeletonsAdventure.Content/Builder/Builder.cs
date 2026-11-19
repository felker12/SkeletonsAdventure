using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGame.Extended.Content.Pipeline.Tilemaps;
using MonoGame.Extended.Content.Pipeline.Tilemaps.Tiled;
using MonoGame.Framework.Content.Pipeline.Builder;

// The XML importer resolves the RPG data types from loaded assemblies.
_ = typeof(RpgLibrary.AttackData.AttackData).Assembly;

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
        content.Exclude<WildcardRule>("AttackSprites/FireBallSpriteSheet_OG.png");
        content.Exclude<WildcardRule>("Player/SkeletonSpriteSheet_LeftHalf.png");
        content.Exclude<WildcardRule>("Player/SkeletonSpriteSheet_Stylized.png");

        // Tilesets build their own texture dependencies. A PNG and TSX with the same stem
        // would otherwise both write to the same XNB path.
        content.Exclude<WildcardRule>("TiledFiles/CrossedOffBlock.png");
        content.Exclude<WildcardRule>("TiledFiles/doors_lever_chest_animation.png");
        content.Exclude<WildcardRule>("TiledFiles/fire_animation.png");
        content.Exclude<WildcardRule>("TiledFiles/Objects.png");
        content.Exclude<WildcardRule>("TiledFiles/ProjectUtumno_full.png");
        content.Exclude<WildcardRule>("TiledFiles/walls_floor.png");

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

        content.Include<WildcardRule>("TiledFiles/*.tmx", new TiledTilemapImporter(), new TilemapProcessor());
        content.Include<WildcardRule>("TiledFiles/doors_lever_chest_animation.tsx", new TilemapTilesetImporter(), new TilemapTilesetProcessor());

        return content;
    }
}
