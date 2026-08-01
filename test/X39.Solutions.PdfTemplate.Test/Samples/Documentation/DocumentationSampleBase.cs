using System.Globalization;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Rendering.PdfSharp;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;
using X39.Solutions.Papercraft.Rendering.Svg;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Samples.Documentation;

public abstract class DocumentationSampleBase : SampleBase
{
    private const float DocumentationPreviewDotsPerInch = 192F;
    internal const string UpdateDocumentationSampleAssetsEnvironmentVariable = "PAPERCRAFT_UPDATE_DOCUMENTATION_SAMPLE_ASSETS";

    protected static DocumentOptions CompactDocumentOptions { get; } = new()
    {
        DotsPerInch = DocumentationPreviewDotsPerInch,
        PageWidthInMillimeters = 100,
        PageHeightInMillimeters = 45,
        Margin = new Thickness(new Length(5, ELengthUnit.Millimeters)),
    };

    protected async Task RenderDocumentationSampleAsync(
        string sampleName,
        string xml,
        DocumentOptions? documentOptions = null,
        Action<Generator>? configureGenerator = null,
        CancellationToken cancellationToken = default,
        Action<PdfTemplateServiceBuilder>? configureServices = null)
    {
        var outputDirectory = GetSampleOutputDirectory();
        Directory.CreateDirectory(outputDirectory);
        DeleteStaleSampleFiles(outputDirectory, sampleName);
        var renderOptions = new PapercraftRenderOptions
        {
            DocumentOptions = WithDocumentationPreviewDensity(documentOptions ?? CompactDocumentOptions),
        };
        await RenderSkiaSharpPngAsync(
                outputDirectory,
                sampleName,
                xml,
                renderOptions,
                configureGenerator,
                configureServices,
                cancellationToken)
            .ConfigureAwait(false);
        await RenderArtifactAsync(
                outputDirectory,
                $"{sampleName}-skiasharp.pdf",
                xml,
                PapercraftMediaTypes.ApplicationPdf,
                (services) => services.AddPapercraftSkiaSharpRenderer(),
                renderOptions,
                configureGenerator,
                configureServices,
                cancellationToken)
            .ConfigureAwait(false);
        await RenderArtifactAsync(
                outputDirectory,
                $"{sampleName}.svg",
                xml,
                SvgRenderBackend.MediaType,
                (services) => services.AddPapercraftSvgRenderer(),
                renderOptions,
                configureGenerator,
                configureServices,
                cancellationToken)
            .ConfigureAwait(false);
        await RenderArtifactAsync(
                outputDirectory,
                $"{sampleName}-pdfsharp.pdf",
                xml,
                PapercraftMediaTypes.ApplicationPdf,
                (services) => services.AddPapercraftPdfSharpRenderer(),
                renderOptions,
                configureGenerator,
                configureServices,
                cancellationToken)
            .ConfigureAwait(false);

        AssertDocumentationArtifactsExist(outputDirectory, sampleName);
    }

    internal static string GetSampleOutputDirectory()
    {
        var repositoryRoot = GetRepositoryRoot();
        return ShouldUpdateCheckedInDocumentationSampleAssets()
            ? Path.Combine(repositoryRoot, "docs", "assets", "samples")
            : Path.Combine(
                repositoryRoot,
                "test",
                "X39.Solutions.PdfTemplate.Test",
                "TestResults",
                "documentation-samples");
    }

