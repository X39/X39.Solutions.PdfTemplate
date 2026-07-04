using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Canvas;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// Base class for list controls that render markers beside list item content.
/// </summary>
public abstract class ListControlBase : AlignableContentControl
{
    private readonly List<Size> _arrangedItemSizes = new();
    private readonly ITextService _textService;
    private readonly TextStyle _markerTextStyle = new();

    /// <summary>
    /// Creates a new instance of <see cref="ListControlBase"/>.
    /// </summary>
    /// <param name="textService">The text service used for marker measurement and rendering.</param>
    protected ListControlBase(ITextService textService)
    {
        _textService = textService;
    }

    /// <summary>
    /// Distance from the list left edge to item content.
    /// </summary>
    [Parameter]
    public Length Indent { get; set; } = new(6F, ELengthUnit.Millimeters);

    /// <summary>
    /// Width reserved for marker rendering.
    /// </summary>
    [Parameter]
    public Length MarkerWidth { get; set; } = new(4F, ELengthUnit.Millimeters);

    /// <summary>
    /// Vertical spacing inserted between list items.
    /// </summary>
    [Parameter]
    public Length ItemSpacing { get; set; } = new(0F, ELengthUnit.Pixel);

    /// <inheritdoc />
    public override void Add(IControl item)
    {
        if (item is not ListItemControl)
            throw new ArgumentException("Only ListItemControl can be added to a list control.", nameof(item));

        base.Add(item);
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type)
        => type.IsEquivalentTo(typeof(ListItemControl));

    /// <inheritdoc />
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => MeasureOrArrangeItems(dpi, fullPageSize, remainingSize, cultureInfo, arrange: false);

    /// <inheritdoc />
    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
    {
        _arrangedItemSizes.Clear();
        return MeasureOrArrangeItems(dpi, fullPageSize, remainingSize, cultureInfo, arrange: true);
    }

    /// <inheritdoc />
    protected override Size PreRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var baseAdditionalSize = base.PreRender(canvas, dpi, parentSize, cultureInfo);
        if (!Clip)
            return baseAdditionalSize;

        var dryRunCanvas = DryRunDeferredCanvas.From(canvas);
        dryRunCanvas.Translate(ArrangementInner);
        var contentAdditionalSize = RenderItems(dryRunCanvas, dpi, parentSize, cultureInfo);

        return new Size(
            Math.Max(baseAdditionalSize.Width, contentAdditionalSize.Width),
            baseAdditionalSize.Height + contentAdditionalSize.Height);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        => RenderItems(canvas, dpi, parentSize, cultureInfo);

    private Size RenderItems(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var indentPx = Indent.ToPixels(parentSize.Width, dpi);
        var markerWidthPx = MarkerWidth.ToPixels(parentSize.Width, dpi);
        var itemSpacingPx = ItemSpacing.ToPixels(parentSize.Height, dpi);
        var additionalWidth = 0F;
        var additionalHeight = 0F;
        var items = Children.OfType<ListItemControl>().ToArray();
        for (var index = 0; index < items.Length; index++)
        {
            var item = items[index];
            var itemSize = index < _arrangedItemSizes.Count
                ? _arrangedItemSizes[index]
                : item.ArrangementOuter;
            var marker = GetMarkerText(index, cultureInfo);
            RenderMarker(canvas, dpi, marker, parentSize.Width, markerWidthPx);

            Size additionalItemSize;
            using (canvas.CreateState())
            {
                canvas.Translate(indentPx, 0F);
                additionalItemSize = item.Render(
                    canvas,
                    dpi,
                    new Size(Math.Max(0F, parentSize.Width - indentPx), parentSize.Height),
                    cultureInfo);
            }

            if (additionalItemSize.Width > 0F)
                additionalWidth = Math.Max(additionalWidth, indentPx + additionalItemSize.Width);
            additionalHeight += additionalItemSize.Height;

            var offset = itemSize.Height + additionalItemSize.Height;
            if (index < items.Length - 1)
                offset += itemSpacingPx;
            canvas.Translate(0F, offset);
        }

        return new Size(additionalWidth, additionalHeight);
    }

    /// <summary>
    /// Gets the marker text for the zero-based item index.
    /// </summary>
    protected abstract string GetMarkerText(int itemIndex, CultureInfo cultureInfo);

    private Size MeasureOrArrangeItems(
        float dpi,
        Size fullPageSize,
        Size remainingSize,
        CultureInfo cultureInfo,
        bool arrange)
    {
        var indentPx = Indent.ToPixels(remainingSize.Width, dpi);
        var markerWidthPx = MarkerWidth.ToPixels(remainingSize.Width, dpi);
        var itemSpacingPx = ItemSpacing.ToPixels(remainingSize.Height, dpi);
        var contentWidth = Math.Max(0F, remainingSize.Width - indentPx);
        var contentSize = new Size(contentWidth, remainingSize.Height);
        var totalHeight = 0F;
        var maxWidth = 0F;
        var items = Children.OfType<ListItemControl>().ToArray();
        for (var index = 0; index < items.Length; index++)
        {
            var item = items[index];
            var markerSize = MeasureMarker(GetMarkerText(index, cultureInfo), dpi, remainingSize.Width, markerWidthPx);
            var itemContentSize = arrange
                ? item.Arrange(dpi, fullPageSize, contentSize, contentSize, cultureInfo)
                : item.Measure(dpi, fullPageSize, contentSize, contentSize, cultureInfo);
            var itemSize = new Size(
                Math.Max(markerWidthPx, indentPx + itemContentSize.Width),
                Math.Max(markerSize.Height, itemContentSize.Height));
            if (arrange)
                _arrangedItemSizes.Add(itemSize);

            maxWidth = Math.Max(maxWidth, itemSize.Width);
            totalHeight += itemSize.Height;
            if (index < items.Length - 1)
                totalHeight += itemSpacingPx;
        }

        return new Size(maxWidth, totalHeight);
    }

    private Size MeasureMarker(string marker, float dpi, float maxWidth, float markerWidth)
    {
        if (string.IsNullOrEmpty(marker))
            return Size.Zero;

        return _textService.Measure(_markerTextStyle, dpi, marker.AsSpan(), Math.Max(maxWidth, markerWidth));
    }

    private void RenderMarker(IDrawableCanvas canvas, float dpi, string marker, float maxWidth, float markerWidth)
    {
        if (string.IsNullOrEmpty(marker))
            return;

        _textService.Draw(canvas, _markerTextStyle, dpi, marker.AsSpan(), Math.Max(maxWidth, markerWidth));
    }
}
