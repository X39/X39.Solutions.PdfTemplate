using System.Globalization;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services.ResourceResolver;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// A control that draws an image.
/// </summary>
[Control(Constants.ControlsNamespace)]
public sealed class ImageControl : AlignableControl, IInitializeAsync, IDisposable
{
    private readonly IResourceResolver _resourceResolver;

    /// <summary>
    /// Creates a new <see cref="ImageControl"/>.
    /// </summary>
    /// <param name="resourceResolver">The <see cref="IResourceResolver"/> to use.</param>
    public ImageControl(IResourceResolver resourceResolver)
    {
        _resourceResolver = resourceResolver;
    }

    /// <summary>
    /// The source of the image to draw.
    /// </summary>
    /// <remarks>
    /// This always has to be resolved, using a <see cref="IResourceResolver"/>.
    /// The default implementation of <see cref="IResourceResolver"/> is <see cref="DefaultResourceResolver"/>.
    /// It will only accept base64 encoded images for security reasons!
    /// Make sure to provide your own <see cref="IResourceResolver"/> if you
    /// want to use other sources, like a file path.
    /// </remarks>
    [Parameter]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// The width of the image.
    /// </summary>
    [Parameter]
    public Length Width { get; set; } = new();

    /// <summary>
    /// The height of the image.
    /// </summary>
    [Parameter]
    public Length Height { get; set; } = new();

    private SKBitmap? _bitmap;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var image = await _resourceResolver.ResolveImageAsync(Source, cancellationToken)
            .ConfigureAwait(false);
        _bitmap = SKBitmap.Decode(image);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _bitmap?.Dispose();
    }

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var width = Width.ToPixels(framedPageSize.Width, dpi);
        var bitmapWidth = _bitmap?.Width ?? 0F;
        var height = Height.ToPixels(framedPageSize.Height, dpi);
        var bitmapHeight = _bitmap?.Height ?? 0F;
        return new Size(
            Width.Unit is ELengthUnit.Auto
                ? Height.Unit is ELengthUnit.Auto
                    ? bitmapWidth
                    : bitmapWidth / bitmapHeight * height
                : width,
            Height.Unit is ELengthUnit.Auto
                ? Width.Unit is ELengthUnit.Auto
                    ? bitmapHeight
                    : bitmapHeight / bitmapWidth * width
                : height
        );
    }

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        var width = Width.ToPixels(framedPageSize.Width, dpi);
        var bitmapWidth = _bitmap?.Width ?? 0F;
        var height = Height.ToPixels(framedPageSize.Height, dpi);
        var bitmapHeight = _bitmap?.Height ?? 0F;
        return new Size(
            Width.Unit is ELengthUnit.Auto
                ? Height.Unit is ELengthUnit.Auto
                    ? bitmapWidth
                    : bitmapWidth / bitmapHeight * height
                : width,
            Height.Unit is ELengthUnit.Auto
                ? Width.Unit is ELengthUnit.Auto
                    ? bitmapHeight
                    : bitmapHeight / bitmapWidth * width
                : height
        );
    }

    /// <inheritdoc />
    protected override void DoRender(ICanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        if (_bitmap is null)
            return;
        canvas.DrawBitmap(_bitmap, ArrangementInner - Arrangement);
    }
}