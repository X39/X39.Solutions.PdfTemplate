using PdfSharp.Drawing;
using PdfSharp.Pdf;
using X39.Solutions.Papercraft.Rendering.PdfSharp.Services;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp;

/// <summary>
/// Papercraft render backend backed by PDFsharp.
/// </summary>
public sealed class PdfSharpRenderBackend : IPapercraftRenderBackend
{
    /// <summary>
    /// The backend id used by <see cref="PapercraftRenderOptions.BackendId"/> to select PDFsharp.
    /// </summary>
    public const string RendererId = "pdfsharp";

    private static readonly RendererCapabilities StaticCapabilities = new(
        RendererId,
        "PDFsharp",
        RendererOutputKind.Pdf,
        new[] { PapercraftMediaTypes.ApplicationPdf },
        new Dictionary<string, RendererSupportLevel>(StringComparer.OrdinalIgnoreCase)
        {
            [RendererFeatures.PdfOutput] = RendererSupportLevel.Supported,
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
            [RendererFeatures.EmbeddedFiles] = RendererSupportLevel.Supported,
        },
        "MIT-licensed PDFsharp backend for PDF output.");

    private readonly PdfSharpDisplayListRenderer _displayListRenderer = new();

    /// <inheritdoc />
    public ITextService TextService { get; } = new PdfSharpTextService();

    /// <inheritdoc />
    public RendererCapabilities Capabilities => StaticCapabilities;

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
            Capabilities.ValidateDocument(document),
            EmbeddedFileValidation.Validate(document),
            PdfSharpSystemFontResolver.Instance.ValidateDocumentFonts(document));
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
        var ownsDiagnosticScope = !RenderDiagnosticScope.IsActive;
        using var diagnosticScope = ownsDiagnosticScope
            ? RenderDiagnosticScope.Begin(output.DiagnosticSink)
            : null;
        var validation = await ValidateAsync(document, output.Target, cancellationToken)
            .ConfigureAwait(false);
        RenderDiagnosticScope.Report(validation.Diagnostics);
        validation.ThrowIfUnsupported();

        if (output.Target.OutputKind is not RendererOutputKind.Pdf)
        {
            throw new RenderValidationException(
                Capabilities.ValidateTarget(output.Target));
        }

        RenderPdf(document, output.Stream, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);
        cancellationToken.ThrowIfCancellationRequested();
        var validation = Capabilities.ValidateTarget(output.Target);
        validation.ThrowIfUnsupported();

        throw new RenderValidationException(
            new RenderValidationResult(
                new[]
                {
                    new RenderDiagnostic(
                        RenderDiagnosticCodes.UnsupportedOutputKind,
                        RendererSupportLevel.Unsupported,
                        RendererFeatures.RasterImageOutput,
                        $"Backend '{Capabilities.DisplayName}' only produces PDF output.",
                        "Use a raster-capable backend for page-by-page raster rendering."),
                }));
    }

    private void RenderPdf(
        PapercraftDocument document,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        PdfSharpFontResolverRegistration.EnsureConfigured();

        using var pdfDocument = new PdfDocument();
        ApplyDocumentInformation(pdfDocument, document.DocumentOptions);

        if (document.Pages.Count is 0)
        {
            AddPage(pdfDocument, GetFallbackPageSize(document.DocumentOptions));
        }
        else
        {
            foreach (var page in document.Pages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var pdfPage = AddPage(pdfDocument, page.PageSize);
                using var graphics = XGraphics.FromPdfPage(
                    pdfPage,
                    XGraphicsPdfPageOptions.Replace,
                    XGraphicsUnit.Point,
                    XPageDirection.Downwards);
                _displayListRenderer.Render(graphics, pdfPage, page.DisplayList);
            }
        }

        PdfSharpEmbeddedFileWriter.Write(pdfDocument, document.DocumentOptions.EmbeddedFiles);
        pdfDocument.Save(outputStream, false);
    }

    private static void ApplyDocumentInformation(PdfDocument pdfDocument, DocumentOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Producer))
            pdfDocument.Info.Creator = options.Producer;

        pdfDocument.Info.CreationDate = options.Modified;
        pdfDocument.Info.ModificationDate = options.Modified;
    }

    private static PdfPage AddPage(PdfDocument pdfDocument, X39.Solutions.Papercraft.Data.Size pageSize)
    {
        var pdfPage = pdfDocument.AddPage();
        pdfPage.Width = XUnit.FromPoint(Math.Max(1D, pageSize.Width));
        pdfPage.Height = XUnit.FromPoint(Math.Max(1D, pageSize.Height));
        return pdfPage;
    }

    private static X39.Solutions.Papercraft.Data.Size GetFallbackPageSize(DocumentOptions options)
        => new(
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);
}
