using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.Papercraft.Rendering.Svg.Services;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Rendering.Svg;

/// <summary>
/// Papercraft render backend that emits SVG XML.
/// </summary>
public sealed class SvgRenderBackend : IPapercraftRenderBackend
{
    /// <summary>
    /// The SVG media type supported by this backend.
    /// </summary>
    public const string MediaType = "image/svg+xml";

    private static readonly RendererCapabilities StaticCapabilities = new(
        "svg",
        "SVG",
        RendererOutputKind.VectorImage,
        new[] { MediaType },
        new Dictionary<string, RendererSupportLevel>(StringComparer.OrdinalIgnoreCase)
        {
            [RendererFeatures.VectorImageOutput] = RendererSupportLevel.Supported,
            [RendererFeatures.Multipage] = RendererSupportLevel.Supported,
            [RendererFeatures.TextMeasurement] = RendererSupportLevel.Supported,
            [RendererFeatures.TextDrawing] = RendererSupportLevel.Supported,
            [RendererFeatures.Images] = RendererSupportLevel.Supported,
            [RendererFeatures.Clipping] = RendererSupportLevel.Supported,
            [RendererFeatures.Transparency] = RendererSupportLevel.Supported,
            [RendererFeatures.Fonts] = RendererSupportLevel.Supported,
            [RendererFeatures.Color] = RendererSupportLevel.Supported,
            [RendererFeatures.AbsolutePositioning] = RendererSupportLevel.Supported,
            [RendererFeatures.LinkAnnotations] = RendererSupportLevel.Supported,
            [RendererFeatures.EmbeddedFiles] = RendererSupportLevel.Unsupported,
        },
        "Dependency-free SVG vector image output. Multi-page documents are emitted as vertically stacked page groups.");

    /// <inheritdoc />
    public RendererCapabilities Capabilities => StaticCapabilities;

    /// <inheritdoc />
    public ITextService TextService { get; } = new SvgTextService();

    /// <inheritdoc />
    public ValueTask<RenderValidationResult> ValidateAsync(
        PapercraftDocument document,
        RenderTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(target);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = RenderValidationResult.Combine(
            Capabilities.ValidateTarget(target),
            Capabilities.ValidateDocument(document));
        return ValueTask.FromResult(validation);
    }

    /// <inheritdoc />
    public async ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);

        var validation = await ValidateAsync(document, output.Target, cancellationToken)
            .ConfigureAwait(false);
        validation.ThrowIfUnsupported();

        await WriteSvgAsync(document, output.Stream, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);

        var validation = await ValidateAsync(document, output.Target, cancellationToken)
            .ConfigureAwait(false);
        validation.ThrowIfUnsupported();
    }

    private static async ValueTask WriteSvgAsync(
        PapercraftDocument document,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        var canvasSize = GetCanvasSize(document);
        var settings = new XmlWriterSettings
        {
            Async = true,
            CloseOutput = false,
            Encoding = new UTF8Encoding(false),
            Indent = true,
        };

        using var writer = XmlWriter.Create(outputStream, settings);
        var displayListWriter = new SvgDisplayListWriter();

        writer.WriteStartDocument();
        SvgXml.WriteStartSvgElement(writer, "svg");
        writer.WriteAttributeString("version", "1.1");
        writer.WriteAttributeString("xmlns", "xlink", null, SvgXml.XlinkNamespace);
        writer.WriteAttributeString("xmlns", "papercraft", null, SvgXml.PapercraftNamespace);
        writer.WriteAttributeString("width", SvgXml.FormatNumber(canvasSize.Width));
        writer.WriteAttributeString("height", SvgXml.FormatNumber(canvasSize.Height));
        writer.WriteAttributeString("viewBox", SvgXml.FormatViewBox(0, 0, canvasSize.Width, canvasSize.Height));
        writer.WriteAttributeString("role", "img");

        SvgXml.WriteStartSvgElement(writer, "title");
        writer.WriteString("Papercraft SVG document");
        writer.WriteEndElement();

        WriteMetadata(writer, document);
        SvgXml.WriteComment(
            writer,
            $"Papercraft SVG backend output. Pages: {document.Pages.Count}. Pages are stacked vertically.");

        var offsetY = 0F;
        foreach (var page in document.Pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SvgXml.WriteComment(writer, $"Page {page.PageNumber} of {page.TotalPages}");
            displayListWriter.WritePage(writer, page, offsetY, cancellationToken);
            offsetY += page.PageSize.Height;
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
        await writer.FlushAsync()
            .ConfigureAwait(false);
    }

    private static void WriteMetadata(XmlWriter writer, PapercraftDocument document)
    {
        SvgXml.WriteStartSvgElement(writer, "metadata");
        writer.WriteStartElement("papercraft", "document", SvgXml.PapercraftNamespace);
        writer.WriteAttributeString("renderer", StaticCapabilities.RendererId);
        writer.WriteAttributeString("media-type", MediaType);
        writer.WriteAttributeString("pages", document.Pages.Count.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString(
            "modified",
            document.DocumentOptions.Modified.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        if (!string.IsNullOrWhiteSpace(document.DocumentOptions.Producer))
            writer.WriteAttributeString("producer", document.DocumentOptions.Producer);
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static (float Width, float Height) GetCanvasSize(PapercraftDocument document)
    {
        if (document.Pages.Count is 0)
        {
            return (
                document.DocumentOptions.DotsPerMillimeter * document.DocumentOptions.PageWidthInMillimeters,
                document.DocumentOptions.DotsPerMillimeter * document.DocumentOptions.PageHeightInMillimeters);
        }

        var width = 0F;
        var height = 0F;
        foreach (var page in document.Pages)
        {
            width = Math.Max(width, page.PageSize.Width);
            height += page.PageSize.Height;
        }

        return (width, height);
    }
}
