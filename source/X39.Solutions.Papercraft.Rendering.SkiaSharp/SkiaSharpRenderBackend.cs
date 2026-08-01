using SkiaSharp;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;
using X39.Solutions.Papercraft.Services.TextService;
using SkiaTextService = X39.Solutions.Papercraft.Rendering.SkiaSharp.Services.TextService.TextService;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp;

/// <summary>
/// Papercraft render backend backed by SkiaSharp.
/// </summary>
public sealed class SkiaSharpRenderBackend : IPapercraftRenderBackend
{
    private static readonly RendererCapabilities StaticCapabilities = new(
        "skiasharp",
        "SkiaSharp",
        RendererOutputKind.Pdf | RendererOutputKind.RasterImage,
        new[] { PapercraftMediaTypes.ApplicationPdf, PapercraftMediaTypes.ImagePng },
        new Dictionary<string, RendererSupportLevel>(StringComparer.OrdinalIgnoreCase)
        {
            [RendererFeatures.PdfOutput] = RendererSupportLevel.Supported,
            [RendererFeatures.RasterImageOutput] = RendererSupportLevel.Supported,
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
        "Default Papercraft backend. Supports PDF, single-page PNG stream output and page-by-page PNG raster output through SkiaSharp.");

    private readonly SkiaSharpDisplayListRenderer _displayListRenderer;

    /// <summary>
    /// Creates a new SkiaSharp backend.
    /// </summary>
    public SkiaSharpRenderBackend(
        SkiaSharpDisplayListRenderer displayListRenderer,
        SkPaintCache paintCache)
    {
        ArgumentNullException.ThrowIfNull(displayListRenderer);
        ArgumentNullException.ThrowIfNull(paintCache);
        _displayListRenderer = displayListRenderer;
        TextService = new SkiaTextService(paintCache);
    }

    /// <inheritdoc />
    public RendererCapabilities Capabilities => StaticCapabilities;

    /// <inheritdoc />
    public ITextService TextService { get; }

    /// <inheritdoc />
    public ValueTask<RenderValidationResult> ValidateAsync(
        PapercraftDocument document,
        RenderTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(target);
        using var activity = PapercraftActivity.Start(SkiaSharpActivityNames.Validate);
        PapercraftActivity.SetRenderTarget(activity, target);
        PapercraftActivity.SetDocument(activity, document);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var validation = Capabilities.ValidateTarget(target);
            if (target.OutputKind is RendererOutputKind.Pdf)
            {
                validation = RenderValidationResult.Combine(
                    validation,
                    EmbeddedFileValidation.Validate(document));
            }
            else if (document.DocumentOptions.EmbeddedFiles.Count is not 0)
            {
                validation = RenderValidationResult.Combine(
                    validation,
                    new RenderValidationResult(
                        new[]
                        {
                            new RenderDiagnostic(
                                RenderDiagnosticCodes.UnsupportedFeature,
                                RendererSupportLevel.Unsupported,
                                RendererFeatures.EmbeddedFiles,
                                $"Renderer '{Capabilities.DisplayName}' cannot carry embedded files in '{target.OutputKind}' output.",
                                "Use PDF output to preserve document attachments."),
                        }));
            }
            if (target.OutputKind is RendererOutputKind.RasterImage
                && document.FeatureUses.Any((q) => q.Feature == RendererFeatures.LinkAnnotations))
            {
                validation = RenderValidationResult.Combine(
                    validation,
                    new RenderValidationResult(
                        new[]
                        {
                            new RenderDiagnostic(
                                RenderDiagnosticCodes.DegradedFeature,
                                RendererSupportLevel.Degraded,
                                RendererFeatures.LinkAnnotations,
                                $"Renderer '{Capabilities.DisplayName}' ignores clickable link annotations for raster image output.",
                                "Raster image output preserves the visible hyperlink text but cannot carry PDF URI annotations."),
                        }));
            }
            PapercraftActivity.SetValidation(activity, validation);
            return ValueTask.FromResult(validation);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);
        using var activity = PapercraftActivity.Start(SkiaSharpActivityNames.Render);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        PapercraftActivity.SetDocument(activity, document);
        try
        {
            var validation = await ValidateAsync(document, output.Target, cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetValidation(activity, validation);
            validation.ThrowIfUnsupported();

            switch (output.Target.OutputKind)
            {
                case RendererOutputKind.Pdf:
                    RenderPdf(document, output.Stream, cancellationToken);
                    break;
                case RendererOutputKind.RasterImage:
                    RenderPng(document, output.Stream, cancellationToken);
                    break;
                default:
                    throw new RenderValidationException(
                        new RenderValidationResult(
                            new[]
                            {
                                new RenderDiagnostic(
                                    RenderDiagnosticCodes.UnsupportedOutputKind,
                                    RendererSupportLevel.Unsupported,
                                    RendererFeatures.ForOutputKind(output.Target.OutputKind),
                                    $"Backend '{Capabilities.DisplayName}' does not have a render path for output kind '{output.Target.OutputKind}'."),
                            }));
            }
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);
        using var activity = PapercraftActivity.Start(SkiaSharpActivityNames.RenderRasterPages);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        PapercraftActivity.SetDocument(activity, document);
        try
        {
            var validation = await ValidateAsync(document, output.Target, cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetValidation(activity, validation);
            validation.ThrowIfUnsupported();

            using var imageDecodeCache = new SkiaImageDecodeCache();
            foreach (var page in document.Pages)
            {
                using var pageActivity = PapercraftActivity.Start(SkiaSharpActivityNames.RenderRasterPage);
                pageActivity?.SetTag(PapercraftActivity.PageIndexTag, page.PageIndex);
                pageActivity?.SetTag(PapercraftActivity.PageNumberTag, page.PageNumber);
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using var bitmap = RenderPageToBitmap(page, document.DocumentOptions, imageDecodeCache);
                    var pageInfo = new RasterPageInfo(
                        page.PageIndex,
                        page.PageNumber,
                        output.MediaType,
                        bitmap.Width,
                        bitmap.Height,
                        page.DotsPerMillimeter);
                    var pageStream = await output.OpenPageStreamAsync(pageInfo, cancellationToken)
                        .ConfigureAwait(false)
                                     ?? throw new InvalidOperationException(
                                         "The raster page output callback must return a stream.");
                    try
                    {
                        using (var encodeActivity = PapercraftActivity.Start(SkiaSharpActivityNames.EncodePng))
                        {
                            encodeActivity?.SetTag(PapercraftActivity.PageIndexTag, page.PageIndex);
                            encodeActivity?.SetTag(PapercraftActivity.PageNumberTag, page.PageNumber);
                            using var image = SKImage.FromBitmap(bitmap);
                            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                            data.SaveTo(pageStream);
                        }

                        await pageStream.FlushAsync(cancellationToken)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        if (!output.LeaveStreamsOpen)
                            await pageStream.DisposeAsync()
                                .ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    PapercraftActivity.SetError(pageActivity, ex);
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private void RenderPdf(
        PapercraftDocument document,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        using var activity = PapercraftActivity.Start(SkiaSharpActivityNames.RenderPdf);
        PapercraftActivity.SetRenderTarget(activity, RenderTarget.Pdf);
        PapercraftActivity.SetDocument(activity, document);
        try
        {
            if (document.DocumentOptions.EmbeddedFiles.Count is 0)
            {
                RenderPdfDocument(document, outputStream, cancellationToken, pdfA: true);
                return;
            }

            using var buffer = new MemoryStream();
            RenderPdfDocument(document, buffer, cancellationToken, pdfA: false);
            SkiaPdfEmbeddedFileAppender.Append(buffer, document.DocumentOptions.EmbeddedFiles);
            buffer.Position = 0;
            buffer.CopyTo(outputStream);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private void RenderPdfDocument(
        PapercraftDocument document,
        Stream outputStream,
        CancellationToken cancellationToken,
        bool pdfA)
    {
        var options = document.DocumentOptions;
        using var skDocument = SKDocument.CreatePdf(
            outputStream,
            new SKDocumentPdfMetadata
            {
                RasterDpi = options.DotsPerInch,
                Producer = options.Producer,
                Modified = options.Modified,
                PdfA = pdfA,
            });

        if (document.Pages.Count is 0)
        {
            var size = GetFallbackPageSize(options);
            skDocument.BeginPage(size.Width, size.Height).Dispose();
            skDocument.EndPage();
            skDocument.Close();
            return;
        }

        using var imageDecodeCache = new SkiaImageDecodeCache();
        foreach (var page in document.Pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var canvas = skDocument.BeginPage(page.PageSize.Width, page.PageSize.Height);
            _displayListRenderer.Render(canvas, page.DisplayList, imageDecodeCache);
            skDocument.EndPage();
        }

        skDocument.Close();
    }

    private void RenderPng(
        PapercraftDocument document,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        using var activity = PapercraftActivity.Start(SkiaSharpActivityNames.RenderPng);
        PapercraftActivity.SetRenderTarget(activity, RenderTarget.ImagePng);
        PapercraftActivity.SetDocument(activity, document);
        try
        {
            if (document.Pages.Count is 0)
                return;
            if (document.Pages.Count > 1)
                throw new NotSupportedException(
                    "Single-stream PNG output supports one rendered page. Use RenderRasterPagesAsync for multi-page raster output.");

            cancellationToken.ThrowIfCancellationRequested();
            var page = document.Pages[0];
            using var imageDecodeCache = new SkiaImageDecodeCache();
            using var bitmap = RenderPageToBitmap(page, document.DocumentOptions, imageDecodeCache);
            using (var encodeActivity = PapercraftActivity.Start(SkiaSharpActivityNames.EncodePng))
            {
                encodeActivity?.SetTag(PapercraftActivity.PageIndexTag, page.PageIndex);
                encodeActivity?.SetTag(PapercraftActivity.PageNumberTag, page.PageNumber);
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                data.SaveTo(outputStream);
            }
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private SKBitmap RenderPageToBitmap(
        PapercraftPage page,
        DocumentOptions options,
        SkiaImageDecodeCache? imageDecodeCache = null)
    {
        using var activity = PapercraftActivity.Start(SkiaSharpActivityNames.RenderPageToBitmap);
        activity?.SetTag(PapercraftActivity.PageIndexTag, page.PageIndex);
        activity?.SetTag(PapercraftActivity.PageNumberTag, page.PageNumber);
        try
        {
            var bitmap = new SKBitmap(
                (int)Math.Ceiling(page.PageSize.Width),
                (int)Math.Ceiling(page.PageSize.Height));
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);
            if (imageDecodeCache is null)
                _displayListRenderer.Render(canvas, page.DisplayList);
            else
                _displayListRenderer.Render(canvas, page.DisplayList, imageDecodeCache);
            return bitmap;
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private static (float Width, float Height) GetFallbackPageSize(DocumentOptions options)
        => (
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);
}
