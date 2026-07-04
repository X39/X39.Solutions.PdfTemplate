using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Canvas;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Controls;

/// <summary>
/// A list item that can contain arbitrary controls.
/// </summary>
[Control(Constants.ControlsNamespace, "li")]
public sealed class ListItemControl : AlignableContentControl
{
    private readonly List<Size> _arrangedChildSizes = new();

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
        if (!Clip)
            return baseAdditionalSize;

        var dryRunCanvas = DryRunDeferredCanvas.From(canvas);
        dryRunCanvas.Translate(ArrangementInner);
        var contentAdditionalSize = RenderChildren(dryRunCanvas, dpi, parentSize, cultureInfo);

        return new Size(
            Math.Max(baseAdditionalSize.Width, contentAdditionalSize.Width),
            baseAdditionalSize.Height + contentAdditionalSize.Height);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        => RenderChildren(canvas, dpi, parentSize, cultureInfo);

    private Size RenderChildren(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        var additionalWidth = 0F;
        var additionalHeight = 0F;
        foreach (var (child, arrangedChildSize) in Children.Zip(_arrangedChildSizes))
        {
            var childAdditionalSize = child.Render(canvas, dpi, parentSize, cultureInfo);
            additionalWidth = Math.Max(additionalWidth, childAdditionalSize.Width);
            additionalHeight += childAdditionalSize.Height;
            canvas.Translate(0F, arrangedChildSize.Height + childAdditionalSize.Height);
        }

        return new Size(additionalWidth, additionalHeight);
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

        return new Size(maxWidth, totalHeight);
    }
}
