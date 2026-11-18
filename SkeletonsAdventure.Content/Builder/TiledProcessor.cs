using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content.Pipeline.Tiled;

// Extended 4 invokes the original name-based BuildAsset overload for Tiled dependencies.
public sealed class TiledMapContentProcessor : TiledMapProcessor
{
    public override TiledMapContentItem Process(TiledMapContentItem input, ContentProcessorContext context) =>
        base.Process(input, new TiledProcessorContext(context));
}

public sealed class TiledTilesetContentProcessor : TiledMapTilesetProcessor
{
    public override TiledMapTilesetContentItem Process(TiledMapTilesetContentItem input, ContentProcessorContext context) =>
        base.Process(input, new TiledProcessorContext(context));
}

internal sealed class TiledProcessorContext(ContentProcessorContext context) : ContentProcessorContext
{
    public override string BuildConfiguration => context.BuildConfiguration;
    public override string IntermediateDirectory => context.IntermediateDirectory;
    public override ContentBuildLogger Logger => context.Logger;
    public override ContentIdentity SourceIdentity => context.SourceIdentity;
    public override string OutputDirectory => context.OutputDirectory;
    public override string OutputFilename => context.OutputFilename;
    public override OpaqueDataDictionary Parameters => context.Parameters;
    public override string ProjectDirectory => context.ProjectDirectory;
    public override TargetPlatform TargetPlatform => context.TargetPlatform;
    public override GraphicsProfile TargetProfile => context.TargetProfile;

    public override void AddDependency(string filename) => context.AddDependency(filename);
    public override void AddOutputFile(string filename) => context.AddOutputFile(filename);

    public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset,
        IContentImporter importer, IContentProcessor processor, string? assetName) =>
        context.BuildAsset<TInput, TOutput>(sourceAsset, importer, processor, assetName);

    public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset,
        IContentImporter importer, IContentProcessor processor) =>
        context.BuildAndLoadAsset<TInput, TOutput>(sourceAsset, importer, processor);

    public override TOutput Convert<TInput, TOutput>(TInput input, IContentProcessor processor) =>
        context.Convert<TInput, TOutput>(input, processor);

    [Obsolete]
    public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(
        ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary? processorParameters,
        string? importerName, string? assetName)
    {
        string sourcePath = Path.GetRelativePath(ProjectDirectory, sourceAsset.Filename).Replace('\\', '/');
        var relativeReference = new ExternalReference<TInput>(sourcePath);
        if (Path.GetExtension(sourcePath).Equals(".tsx", StringComparison.OrdinalIgnoreCase))
        {
            return context.BuildAsset<TInput, TOutput>(relativeReference,
                new TiledMapTilesetImporter(), new TiledTilesetContentProcessor(),
                Path.ChangeExtension(sourcePath, ".xnb"));
        }

        var textureProcessor = new TextureProcessor();
        if (processorParameters?.TryGetValue(nameof(TextureProcessor.ColorKeyColor), out var color) == true
            && color is Color colorKey)
        {
            textureProcessor.ColorKeyColor = colorKey;
        }

        return context.BuildAsset<TInput, TOutput>(relativeReference, new TextureImporter(), textureProcessor,
            Path.ChangeExtension(sourcePath, ".xnb"));
    }

    [Obsolete]
    public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset,
        string processorName, OpaqueDataDictionary? processorParameters, string? importerName) =>
        throw new NotSupportedException("Tiled dependencies must be written as XNB files.");

    [Obsolete]
    public override TOutput Convert<TInput, TOutput>(TInput input, string processorName,
        OpaqueDataDictionary processorParameters) =>
        throw new NotSupportedException("Tiled dependencies must be written as XNB files.");
}