    private static bool ShouldUpdateCheckedInDocumentationSampleAssets()
    {
        var value = Environment.GetEnvironmentVariable(UpdateDocumentationSampleAssetsEnvironmentVariable);
        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static DocumentOptions WithDocumentationPreviewDensity(DocumentOptions documentOptions)
        => documentOptions with { DotsPerInch = DocumentationPreviewDotsPerInch };

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "docs", "manual", "work-plan.md")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the repository root from the test output directory.");
    }

    private static void DeleteStaleSampleFiles(string outputDirectory, string sampleName)
    {
        DeleteFileIfExists(Path.Combine(outputDirectory, $"{sampleName}.png"));
        DeleteFileIfExists(Path.Combine(outputDirectory, $"{sampleName}.svg"));
        DeleteFileIfExists(Path.Combine(outputDirectory, $"{sampleName}-skiasharp.pdf"));
        DeleteFileIfExists(Path.Combine(outputDirectory, $"{sampleName}-pdfsharp.pdf"));
        foreach (var staleFile in Directory.EnumerateFiles(outputDirectory, $"{sampleName}-page-*.png"))
        {
            File.Delete(staleFile);
        }
    }

    private static async Task RenderSkiaSharpPngAsync(
        string outputDirectory,
        string sampleName,
        string xml,
        PapercraftRenderOptions renderOptions,
        Action<Generator>? configureGenerator,
        Action<PdfTemplateServiceBuilder>? configureServices,
        CancellationToken cancellationToken)
    {
        await using var serviceProvider = CreateRendererServiceProvider(
            (services) => services.AddPapercraftSkiaSharpRenderer(),
            configureServices);
        var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var generator = new Generator(renderer);
        configureGenerator?.Invoke(generator);
        var renderedPages = 0;
        using var textReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(textReader);

        await renderer.RenderRasterPagesAsync(
                xmlReader,
                new RasterPageRenderOutput(
                    PapercraftMediaTypes.ImagePng,
                    (page, _) =>
                    {
                        var fileName = page.PageIndex is 0
                            ? $"{sampleName}.png"
                            : $"{sampleName}-page-{page.PageNumber}.png";
                        var stream = new FileStream(
                            Path.Combine(outputDirectory, fileName),
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.Read);
                        renderedPages++;
                        return ValueTask.FromResult<Stream>(stream);
                    }),
                CultureInfo.InvariantCulture,
                renderOptions,
                cancellationToken)
            .ConfigureAwait(false);

        Assert.True(renderedPages > 0, $"Documentation sample '{sampleName}' did not render any PNG pages.");
    }

    private static async Task RenderArtifactAsync(
        string outputDirectory,
        string fileName,
        string xml,
        string mediaType,
        Action<IServiceCollection> registerRenderer,
        PapercraftRenderOptions renderOptions,
        Action<Generator>? configureGenerator,
        Action<PdfTemplateServiceBuilder>? configureServices,
        CancellationToken cancellationToken)
    {
        await using var serviceProvider = CreateRendererServiceProvider(registerRenderer, configureServices);
        var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var generator = new Generator(renderer);
        configureGenerator?.Invoke(generator);
        using var textReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(textReader);

        await using var outputStream = new FileStream(
            Path.Combine(outputDirectory, fileName),
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read);
        await renderer.RenderAsync(
                xmlReader,
                new RenderOutput(mediaType, outputStream),
                CultureInfo.InvariantCulture,
                renderOptions,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static ServiceProvider CreateRendererServiceProvider(
        Action<IServiceCollection> registerRenderer,
        Action<PdfTemplateServiceBuilder>? configureServices)
    {
        var services = new ServiceCollection();
        registerRenderer(services);
        var builder = new PdfTemplateServiceBuilder(services);
        if (configureServices is null)
            builder.AddControl<MockControl>();
        else
            configureServices(builder);
        return services.BuildServiceProvider();
    }

    private static void AssertDocumentationArtifactsExist(string outputDirectory, string sampleName)
    {
        AssertFileExists(Path.Combine(outputDirectory, $"{sampleName}.png"));
        AssertFileExists(Path.Combine(outputDirectory, $"{sampleName}.svg"));
        AssertFileExists(Path.Combine(outputDirectory, $"{sampleName}-skiasharp.pdf"));
        AssertFileExists(Path.Combine(outputDirectory, $"{sampleName}-pdfsharp.pdf"));
    }

    private static void AssertFileExists(string filePath)
        => Assert.True(File.Exists(filePath) && new FileInfo(filePath).Length > 0, $"Expected generated sample asset '{filePath}'.");

    private static void DeleteFileIfExists(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}
