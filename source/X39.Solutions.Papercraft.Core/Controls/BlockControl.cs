using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Canvas;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Generic vertical grouping container for document flow content.
/// </summary>
[Control(Constants.ControlsNamespace, "block")]
public sealed class BlockControl : AlignableContentControl
{
    private const float PageBoundaryTolerance = 0.001F;

    private readonly List<Size> _arrangedChildSizes = new();
    private float _preRenderAdditionalHeight;

    /// <summary>
    /// The optional background fill for the block.
    /// </summary>
    [Parameter]
    public Color Background { get; set; } = Colors.Transparent;

    /// <summary>
    /// The minimum height reserved by the block.
    /// </summary>
    [Parameter]
    public Length MinHeight { get; set; } = new(0F, ELengthUnit.Pixel);

    /// <summary>
    /// Whether the block should start on a new page when it is not already at a page boundary.
    /// </summary>
    [Parameter]
    public bool PageBreakBefore { get; set; }

    /// <summary>
    /// Whether following content should start on a new page when this block does not end at a page boundary.
    /// </summary>
    [Parameter]
    public bool PageBreakAfter { get; set; }

    /// <inheritdoc />
    public override bool CanAdd(Type type) => true;

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => MeasureOrArrangeChildren(dpi, fullPageSize, remainingSize, cultureInfo, arrange: false);

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        _arrangedChildSizes.Clear();
        return MeasureOrArrangeChildren(dpi, fullPageSize, remainingSize, cultureInfo, arrange: true);
    }

    /// <inheritdoc />
    protected override Size PreRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var baseAdditionalSize = base.PreRender(canvas, dpi, parentSize, cultureInfo);
        var pageHeight = canvas.PageSize.Height > 0F ? canvas.PageSize.Height : parentSize.Height;
        _preRenderAdditionalHeight = CalculatePageBreakAdditionalHeight(
            canvas,
            pageHeight,
            PageBreakBefore);
        if (_preRenderAdditionalHeight > 0F)
            canvas.Translate(0F, _preRenderAdditionalHeight);

        var keepTogetherAdditionalHeight = CalculateKeepTogetherAdditionalHeight(
            canvas,
            pageHeight,
            ArrangementOuter.Height);
        if (keepTogetherAdditionalHeight > 0F)
        {
            canvas.Translate(0F, keepTogetherAdditionalHeight);
            _preRenderAdditionalHeight += keepTogetherAdditionalHeight;
        }

        if (!Clip)
            return baseAdditionalSize with
            {
                Height = baseAdditionalSize.Height + _preRenderAdditionalHeight,
            };

        var dryRunCanvas = DryRunDeferredCanvas.From(canvas);
        dryRunCanvas.Translate(ArrangementInner);
        var contentAdditionalSize = RenderContent(
            dryRunCanvas,
            dpi,
            parentSize,
            cultureInfo,
            renderBackground: false);

        return new Size(
            Math.Max(baseAdditionalSize.Width, contentAdditionalSize.Width),
            baseAdditionalSize.Height + _preRenderAdditionalHeight + contentAdditionalSize.Height);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var contentAdditionalSize = RenderContent(
            canvas,
            dpi,
            parentSize,
            cultureInfo,
            renderBackground: true);
        return new Size(
            contentAdditionalSize.Width,
            _preRenderAdditionalHeight + contentAdditionalSize.Height);
    }

    private Size MeasureOrArrangeChildren(
        float dpi,
        Size fullPageSize,
        Size remainingSize,
        CultureInfo cultureInfo,
        bool arrange)
    {
        var totalHeight = 0F;
        var maxWidth = 0F;
        foreach (var child in Children)
        {
            var childSize = arrange
                ? child.Arrange(dpi, fullPageSize, remainingSize, remainingSize, cultureInfo)
                : child.Measure(dpi, fullPageSize, remainingSize, remainingSize, cultureInfo);
            if (arrange)
                _arrangedChildSizes.Add(childSize);

            maxWidth = Math.Max(maxWidth, childSize.Width);
            totalHeight += childSize.Height;
        }

        var minHeight = Math.Max(0F, MinHeight.ToPixels(remainingSize.Height, dpi));
        return new Size(maxWidth, Math.Max(totalHeight, minHeight));
    }

    private Size RenderContent(
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo,
        bool renderBackground)
    {
        var pageHeight = canvas.PageSize.Height > 0F ? canvas.PageSize.Height : parentSize.Height;
        if (renderBackground)
            RenderBackground(canvas);

        var additionalWidth = 0F;
        var additionalHeight = 0F;
        foreach (var (child, arrangedChildSize) in Children.Zip(_arrangedChildSizes))
        {
            var childAdditionalSize = child.Render(canvas, dpi, parentSize, cultureInfo);
            additionalWidth = Math.Max(additionalWidth, childAdditionalSize.Width);
            additionalHeight += childAdditionalSize.Height;
            canvas.Translate(0F, arrangedChildSize.Height + childAdditionalSize.Height);
        }

        var pageBreakAfterAdditionalHeight = CalculatePageBreakAdditionalHeight(
            canvas,
            pageHeight,
            PageBreakAfter);
        if (pageBreakAfterAdditionalHeight > 0F)
        {
            canvas.Translate(0F, pageBreakAfterAdditionalHeight);
            additionalHeight += pageBreakAfterAdditionalHeight;
        }

        return new Size(additionalWidth, additionalHeight);
    }

    private void RenderBackground(IDeferredCanvas canvas)
    {
        if (Background.Alpha is 0)
            return;

        using var state = canvas.CreateState();
        canvas.Translate(-ArrangementInner);
        canvas.Translate(Arrangement);
        canvas.DrawRect(new Rectangle(0F, 0F, Arrangement.Width, Arrangement.Height), Background);
    }

    private static float CalculatePageBreakAdditionalHeight(
        IDeferredCanvas canvas,
        float pageHeight,
        bool shouldBreak)
    {
        if (!shouldBreak || pageHeight <= 0F)
            return 0F;

        var usedHeight = canvas.GetUsedPageHeight(pageHeight);
        if (usedHeight <= PageBoundaryTolerance)
            return 0F;

        var remainingPageHeight = canvas.GetRemainingPageHeight(pageHeight);
        return remainingPageHeight <= PageBoundaryTolerance
            ? 0F
            : remainingPageHeight;
    }

    private static float CalculateKeepTogetherAdditionalHeight(
        IDeferredCanvas canvas,
        float pageHeight,
        float blockHeight)
    {
        if (pageHeight <= 0F || blockHeight <= 0F || blockHeight > pageHeight + PageBoundaryTolerance)
            return 0F;

        var usedHeight = canvas.GetUsedPageHeight(pageHeight);
        if (usedHeight <= PageBoundaryTolerance)
            return 0F;

        if (usedHeight + blockHeight <= pageHeight + PageBoundaryTolerance)
            return 0F;

        var remainingPageHeight = canvas.GetRemainingPageHeight(pageHeight);
        return remainingPageHeight <= PageBoundaryTolerance
            ? 0F
            : remainingPageHeight;
    }

}
